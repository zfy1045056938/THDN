using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;



//Zombie sate
//Zombie Client basic state und FSM
public class AIZombieState:AIState{
    
    
    //basic variable
    public AIState state { get; set; }
    public AIStateType stateType { get; set; }
    public AITriggerType TriggerType { get; set; }
    public AIBoneAlignmentType alignmetType { get; set; }
    
    //
    public Transform pos { get; set; }
    public int lowerBodyPart { get; set; }
    public int topBodyPart { get; set; }

    public NavMeshAgent agent;
    public Collider collider;
    public Rigidbody bodyPart;

    public AIState[] staets;
    public Dictionary<AIStateType,AIState>stateDic = new Dictionary<AIStateType, AIState>();

    //Ai motion behaviour
    public float POV { get; set; }
    public float Distance { get; set; }
    public bool IsReachedPos { get; set; }    //has touch object ? Change sate : moving|idle
   


    //Animation HashCache
    
    public override AIStateType GetStateType()
    {
        throw new System.NotImplementedException();
    }

    public override AIStateType OnUpdate()
    {
        throw new System.NotImplementedException();
    }
} 