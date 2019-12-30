using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//AI STATE FOR ALL AI SYSTEM(FSM)
public abstract class AIState : MonoBehaviour
{

    public AIState[] state { get; set; }


    //State Event Visutal 
    public virtual void OnEnterState(){}
    public virtual void OnExitState(){}
    public virtual void OnAnimationIKUpdated(){}
    public virtual void OnTriggerEvent(AITriggerType aITriggerType,Collider other){}
    public virtual void OnDestionReached(bool isReach){}
    
    //
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();


    protected AIStateMachine stateMachine{get;set;}

    //TODO
    public virtual void OnAnimationUpdated(){

    }


    // Start is called before the first frame update
    void Start()
    {
        state=GetComponents<AIState>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
