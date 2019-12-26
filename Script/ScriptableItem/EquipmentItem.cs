using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Mirror;
using Mirror.Examples.Basic;


//equipment item in thdn for player who can equip or business with merchant
//equipment item in game has some type includes
//WEAPON || ARMOR || SPECIALITEM(IN DUNGEON CAN DESTORY WHEN LEAVE DUNGEON)
[CreateAssetMenu(menuName = "ItemManager/EquipmentItem")]
public class EquipmentItem : UsableItem
{
    
    //Equipment Stats
    
  

    //equipment extra
    public int healthBouns;
    public int damageBouns;
    public int armorBouns;

    public int speedBouns;
    //other vs
    [Range(0,1)]
    public float BlockPer;
    [Range(0,1)]
    public float CriPer;       
    
    public bool canBlock;
    public bool canLevelUp;    //BLACKSMITH


    
    public void CanEquip(Players p, int inventoryIndex,int equipmentIndex)
    {
        
    }

    public  bool CanUse(Player p, int inventoryIndex)
    {
        return true;
    }
    
    public override string Tooltip()
    {
       StringBuilder sb = new StringBuilder(tooltip);
   
       sb.Replace("BLOCKPER", BlockPer.ToString());
       sb.Replace("CRIPER", CriPer.ToString());
       sb.Replace("CANBLOCK", canBlock?"YES":"NO");
       sb.Replace("CANLEVELUP",canLevelUp?"YES":"NO");

       return sb.ToString();
    }
}