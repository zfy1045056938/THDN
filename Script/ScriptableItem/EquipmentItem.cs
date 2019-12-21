using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Mirror;


//equipment item in thdn for player who can equip or business with merchant
//equipment item in game has some type includes
//WEAPON || ARMOR || SPECIALITEM(IN DUNGEON CAN DESTORY WHEN LEAVE DUNGEON)
public class EquipmentItem : UsableItem
{
    
    //Equipment Stats
    
    //BASIC
    
    public int STR;
    public int DEX;
    public int INT;
    
    //other vs
    [Range(0,1)]
    public float BlockPer;
    [Range(0,1)]
    public float CriPer;       
    
    public bool canBlock;
    public bool canLevelUp;    //BLACKSMITH

    public string tooltip = "";
    
    public void CanEquip(Players p, EquipmentSlot slot)
    {
        
    }
    
    
    
    
    public override StringBuilder Tooltip(string text)
    {
       StringBuilder sb = new StringBuilder(tooltip);
       sb.Replace("STR", STR.ToString());
       sb.Replace("DEX", DEX.ToString());
       sb.Replace("INT", INT.ToString());
       sb.Replace("BLOCKPER", BlockPer.ToString());
       sb.Replace("CRIPER", CriPer.ToString());
       sb.Replace("CANBLOCK", canBlock?"YES":"NO");
       sb.Replace("CANLEVELUP",canLevelUp?"YES":"NO");

       return sb;
    }
}