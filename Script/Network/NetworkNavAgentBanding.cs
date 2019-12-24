using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;


[RequireComponent(typeof(NavMeshAgent))]
public class NetworkNavAgentBanding : ScriptableObjectNonAlloc
{

    public NavMeshAgent agent;
    public Entity entity;


    //
    Vector3 lastServerPos;
    Vector3 lastSentPos;
    double lastSentTime;

    //
    const float epsilon=0.1f;


    //
    void IsValidDestintion(Vector3 pos){

    }

    [Command]
    public void CmdMoved(Vector3 pos){

    }

     void Update()
    {
        if(isServer){
            if(Vector3.Distance(transform.position,lastServerPos)>agent.speed){
                SetDirtyBit(1);
            }
            lastServerPos = transform.position;

        }
        //
        if(isLocalPlayer){
            if(NetworkTime.time >= lastSentTime + syncInterval){
                if(isServer){
                    SetDirtyBit(1);
                }else{
                    CmdMoved(transform.position);
                }
                //
                lastSentTime = NetworkTime.time;
                lastSentPos= transform.position;
            }
        }
    }

    [Server]
    public void ResetMovement(){
        TargetResetMovment(transform.position);

        SetDirtyBit(1);
    }

    [TargetRpc]
    public void TargetResetMovment(Vector3 pos){
        agent.ResetMovement();
        agent.Warp(pos);
    }

    public  bool OnSerize(NetworkWriter write ,bool isInit){
        write.WriteVector3(transform.position);
        write.WriteSingle(agent.speed);

        return true;
    }

    public override void OnDeserialize(NetworkReader reader,bool isInit){
        Vector3 pos = reader.ReadVector3();
        float spd= reader.ReadSingle();

        if(agent.isOnNavMesh){
            if(NavMesh.SamplePosition(pos,out NavMeshHit hit,0.1f,NavMesh.AllAreas)){
                if(!isLocalPlayer){
                    agent.stoppingDistance=0;
                    agent.speed=spd;
                    agent.destination=pos;
                }
            }
            //
            if(Vector3.Distance(transform.position,pos)>agent.speed *2 && agent.isOnNavMesh){
                agent.Warp(pos);
            }
        }else{
            Debug.Log("");
        }
    }
   
}
