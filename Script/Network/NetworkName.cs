using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class NetworkName : ScriptableObjectNonAlloc
{
 
 public bool OnSerialize(NetworkWriter writer,bool init)
{
    writer.WriteString(name);
    return true;
}
 public override void OnDeserialize(NetworkReader reader,bool init)
{
   name =reader.ReadString();
}

}

