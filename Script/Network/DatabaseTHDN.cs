using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using SQLite;
using Mirror;
using UnityEngine.AI;
using System;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using System.IO;


//////////////DB QUERY => TABLE SAVE APPLICATION PATH
//SQL TABLE QUERY
//character
#region DB TABLE
class characters
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        [Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string name { get; set; }
        [Indexed] // add index on account to avoid full scans when loading characters
        public string account { get; set; }
        public string classname { get; set; } // 'class' isn't available in C#
        // online status can be checked from external programs with either just
        // just 'online', or 'online && (DateTime.UtcNow - lastsaved) <= 1min)
        // which is robust to server crashes too.
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public int health { get; set; }
        public int mana { get; set; }
        public int str { get; set; }
        public int dex { get; set; }
        public int inte { get; set; }
        public int muti { get; set; }
        public int coins { get; set; }
        public float exp { get; set; }
        public float skillExp { get; set; }
        public bool online { get; set; }
        public DateTime lastsaved { get; set; }
        public bool deleted { get; set; }
    }
   
 class accounts
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string name { get; set; }
        public string password { get; set; }
        // created & lastlogin for statistics like CCU/MAU/registrations/...
        public DateTime created { get; set; }
        public DateTime lastlogin { get; set; }
        public bool banned { get; set; }
    }
//inventory
class character_inventory{
    [PrimaryKey]
    public string character { get; set; }

    public int slot { get; set; }
    public string name { get; set; }
    public int amount { get; set; }
    public int summonHealth { get; set; }
    public int summonLevel { get; set; }
    public int summonExp { get; set; }    
}

//equipment
class character_equipment:character_inventory{}

class character_skills{
    [PrimaryKey]
    public string character { get; set; }

    public string name { get; set; }
    public int level { get; set; }
    public float costTimeEnd { get; set; }
    public float cooldownEnd { get; set; }
}

class characters_buff{
    [PrimaryKey]
    public string character { get; set; }

    public string name { get; set; }
    public int level { get; set; }
    public float buffTimeEnd { get; set; }
    
}


//Monster
//Skill
//RogueItems =>TODO
#endregion

//GAME DATABSE STORGE GAMES DATA INCLUDE
//Players & ACCOUNT ->CREATE & LOADSAVE &LOGIN
//CHARACTER PO DATA INCLUDES (INVENTORY & EQUIPMENT)
//LOAD&SAVE Character
public class DatabaseTHDN:MonoBehaviour{
    public static DatabaseTHDN instance;

    //DB file storge at applicationpath
    public string dbFiles = "THDN.sqlite";
    public NetworkManagerTHDN nettmanager;

    public SQLiteConnection sqlConect;

    void Awake()
    {
        if(instance=null)instance=this;

        //ADD HOOKS
    }

   public void Connect(){
        //Create File
        Debug.Log("Connect DB");
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName,dbFiles);
        Debug.Log("Path"+path.ToString());
      
          Debug.Log("Connect");
         sqlConect = new SQLiteConnection(path);


          Debug.Log("create table");
        // create tables if they don't exist yet or were deleted
        sqlConect.CreateTable<accounts>();
        sqlConect.CreateTable<characters>();
        sqlConect.CreateTable<character_inventory>();
        sqlConect.CreateIndex(nameof(character_inventory), new[] {"character", "slot"});
        sqlConect.CreateTable<character_equipment>();
        sqlConect.CreateIndex(nameof(character_equipment), new[] {"character", "slot"});
        sqlConect.CreateTable<character_skills>();
        sqlConect.CreateIndex(nameof(character_skills), new[] {"character", "name"});
        sqlConect.CreateTable<characters_buff>();
        sqlConect.CreateIndex(nameof(characters_buff), new[] {"character", "name"});
        

        // addon system hooks
        Util.InvokeMany(typeof(DatabaseTHDN), this, "Initialize_"); // TODO remove later. let's keep the old hook for a while to not break every single addon!
        Util.InvokeMany(typeof(DatabaseTHDN), this, "Connect_"); // the new hook!

        Debug.Log("connected to database");
    }

   void OnApplicationQuit()
    {
        sqlConect?.Close();
    }

    // account data ////////////////////////////////////////////////////////////
    // try to log in with an account.
    // -> not called 'CheckAccount' or 'IsValidAccount' because it both checks
    //    if the account is valid AND sets the lastlogin field
    public bool TryLogin(string account, string password)
    {
        // this function can be used to verify account credentials in a DatabaseTHDN
        // or a content management system.
        //
        // for example, we could setup a content management system with a forum,
        // news, shop etc. and then use a simple HTTP-GET to check the account
        // info, for example:
        //
        //   var request = new WWW("example.com/verify.php?id="+id+"&amp;pw="+pw);
        //   while (!request.isDone)
        //       print("loading...");
        //   return request.error == null && request.text == "ok";
        //
        // where verify.php is a script like this one:
        //   <?php
        //   // id and pw set with HTTP-GET?
        //   if (isset($_GET['id']) && isset($_GET['pw'])) {
        //       // validate id and pw by using the CMS, for example in Drupal:
        //       if (user_authenticate($_GET['id'], $_GET['pw']))
        //           echo "ok";
        //       else
        //           echo "invalid id or pw";
        //   }
        //   ?>
        //
        // or we could check in a MYSQL DatabaseTHDN:
        //   var dbConn = new MySql.Data.MySqlClient.MySqlsqlConect("Persist Security Info=False;server=localhost;DatabaseTHDN=notas;uid=root;password=" + dbpwd);
        //   var cmd = dbConn.CreateCommand();
        //   cmd.CommandText = "SELECT id FROM Account WHERE id='" + account + "' AND pw='" + password + "'";
        //   dbConn.Open();
        //   var reader = cmd.ExecuteReader();
        //   if (reader.Read())
        //       return reader.ToString() == account;
        //   return false;
        //
        // as usual, we will use the simplest solution possible:
        // create account if not exists, compare password otherwise.
        // no CMS communication necessary and good enough for an Indie MMORPG.
        print("Try Login "+account);
        // not empty?
        if (!string.IsNullOrWhiteSpace(account) && !string.IsNullOrWhiteSpace(password))
        {
            print("Try Login module"+account);
            // demo feature: create account if it doesn't exist yet.
            // note: sqlite-net has no InsertOrIgnore so we do it in two steps
            print("create account if null");
            if (sqlConect.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=?", account) == null)
                sqlConect.Insert(new accounts{ name=account, password=password});

            // check account name, password, banned status
            if (sqlConect.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=? AND password=? and banned=0", account, password) != null)
            {
                // save last login time and return true
                sqlConect.Execute("UPDATE accounts SET lastlogin=? WHERE name=?", DateTime.UtcNow, account);
                return true;
            }
        }
        return false;
    }

   

    // character data //////////////////////////////////////////////////////////
    public bool CharacterExists(string characterName)
    {
        // checks deleted ones too so we don't end up with duplicates if we un-
        // delete one
        return sqlConect.FindWithQuery<characters>("SELECT * FROM characters WHERE name=?", characterName) != null;
    }

    public void CharacterDelete(string characterName)
    {
        // soft delete the character so it can always be restored later
        sqlConect.Execute("UPDATE characters SET deleted=1 WHERE name=?", characterName);
    }

    // returns the list of character names for that account
    // => all the other values can be read with CharacterLoad!
    public List<string> CharacterForAccount(string account)
    {
        List<string> result = new List<string>();
        foreach (characters character in sqlConect.Query<characters>("SELECT * FROM characters WHERE account=? AND deleted=0", account))
            result.Add(character.name);
        return result;
    }

    void LoadInventory(Players Players)
    {
        Debug.Log("Load Inventory DB");
        for (int i = 0; i < Players.inventorySize; i++)
        {
            Players.Inventory.Add(new InventorySlot());
            
        }
        //
        foreach (character_inventory row in sqlConect.Query<character_inventory>(
            "SELECT *FROM character WHERE account=? ", Players.name))
        {
            if (row.slot < Players.inventorySize)
            {
                //has slot then load
                if (ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
                {
                    Item item =new Item();
                    item.summonedHealth = row.summonHealth;
                    item.summonedLevel = row.summonLevel;
                    item.summonedExp = row.summonExp;
                    Players.Inventory[row.slot]=new InventorySlot(item,row.amount);
                }
                else
                {
                    Debug.Log("LoadInventory Failed");
                }
            }
            else
            {
                Debug.Log("LoadInventory Slot");
            }
        }
        

    }

    void LoadEquipment(Players Players)
    {
        
        Debug.Log("Load Equipment DB");
        for (int i = 0; i < Players.equipmentInfos.Length; i++)
        {
            Players.equipment.Add(new InventorySlot());
        }
        foreach (character_equipment row in sqlConect.Query<character_equipment>("SELECT * FROM character_equipment WHERE name=?",
            Players.name))
        {
           if(ScriptableItem.dict.TryGetValue(row.name.GetStableHashCode(),out ScriptableItem itemdata))
           {
               Item item = new Item(itemdata);
               
               item.summonedHealth = row.summonHealth;
               item.summonedLevel = row.summonLevel;
               item.summonedExp = row.summonExp;
               Players.equipment[row.slot]=new InventorySlot(item,row.amount);
           }
        }
    }

    // void LoadSkills(Players Players)
    // {
    //    
    // }
    //
    // void LoadBuffs(Players Players)
    // {
    //     
    // }

    // void LoadQuests(Players Players)
    // {
    //     // load quests
    //     foreach (character_quests row in sqlConect.Query<character_quests>("SELECT * FROM character_quests WHERE character=?", Players.name))
    //     {
    //         ScriptableQuest questData;
    //         if (ScriptableQuest.dict.TryGetValue(row.name.GetStableHashCode(), out questData))
    //         {
    //             Quest quest = new Quest(questData);
    //             quest.progress = row.progress;
    //             quest.completed = row.completed;
    //             Players.quests.Add(quest);
    //         }
    //         else Debug.LogWarning("LoadQuests: skipped quest " + row.name + " for " + Players.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
    //     }
    // }

    // // only load guild when their first Players logs in
    // // => using NetworkManager.Awake to load all guilds.Where would work,
    // //    but we would require lots of memory and it might take a long time.
    // // => hooking into Players loading to load guilds is a really smart solution,
    // //    because we don't ever have to load guilds that aren't needed
    // void LoadGuildOnDemand(Players Players)
    // {
    //     string guildName = sqlConect.ExecuteScalar<string>("SELECT guild FROM character_guild WHERE character=?", Players.name);
    //     if (guildName != null)
    //     {
    //         // load guild on demand when the first Players of that guild logs in
    //         // (= if it's not in GuildSystem.guilds yet)
    //         if (!GuildSystem.guilds.ContainsKey(guildName))
    //         {
    //             Guild guild = LoadGuild(guildName);
    //             GuildSystem.guilds[guild.name] = guild;
    //             Players.guild = guild;
    //         }
    //         // assign from already loaded guild
    //         else Players.guild = GuildSystem.guilds[guildName];
    //     }
    // }

    public GameObject CharacterLoad(string characterName, List<Players> prefabs, bool isPreview)
    {
        characters row = sqlConect.FindWithQuery<characters>("SELECT * FROM characters WHERE name=? AND deleted=0", characterName);
        if (row != null)
        {
            // instantiate based on the class name
            Players prefab = prefabs.Find(p => p.name == row.classname);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                Players players = go.GetComponent<Players>();

                players.name               = row.name;
                players.account            = row.account;
                players.className          = row.classname;
                Vector3 position           =new Vector3(row.x,row.y,row.z);
                players.health = row.health;
                players.manaMax = row.mana;
                players.str = row.str;
                players.dex = row.dex;
                players.inte = row.inte;
                players.muti = row.muti;
                players.exp = row.exp;
                players.skillExp = row.skillExp;
                players.gold = row.coins;


                // is the position on a navmesh?
                // it might not be if we changed the terrain, or if the Players
                // logged out in an instanced dungeon that doesn't exist anymore
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 0.1f, NavMesh.AllAreas))
                {
                    // agent.warp is recommended over transform.position and
                    // avoids all kinds of weird bugs
                    players.agent.Warp(position);
                }
                // otherwise warp to start position
                else
                {
                    Transform start = NetworkManagerTHDN.GetNearestStartPosition(position);
                    players.agent.Warp(start.position);
                    // no need to show the message all the time. it would spam
                    // the server logs too much.
                    //Debug.Log(Players.name + " spawn position reset because it's not on a NavMesh anymore. This can happen if the Players previously logged out in an instance or if the Terrain was changed.");
                }
                
                
                //other
                LoadInventory(players);
                LoadEquipment(players);
               
                // assign health / mana after max values were fully loaded
                // (they depend on equipment, buffs, etc.)
               
                // set 'online' directly. otherwise it would only be set during
                // the next Characterave() call, which might take 5-10 minutes.
                // => don't set it when loading previews though. only when
                //    really joining the world (hence setOnline flag)
                if (!isPreview)
                    sqlConect.Execute("UPDATE characters SET online=1, lastsaved=? WHERE name=?", DateTime.UtcNow, characterName);

                // addon system hooks
                Util.InvokeMany(typeof(DatabaseTHDN), this, "CharacterLoad_", players);

                return go;
            }
            else Debug.LogError("no prefab found for class: " + row.classname);
        }
        return null;
    }

    void SaveInventory(Players Players)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        sqlConect.Execute("DELETE FROM character_inventory WHERE character =?", Players.name);
        for (int i = 0; i < Players.Inventory.Count; i++)
        {
            InventorySlot slot = Players.Inventory[i];
            //
            if (slot.amount > 0)
            {
                sqlConect.InsertOrReplace((new character_inventory
                {
                    character =  Players.name,
                    slot=i,
                    name=slot.item.name,
                    amount=slot.amount,
                    summonHealth = slot.item.summonedHealth,
                    summonExp = slot.item.summonedExp,
                    summonLevel = slot.item.summonedLevel
                }));
                
            }
        }
    }

    void SaveEquipment(Players Players)
    {
        // equipment: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        sqlConect.Execute("DELETE FROM character_equipment WHERE character=?", Players.name);
        for (int i = 0; i < Players.equipment.Count; ++i)
        {
            InventorySlot slot = Players.equipment[i];
            if (slot.amount > 0) // only relevant equip to save queries/storage/time
            {
                sqlConect.InsertOrReplace(new character_equipment{
                    character = Players.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    summonHealth = slot.item.summonedHealth,
                    summonLevel = slot.item.summonedLevel,
                    summonExp = slot.item.summonedExp
                });
            }
        }
    }

    // void SaveSkills(Players Players)
    // {
    //     // skills: remove old entries first, then add all new ones
    //     sqlConect.Execute("DELETE FROM character_skills WHERE character=?", Players.name);
    //     foreach (Skill skill in Players.skills)
    //         if (skill.level > 0) // only learned skills to save queries/storage/time
    //         {
    //             // castTimeEnd and cooldownEnd are based on NetworkTime.time,
    //             // which will be different when restarting the server, so let's
    //             // convert them to the remaining time for easier save & load
    //             // note: this does NOT work when trying to save character data
    //             //       shortly before closing the editor or game because
    //             //       NetworkTime.time is 0 then.
    //             sqlConect.InsertOrReplace(new character_skills{
    //                 character = Players.name,
    //                 name = skill.name,
    //                 level = skill.level,
    //                 castTimeEnd = skill.CastTimeRemaining(),
    //                 cooldownEnd = skill.CooldownRemaining()
    //             });
    //         }
    // }

    // void SaveBuffs(Players Players)
    // {
    //     // buffs: remove old entries first, then add all new ones
    //     sqlConect.Execute("DELETE FROM character_buffs WHERE character=?", Players.name);
    //     foreach (Buff buff in Players.buffs)
    //     {
    //         // buffTimeEnd is based on NetworkTime.time, which will be different
    //         // when restarting the server, so let's convert them to the
    //         // remaining time for easier save & load
    //         // note: this does NOT work when trying to save character data
    //         //       shortly before closing the editor or game because
    //         //       NetworkTime.time is 0 then.
    //         sqlConect.InsertOrReplace(new character_buffs{
    //             character = Players.name,
    //             name = buff.name,
    //             level = buff.level,
    //             buffTimeEnd = buff.BuffTimeRemaining()
    //         });
    //     }
    // }

    // void SaveQuests(Players Players)
    // {
    //     // quests: remove old entries first, then add all new ones
    //     sqlConect.Execute("DELETE FROM character_quests WHERE character=?", Players.name);
    //     foreach (Quest quest in Players.quests)
    //     {
    //         sqlConect.InsertOrReplace(new character_quests{
    //             character = Players.name,
    //             name = quest.name,
    //             progress = quest.progress,
    //             completed = quest.completed
    //         });
    //     }
    // }

    // adds or overwrites character data in the DatabaseTHDN
    public void CharacterSave(Players Players, bool online, bool useTransaction = true)
    {
        // only use a transaction if not called within SaveMany transaction
        if (useTransaction) sqlConect.BeginTransaction();

        //Query DB for field
        sqlConect.InsertOrReplace(new characters{
            name = Players.name,
            account = Players.account,
            classname = Players.className,  
            x=Players.transform.position.x,
            y=Players.transform.position.y,
            z=Players.transform.position.z,
            health = Players.health,
            mana=Players.manaMax,
            str=Players.str,
            inte = Players.inte,
            muti = Players.muti,
            exp = Players.exp,
            skillExp = Players.skillExp,
            online = online,
            lastsaved = DateTime.Now
            
        });

        SaveInventory(Players);
        SaveEquipment(Players);
       
        // addon system hooks
        Util.InvokeMany(typeof(DatabaseTHDN), this, "Characterave_", Players);

        if (useTransaction) sqlConect.Commit();
    }

    // save multiple Character at once (useful for ultra fast transactions)
    public void CharacteraveMany(IEnumerable<Players> Playerss, bool online = true)
    {
        sqlConect.BeginTransaction(); // transaction for performance
        foreach (Players Players in Playerss)
            CharacterSave(Players, online, false);
        sqlConect.Commit(); // end transaction
    }

    // guilds //////////////////////////////////////////////////////////////////
    // public bool GuildExists(string guild)
    // {
    //     return sqlConect.FindWithQuery<guild_info>("SELECT * FROM guild_info WHERE name=?", guild) != null;
    // }

    // Guild LoadGuild(string guildName)
    // {
    //     Guild guild = new Guild();

    //     // set name
    //     guild.name = guildName;

    //     // load guild info
    //     guild_info info = sqlConect.FindWithQuery<guild_info>("SELECT * FROM guild_info WHERE name=?", guildName);
    //     if (info != null)
    //     {
    //         guild.notice = info.notice;
    //     }

    //     // load members list
    //     List<character_guild> rows = sqlConect.Query<character_guild>("SELECT * FROM character_guild WHERE guild=?", guildName);
    //     GuildMember[] members = new GuildMember[rows.Count]; // avoid .ToList(). use array directly.
    //     for (int i = 0; i < rows.Count; ++i)
    //     {
    //         character_guild row = rows[i];

    //         GuildMember member = new GuildMember();
    //         member.name = row.character;
    //         member.rank = (GuildRank)row.rank;

    //         // is this Players online right now? then use runtime data
    //         if (Players.onlinePlayerss.TryGetValue(member.name, out Players Players))
    //         {
    //             member.online = true;
    //             member.level = Players.level;
    //         }
    //         else
    //         {
    //             member.online = false;
    //             // note: FindWithQuery<Character> is easier than ExecuteScalar<int> because we need the null check
    //             Character character = sqlConect.FindWithQuery<Character>("SELECT * FROM Character WHERE name=?", member.name);
    //             member.level = character != null ? character.level : 1;
    //         }

    //         members[i] = member;
    //     }
    //     guild.members = members;
    //     return guild;
    // }

    // public void SaveGuild(Guild guild, bool useTransaction = true)
    // {
    //     if (useTransaction) sqlConect.BeginTransaction(); // transaction for performance

    //     // guild info
    //     sqlConect.InsertOrReplace(new guild_info{
    //         name = guild.name,
    //         notice = guild.notice
    //     });

    //     // members list
    //     sqlConect.Execute("DELETE FROM character_guild WHERE guild=?", guild.name);
    //     foreach (GuildMember member in guild.members)
    //     {
    //         sqlConect.InsertOrReplace(new character_guild{
    //             character = member.name,
    //             guild = guild.name,
    //             rank = (int)member.rank
    //         });
    //     }

    //     if (useTransaction) sqlConect.Commit(); // end transaction
    // }

    // public void RemoveGuild(string guild)
    // {
    //     sqlConect.BeginTransaction(); // transaction for performance
    //     sqlConect.Execute("DELETE FROM guild_info WHERE name=?", guild);
    //     sqlConect.Execute("DELETE FROM character_guild WHERE guild=?", guild);
    //     sqlConect.Commit(); // end transaction
    // }

    // // item mall ///////////////////////////////////////////////////////////////
    // public List<long> GrabCharacterOrders(string characterName)
    // {
        // grab new orders from the DatabaseTHDN and delete them immediately
        //
        // note: this requires an orderid if we want someone else to write to
        // the DatabaseTHDN too. otherwise deleting would delete all the new ones or
        // updating would update all the new ones. especially in sqlite.
        //
        // note: we could just delete processed orders, but keeping them in the
        // DatabaseTHDN is easier for debugging / support.
    //     List<long> result = new List<long>();
    //     List<character_orders> rows = sqlConect.Query<character_orders>("SELECT * FROM character_orders WHERE character=? AND processed=0", characterName);
    //     foreach (character_orders row in rows)
    //     {
    //         result.Add(row.coins);
    //         sqlConect.Execute("UPDATE characters_orders SET processed=1 WHERE orderid=?", row.orderid);
    //     }
    //     return result;
    // }
}