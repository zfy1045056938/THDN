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

public class UICreate:MonoBehaviour{
    public GameObject panel;
    public NetworkManagerTHDN managerTHDN;

    //
    public Button createBtn;
    public Button cancelBtn;

    //
    public Text nameText;
    public Dropdown classText;



    void Start()
    {

        managerTHDN.GetComponent<NetworkManagerTHDN>();
        classText.options = new List<Dropdown.OptionData>{
            
        };

    }
    void Update()
    {

       
         //Add Classes to dropdown list
        classText.options= managerTHDN.playerClasses.Select(
            p=>new Dropdown.OptionData(p.name)
        ).ToList();


        //
        if(NetworkClient.isConnected && panel.activeSelf){
        createBtn.onClick.AddListener(()=>{
            CharacterCreateMsg message=new CharacterCreateMsg{
                names =nameText.text,
                className=classText.value,
                
            };
            NetworkClient.Send(message);
            Hide();
        });
        //
        cancelBtn.onClick.AddListener(()=>{
            panel.SetActive(false);
        });
        }
    }

    public void Open(){
        panel.SetActive(true);
    }
    public void Hide(){
        if(panel.activeSelf){
            panel.SetActive(false);
        }
    }
}