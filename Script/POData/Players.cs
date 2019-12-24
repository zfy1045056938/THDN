using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Mathematics;
using Unity.Entities;
using UnityEngine.AI;
using static ScriptableItem;

[SerializeField]
public partial struct EquipmentInfo{
    public string requiredCategory;
    public Transform location;
    public ScriptObjectItemUndAmount defaultItem;
}


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkName))]
[RequireComponent(typeof(NetworkNavAgentBanding))]
    public class Players : Entity
    {

        [Header("Component")]
        public Camera avatarCamera;
        public NetworkNavAgentBanding moveMent;

        [Header("Text Mesh")]


        [Header("Icons")]
        public Sprite portrainIcon;

        [HideInInspector] public string account ="";
        [HideInInspector] public string className="";

        public static Players players;

#region player extends entity implements stats (basic + str und dex und int)
      
        [SerializeField]
        public override int healthMax{
            get{
                int equipBouns=0;
                foreach(InventorySlot slot in equipment)
                    if(slot.amount>0){
                        equipBouns += ((EquipmentItem)slot.item.data).healthBouns;

                        //
                        return base.healthMax + equipBouns ;
                    }
                    return base.healthMax;
                
            }
        }

        //
        [SerializeField]
        public override  int damage{
            get{
                int equipBouns=0;
                foreach(InventorySlot slot in equipment)
                        if(slot.amount>0){
                            equipBouns += ((EquipmentItem)slot.item.data).damageBouns;
                            //
                            return base.damage + equipBouns;
                        }
                        return base.damage;
            }
        }


         //
        [SerializeField]
        public override  int armor{
            get{
                int equipBouns=0;
                foreach(InventorySlot slot in equipment)
                        if(slot.amount>0){
                            equipBouns += ((EquipmentItem)slot.item.data).armorBouns;
                            //
                            return base.armor + armor;
                        }
                return base.armor;
            }
        }

        [SerializeField]
        public override  float blockChance{
            get{
                float equipBouns=0f;
                foreach(InventorySlot slot in equipment)
                        if(slot.amount>0){
                            equipBouns += ((EquipmentItem)slot.item.data).BlockPer;
                            //
                            return base.blockChance + equipBouns;
                        }
                return base.blockChance;
            }
        } //
        [SerializeField]
        public override float crit{
            get{
                float equipBouns=0f;
                foreach(InventorySlot slot in equipment)
                        if(slot.amount>0){
                            equipBouns += ((EquipmentItem)slot.item.data).CriPer;
                            //
                            return base.crit + equipBouns;
                        }
                return base.crit;
            }
        } //
#endregion
 
 
        [SyncVar]public int str;
        [SyncVar]public int dex;
        [SyncVar]public int inte;
        [SyncVar]public int muti;


        [SyncVar,SerializeField]public int _exp=0;
        [SyncVar,SerializeField] public  int level=0;
        public int exp{
            get{return _exp;}
            set{
                _exp=value;
            }
        }

        //
        [Header("Inventory")]
        public int inventorySize =30 ;   //Slot is 30
        
        public ScriptObjectItemUndAmount[] defaultItem;
        [Header("Indict")]


        //default equipment in thdn has 3 slot (weapon,armor,specials(can destory ))
        [Header("Equipment Info")]
        public EquipmentInfo [] equipmentInfos={
            new EquipmentInfo{requiredCategory="Weapon",location=null,defaultItem=new ScriptObjectItemUndAmount()},
            new EquipmentInfo{requiredCategory="Amor",location=null,defaultItem=new ScriptObjectItemUndAmount()},
            new EquipmentInfo{requiredCategory="Specials",location=null,defaultItem=new ScriptObjectItemUndAmount()},  
        };

        //Skill Hot key when learn new skill then add to new bar auto 


     
    public  NavMeshAgent agent;
    public static Dictionary<string,Players> onlinePlayers =new Dictionary<string, Players>();
   

    [SyncVar]
    public float remainingLogoutTime;

    [SyncVar]
    public Classes classType = Classes.Normal;


    public float nextRiskyActionTime=1.0f;

    //
    Camera camera;

    //Player bone use by change equipment(armor und weapon)
    Dictionary<string,Transform> skinBones= new Dictionary<string, Transform>();


   public override void Awake()
    {
       base.Awake();

       //Get Mesh Renderer get the skin bone
       foreach(SkinnedMeshRenderer mesh in GetComponentsInChildren<SkinnedMeshRenderer>()){
           foreach(Transform bone in mesh.bones){
               skinBones[bone.name]=bone;
           }
       }
        
        //
        Util.InvokeMany(typeof(Players),this,"Awake_");

    }
    public Players()
    {
    }

    //Get Player with netId und get player with camera
    public override void  OnStartLocalPlayer(){
            base.OnStartLocalPlayer();

        //Get Avatar in game
        camera.GetComponent<CamMMO>().target=transform;
        GameObject.FindWithTag("MainCamera").GetComponent<CopyPos>();
        if(avatarCamera)avatarCamera.enabled=true;
    
        //Load SKill Bar
        //Load Skill Bar PreLoad 


        //
        Util.InvokeMany(typeof(Players),this,"OnStartLocalPlayer_",camera);
           

    }

    #region  Client
  public override void OnStartClient()
    {
        base.OnStartClient();
         //Func with Equipment callback  Update Model
            // equipment.Callback += OnEquipmentChanged;

            for(int i=0;i<equipment.Count;++i){
                RefreashLoc(i);
            }
    }

    [Client]
    protected override void UpdateClient(){

    }   
    #endregion

    #region  Server

    public override void OnStartServer()
    {
        base.OnStartServer();



        Util.InvokeMany(typeof(Players),this,"OnStartServer_");
    }
    #endregion

    #region FSMC/S  
    //FSM with player has these motion with state
    //1.IDLE
    //2.MOVING(S->E)
    //3.TRADE(S->E)
    //4.BATTLE(THDN S->E)
    //5.DIED
    //--------RPG---------------
    //6.CRAFT
    //7.LOOT FROM ENEMY
    //8.SKILLCASTING(S->E)
    // ->SKILLRequest
    // ->SkillStart
    // ->SkillFinish
    ///desc: after the cal player can use skill to enemy in runtime(Match Time)
    //
    ////////////////////////////
    


    //FSM SERVER
    [Server]
    string UpdateServer_IDLE(){
        if(EventDied()){

        }

        return"IDLE";
    }

    [Server]
    string UpdateServer_MOVING(){

        return "IDLE";
    }


    [Server]
    string UpdateServer_TRADE(){

        return "IDLE";
    }

    [Server]
    string UpdateServer_BATTLE(){

        return "IDLE";
    }

    [Server]
    string UpdateServer_CASTING(){

        return "IDLE";
    }

    [Server]
    string UpdateServer_DEAD(){

        return "IDLE";
    }


    


    [Client]
    string UpdateClient_IDLE(){


        return "IDLE";
    }
    public override void LateUpdate()
    {
        base.LateUpdate();
    }

  


    protected override void UpdateOverlays()
    {
        base.UpdateOverlays();
    }

    #endregion


    //////For interactive for object that occur event (loot || business)
    #region Command

    #endregion

  

    //FSM Callback to server

    [Server]
    public override string UpdateServer()
    {
        if(state=="IDLE")return UpdateServer_IDLE();
        if(state=="MOVING")return UpdateServer_MOVING();
        if(state=="TRADE")return UpdateServer_TRADE();
        if(state=="BATLLE")return UpdateServer_BATTLE();
        if(state=="CASTING")return UpdateServer_CASTING();
        if(state=="DEAD")return UpdateServer_DEAD();
        //
        return "IDLE";
    }

    
    #region  Equipment Module
    void OnEquipmentChanged(){}


    void RefreashLoc(int equipIndex){

    }
    #endregion

    #region Inventory

        [ClientRpc]
        public void RPCUseItem(Item item){}
        
        [Command]
        public void CMDInventoryItemUse(int index){}


    #endregion

    #region Skill Module

    #endregion

    public override void Start()
    {
      if(!isServer && !isClient) return;
      //
      base.Start();
      //get name to server player
      onlinePlayers[name]=this;

      //buff effect TODO
      //IN THDN BUFF ALWAYS HAPPEN IN SET OR EQUIPMENT EFFECT
      

      //
      Util.InvokeMany(typeof(Players),this,"OnStart_");
    }

    #region EVENT FOR FSM 
    bool EventDied(){
        return target.health==0;
    }

    bool EventTargetDisappeared(){return target==null;}
    bool EventMoveStart(){return state!="MOVING" && IsMoving();}

    private bool IsMoving()
    {
        throw new NotImplementedException();
    }

    bool EventMoveEnd(){return state =="MOVING" && !IsMoving();}

    //target is  npc who can business or special
    bool EventTradeStart(){return state!="IDLE"&&!CanTrade();}

    private bool CanTrade()
    {
        throw new NotImplementedException();
    }

    bool EventTradeOver(){return state=="IDLE"&&CanTrade();}
    //Emeny in Dungeon or event in NPC
    bool EventBattle(){return state=="IDLE" && CanBattle();}

    private bool CanBattle()
    {
        throw new NotImplementedException();
    }

    bool EventTalk(){return state=="IDEL" && CanTalk();}

    private bool CanTalk()
    {
        throw new NotImplementedException();
    }

    internal void RefreshLocation(int i)
    {
        throw new NotImplementedException();
    }





    #endregion


    #region CombatSystem

    #endregion

}
