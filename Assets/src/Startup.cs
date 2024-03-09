using UnityEngine;
using Leopotam.EcsLite;
using static Globals;
using static Ships;
using static Sync;
using static GameInput;
using static Players;
using static Entities;
using static Projectiles;
using static Ai;

#if UNITY_EDITOR
using Leopotam.EcsLite.UnityEditor;
#endif

public class Startup : MonoBehaviour
{
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
        
        CreatePlayer(Vector3.zero, 0);
        CreateMultipleShipsRandomly(ShipsCount, 30f);
        
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
        
        UpdateInput();

        UpdateAi();
        
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
