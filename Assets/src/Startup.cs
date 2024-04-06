using UnityEngine;
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
    public UnitedProjectile[] ProjectilesTable;
    public Entity[]           PrefabsTable;
    public ShipConfig[]       ShipsAssetsTable;
    public Sprite[]           Sprites;
    public Material[]         Materials;
    public Texture            InstancedBulletTexture;
    public int                ShipsCount = 100;
    
    #if UNITY_EDITOR
    private IEcsSystems _debugSystems;
    #endif
    
    private void Start()
    {
        MainWorld = new EcsWorld();
        MainCamera = Camera;
        Pools.InitPools();
        Queries.InitQueries();
        Assets.PrefabTable     = PrefabsTable;
        Assets.ShipAssetTable  = ShipsAssetsTable;
        Assets.ProjectileTable = ProjectilesTable;
        Assets.MaterialTable   = Materials;
        World.Size             = WorldSize;
        
        
        Assets.MeshTable = new Mesh[Sprites.Length];
        
        Assets.MaterialTable[0].SetTexture("_MainTex", InstancedBulletTexture);
        
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
        
        CreatePlayer(Vector3.zero, 0f, Vector3.one, 0);
        CreateMultipleShipsRandomly(ShipsCount);
        
        Vars.ParseVars(VarsAsset);
        
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
        
        DestroyQueuedEntities();

        
        UpdateTemp(dt);
        MoveEntities(dt);
        UpdateShips(dt);
        UpdateBullets();
        UpdateHealth();
        
        SyncReferences();
        CreateObjectToWorld();
        DrawBullets();
        
           
        #if UNITY_EDITOR
        _debugSystems.Run();
        #endif
    }
}
