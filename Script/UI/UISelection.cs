using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Mirror;
using System.Linq;
using UnityEngine.AI;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UI;
public class UISelection:MonoBehaviour{
    public GameObject panel;
    public NetworkManagerTHDN managerTHDN;
    public int selectionIndex=-1;
    [Header("Button")]

    public Button createBtn;
    public Button startBtn;
    public Button quitBtn;
    public Button deleteBtn;

    public UICreate createPanel;
    //
     void Start()
    {
        
    }

     void Update()
    {
        if(NetworkClient.isConnected && panel.activeSelf){
            createBtn.onClick.AddListener(()=>{
                createPanel.Open();
            });

            startBtn.onClick.AddListener(()=>{
                CharacterSelectMsg msg = new CharacterSelectMsg{
                    index=selectionIndex,
                };
            NetworkClient.Send(msg);    
            //
            });

            deleteBtn.onClick.AddListener(()=>{
                CharacterDeleteMsg msg =new CharacterDeleteMsg{
                    index=selectionIndex,
                };
                //
                NetworkClient.Send(msg);
                
                
            });

            quitBtn.onClick.AddListener(()=>{
                managerTHDN.OnApplicationQuit();
            });
        }    
    }


    public void Open(){
        if(panel!=null){panel.SetActive(true);}

    }
    public void Close(){
        if(panel!=null && panel.activeSelf){panel.SetActive(false);}
    }
}