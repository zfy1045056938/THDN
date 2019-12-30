using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.AI;
using System.Collections.Generic;

//Target of AI,when player trigger to target
public class AITarget{
    public AITarget(AITargetType aITarget, Collider collider, Vector3 position, float distance, float time)
    {
        this.aITarget = aITarget;
        this.collider = collider;
        this.position = position;
        this.distance = distance;
        this.time = time;
    }

    public AITargetType aITarget { get; set; }
    public Collider collider { get; set; }
    public Vector3 position { get; set; }
    public float distance { get; set; }
    public float time { get; set; }

     public void  Clear()
    {
        this.aITarget = AITargetType.None;
        this.collider = null;
        this.position = Vector3.zero;
        this.distance = 0;
        this.time = Mathf.Infinity;
    }

    

    public AITarget(){}
}
//AI State For Emeny(zombie ...)
//abstract class for animation object can do these things
//Animtor
//1.IDLE,WALING,RUNNING,FEEDING(ZOMBIE),ATTACK(CORE!),DEAD
//about relationship with interactive object
//1.COLLIDER,SENSOR(RADIUS),WAYPOINT POS,
public  abstract class AIStateMachine : MonoBehaviour
{
   
   //basic variable 
    public AITarget visualTarget= new AITarget();
    public AITarget threadTarget=new AITarget();

    //proptected
    public AIState currentState { get; set; }
    public AITarget aITarget { get; set; }
    public Dictionary<AIStateType,AIState> stateDic { get; set; }
    public int target { get; set; }
    public int rootPosRefCount { get; set; }
    public int rootRotationRefCount { get; set; }
    public bool isTargetReached { get; set; }
    public List<Rigidbody> bodyPart { get; set; }
    public int aiBoardpartLayer { get; set; }

    public Dictionary<string,bool> animLayersActive { get; set; }
    
    [SerializeField]public AIStateType currentStateType { get; set; }
    [SerializeField]public Transform rootBone { get; set; }
    [SerializeField]public AIBoneAlignmentType aIBoneAlignmentType { get; set; }
    [SerializeField]public SphereCollider targetTrigger { get; set; }
    [SerializeField]public SphereCollider sensorTrigger { get; set; }
    [SerializeField]public bool randomPatrol { get; set; }
    [SerializeField]public int currentWayPoint { get; set; }
    [SerializeField][UnityEngine.Range(0f,15f)]public float stoppingDistance { get; set; }

    //layer audio conrol
    public ILayerAudioSource layeraudioSource { get; set; }

    //component cache
    public Animator animator=null;
    public NavMeshAgent agent=null;
    public Collider colliders= null;
    public Transform transforms=null;

    //
   
    public int inMeeleRange { get; set; }
    //反射器距离
    public Vector3 sensorPos { get; set; }
    public Vector3 sensorRaduid { get; set; }

    //target id
    public int targetColliderID { get; set; }

    public virtual  void Awake()
    {
        transforms=transform;
        animator=GetComponent<Animator>();
        agent=GetComponent<NavMeshAgent>();
        colliders=GetComponent<Collider>();
        
        //
        AudioSource source=GetComponent<AudioSource>();

        //Register AiStateMachine TODO

        //get body
        if(rootBone!=null){
            Rigidbody[]bones = rootBone.GetComponentsInChildren<Rigidbody>();
            foreach(Rigidbody bs in bones){
                if(bodyPart!=null && bs.gameObject.layer==aiBoardpartLayer){
                    bodyPart.Add(bs);
                    //Register machine
                }
            }
        }

        //Register Audio Machine
    }

    public virtual void Start(){
            //TODO SENSOR
            if(sensorTrigger!=null){AISensor sensor = sensorTrigger.GetComponent<AISensor>();}

            //
            AIState[] states=GetComponents<AIState>();

            //
            foreach(AIState s in states){
                if(states.Length!=0&&!stateDic.ContainsKey(states.GetStateType())){
                    states[states.GetStateType()]=s;
                    //
                    states.SetStateMachine(this);
                }
            }

            //current state
            if(stateDic.ContainsKey(currentStateType)){
                currentState = stateDic[currentStateType];
                currentState.OnEnterState();
            }else{
                currentState=null;
            }

            //animator foreach way
            if(animator){
                //TODO

            }
    }

    //Cal distance by player und enemy
    public virtual  void FixedUpdate()
    {
        visualTarget.Clear();
        threadTarget.Clear();
        
        //distace
        if(aITarget.aITarget != AITargetType.None){
            aITarget.distance=Vector3.Distance(aITarget.position,transforms.position);
        }


        isTargetReached=false;
    }

    //Get state time 
    public virtual void Update()
    {
        if(currentState==null)return;

        //
        AIStateType newStateType =currentState.OnUpdate();


        //If target change state in animation(IDLE->Any Animation)
        if(newStateType!=currentStateType){
            AIState states=null;
            if(stateDic.TryGetValue(newStateType,out states)){
                currentState.OnExitState();
                states.OnEnterState();
                currentState=states;    
            }else if(stateDic.TryGetValue(AIStateType.Idle,out states)){
                currentState.OnExitState();
                states.OnEnterState();
                currentState=states;    //Update by animation
            }

            currentStateType=newStateType;
        }

        //
    }

   

    #region SetTarget
    public void SetTarget(){}

    #endregion

   #region WayPoint Module

   #endregion


   #region Trigger Event
    protected virtual void OnTriggerEnter(Collider other){}
    protected virtual void OnTriggerExit(Collider other){}
    protected virtual void OnTriggerStay(Collider other){}

    public virtual void OnTriggerEvent(Collider other){}
    
   #endregion


   #region Animation Event

   #endregion

    #region  object audio
    public void PlayAudio(){}
    public void StopAudio(){}
    public void MuteAudio(){}

    #endregion

    #region LayerActive

    #endregion
    

    public virtual void TakeDamage(Vector3 pos,Vector3 zero,int damage,Rigidbody body,Players player){}
}
