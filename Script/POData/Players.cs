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
        
        

        [SyncVar,SerializeField]public float _exp=0;
        [SyncVar,SerializeField] public  int level=0;
        public float exp{
            get{return _exp;}
            set{
                _exp=value;
            }
        }

        [SyncVar, SerializeField] private float _skillExp = 0;

        public float skillExp
        {
            get
            {
                return _skillExp; 
                
            }
            set { _skillExp = value; }
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


     
   
    public static Dictionary<string,Players> onlinePlayers =new Dictionary<string, Players>();
   

    //last server time
    public double allowLogoutTime => lastCombatTime+((NetworkTime.time));
    public double remainingLogoutTime=>NetworkTime.time < allowLogoutTime?(allowLogoutTime-NetworkTime.time):0;

    [SyncVar]
    public Classes classType = Classes.Normal;


    public double nextRiskyActionTime=1.0;

    [Header("Interactive with Movement")]
    Camera camera;
    bool localPlayerClickThrough;
    int CloserDistance;


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


    //更新客户端信息
    //1.FSM更新状态
    //2.状态变更()
    //3.实体接触触发事件(Talking,Business,Battle(Match3))
    //4.
    [Client]
    protected override void UpdateClient(){
        //根据状态更新行为,分为移动状态,死亡状态，技能状态,攻击状态
        //Inclient
        if(isLocalPlayer){
            if(state=="Moving"||state=="Idle"){
                
            }else if(state=="Casting"){
                
            }else if(state=="Dead"){

            }

        }
        //hooks
        Util.InvokeMany(typeof(Players),this,"UpdateClient_");
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

    //IDLE=>Only Check Dead 
    [Server]
    string UpdateServer_IDLE(){
         // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied())
        {
            // we died.
            OnDeath();
            return "DEAD";
        }
         return "IDLE"; // nothing interesting happened
   
    }

    private bool OnDeath()
    {
        return state=="Dead"&& health<=0;
    }

    [Server]
    string UpdateServer_MOVING(){

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

    //用于角色动作控制器，通过fsm获取动机并执行对应动作
    //在服务器中，获取报文并将请求发送给客户端
    //客户端接受报文并执行动机
    public override void LateUpdate()
    {
        base.LateUpdate();

        //
        foreach(Animator anim in GetComponentsInChildren<Animator>()){
            if(anim!=null){
                //IDLE -> Moving
                anim.SetBool("Moving",state=="Moving"&&IsMoving()&&!IsCasting()&& entityState==EntityAnimState.MOVING);
                //when player health <=0 then dead
                anim.SetBool("Dead",state=="Dead" && state=="Dead"&&IsDead() && entityState==EntityAnimState.DEAD);
                //Use SKill IN THDN THEER skill slot who can config in town, in dungeon player can't change skill .
                anim.SetBool("Casting",state=="Casting"&&IsCasting()&&entityState==EntityAnimState.CASTING);
                // anim.SetBool("")
            }
            //skill module TODO
        }

        //hooks
        Util.InvokeMany(typeof(Players),this,"LateUpdate_");


    }

    #region Motion


    #endregion

  


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
        //
        if(state=="IDLE")return UpdateServer_IDLE();
        if(state=="MOVING")return UpdateServer_MOVING();
       
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

    #region Player Common Module

    [Client]
    //Movement ,player click target place then can move und show indic
    //if trigger on entity check that type und active
    //type==Merchant => business || quest accept || talking(dialogue system)
    //type==Enemy => battle && quest track 
    //typr==Normal=>InteractiveObject(Chest || EventPieces)
    void SelectionHandling(){
        //
        if(Input.GetMouseButtonDown(0) && !Util.IsCursorOverUserInterface() && Input.touchCount <=1){
            Ray ray = camera.ScreenPointToRay(Input.mousePosition); 
            //
            RaycastHit hit;
            bool cast = localPlayerClickThrough?Util.RaycastWithout(ray,out hit):Physics.Raycast(ray,out hit);
            //if get targtet in TriggerEnter then active motion
            Entity entity = hit.transform.GetComponent<Entity>();
            //if server.player >=2 needs check is localplayer
            if(entity){
                //Set Indicator in floor,circle
                SetIndicator(hit.transform);
                //
                if(entity==target && entity!=this){
                    //Check entity Type
                    if(entity is Monster && npcType==NpcType.ENEMY && Util.CloserDistance(collider,target.collider)<= CloserDistance&& target.health>0 ){
                        //Battle Time und active quest track if has quest(incrmen o n destory) 
                        //active dialogue with enemy 
                        

                    }else if(entity is NPC && npcType==NpcType.MERCHANT && Util.CloserDistance(collider,target.collider)<=CloserDistance&&target.health>0){
                        //Business panel
                    }else if(entity is Monster && npcType==NpcType.ENEMY && Util.CloserDistance(collider,target.collider)<=CloserDistance&&target.health==0 &&((Monster)entity).HasLoot()){
                        //Loot From Monster
                    }
                    
                }
                
            }

        }
        //system hooks
        Util.InvokeMany(typeof(Players),this,"HandlingMovement_");
    }

    //target place needs show Circle 
    public void SetIndicator(Transform transform){

    }

    #endregion



    #endregion


    #region CombatSystem

    #endregion


   
}
