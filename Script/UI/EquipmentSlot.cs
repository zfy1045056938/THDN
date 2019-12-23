using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class EquipmentSlot : MonoBehaviour
{
    public int index;
    
}


//sync drmatic data
public class SyncEquipSlot:SyncList<EquipmentSlot>{}
