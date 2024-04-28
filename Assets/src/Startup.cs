using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;
using Leopotam.EcsLite;
using static Globals;
using static Sync;
using static GameInput;
using static Entities;
using static Ai;
using static Rendering;


#if UNITY_EDITOR
using Leopotam.EcsLite.UnityEditor;
#endif

public class Startup : MonoBehaviour
{
    public TextAsset          VarsAsset;
    public Vector3            WorldSize = new Vector3(100, 100, 0);
    public Camera             Camera;
    public ShipConfig[]       ShipsAssetsTable;
    public Sprite[]           Sprites;
    public Material[]         Materials;
    public int                ShipsCount = 100;
    
    #if UNITY_EDITOR
    private IEcsSystems _debugSystems;
    #endif
    
    private void Start()
    {
        Vars.ParseVars(VarsAsset);
        MainWorld = new EcsWorld();
        MainCamera = Camera;
        Pools.InitPools();
        Queries.InitQueries();
        Assets.ShipAssetTable  = ShipsAssetsTable;
        Assets.MaterialTable   = Materials;
        World.Size             = WorldSize;
        
        Assets.MeshTable = new Mesh[Sprites.Length];
        
        //convert sprites to meshes
        for(var i = 0; i < Sprites.Length; ++i)
        {
            var mesh   = new Mesh();
            var sprite = Sprites[i];
            
            mesh.SetVertices(Array.ConvertAll(sprite.vertices, i => (Vector3)i).ToList());
            mesh.SetUVs(0, sprite.uv.ToList());
            mesh.SetTriangles(Array.ConvertAll(sprite.triangles, i => (int)i), 0);
            
            Assets.MeshTable[i] = mesh;
        }
        
        CreatePlayer(Vector3.zero, 0f, Vector3.one, Vars.PlayerShip);
        CreateMultipleShipsRandomly(ShipsCount);
        
        
        #if UNITY_EDITOR
        _debugSystems = new EcsSystems(MainWorld);
        _debugSystems
            .Add(new EcsWorldDebugSystem())
            .Init();
        #endif
    }

    private void Update()
    {
        var dt = Time.deltaTime;
        
        UpdateInput(dt);

        UpdateAi(dt);
        UpdatePlayer(dt);
        UpdateTemp(dt);
        UpdateShips(dt);
        UpdateWeapons(dt);
        UpdateBullets();
        UpdateMissiles(dt);
        MoveEntities(dt);
        UpdateHealth();
        
        DestroyQueuedEntities();
        
        SyncReferences();
        CreateObjectToWorld();
        DrawBullets();
        DrawLasers();
        FlushLasers();
        
           
        #if UNITY_EDITOR
        _debugSystems.Run();
        #endif
    }
}
