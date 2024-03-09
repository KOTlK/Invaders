using static Assets;
using static Queries;
using static Pools;
using static Entities;
using static Globals;
using UnityEngine;

public enum ProjectileType
{
    Bullet,
    Rocket,
    Laser
}

[System.Serializable]
public struct UnitedProjectile
{
    public ProjectileType type;
    public int            prefabId; // prefab id in prefab table
    public int            damage;
    public bool           canDamageSender;
    public float          speed;
    public float          radius;
    public float          timeToLive;
}


public static class Projectiles
{
    public static void CreateProjectile(Vector3 position, Vector3 direction, int owner, int assetId)
    {        
        var orientation = Mathf.Atan2(direction.y, direction.x);

        var entity      = CreateEntity(EntityType.Projectile, position, orientation, Vector3.one, assetId);
        ref var bullet = ref BulletPool.Get(entity);
        bullet.sender = owner;
    }    
    
    private static Collider2D[] results = new Collider2D[32];
    
    public static void UpdateBullets()
    {
        foreach(var entity in BulletsQuery)
        {
            ref var transform = ref TransformPool.Get(entity);
            ref var bullet    = ref BulletPool.Get(entity);
            
            var collCount = Physics2D.OverlapCircleNonAlloc(transform.position, bullet.radius, results);
            
            for(var i = 0; i < collCount; ++i)
            {
                var coll = results[i];
                
                if(coll.TryGetComponent(out Entity collidedEntity))
                {
                    //exclude ourself
                    if(collidedEntity.Id == entity)
                        continue;
                
                    //if hit sender
                    if(collidedEntity.Id == bullet.sender)
                    {
                        if(bullet.canDamageSender == false)
                            continue;
                    }
                    
                    ApplyDamageToEntity(collidedEntity.Id, bullet.damage);
                    DestroyEntity(entity);
                    return;
                }
            }
        }
    }
}
