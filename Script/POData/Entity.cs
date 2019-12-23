using UnityEngine;
using Mirror;
using System.IO;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using Unity.Mathematics;

public class LinearInt{
    public int baseValue;
    
    public LinearInt(){}
    public LinearInt(int v){
        this.baseValue=v;
    }

    public int Get(int v){
        return v;
    }
}



public class LinearFloat{
    public float baseValue;
    public LinearFloat (){}
    public LinearFloat(float f){
        this.baseValue=f;
    }
    public  float Get(float fs)
    {
        return fs;
    }

}

//Game Entity Base Classes Includes(For THDN has 3 Entities(Player,NPC,Monster))
//1.Stats 
//2.C/S Callback Handler Msg
//3.FSM handler
//4.extra for thdn ,player in dungeon needs interactive object,base on the entity und add extra tools
//for player 
//5.entity for all object who can interactive  with each other 
//6.TODO IF 

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NetworkProximityChecker))]
public abstract class Entity : ScriptableObjectNonAlloc
{
    [Header("Component For Object")]
    public NavMeshAgent agent;
    public NetworkProximityChecker checker;
    public Animator animator;
    public AudioSource audioSource;
    public Collider collider;

    public bool is3D;
    [Header("State FSM")]
    [SyncVar,SerializeField]string _state="IDLE";    //defaul state if move change state from entity
    public string state=> _state;
    [SyncVar]
    public double lastCombatTime=0.0;
    [SyncVar]
    public double lastStateTime=0.0;


    [Header("Component Stats")]
    //for component with basic stats (static )
    [SyncVar]GameObject _target;
    public Entity target{
        get{return _target!=null ? _target.GetComponent<Entity>():null;}
        set{
            _target=value!=null?_target :null;
        }
    }
    [SyncVar]int level=0;
    //player basic in entity are player health + skillability+powerBouns
    //in player have weapon und  equipment bouns then add to the entity values
    
    [SerializeField]protected LinearInt _healthMax=new LinearInt{baseValue=100};
    public virtual int healthMax{
        get{
            //Basic + skillBouns + initExtraAbility +(player)equipmentBouns+ (special)itemBouns
            int passiveBouns=0;
            //skill bouns

            //buff bouns
            int buffBouns=0;
            
            return  _healthMax.Get(healthMax)+passiveBouns+buffBouns;

        }
    
    }
    [SerializeField]protected LinearInt _manaMax=new LinearInt{baseValue=100};
    public virtual int manaMax{
        get{
            int passiveBouns=0;
            //
            int buffBouns=0;
            return _manaMax.Get(manaMax) + passiveBouns+buffBouns;
        }
    }

    [SerializeField]protected LinearInt _damage =new LinearInt{baseValue=100};
    public virtual int damage{
        get{
            int buffBouns=0;
            int skillBouns=0;
            return _damage.Get(damage)+buffBouns+skillBouns;
        }
    }

    [SerializeField]LinearInt _armor=new LinearInt{baseValue=0};
    public virtual int armor{
        get{
            int bufferBouns=0;
            int skillBouns=0;
            return _armor.Get(armor)+bufferBouns+skillBouns;
        }
    }

    [SerializeField]LinearFloat _crit=new LinearFloat{baseValue=0.0f};
    public virtual float crit{
        get{
            return _crit.Get(crit);
        }
    }

    //block needs equipment required that has percent by block(0.0~0.5)
    [SerializeField]LinearFloat _blockChance=new LinearFloat{baseValue=0.0f};
    public float blockChance {
        get{
            int buffBouns =0;
            int skillBouns=0;
            return _blockChance.Get(blockChance)+skillBouns+buffBouns;
        }
    }

    [SerializeField]public int health=0;

    
    [Header("COMMON")]
    [SyncVar,SerializeField] int _gold =0;
    public int gold {get{return _gold;}set{_gold=math.max(value,0);}}

    [HideInInspector] public bool invincible=false;
    [Header("TextUI")]
    public GameObject damagePopupPrefab;
    public GameObject stunnedOverlay;

    protected double stunTimed;

    [HideInInspector]public bool inSafeZone;    //Save house

    [Header("Other")]
    [HideInInspector]public bool isClient=false;
    [HideInInspector]public bool isServer=false;
    
    [Header("Sync")]
    //sync player inventory und equipment
    public SyncItemSlot Inventory =new SyncItemSlot();
    public SyncEquipSlot equipment =new SyncEquipSlot();
    //TODO
    //public SyncListBuff buffs=new SyncListBuff(); 

    //TODO
    //[Header("SKILL MODULE")]
    //skill module contains skill tree that player can use by bouns after level up got bouns.
    // the skill module contains sync includes (buff sync list und skill sync list )entity 
    //public SyncSkillList skill=new SyncSkillList();
    //public SyncBuffList buff=new SyncBuffList();


    public virtual void Awake(){


        Util.InvokeMany(typeof(Entity),this,"_Entity");
    }
    public virtual  void Start()
    {
        if(isClient)animator.enabled=false;

        Util.InvokeMany(typeof(Entity),this,"_Start");
    }

    public virtual void Update(){

    }

    public virtual void LateUpdate()
    {
        
    }

    #region Client
    public virtual void OnStartClient(){}
    protected abstract void UpdateClient();

    #endregion

    #region Server
    public virtual void OnStartServer(){
        if(health==0)_state="DEAD";

        //
    Util.InvokeMany(typeof(Entity),this,"OnStartServer_");
    }
    public abstract string UpdateServer();
    
    #endregion

    #region CommonFSM

    #endregion

        #region Serialized  
       public void Serialized(NetworkWriter writer){
           writer.WriteInt32(healthMax);
           writer.WriteInt32(manaMax);
           writer.WriteInt32(damage);
       }

      

        #endregion

        protected virtual void UpdateOverlays(){}

        #region SkillModule

        #endregion

        #region UI Module Interactive

        //Inventory 

        //Equipment


        #endregion


        #region Combat System (C/S)

        //combat system in THDN 
        //after the time over, collect the pieces u collect und cal the final damage to tatget
        //when health ==0 then DEAD win the battle show the console return the dungeon und keep explore
        //TOTALDAMAGE=PlayerDamaager{BASIC(playerBasic+skillBouns+buffBouns)+MatchesCollect(STR||DEX||INT||MUT||Mixed())+AVG(specialItem value)}
        [Server]
        public void DealDamageToTarget(Entity entity,int amount,float fTime=0f,float lTime=0f){
            int damageDelt=0;
            DamageType damageType=DamageType.Normal;
            //
            if(!entity.invincible){
                //3 state(block,normal atk,crit)
                if(UnityEngine.Random.value < blockChance){
                    damageType=DamageType.Block;
                }else{
                    //deal damage 
                    damageDelt =Mathf.Max(amount-entity.armor,health);
                    if(UnityEngine.Random.value< crit){
                        damageDelt*=2;
                        damageType=DamageType.Crit;
                    }
                    //
                    entity.health -= damageDelt;

                    //
                }
            }
            //
            entity.OnAggro(this);
            //
            entity.RPCOnDamageReceived(damageDelt,damageType);
            //cast time
            lastCombatTime = NetworkTime.time;
            entity.lastCombatTime=NetworkTime.time;
            //system hooks
            Util.InvokeMany(typeof(Entity),this,"_DealDamageAt",damageDelt,damageType,entity,lastCombatTime,lTime,fTime);



        }

    [ClientRpc]
    public void RPCOnDamageReceived(int damage,DamageType damageType){

    }
        #endregion

    public abstract Entity OnAggro(Entity e);
}
