using Leopotam.EcsLite;
using System.Runtime.CompilerServices;
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
    public static EcsPool<Movement>            MovementPool;
    public static EcsPool<Patrol>              PatrolPool;
    public static EcsPool<HasTarget>           TargetPool;
    public static EcsPool<AiShip>              AiPool;
    public static EcsPool<HoldDistance>        HoldDistancePool;
    public static EcsPool<Engage>              EngagePool;
    public static EcsPool<Instanced>           InstancedPool;
    public static EcsPool<EnemyStateMachine>   StateMachinePool;
    public static EcsPool<Missile>             MissilePool;
    public static EcsPool<Weapon>              WeaponPool;
    public static EcsPool<Effect>              EffectPool;
    public static EcsPool<DroppedEffect>       DroppedEffectPool;
    
        
    public static void InitPools()
    {
        TransformPool     = MainWorld.GetPool<Transform>();
        GoReferencePool   = MainWorld.GetPool<GameObjectReference>();
        ShipPool          = MainWorld.GetPool<Ship>();
        PlayerPool        = MainWorld.GetPool<Player>();
        DestroyPool       = MainWorld.GetPool<Destroy>();
        BulletPool        = MainWorld.GetPool<Bullet>();
        DamagePool        = MainWorld.GetPool<Damage>();
        HealthPool        = MainWorld.GetPool<Health>();
        TempPool          = MainWorld.GetPool<Temporary>();
        FollowPool        = MainWorld.GetPool<FollowTarget>();
        MovementPool      = MainWorld.GetPool<Movement>();
        PatrolPool        = MainWorld.GetPool<Patrol>();
        TargetPool        = MainWorld.GetPool<HasTarget>();
        AiPool            = MainWorld.GetPool<AiShip>();
        HoldDistancePool  = MainWorld.GetPool<HoldDistance>();
        EngagePool        = MainWorld.GetPool<Engage>();
        InstancedPool     = MainWorld.GetPool<Instanced>();
        StateMachinePool  = MainWorld.GetPool<EnemyStateMachine>();
        MissilePool       = MainWorld.GetPool<Missile>();
        WeaponPool        = MainWorld.GetPool<Weapon>();
        EffectPool        = MainWorld.GetPool<Effect>();
        DroppedEffectPool = MainWorld.GetPool<DroppedEffect>();
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DelComponentIfExist<T>(EcsPool<T> pool, int entity)
    where T : struct
    {
        if(pool.Has(entity))
        {
            pool.Del(entity);
            return true;
        }
        
        return false;
    }    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AddComponentIfNotExist<T>(EcsPool<T> pool, int entity)
    where T : struct
    {
        if(pool.Has(entity) == false)
        {
            return ref pool.Add(entity);
        }
        
        return ref pool.Get(entity);
    }
}

