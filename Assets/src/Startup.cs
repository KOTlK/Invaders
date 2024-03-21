using UnityEngine;
using Leopotam.EcsLite;
using static Globals;
using static Sync;
using static GameInput;
using static Entities;
using static Ai;


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
        Assets.PrefabTable = PrefabsTable;
        Assets.ShipAssetTable = ShipsAssetsTable;
        Assets.ProjectileTable = ProjectilesTable;
        World.Size = WorldSize;
        
        CreatePlayer(Vector3.zero, 0);
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
        
           
        #if UNITY_EDITOR
        _debugSystems.Run();
        #endif
    }
}
