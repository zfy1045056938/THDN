using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using Mirror;

/// <summary>
/// CORE MODULE
/// Include these module
///  Match Module
///  
/// 
/// </summary>
[RequireComponent((typeof(BoardDeadLock)))]
[RequireComponent((typeof(BoardShuffle)))]
public class Board : NetworkBehaviour
{
    public static Board instance;

    public int width;
    public int height;

    public int borderSize;

    [Header("Prefab")] public GameObject tileNormalPrefab;

    public GameObject tileObstaclePrefab;
    public GameObject[] gamePiecePrefab;
    public GameObject[] adjacentBombPrefab;
    public GameObject[] columnPrefabs;
    public GameObject[] rowPrefabs;
    public GameObject colorBombprefabs;
    
    //
    public GameObject m_clickedBomb;
    public GameObject m_targetBomb;

    public float swapTime = 0.4f;
    
    //
    public int maxCollectiable = 3;    //Can Match

    public int collectiableCount = 0;

    [Range(0, 1)] public float chanceForCollectiable = 0.1f;

    public GameObject[] collectiablePrefabs;
    
    //
    private Tile[,] m_allTiles;
    private GamePieces[,] m_allGamePieces;

    private Tile m_clickedTiles;
    private Tile m_targetTiles;

    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;
    
    //
    private ParticleManager particleManager;

    public int fillYoffset = 10;

    public float fillMovetime = 0.4f;

    private int scoreMutiplier = 0;

    public bool isRefilling = false;

    private BoardDeadLock m_boardDeadLock;
    private BoardShuffle m_boaardShuffle;
    
    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;
        public int x;
        public int y;
        public int z;
    }

    [Header("RPG")] public Players player;
    public Enemy enemy;


    void Start()
    {
        if (instance = null) instance = this;
        //
        m_allTiles = new Tile[width,height];
        //
        m_allGamePieces = new GamePieces[width,height];
        //
        particleManager = GetComponent<ParticleManager>();
        //
        player = FindObjectOfType<Players>();
        //
        enemy = FindObjectOfType<Enemy>();
        //
        m_boardDeadLock = GetComponent<BoardDeadLock>();
        //
        m_boaardShuffle = GetComponent<BoardShuffle>();
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetupBoard()
    {
        SetupTile();

        SetGamePieces();

        List<GamePieces> startCollectiables = FindCollectiables();
        collectiableCount = startCollectiables.Count;

        SetCamera();
        
        //
        FillBoard(fillYoffset, fillMovetime);
        
        //Config GameData(p&E) TODO
        SetPlayerAndEnemey(player,enemy);
    }

    private void SetPlayerAndEnemey(Players players, Enemy enemy1)
    {
        throw new System.NotImplementedException();
    }

  

    private void SetCamera()
    {
        throw new System.NotImplementedException();
    }

    #region Collectiable


    //Find all collectiable in the board
    private List<GamePieces> FindCollectiables()
    {
       List<GamePieces> canCollectiable = new List<GamePieces>();
       //found collectiable at column
       for(int i=0;i<height;i++){
       		List<GamePieces> rowCollectiables = FindCollectiableAt(i);
       		canCollectiable  = canCollectiable.Union(rowCollectiables).ToList();

       }
       return canCollectiable;
    }

    //
    private List<GamePieces> FindCollectiableAt(int i,bool clearAtBottom=false){
    		List<GamePieces> foundCollectiables = new List<GamePieces>();


    		//found collectiable at row
    		for(int i=0;i<width;i++){
    			if(m_allGamePieces[i,row]!=null){
    				Collectiable collectiableComponent = m_allGamePieces[i,row].GetComponent<Collectiable>();
    				//
    				if(collectiableComponent!=null){
    					if(!clearAtBottom ){
    						//add collectiable 
    						foundCollectiables.Add(m_allGamePieces[i,row]);
    					}
    				}
    			}
    		}
    		reurn foundCollectiables;
    }

    //Can add collectiable
    private bool CanAddCollectiable(){
    	return (Random.Range(0f,1f)<=chanceForCollectiable && collectiableCount>=0);
    }

    private GamePieces FillRandomCollectiablesAt(int x,int y ,float falseYOffset =0f, float moveTime=1.0f){
        if(IsInBound(x,y)){
            GameObject rndPieces= Instantiate(GetRandomCollectiable(),new Vector3(x,y),Quaternion.identity)as GameObject;
            rndPieces.transform.parent=transform;
            MakeGamePieces(rndPieces,x,y,falseYOffset,moveTime);
            return rndPieces.GetComponent<GamePieces>();
        }
        return null;
    }

    public GameObject GetRandomCollectiable(){
        return GetRandomObject(collectiablePrefabs);
    }
    #endregion 
    #region GamePieces

    

   
    private void SetGamePieces()
    {
        foreach (StartingObject gp in startingGamePieces)
        {
            if (gp!=null)
            {
                GameObject gps = Instantiate(gp.prefab,new Vector3(gp.x,gp.y,0),quaternion.identity) as GameObject;
                MakeGamePieces(gps, gp.x, gp.y, fillYoffset, fillMovetime);
            }
        }
    }

    private void MakeGamePieces(GameObject gps, int gpX, int gpY, int faseYOffset, float moveTime=0.1f)
    {
        if (gps != null && IsInBound(gpX, gpY))
        {
            gps.GetComponent<GamePieces>();
            PlaceGamePieces(gps.GetComponent<GamePieces>(), gpX, gpY);
            //
            if (faseYOffset !=0)
            {
                gps.transform.position = new Vector3(gpX,gpY+fillYoffset,0);
                gps.GetComponent<GamePieces>().Move(gpX, gpY, moveTime);
            }
            //
            gps.transform.parent = transform;
        }
    }

    private void PlaceGamePieces(GamePieces gamePiece, int gpX, int gpY)
    {
        if (gamePiece==null)
        {
            return;
        }
        //
        gamePiece.transform.position = new Vector3(gpX,gpY,0);
        gamePiece.transform.rotation = quaternion.identity;
        
        //
        if (IsInBound(gpX,gpY))
        {
            m_allGamePieces[gpX, gpY] = gamePiece;
        }
        //
        gamePiece.SetCoord(gpX, gpY);

    }

    private GamePieces FillRandomGamePieceAt(int x,int y, float falseYOffset=0, float moveTime=1.0f){
    	if(IsInBound(x,y)){
            GameObject rndgp =Instantiate(GetRandomGamePieces(),new Vector3(x,y),Quaternion.identity)as GameObject;
            rndgp.transform.parent=transform;
            MakeGamePieces(rndgp,x,y,falseYOffset,moveTime);
            return rndgp.GetComponent<GamePieces>();
        }
        return null;
    }


    //rnd gamepiece when refill board
    private GameObject GetRandomGamePieces(){
        return GetRandomObject(gamePiecePrefab);
    }

    #endregion

    #region Tile

    

 
    private void SetupTile()
    {
        foreach (StartingObject sobj in startingTiles)
        {
            if (sobj!=null)
            {
                MakeTile(sobj.prefab,sobj.x, sobj.y, sobj.z);
            }
        }
        //
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                MakeTile(tileNormalPrefab, i, j);
            }
        }
    }

    private void MakeTile(GameObject prefab, int sx, int sy, int sz = 0)
    {
        if (prefab!=null && IsInBound(sx,sy))
        {
            GameObject obj = Instantiate(prefab,new Vector3(sx,sx,sz),quaternion.identity)as GameObject;
            obj.name = "Tile+(" + sx + "," + sx + ")";
            m_allTiles[sx, sy] = obj.GetComponent<Tile>();
            obj.transform.parent = transform;
            m_allTiles[sx, sy].Init(sx, sy, this);
        }
    }

    private bool IsInBound(int w, int h)
    {
       return w>width && h>height;
    }

    #endregion

    #region FillBoard
    //Fill with the board .
  private void FillBoard(int falseYOffset=0, float moveTime=0.1f)
    {
    	int maxIterations = 100;
    	int iterations = 0;

    	//Get Board
    	for(int i=0;i<width;i++){
    		for(int j =0;j<height;j++){
    			if(m_allGamePieces[i,j] ==null && m_allTiles[i,j].tileType !=Tiletype.Obstacle){
    				//Can add collectiable
    				if(j==height-1 && CanAddCollectiable()){
    					FillRandomCollectiablesAt(i,j,falseYOffset,moveTime);
    					collectiableCount++;
    				}else{
    					FillRandomGamePieceAt(i,j,falseYOffset,moveTime);
    					iterations=0;
    					//IF Match then refill
    					while(HasMatchOnFill(i,j)){
    						ClearPiecesAt(i,j);
    						FillRandomGamePieceAt(i,j,falseYOffset,moveTime);
    					   
                           //
                            iterations++;

                            //
                            if(iterations>=maxIterations){
                                break;
                            }
                        }
    				}
    			}
    		}
    	}
      
    }


    #endregion


    #region	CalTotalRoundEnd
    //RPG MODULE Load Player und enemy from server
    #endregion

    #region ClearModule
    //das module include all clear by tile und check can refill



    #endregion


    #region Common  
    //das module ist suit for check board bonud und can refill the routline
    bool HasMatchOnFill(int i,int j,int minLen=3){
        //when collectiable(3) then cause match effect und active refillroutline
        List<GamePieces> leftMatches = FindMatches(i,j,new Vector3(0,-1),minLen);
        List<GamePieces> downwardMatches=FindMatches(i,j,new Vector3(-1,0),minLen);
        //
        if(leftMatches==null){
            leftMatches =new List<GamePieces>();
        }

        if(downwardMatches==null){
            downwardMatches = new List<GamePieces>();


        }
        //
        return ((leftMatches.Count>0) || (downwardMatches.Count>0);

    }

    bool IsNextTo(Tile start ,Tile end){

    }

    //return array of rnd gameobject
    public GameObject GetRandomObject(GameObject[] prefabarray){
        int rndIndex= Random.Range(0,prefabarray.Length);
        //
        return prefabarray[rndIndex];
    }


    #endregion

    #region TileInteactive
    public void DragTile(){

    }
    public void RelaseTile(){

    }


    #endregion


    #region Matches Module
    List<GamePieces> FindMatches(int x,int y,int z=0,float falseYOffset=0,float moveTime=1.0f){
        return null;
    }
    List<GamePieces>FindHorizontial(int x,int y,int z=0,Vector3 pos,float falseYOffset =0,flost moveTime=1.0f){
        return null;
    }

    List<GamePieces> FindVertialMatches(int x,int y ,int z=0,Vector3 pos,float falseYOffset=0,float moveTime=1.0f){
        return null;
    }

    #endregion

}

