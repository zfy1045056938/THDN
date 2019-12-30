
//MATCH MODULE ALL [A] SIZE

//TILE

//Matches Collectiable Type
public enum TileType
{
    Obstcle,
    Normal,
	Breakable,
}

public enum MatchValue{
	MUT=0,
	STR=1,
	DEX=2,
	INT=3,
	SWORD=4,
	ARMOR=5,
	OTHER=6,	//other collectiable
}
public enum SlotType
{
	Normal,
	Obstacle,
	Breakable,
    
}

//Breakable Type
public enum BombType
{
	None,
	Column,
	Row,
	Adjacent,
	Color,
	Mixed,

}


public enum InterType
{
	Linear,
	EaseOut,
	EaseIn,
	SmoothStep,
	SmootherStep,
}


////////////////RPG Module

//Items
public enum ItemType{
	JUNK,
	WEAPON,
	ARMOR,
	SPECIALITEMS,
	
}

public enum StageType
{
	TOWN,
	DUNGEON,
	OUTDOOR,
}

public enum ItemRatity
{
	JUNK,
	NORMAL,
	RARE,
	EPIC,
}
//Enemy
public enum NpcType{
	NORMAL,
	ENEMY,	//InBattle(match3),or kill use by tools()
	QUEST,
	MERCHANT,
	BLACKSMITH,
	OTHER,
}

//FSM
public enum EntityAnimState{
	IDLE,
	MOVING,
	BATTLE,
	CASTING,
	DEAD,
	ATTACK,	//In AI plugins


}

//Classes (For Player Classes to classIndex)
public enum Classes{
	Normal,
	Warrior,
	Hunter,
}


//FSM(COMBAT UND COMBO effect)
public enum DamageType {
	Normal=0,
	Block=1,
	Crit=2,
}

///////////////////COMMON
//Common Enum
public enum SceneType{
	MainMenu,
	MainGame,
	LoadingPro,

}



////////////////////MIRROR::SERVER&CLIENT
//NETWORKSTATE
public enum NetworkState{
	None,
	Lobby,
	Online,
	World,
	HandShake,
    Offline,
}


////////AI State
public enum AIStateType{
	None,
	Idle,
	Walking,
	Deading,
	Feeding,
	Running,
	Atk,
	Dead,
}

//AI Type  For THDN
//1.target can animaion behaviour by motion or object
public enum AITargetType{
	None,
	VisualPlayer,
	VisualObject,
	VisualFood,
	VisualLight,
	VisualNoise,	//Noise from interactive object occur
	VisualTrap,

	
}


//trigger by enemy state
public enum AITriggerType{Enter,Stay,Exit
}

//
public enum AIBoneAlignmentType{
	XAxis,
	YAxis,
	ZAxis,
	XAxInverted,
	YAxInverted,
	ZAxInverted

}