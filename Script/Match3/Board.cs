using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using Mirror;

using Random = Unity.Mathematics.Random;


/// <summary>
/// CORE MODULE
/// Include these module
///  Match Module
///  
/// 
/// </summary>
[RequireComponent((typeof(BoardDeadLock)))]
[RequireComponent((typeof(BoardShuffle)))]
public class Board : MonoBehaviour
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
    public bool m_playerInputEnabled = false;

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
    public Monster enemy;


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
        enemy = FindObjectOfType<Monster>();
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

    private void SetPlayerAndEnemey(Players players, Monster enemy1)
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
    private List<GamePieces> FindCollectiableAt(int row,bool clearAtBottom=false){
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
    		return foundCollectiables;
    }

    //Can add collectiable
    private bool CanAddCollectiable(){
    	return (UnityEngine.Random.Range(0f,1f)<=chanceForCollectiable && collectiableCount>=0);
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

    private void MakeGamePieces(GameObject gps, int gpX, int gpY, float faseYOffset, float moveTime=0.1f)
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

    public void PlaceGamePieces(GamePieces gamePiece, int gpX, int gpY)
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

    public void SwitchTiles(Tile click, Tile target)
    {
        StartCoroutine(SwitchTileRoutine(click, target));
    }

    public IEnumerator SwitchTileRoutine(Tile clickTile, Tile targetTile)
    {
        if (m_playerInputEnabled && !GameManager.Instance.IsGameover)
        {
            GamePieces clickPiece = m_allGamePieces[clickTile.xIndex, clickTile.yIndex];
            GamePieces targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];
            
            //
            if (clickPiece !=null && targetPiece !=null)
            {
                clickPiece.Move(targetTile.xIndex,targetTile.yIndex,fillMovetime);
                targetPiece.Move(clickTile.xIndex,clickTile.yIndex,fillMovetime);
                
                //
                yield return new WaitForSeconds(swapTime);
                
                //FindMatch
                List<GamePieces> clickPiecesMatches = FindMatchesAt(clickTile.xIndex, clickTile.yIndex);
                List<GamePieces> targetPiecesMatches =FindMatchesAt(targetTile.xIndex,targetTile.yIndex);
                
                #region ColorBomb
                List<GamePieces> colorMatches = new List<GamePieces>();
                //
                if (IsColorBomb(clickPiece) && IsColorBomb(targetPiece))
                {
                    targetPiece.matchValue = clickPiece.matchValue;
                    colorMatches = FindAllMatchValue(clickPiece.matchValue);
                }else if(!IsColorBomb(clickPiece) && IsColorBomb(targetPiece))
                {
                    foreach (GamePieces piece in m_allGamePieces)
                    {
                        if (!colorMatches.Contains(piece))
                        {
                            colorMatches.Add(piece);
                        }
                    }
                }
                #endregion
                
                //no match return 
                //cost if has point
                if (targetPiecesMatches.Count==0 && clickPiecesMatches.Count==0 && colorMatches.Count==0)
                {
                    clickPiece.Move(clickTile.xIndex,clickTile.yIndex,fillMovetime);
                    targetPiece.Move(targetTile.xIndex,targetTile.yIndex,fillMovetime);
                    
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);
                    
                    //

                    #region Drop Bomb
                    Vector2 swapDirection=new Vector2(targetTile.xIndex - clickTile.yIndex , targetTile.xIndex-clickTile.yIndex);
                    
                    //
                    m_clickedBomb = DropBomb(clickTile.xIndex, clickTile.yIndex, swapDirection, clickPiecesMatches);
                    m_targetBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swapDirection, targetPiecesMatches);
                    
                    //
                    if (m_clickedBomb != null && targetPiece != null)
                    {
                        GamePieces clickBombPieces = m_clickedBomb.GetComponent<GamePieces>();
                        if (!IsColorBomb(clickBombPieces))
                        {
                            clickBombPieces.ChangeColor(targetPiece);
                        }
                    }
                    
                    //
                    if (m_targetBomb!=null && clickPiece!=null)
                    {
                        GamePieces tbp = m_targetBomb.GetComponent<GamePieces>();
                        //
                        if (!IsColorBomb(tbp))
                        {
                            tbp.ChangeColor(clickPiece);
                        }
                    }

                    #endregion
                    
                    //
                    List<GamePieces> pieceToClear = clickPiecesMatches.Union(targetPiecesMatches).ToList()
                        .Union(colorMatches).ToList();
                    //REFILL PIECES CORE
                    yield return StartCoroutine(ClearAndRefill(pieceToClear));
                    if (GameManager.Instance !=null)
                    {
                        GameManager.Instance.UpdateMoves();
                    }


                }

            }
        }
    }

    private GameObject DropBomb(int clickTileXIndex, int clickTileYIndex, Vector2 swapDirection, List<GamePieces> clickPiecesMatches)
    {
        throw new NotImplementedException();
    }

  


    private bool IsColorBomb(GamePieces clickPiece)
    {
        throw new NotImplementedException();
    }


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
  private void FillBoard(float falseYOffset=0, float moveTime=0.1f)
    {
    	int maxIterations = 100;
    	int iterations = 0;

    	//Get Board
    	for(int i=0;i<width;i++){
    		for(int j =0;j<height;j++){
    			if(m_allGamePieces[i,j] ==null && m_allTiles[i,j].tileType !=TileType.Obstcle){
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

    //Refill und clear matches gps
   IEnumerator ClearAndRefillBoardRoutline(List<GamePieces> ptc)
   {
      yield return StartCoroutine(ClearAndRefill(ptc));
   }

    //When Matches the Refill according to Breaking row column(hor&ver)
   public IEnumerator ClearAndRefill(List<GamePieces> gps)
   {
    m_playerInputEnabled=false;
    isRefilling=true;

    //
    List<GamePieces> matches =gps;

    //Cal collect score
    scoreMutiplier=0;
    //REFILL MODULE
    do{
        scoreMutiplier++;
        //Collapse
        yield return StartCoroutine(RefillCollapseRoutine(matches));
        //
        yield return null;
        //
        yield return StartCoroutine(RefillRoutine());
        //
        matches =FindAllMatches();
        //
        yield return new WaitForSeconds(fillMovetime);
    }while(matches.Count!=0);

    //CHECK DEADLOCK if yes the shuffle else keep matches
    if(m_boardDeadLock.IsDeadLocked(m_allGamePieces,3)){
        yield return new WaitForSeconds(0.4f);
        //Shuffle Board
        yield return StartCoroutine(ShuffleBoard());
        //
        yield return new WaitForSeconds(0.4f);
        //
        yield return StartCoroutine(RefillRoutine());

        //
        yield return new WaitForSeconds(0.4f);
    }
    //
    m_playerInputEnabled=false;
    isRefilling=false;
    
    
       yield return null;
   }


    //Shuffle When board has deadlock 
    //For Rpg module decrease player health with a buff
   public IEnumerator ShuffleBoard(){
       List<GamePieces> allpcs = new List<GamePieces>();
       //
       foreach(GamePieces p in m_allGamePieces){
           allpcs.Add(p);
       }

       //
       while(!IsCollapsed(allpcs)){
           yield return null;
       }

       //Remove Shuffle
       List<GamePieces> normalPieces= m_boaardShuffle.RemoveNormalPieces(m_allGamePieces);


        //
        m_boaardShuffle.SuffleList(normalPieces);

        //Fill Board From LIst
        FillBoardFromList(normalPieces);

        //
        m_boaardShuffle.MovePieces(m_allGamePieces,swapTime);

        //
        List<GamePieces> matches = FindAllMatches();
        //
        StartCoroutine(ClearAndRefillBoardRoutline(matches));
       yield return null;
   }

    private void FillBoardFromList(List<GamePieces> normalPieces)
    {
        Queue<GamePieces> unsedPieces = new Queue<GamePieces>(normalPieces);

        //
        int maxIterations =100;
        int iterations =0;
        //
        for(int i=0;i<width;i++){
            for(int j=0;j<height;j++){
                if(m_allGamePieces[i,j]==null && m_allTiles[i,j].tileType!=TileType.Obstcle){
                    //
                    unsedPieces.Enqueue(m_allGamePieces[i,j]);
                    //
                    m_allGamePieces[i,j] = unsedPieces.Dequeue();
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

    public IEnumerator RefillCollapseRoutine(List<GamePieces> gps){
        List<GamePieces> mgps = new List<GamePieces>();
        //
        List<GamePieces> matches = new List<GamePieces>();
        
        yield return null;

        //
        bool isFinished=false;
        while(!isFinished){
            //Check Refill
            List<GamePieces> bps = GetBombPieces(gps);
            //
            gps = gps.Union(bps).ToList();
            //
            bps = GetBombPieces(gps);
            //collet
            List<GamePieces> collectedPieces = FindCollectiableAt(0,true);
            
            //Collectiable
            List<GamePieces> allCollectiables= FindCollectiables();
            //
            List<GamePieces> blockers= gps.Intersect(allCollectiables).ToList();
            //union blockers to collectiable pieces
            collectedPieces = collectedPieces.Union(blockers).ToList();
            //
            collectiableCount -= collectedPieces.Count;
            //Union collect pieces
            gps = gps.Union(collectedPieces).ToList();

            //
            List<int>columnToCollapse = GetColumn(gps);

            //Clear Pieces At
            ClearPiecesAt(gps,bps);
            //
            BreakTileAt(gps);
            //Check Bomb Effect active (click)
            if(m_clickedBomb !=null){
                //Activity Effect
                ActivateBomb(m_clickedBomb);
                m_clickedBomb=null;
            }

             if(m_clickedBomb !=null){
                //Activity Effect
                ActivateBomb(m_targetBomb);
                m_targetBomb=null;
            }

            //Refill Collapse Routine 
            yield  return new WaitForSeconds(0.4f);
            //Collapse for column matches pieces
            mgps =   CollapseColumn(columnToCollapse);
            //
            while(!IsCollapsed(mgps)){
                yield return null;
            }

            yield return new WaitForSeconds(0.4f);

            //find any mathes that can collapse
            matches = FindMatches(mgps);
            //
            collectedPieces = FindCollectiables(0,true);
            //
            matches = matches.Union(collectedPieces).ToList();

            //End Matches activate RefillRoutine
            if(matches.Count == 0){
                isFinished=true;

                yield return StartCoroutine(RefillRoutine());
                //
                yield return new WaitForSeconds(0.4f);

            }else{
                scoreMutiplier++;
                //TODO SOUND PLAY SOUND
                //
                yield return StartCoroutine(ClearAndRefillBoardRoutline(matches));
                
            }
            //

            
        }
        yield return null;
    }

    private List<int> GetColumn(List<GamePieces> gps)
    {
       List<int>columns=new List<int>();

       //
       foreach(GamePieces p in gps){
           if(p!=null){
               if(!columns.Contains(p.xIndex)){
                   columns.Add(p.xIndex);
               }
           }
       }
       return columns;
    }

    private void ActivateBomb(GameObject m_clickedBomb)
    {
        throw new NotImplementedException();
    }

    private void BreakTileAt(List<GamePieces> gps)
    {
        throw new NotImplementedException();
    }

    private List<GamePieces> GetBombPieces(List<GamePieces> gps)
    {
        throw new NotImplementedException();
    }

    private List<GamePieces> FindCollectiables(int v1, bool v2)
    {
        throw new NotImplementedException();
    }

    private List<GamePieces> FindMatches(List<GamePieces> mgps)
    {
        throw new NotImplementedException();
    }

    private bool IsCollapsed(List<GamePieces> mgps)
    {
        throw new NotImplementedException();
    }

    private List<GamePieces> CollapseColumn(List<int> columnToCollapse)
    {
        throw new NotImplementedException();
    }

    public IEnumerator RefillRoutine(){

       FillBoard(fillYoffset,fillMovetime);
       yield return null;
    }

  #endregion


    #region	CalTotalRoundEnd
    //RPG MODULE Load Player und enemy from server
    #endregion

    #region ClearModule
    //das module include all clear by tile und check can refill

  private void ClearPiecesAt(List<GamePieces> gps, List<GamePieces> bps)
  {
      throw new NotImplementedException();
  }

  private void ClearPiecesAt(int x, int y)
  {
      throw new NotImplementedException();
  }


    #endregion


    #region Common  
    //das module ist suit for check board bonud und can refill the routline
    bool HasMatchOnFill(int i,int j,int minLen=3){
        //when collectiable(3) then cause match effect und active refillroutline
        List<GamePieces> leftMatches = FindMatches(i,j,0,new Vector3(0,-1),minLen);
        List<GamePieces> downwardMatches=FindMatches(i,j,0,new Vector3(-1,0),minLen);
        //
        if(leftMatches==null){
            leftMatches =new List<GamePieces>();
        }

        if(downwardMatches==null){
            downwardMatches = new List<GamePieces>();


        }
        //
        return ((leftMatches.Count>0) || (downwardMatches.Count>0));

    }



    bool IsNextTo(Tile start ,Tile end){
        return true;
    }

    //return array of rnd gameobject
    public GameObject GetRandomObject(GameObject[] prefabarray){
        int rndIndex=UnityEngine.Random.Range(0,prefabarray.Length);
        //
        return prefabarray[rndIndex];
    }


    #endregion

    #region TileInteactive
    public void DragTile(Tile tile){
        if (m_clickedTiles!=null && IsNextTo(tile,m_clickedTiles))
        {
            m_targetTiles = tile;
        }
    }
    public void RelaseTile(){
        if (m_clickedTiles != null && m_targetTiles!=null)
        {
            SwitchTiles(m_clickedTiles, m_targetTiles);
            
        }

        m_clickedTiles = null;
        m_targetTiles = null;
    }

public void ClickTile(Tile tile)
    {
        if (m_clickedTiles==null)
        {
            m_clickedTiles = tile;
        }
    }
    #endregion

    #region Matches

    private List<GamePieces> FindAllMatchValue(object matchValue)
    {
        throw new NotImplementedException();
    }

        #endregion

    #region Matches Module
    List<GamePieces> FindMatches(int x,int y,int z,Vector3 pos,float falseYOffset=0,float moveTime=1.0f,int minLength=3)
    {
        List<GamePieces> matches = new List<GamePieces>();
        
        //
        GamePieces startPices = null;
        
        //
        if (IsInBound(x,y))
        {
            startPices = m_allGamePieces[x, y];
        }
        //
        if (startPices != null)
        {
            matches.Add(startPices);
        }
        else
        {
            return null;
        }
        
        //
        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;


        for (int i = 0; i < maxValue; i++)
        {
            nextX=x + (int)math.clamp(pos.x,-1,1)*i;
            nextY = y + (int) math.clamp(pos.y, -1, 1) * i;
            
            //
            
        
        if (!IsInBound(nextX, nextY))
        {
            break;
        }
        //
        GamePieces nextPieces = m_allGamePieces[nextX, nextY];
        if (nextPieces == null)
        {
            break;
        }
        else
        {
            if (nextPieces.matchValue == startPices.matchValue && !matches.Contains(nextPieces) && nextPieces.matchValue != MatchValue.STR)
            {
                matches.Add(nextPieces);
            }
            else
            {
                break;
            }
        }
        
        
        }
        //
        if (matches.Count >= minLength)
        {
            return matches;
        }

        return null;

    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="pos"></param>
    /// <param name="falseYOffset"></param>
    /// <param name="moveTime"></param>
    /// <param name="minLength"></param>
    /// <returns></returns>
    List<GamePieces>FindHorizontial(int x,int y,int z,Vector3 pos,float falseYOffset =0,float moveTime=1.0f,int minLength=3)
    {
        List<GamePieces> rightMatches = FindMatches(x, y, 0, new Vector2(1, 0));
        List<GamePieces> leftMatches = FindMatches(x, y, 0, new Vector2(-1, 0));
        
        //
        if (rightMatches==null)
        {
            rightMatches = new List<GamePieces>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePieces>();
        }
        
        //
        var combineMatches = rightMatches.Union(leftMatches).ToList();

        return (combineMatches.Count >= minLength) ? combineMatches : null;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="pos"></param>
    /// <param name="falseYOffset"></param>
    /// <param name="moveTime"></param>
    /// <returns></returns>
    List<GamePieces> FindVertialMatches(int x,int y ,int z,Vector3 pos,float falseYOffset=0,float moveTime=1.0f,int minLenth=3)
    {
        List<GamePieces> upPieces = FindMatches(x, y, 0, new Vector2(0, 1));
        List<GamePieces> downPieces = FindMatches(x, y, 0,new Vector2(0, -1));
        
        if(upPieces==null ){upPieces=new List<GamePieces>();}

        if (downPieces == null)
        {
            downPieces=new List<GamePieces>();
        }

        if (upPieces == null)
        {
            upPieces = new List<GamePieces>();
        }

        var combinePieces = upPieces.Union(downPieces).ToList();
        return (combinePieces.Count>=minLenth)?combinePieces:null;
    }
    
    private List<GamePieces> FindMatchesAt(int x, int y,int minLength=3)
    {
        List<GamePieces> verPieces = FindVertialMatches(x, y,0,new Vector3(x,y));
        List<GamePieces> horPieces = FindHorizontial(x, y, 0,new Vector3(x,y));
        
        if(verPieces==null ){verPieces=new List<GamePieces>();}

        if (verPieces == null)
        {
            verPieces=new List<GamePieces>();
        }

        if (horPieces == null)
        {
            horPieces = new List<GamePieces>();
        }

        var combinePieces = horPieces.Union(verPieces).ToList();
        return combinePieces;
    }

    public List<GamePieces> FindAllMatches(){
        return null;
    }
    #endregion


    
}

