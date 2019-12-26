using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System.Text;

public class Skill:MonoBehaviour{
    public string skillName
     { get; set; }
     public string desc { get; set; }
     public int level { get; set; }
     public int bouns { get; set; } //SkillBouns Added
     public float castTime { get; set; }
     public float castTimeEnd { get; set; }
     public float coolDown { get; set; }

    public virtual string Tooltip(){
        StringBuilder sb =new StringBuilder();
        sb.Replace("{NAME}",skillName);
        sb.Replace("{DESC}",desc);
        sb.Replace("{LEVEL}",level.ToString());
        sb.Replace("{BOUNS}",bouns.ToString());
        sb.Replace("{BOUNS}",bouns.ToString());
        sb.Replace("{CASTTIME}",castTime.ToString());
        sb.Replace("{CASTTIMEEND}",castTimeEnd.ToString());
        sb.Replace("{COOLDOWN}",coolDown.ToString());
        return sb.ToString();       
    }
}