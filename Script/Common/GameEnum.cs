
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

}

//Enemy
public enum NpcType{

}

//FSM
public enum Motion{

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