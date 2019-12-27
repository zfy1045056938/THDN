using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Text.RegularExpressions;
using System.Linq;



public class NetworkAus : NetworkAuthenticator
{ [Header("Components")]
    public NetworkManagerTHDN manager;

    // login info for the local player
    // we don't just name it 'account' to avoid collisions in handshake
    [Header("Login")]
    public string loginAccount = "admin";
    public string loginPassword = "123";

    [Header("Security")]
    public string passwordSalt = "at_least_16_byte";
    public int accountMaxLength = 16;

    public DatabaseTHDN database;


    void Start()
    {
        database=GetComponent<DatabaseTHDN>();
    }

    // client //////////////////////////////////////////////////////////////////
    public override void OnStartClient()
    {
        // register login success message, allowed before authenticated
        NetworkClient.RegisterHandler<LoginSuccessMsg>(OnClientLoginSuccess, false);
    }

    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        // send login packet with hashed password, so that the original one
        // never leaves the player's computer.
        //
        // it's recommended to use a different salt for each hash. ideally we
        // would store each user's salt in the DatabaseTHDN. to not overcomplicate
        // things, we will use the account name as salt (at least 16 bytes)
        //
        // Application.version can be modified under:
        // Edit -> Project Settings -> Player -> Bundle Version
        string hash = Util.PBKDF2Hash(loginPassword, passwordSalt + loginAccount);
        LoginMsg message = new LoginMsg{account="admin", pwd="123", version=Application.version};
    
                    conn.Send(message);
     
        print("login message was sent");

        // set state
        manager.state = NetworkState.HandShake;
    }

    void OnClientLoginSuccess(NetworkConnection conn, LoginSuccessMsg msg)
    {
        // authenticated successfully. OnClientConnected will be called.
        OnClientAuthenticated.Invoke(conn);
    }

    // server //////////////////////////////////////////////////////////////////
    public override void OnStartServer()
    {
        // register login message, allowed before authenticated
        NetworkServer.RegisterHandler<LoginMsg>(OnServerLogin, false);
    }

    public override void OnServerAuthenticate(NetworkConnection conn)
    {
        // wait for LoginMsg from client
    }

    public bool IsAllowedAccountName(string account)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for DatabaseTHDN safety etc.)
        return account.Length <= accountMaxLength &&
               Regex.IsMatch(account, @"^[a-zA-Z0-9_]+$");
    }

    bool AccountLoggedIn(string account)
    {
        // in lobby or in world?
        return manager.lobby.ContainsValue(account) ||
               Players.onlinePlayers.Values.Any(p => p.account == account);
    }

    void OnServerLogin(NetworkConnection conn, LoginMsg message)
    {
        Debug.Log("Login Module for server");
        // correct version?
        if (message.version == Application.version)
        {   
            Debug.Log("Version"+Application.version);
            // allowed account name?
            if (IsAllowedAccountName(message.account))
            {
                Debug.Log("try login");
                // validate account info
                if (database.TryLogin(message.account, message.pwd))
                {
                    Debug.Log("log success");
                    // not in lobby and not in world yet?
                    if (!AccountLoggedIn(message.account))
                    {
                        // add to logged in accounts
                        manager.lobby[conn] = message.account;

                        // login successful
                        Debug.Log("login successful: " + message.account);

                        // notify client about successful login. otherwise it
                        // won't accept any further messages.
                        conn.Send(new LoginSuccessMsg());

                        // authenticate on server
                        OnServerAuthenticated.Invoke(conn);
                    }
                    else
                    {
                        //print("account already logged in: " + message.account); <- don't show on live server
                        manager.ServerSendError(conn, "already logged in", true);

                        // note: we should disconnect the client here, but we can't as
                        // long as unity has no "SendAllAndThenDisconnect" function,
                        // because then the error message would never be sent.
                        //conn.Disconnect();
                    }
                }
                else
                {
                    //print("invalid account or password for: " + message.account); <- don't show on live server
                    manager.ServerSendError(conn, "invalid account", true);
                }
            }
            else
            {
                //print("account name not allowed: " + message.account); <- don't show on live server
                manager.ServerSendError(conn, "account name not allowed", true);
            }
        }
        else
        {
            //print("version mismatch: " + message.account + " expected:" + Application.version + " received: " + message.version); <- don't show on live server
            manager.ServerSendError(conn, "outdated version", true);
        }
    }
  
}
