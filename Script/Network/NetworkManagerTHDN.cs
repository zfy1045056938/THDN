using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Mirror;
using System.Linq;
using UnityEngine.AI;
using System.Text.RegularExpressions;


//THDN NetworkManager,Server/Client Module With server und have a DatabaseTHDN 
//storge game data with client

[SerializeField]
public class ServerInfo{
    public string ip;
    public string name;
}

[SerializeField]
public class SaveInfo{
    public string playerName;
    public DateTime Datetime =DateTime.Now;

    public bool isOnline;

    public Sprite loc;
}


[RequireComponent(typeof(DatabaseTHDN))]
public class NetworkManagerTHDN: NetworkManager
{
   // current network manager state on client
    public NetworkState state = NetworkState.Offline;

    // <conn, account> dict for the lobby
    // (people that are still creating or selecting characters)
    public Dictionary<NetworkConnection, string> lobby = new Dictionary<NetworkConnection, string>();

    // UI components to avoid FindObjectOfType
    // [Header("UI")]
    // public //uiPopup //uiPopup;

    // we may want to add another game server if the first one gets too crowded.
    // the server list allows people to choose a server.
    //
    // note: we use one port for all servers, so that a headless server knows
    // which port to bind to. otherwise it would have to know which one to
    // choose from the list, which is far too complicated. one port for all
    // servers will do just fine for an Indie MMORPG.
    [Serializable]
    public class ServerInfo
    {
        public string name;
        public string ip;
    }
    public List<ServerInfo> serverList = new List<ServerInfo>() {
        new ServerInfo{name="Local", ip="localhost"}
    };

    [Header("Logout")]
    [Tooltip("Players shouldn't be able to log out instantly to flee combat. There should be a delay.")]
    public float combatLogoutDelay = 5;

    [Header("Character Selection")]
    public int selection = -1;
    public Transform[] selectionLocations;
    public Transform selectionCameraLocation;
    [HideInInspector] public List<Players> playerClasses = new List<Players>(); // cached in Awake

    [Header("DatabaseTHDN")]
    public int characterLimit = 4;
    public int characterNameMaxLength = 16;
    public float saveInterval = 60f; // in seconds
   
    // store characters available message on client so that UI can access it
    [HideInInspector] public CharacterAvailableMsg charactersAvailableMsg;

    public DatabaseTHDN database;

    // name checks /////////////////////////////////////////////////////////////
    public bool IsAllowedCharacterName(string characterName)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for DatabaseTHDN safety etc.)
        return characterName.Length <= characterNameMaxLength &&
               Regex.IsMatch(characterName, @"^[a-zA-Z0-9_]+$");
    }

    // nearest startposition ///////////////////////////////////////////////////
    public static Transform GetNearestStartPosition(Vector3 from) =>
        Util.GetNearestTransform(startPositions, from);

    // player classes //////////////////////////////////////////////////////////]
    public List<Players> FindPlayerClasses()
    {
        // filter out all Player prefabs from spawnPrefabs
        // (avoid Linq for performance/gc. players are spawned a lot. it matters.)
        List<Players> classes = new List<Players>();
        foreach (GameObject prefab in spawnPrefabs)
        {
            Players player = prefab.GetComponent<Players>();
            if (player != null)
                classes.Add(player);
        }
        return classes;
    }

    // events //////////////////////////////////////////////////////////////////
    public override void Awake()
    {
        base.Awake();

        // cache list of player classes from spawn prefabs.
        // => we assume that this won't be changed at runtime (why would it?)
        // => this is way better than looping all prefabs in character
        //    select/create/delete each time!
        playerClasses = FindPlayerClasses();
        //
        database= GetComponent<DatabaseTHDN>();
    }

    public override void Start()
    {
        base.Start();

        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "Start_");
    }

    void Update()
    {
        // any valid local player? then set state to world
        if (ClientScene.localPlayer != null)
            state = NetworkState.World;
    }

    // error messages //////////////////////////////////////////////////////////
    public void ServerSendError(NetworkConnection conn, string error, bool disconnect)
    {
        conn.Send(new ErrorMsg{msg=error, causesDisconnect=disconnect});
    }

    void OnClientError(NetworkConnection conn, ErrorMsg message)
    {
        print("OnClientError: " + message.msg);

        // show a popup
        //uiPopup.Show(message.msg);

        // disconnect if it was an important network error
        // (this is needed because the login failure message doesn't disconnect
        //  the client immediately (only after timeout))
        if (message.causesDisconnect)
        {
            conn.Disconnect();

            // also stop the host if running as host
            // (host shouldn't start server but disconnect client for invalid
            //  login, which would be pointless)
            if (NetworkServer.active) StopHost();
        }
    }

    // start & stop ////////////////////////////////////////////////////////////
    public override void OnStartClient()
    {
        // setup handlers
        NetworkClient.RegisterHandler<ErrorMsg>(OnClientError, false); // allowed before auth!
        NetworkClient.RegisterHandler<CharacterAvailableMsg>(OnClientCharactersAvailable);

        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnStartClient_");
    }

    public override void OnStartServer()
    {
        // connect to DatabaseTHDN
      
        database.Connect();
       
        // handshake packet handlers (in OnStartServer so that reconnecting works)
        NetworkServer.RegisterHandler<CharacterCreateMsg>(OnServerCharacterCreate);
        NetworkServer.RegisterHandler<CharacterSelectMsg>(OnServerCharacterSelect);
        NetworkServer.RegisterHandler<CharacterDeleteMsg>(OnServerCharacterDelete);

        // invoke saving
        InvokeRepeating(nameof(SavePlayers), saveInterval, saveInterval);

        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnStartServer_");
    }

    public override void OnStopServer()
    {
        print("OnStopServer");
        CancelInvoke(nameof(SavePlayers));

        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnStopServer_");
    }

    // handshake: login ////////////////////////////////////////////////////////
    public bool IsConnecting() => NetworkClient.active && !ClientScene.ready;

    // called on the client if a client connects after successful auth
    public override void OnClientConnect(NetworkConnection conn)
    {
        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnClientConnect_", conn);

        // do NOT call base function, otherwise client becomes "ready".
        // => it should only be "ready" after selecting a character. otherwise
        //    it might receive world messages from monsters etc. already
        //base.OnClientConnect(conn);
    }

    // called on the server if a client connects after successful auth
    public override void OnServerConnect(NetworkConnection conn)
    {
        // grab the account from the lobby
        string account = lobby[conn];

        // send necessary data to client
        conn.Send(MakeCharactersAvailableMessage(account));

        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnServerConnect_", conn);
    }

    // the default OnClientSceneChanged sets the client as ready automatically,
    // which makes no sense for MMORPG situations. this was more for situations
    // where the server tells all clients to load a new scene.
    // -> setting client as ready will cause 'already set as ready' errors if
    //    we call StartClient before loading a new scene (e.g. for zones)
    // -> it's best to just overwrite this with an empty function
    public override void OnClientSceneChanged(NetworkConnection conn) {}

    // helper function to make a CharactersAvailableMsg from all characters in
    // an account
    CharacterAvailableMsg MakeCharactersAvailableMessage(string account)
    {
        // load from DatabaseTHDN
        // (avoid Linq for performance/gc. characters are loaded frequently!)
        List<Players> characters = new List<Players>();
        foreach (string characterName in database.CharacterForAccount(account))
        {
            GameObject player = database.CharacterLoad(characterName, playerClasses, true);
            characters.Add(player.GetComponent<Players>());
        }

        // construct the message
        CharacterAvailableMsg message = new CharacterAvailableMsg();
        message.Load(characters);

        // destroy the temporary players again and return the result
        characters.ForEach(player => Destroy(player.gameObject));
        return message;
    }

    // handshake: character selection //////////////////////////////////////////
    void LoadPreview(GameObject prefab, Transform location, int selectionIndex, CharacterAvailableMsg.CharacterPreview character)
    {
        // instantiate the prefab
        GameObject preview = Instantiate(prefab.gameObject, location.position, location.rotation);
        preview.transform.parent = location;
        Players player = preview.GetComponent<Players>();

        // assign basic preview values like name and equipment
        player.name = character.name;
        for (int i = 0; i < character.equipment.Length; ++i)
        {
            InventorySlot slot = character.equipment[i];
            player.equipment.Add(slot);
            if (slot.amount > 0)
            {
                // OnEquipmentChanged won't be called unless spawned, we
                // need to refresh manually
                player.RefreashLoc(i);
            }
        }

        // add selection script
        preview.AddComponent<SelectableCharacter>();
        preview.GetComponent<SelectableCharacter>().index = selectionIndex;
    }

    public void ClearPreviews()
    {
        selection = -1;
        foreach (Transform location in selectionLocations)
            if (location.childCount > 0)
                Destroy(location.GetChild(0).gameObject);
    }

    void OnClientCharactersAvailable(NetworkConnection conn, CharacterAvailableMsg message)
    {
        charactersAvailableMsg = message;
        print("characters available:" + charactersAvailableMsg.characters.Length);

        // set state
        state = NetworkState.Lobby;

        // clear previous previews in any case
        ClearPreviews();

        // load previews for 3D character selection
        for (int i = 0; i < charactersAvailableMsg.characters.Length; ++i)
        {
            CharacterAvailableMsg.CharacterPreview character = charactersAvailableMsg.characters[i];

            // find the prefab for that class
            Players prefab = playerClasses.Find(p => p.name == character.className);
            if (prefab != null)
                LoadPreview(prefab.gameObject, selectionLocations[i], i, character);
            else
                Debug.LogWarning("Character Selection: no prefab found for class " + character.className);
        }

        // setup camera
        Camera.main.transform.position = selectionCameraLocation.position;
        Camera.main.transform.rotation = selectionCameraLocation.rotation;

        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnClientCharactersAvailable_", charactersAvailableMsg);
    }

    // handshake: character creation ///////////////////////////////////////////
    // find a NetworkStartPosition for this class, or a normal one otherwise
    // (ignore the ones with playerPrefab == null)
    public Transform GetStartPositionFor(string className)
    {
        // avoid Linq for performance/GC. players spawn frequently!
        foreach (Transform startPosition in startPositions)
        {
            NetworkStartPositionForClass spawn = startPosition.GetComponent<NetworkStartPositionForClass>();
            if (spawn != null &&
                spawn.playerPrefab != null &&
                spawn.playerPrefab.name == className)
                return spawn.transform;
        }
        // return any start position otherwise
        return GetStartPosition();
    }

    Players CreateCharacter(GameObject classPrefab, string characterName, string account)
    {
        // create new character based on the prefab.
        // -> we also assign default items and equipment for new characters
        // -> skills are handled in DatabaseTHDN.CharacterLoad every time. if we
        //    add new ones to a prefab, all existing players should get them
        // (instantiate temporary player)
        //print("creating character: " + message.name + " " + message.classIndex);
        Players player = Instantiate(classPrefab).GetComponent<Players>();
        player.name = characterName;
        player.account = account;
        player.className = classPrefab.name;
        
        // for (int i = 0; i < player.inventorySize; ++i)
        // {
        //     // add empty slot or default item if any
        //     player.inventory.Add(i < player.defaultItems.Length ? new ItemSlot(new Item(player.defaultItems[i].item), player.defaultItems[i].amount) : new ItemSlot());
        // }
        // for (int i = 0; i < player.equipmentInfo.Length; ++i)
        // {
        //     // add empty slot or default item if any
        //     EquipmentInfo info = player.equipmentInfo[i];
        //     player.equipment.Add(info.defaultItem.item != null ? new ItemSlot(new Item(info.defaultItem.item), info.defaultItem.amount) : new ItemSlot());
        // }
        // player.health = player.healthMax; // after equipment in case of boni
        // player.mana = player.manaMax; // after equipment in case of boni

        return player;
    }

    void OnServerCharacterCreate(NetworkConnection conn, CharacterCreateMsg message)
    {
        print("OnServerCharacterCreate " + conn);

        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            // allowed character name?
            if (IsAllowedCharacterName(message.names))
            {
                // not existant yet?
                string account = lobby[conn];
                if (!database.CharacterExists(message.names))
                {
                    // not too may characters created yet?
                    if (database.CharacterForAccount(account).Count < characterLimit)
                    {
                        // valid class index?
                       
                            // create new character based on the prefab.
                            Players player = CreateCharacter(playerClasses[message.className].gameObject, message.names, account);

                            // addon system hooks
                            Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnServerCharacterCreate_", message, player);

                            if(player!=null){
                            // save the player to db
                            database.CharacterSave(player, false);
                            Destroy(player.gameObject);
                            }
                            // send available characters list again, causing
                            // the client to switch to the character
                            // selection scene again
                            conn.Send(MakeCharactersAvailableMessage(account));

                            Debug.Log("Create Success"+player.name);
                      
                    }
                    else
                    {
                        //print("character limit reached: " + message.name); <- don't show on live server
                        ServerSendError(conn, "character limit reached", false);
                    }
                }
                else
                {
                    //print("character name already exists: " + message.name); <- don't show on live server
                    ServerSendError(conn, "name already exists", false);
                }
            }
            else
            {
                //print("character name not allowed: " + message.name); <- don't show on live server
                ServerSendError(conn, "character name not allowed", false);
            }
        }
        else
        {
            //print("CharacterCreate: not in lobby"); <- don't show on live server
            ServerSendError(conn, "CharacterCreate: not in lobby", true);
        }
    }

    // overwrite the original OnServerAddPlayer function so nothing happens if
    // someone sends that message.
    public override void OnServerAddPlayer(NetworkConnection conn) { Debug.LogWarning("Use the CharacterSelectMsg instead"); }

    void OnServerCharacterSelect(NetworkConnection conn, CharacterSelectMsg message)
    {
        print("OnServerCharacterSelect");
        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            // read the index and find the n-th character
            // (only if we know that he is not ingame, otherwise lobby has
            //  no netMsg.conn key)
            string account = lobby[conn];
            List<string> characters = database.CharacterForAccount(account);

            // validate index
            if (0 <= message.index && message.index < characters.Count)
            {
                //print(account + " selected player " + characters[index]);

                // load character data
                GameObject go = database.CharacterLoad(characters[message.index], playerClasses, false);

                // add to client
                NetworkServer.AddPlayerForConnection(conn, go);

                // addon system hooks
                Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnServerCharacterSelect_", account, go, conn, message);
#pragma warning disable CS0618 // AddPlayerMessage.value is obsolete
                Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnServerAddPlayer_", account, go, conn, new AddPlayerMessage{ value=BitConverter.GetBytes(message.index) }); // old hook
#pragma warning restore CS0618 // AddPlayerMessage.value is obsolete
                // remove from lobby
                lobby.Remove(conn);
            }
            else
            {
                print("invalid character index: " + account + " " + message.index);
                ServerSendError(conn, "invalid character index", false);
            }
        }
        else
        {
            print("CharacterSelect: not in lobby" + conn);
            ServerSendError(conn, "CharacterSelect: not in lobby", true);
        }
    }

    void OnServerCharacterDelete(NetworkConnection conn, CharacterDeleteMsg message)
    {
        print("OnServerCharacterDelete " + conn);

        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            string account = lobby[conn];
            List<string> characters = database.CharacterForAccount(account);

            // validate index
            if (0 <= message.index && message.index < characters.Count)
            {
                // delete the character
                print("delete character: " + characters[message.index]);
                database.CharacterDelete(characters[message.index]);

                // addon system hooks
                Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnServerCharacterDelete_", message);

                // send the new character list to client
                conn.Send(MakeCharactersAvailableMessage(account));
            }
            else
            {
                print("invalid character index: " + account + " " + message.index);
                ServerSendError(conn, "invalid character index", false);
            }
        }
        else
        {
            print("CharacterDelete: not in lobby: " + conn);
            ServerSendError(conn, "CharacterDelete: not in lobby", true);
        }
    }

    // player saving ///////////////////////////////////////////////////////////
    // we have to save all players at once to make sure that item trading is
    // perfectly save. if we would invoke a save function every few minutes on
    // each player seperately then it could happen that two players trade items
    // and only one of them is saved before a server crash - hence causing item
    // duplicates.
    void SavePlayers()
    {
        database.CharacteraveMany(Players.onlinePlayers.Values);
        if (Players.onlinePlayers.Count > 0) Debug.Log("saved " + Players.onlinePlayers.Count + " player(s)");
    }

    // stop/disconnect /////////////////////////////////////////////////////////
    // called on the server when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //print("OnServerDisconnect " + conn);

        // players shouldn't be able to log out instantly to flee combat.
        // there should be a delay.
        float delay = 0;
        if (conn.identity != null)
        {
            Players player = conn.identity.GetComponent<Players>();
            delay = (float)player.remainingLogoutTime;
        }

        StartCoroutine(DoServerDisconnect(conn, delay));
    }

    IEnumerator<WaitForSeconds> DoServerDisconnect(NetworkConnection conn, float delay)
    {
        yield return new WaitForSeconds(delay);

        //print("DoServerDisconnect " + conn);

        // save player (if any. nothing to save if disconnecting while in lobby.)
        if (conn.identity != null)
        {
            database.CharacterSave(conn.identity.GetComponent<Players>(),false);
            print("saved:" + conn.identity.name);
        }

        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnServerDisconnect_", conn);

        // remove logged in account after everything else was done
        lobby.Remove(conn); // just returns false if not found

        // do base function logic (removes the player for the connection)
        base.OnServerDisconnect(conn);
    }

    // called on the client if he disconnects
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        print("OnClientDisconnect");

        // show a popup so that users know what happened
        //uiPopup.Show("Disconnected.");

        // call base function to guarantee proper functionality
        base.OnClientDisconnect(conn);

        // set state
        state = NetworkState.Offline;

        // addon system hooks
        Util.InvokeMany(typeof(NetworkManagerTHDN), this, "OnClientDisconnect_", conn);
    }

 

    // public override void OnValidate()
    // {
    //     base.OnValidate();

    //     // ip has to be changed in the server list. make it obvious to users.
    //     if (!Application.isPlaying && networkAddress != "")
    //         networkAddress = "Use the Server List below!";

    //     // need enough character selection locations for character limit
    //     if (selectionLocations.Length != characterLimit)
    //     {
    //         // create new array with proper size
    //         Transform[] newArray = new Transform[characterLimit];

    //         // copy old values
    //         for (int i = 0; i < Mathf.Min(characterLimit, selectionLocations.Length); ++i)
    //             newArray[i] = selectionLocations[i];

    //         // use new array
    //         selectionLocations = newArray;
    //     }
    // }
}