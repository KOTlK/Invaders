using Leopotam.EcsLite;
using static Globals;

public static class Pools
{
    public static EcsPool<Transform>           TransformPool;
    public static EcsPool<GameObjectReference> GoReferencePool;
    public static EcsPool<Ship>                ShipPool;
    public static EcsPool<Player>              PlayerPool;
    public static EcsPool<Destroy>             DestroyPool;
    public static EcsPool<Bullet>              BulletPool;
    public static EcsPool<Damage>              DamagePool;
    public static EcsPool<Health>              HealthPool;
    public static EcsPool<Temporary>           TempPool;
    public static EcsPool<FollowTarget>        FollowPool;
    public static EcsPool<Movable>             MovablePool;
    
    public static void InitPools()
    {
        TransformPool   = MainWorld.GetPool<Transform>();
        GoReferencePool = MainWorld.GetPool<GameObjectReference>();
        ShipPool        = MainWorld.GetPool<Ship>();
        PlayerPool      = MainWorld.GetPool<Player>();
        DestroyPool     = MainWorld.GetPool<Destroy>();
        BulletPool      = MainWorld.GetPool<Bullet>();
        DamagePool      = MainWorld.GetPool<Damage>();
        HealthPool      = MainWorld.GetPool<Health>();
        TempPool        = MainWorld.GetPool<Temporary>();
        FollowPool      = MainWorld.GetPool<FollowTarget>();
        MovablePool     = MainWorld.GetPool<Movable>();
    }
}
