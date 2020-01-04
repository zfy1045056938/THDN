using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Unity.Mathematics;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = Unity.Mathematics.Random;

//房间尺寸,当两个房间连接时,过道(corner)包含补给物品
public class Room
{
    public int x;
    public int y;
    //
    public int w;
    public int h;
    //
    public Room connectTo = null;
    public int branch = 0;
    //
    public string relative_pos = "x";
    
    //
    public bool dead_end = false;
    //
    public int room_id = 0;

}


//生成位置
public class SpawnList
{
    public int x;
    public int y;
    public bool byWall;
    public string wallLoc;
    public bool inTheMid;
    public bool byCollidor;
    
    //
    public int asDoor = 0;
    public Room room = null;
    public bool spawnObject;
    
}

//内容块，生成主要以下内容
//1.商人图块
//2.对战模块
//3.事件推进模块
//4.地形材质结构
//p-p tile->door--corner--door<-tile
[System.Serializable]
public class CustomRoom
{
    public int roomid = 1;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject cornerPrefab;
    public GameObject doorPrefab;
}

//生成参数，根据配置文件生成地形大小和类型
[System.Serializable]
public class SpawnOption
{
    public int minCount;
    public int maxCount;
    public bool spawnByWall;
    public bool spawnIntheMid;
    public bool spawnRotated;
    //
    public float heightFix = 0;
    public GameObject obj;
    public int spawnRoom = 0;

}


//
public class MapTile
{
    public int type = 0;
    public int oridentation = 0;
    public Room room = null;
}


/// <summary>
/// 地形生成，根据关卡设置进行随机生成，需要配置文件设定参数(怪物参数,TileBG,任务参数)
/// 玩家初始位置根据初始房间生成
/// NPC prefab 根据地点产生，
/// </summary>
///

public class Dungeonize : NetworkManager
{
    public GameObject startPrefab;
    public GameObject endPrefab;
    
   [Header("Content Here To Config")]
    public List<SpawnList> spawnLoc= new List<SpawnList>();
    public List<CustomRoom>roomList=new List<CustomRoom>();
    public List<SpawnOption> spawnOptions=new List<SpawnOption>();
  

    [Header("Common")]
    public GameObject floorprefab;
    public GameObject doorPrefab;
    public GameObject wallPrefab;
    public GameObject cornerPrefab;

    
    public int roomMargin = 3;
    public bool is3D = false;    //default 2D for miniMap,3d in the world
    public bool generate_onLoad = true;    //runtime
    //Common rnd
    public static Random rnd= new Random();
    //Dungeo Config detail
    class Dungeon
    {
        //dungeon size
        public static int map_size;
        public static int min_map_size_x;
        public static int max_map_size_y;
        public int minSize;
        public int maxSize;
        
        //
        public static MapTile[,] MapTile;
        
        public static List<Room> rooms =new List<Room>();

        public static Room goalRoom;
        public static Room spawnRoom; //initPlayer

        public int maxminiumRoomCount;
        public int roomMargin;    //房间邻接点
        public int roomMarginTmp;
        
        //tile type for ease
        public static List<int> roomsandfloors = new List<int> {1, 3};
        public static List<int> corners = new List<int> { 4,5,6,7};
        public static List<int> walls = new List<int> {8, 9, 10, 11};
        //save Loc
        public static List<string> directionLoc = new List<string>() {"x", "-x", "y", "-y"};

        /// <summary>
        /// 生成随机地下城，根据以下原则
        /// 1.方位确认，房间相邻不超过4个(limit of cube)，
        /// 2.房间生成无重叠邻间生成过道
        /// 3.碰撞体
        /// 4.墙体生成
        /// 5。过道生成,在2个房间之间,楼梯(x,y+10,z)产生位置向y轴产生
        /// maptile:0default,1room floor,2wall,corridor floor 3,room corner 4,5,6,7
        /// </summary>
        public void Generate()
        {
            int room_count = this.maxminiumRoomCount;
            int min_size = this.minSize;
            int max_size = this.maxSize;
            //房间邻接点
            if (roomMargin < 2)
            {
                map_size = room_count * max_size * 2;
            }
            else
            {
                map_size = (room_count * (max_size + (roomMargin * 2))) + (room_count * room_count * 2);
            }
            //
            MapTile=new MapTile[map_size,map_size];
            //generate tile in world[x,y]
            for(var x=0;x<max_size;x++)
            {
                for (var y = 0; y < map_size; y++)
                {
                    MapTile=new MapTile[x,y];
                    MapTile[x, y].type = 0; //type equals 
                }
            }
            rooms = new List<Room>();
            
            //room direction
            int collision_count = 0;
            string direction = "set";
            string oldDirection = "set";
            Room lastRoom;
            
            //Set Room Direction
            for (var rc = 0; rc < room_count; rc++)
            {
                Room room=new Room();
                Random r=new Random();
                if (rooms.Count == 0)
                {  
                    //generate first room then 
                    //set first dire
                    room.x = (int) math.floor(map_size / 2);
                    room.y = (int) math.floor(map_size / 2);
                    //Rnd size for [w,h]
                    room.w = r.NextInt(min_size, max_size);
                    if (room.w % 2 == 0) room.w += 1;
                    room.h = r.NextInt(min_size, max_size);
                    if (room.h % 2 == 0) room.h += 1;
                    room.branch = 0;
                    lastRoom = room;
                }
                else
                {
                    //
                    int branch = 0;
                    if (collision_count == 0)
                    {
                        branch = r.NextInt(5, 20);

                    }

                    room.branch = branch;

                    //get all room in list
                    lastRoom = rooms[rooms.Count - 1];

                    int lri = 1;
                    if (lastRoom.dead_end)
                    {
                        lastRoom = rooms[rooms.Count - lri++];
                    }

                    #region ROOMDIRECTION

                    //find new dire set then add
                    if (direction == "set")
                    {
                        //rnd directions
                        string newRndDirection = directionLoc[r.NextInt(0, directionLoc.Count)];
                        direction = newRndDirection;
                        while (direction == oldDirection)
                        {
                            newRndDirection = directionLoc[r.NextInt(0, directionLoc.Count)];
                            direction = newRndDirection;
                        }
                    }

                    //tmp add margin(tmp ist all margin)
                    this.roomMarginTmp = r.NextInt(0, this.roomMargin - 1);
                    //according to x,y,-x,-y to set directions
                    if (direction == "y")
                    {
                        room.x = lastRoom.x + lastRoom.w + r.NextInt(3, 5) + this.roomMarginTmp;
                        room.y = lastRoom.y;
                    }
                    else if (direction == "-y")
                    {
                        room.x = lastRoom.x - lastRoom.w + r.NextInt(3, 5) - this.roomMarginTmp;
                        room.y = lastRoom.y;
                    }
                    else if (direction == "x")
                    {
                        room.y = lastRoom.y + lastRoom.h + r.NextInt(3, 5) + this.roomMarginTmp;
                        room.x = lastRoom.x;
                    }
                    else if (direction == "-x")
                    {
                        room.y = lastRoom.y - lastRoom.h + r.NextInt(3, 5) + this.roomMarginTmp;
                        room.x = lastRoom.x;
                    }

                    //direction by tile[w,h]
                    room.w = r.NextInt(min_size, max_size);
                    if (room.w % 2 == 0) room.w += 1;
                    room.h = r.NextInt(min_size, max_size);
                    if (room.h % 2 == 0) room.h += 1;
                    room.connectTo = lastRoom;

                    //check corridor
                    bool isCorridor = this.DoesCorride(room, 0);
                    if (isCorridor)
                    {
                        rc--;
                        collision_count += 1;
                        if (collision_count > 3)
                        {
                            lastRoom.branch = 1;
                            lastRoom.dead_end = true;
                            collision_count = 0;
                        }
                        else
                        {
                            oldDirection = direction;
                            direction = "set";
                        }
                    }
                    else
                    {
                        room.room_id = rc;
                        rooms.Add(room);
                        oldDirection = direction;
                        direction = "set";
                    }
                }

                #endregion

                    #region MakingRoom

                    for (int k = 0; k < rooms.Count; k++)
                    {
                        Room mr = rooms[k];
                        for (int m=room.x; m < room.x + room.w; m++)
                        {
                            for (int n = 0; n < room.y + room.h; n++)
                            {
                                MapTile[m, n].type = 1;
                                MapTile[m, n].room = room;
                            }
                        }
                    }
                    
                    //Collidor corridor between a und b
                    // A----->corner<-----B
                    for (int crc = 0; crc < rooms.Count; crc++)
                    {
                        //
                        Room rA = rooms[crc];
                        Room rB = rooms[crc].connectTo;
                        
                        //
                        if (rB != null)
                        {
                            var pa = new Room();
                            var pb = new Room();
                            pa.x = rA.x + (int) math.floor(rA.w / 2);
                            pb.x = rB.x + (int) math.floor(rB.w / 2);
                            //
                            pa.y = rA.y + (int) math.floor(rA.h / 2);
                            pb.y = rB.y + (int) math.floor(rB.h / 2);
                            
                            //
                            if (math.abs(pa.x - pb.x) > math.abs(pa.y - pb.y))
                            {
                                //y axis
                                if (rA.h > rB.h)
                                {
                                    pa.y = pb.y;
                                }
                                else
                                {
                                    pb.y = pa.y;
                                }
                            }
                            else
                            {
                                //
                                if (rA.w < rB.w)
                                {
                                    pa.x = pb.x;
                                }
                                else
                                {
                                    pb.x = pa.x;
                                }
                                
                            }
                            
                            //碰撞体是否相等
                            while ((pb.x != pa.x) || (pb.y != pa.y))
                            {
                                if (pb.x != pa.x)
                                {
                                    if (pb.x > pa.x)
                                    {
                                        pb.x--;
                                    }
                                    else
                                    {
                                        pb.x++;
                                    }
                                }
                                else if(pb.y!= pa.y)
                                {
                                    if (pb.y > pa.y)
                                    {
                                        if (pb.x > pa.x)
                                        {
                                            pb.y--;
                                        }
                                        else
                                        {
                                            pb.y++;
                                        }
                                    }
                                    
                                    //
                                    if (MapTile[pb.x, pb.y].room == null)
                                    {
                                        MapTile[pb.x, pb.y].type = 3;
                                    }
                                    
                                }
                            }

                        }
                        

                    }
                    
                    //Push tile to bottom of edge from mid
                    //x crops
                    int row = 1;
                    int min_crop_x = map_size;
                    for (int mx = 1; mx < map_size - 1; mx++)
                    {
                        bool x_empty = true;
                        for (int my = 1; my < map_size - 1; my++)
                        {
                            if (MapTile[mx, my].type != 0)
                            {
                                x_empty = false;
                                if (mx < min_crop_x)
                                {
                                    min_crop_x = mx;
                                }

                                break;
                            }
                        }
                        //
                        if (!x_empty)
                        {
                            for (int mys = 0; mys < map_size - 1; mys++)
                            {
                                //Generate by maptile[x,nys]
                                MapTile[row, mys] = MapTile[mx, mys];
                                MapTile[mx,mys]=new MapTile();
                            }
                            //
                            row += 1;
                        }
                    }
                    
                    //y crops y轴
                     row = 1;
                    int min_crop_y = map_size;
                    for (int y = 0; y < map_size - 1; y++)
                    {
                        bool y_empty = false;
                        for (int mxs=0;mxs <map_size-1;mxs++)
                        {
                            if (MapTile[mxs, y].type != 0)
                            {
                                y_empty = false;
                                if (y < min_crop_y)
                                {
                                    min_crop_y = y;
                                }

                                break;
                            }    
                        }
                        //generate new y tile to bottom of edges
                        if (!y_empty)    
                        {
                            for (int xs = 0; xs < map_size - 1; xs++)
                            {
                                MapTile[xs, row] = MapTile[xs, y];
                                MapTile[xs,row]=new MapTile();
                            }

                            row += 1;

                        }
                    }
                    //
                    foreach (Room rs in rooms)
                    {
                        rs.x -= min_crop_x;
                        rs.y -= min_crop_y;
                    }
                    
                    //testing ms
                    int final_map_size_x = 0;
                    int final_map_size_y = 0;
                    for (int y = 0; y < map_size - 1; y++)
                    {
                        for (int x = 0; x < map_size - 1; x++)
                        {
                            if (MapTile[x, y].type != 0)
                            {
                                final_map_size_y += 1;
                                break;
                            }
                        }
                    }
                    //
                    for (int x = 0; x < map_size - 1; x++)
                    {
                        for (int y = 0; y < map_size - 1; y++)
                        {
                            if (MapTile[x, y].type != 0)
                            {
                                
                            }
                        }
                    }

                    #endregion


                    #region Wall



                    #endregion

                    #region Corridor



                    #endregion


            }

        }

        public bool DoesCorride(Room room, int count)
        {
            return true;
        }
    }
    

    void Start()
    {
        
        //
        Util.InvokeMany(typeof(Dungeonize),this,"OnStart_");
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Util.InvokeMany(typeof(Dungeonize),this,"OnStartClient_");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        Util.InvokeMany(typeof(Dungeonize),this,"OnStartServer_");
    }


    void Update()
    {
        
    }
    
}
