using Leopotam.EcsLite;
using static Assets;
using static Pools;
using static Queries;
using UnityEngine;
using static Globals;


public enum EntityType
{
    Ship,
    Projectile
}

public static class Entities
{
    public static int CreateEntity(EntityType type, int assetId)
    {
        return CreateEntity(type, Vector3.zero, 0, Vector3.one, assetId);
    }
    
    public static int CreateEntity(EntityType type, Vector3 position, int assetId)
    {
        return CreateEntity(type, position, 0, Vector3.one, assetId);
    }
    
    //Create basic entity with transform and view
    public static int CreateEntity(EntityType type, Vector3 position, float orientation, Vector3 scale, int assetId)
    {
        var entity = MainWorld.NewEntity();
        
        switch (type)
        {
            case EntityType.Ship:
            {
                var asset  = ShipAssetTable[assetId];
                var prefab = PrefabTable[asset.prefabId];
                
                ref var ship    = ref ShipPool.Add(entity);
                ref var hp      = ref HealthPool.Add(entity);
                ref var movable = ref MovablePool.Add(entity);
                
                ship.size          = asset.size;
                ship.rotationSpeed = asset.rotationSpeed;
                ship.maxSpeed      = asset.maxSpeed;
                ship.acceleration  = asset.acceleration;
                ship.reloadTime    = asset.reloadTime;
                ship.projectileId  = asset.projectileId;
                
                hp.current = asset.maxHp;
                hp.max     = asset.maxHp;
                
                movable.direction = Vector3.zero;
                movable.velocity  = Vector3.zero;
                
                CreateReference(entity, EntityType.Ship, position, orientation, scale, prefab);
            }
            break;
            
            case EntityType.Projectile:
            {
                var asset       = ProjectileTable[assetId];
                var prefab      = PrefabTable[asset.prefabId];
                
                switch(asset.type)
                {
                    case ProjectileType.Bullet:
                    {
                        ref var bullet  = ref BulletPool.Add(entity);
                        ref var temp    = ref TempPool.Add(entity);
                        ref var movable = ref MovablePool.Add(entity);
                        //in this case orientation passed as radians, not degrees. otherwise there will be unnecessary translations.
                        var direction = new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation), 0);
                        
                        direction.Normalize();
                        
                        bullet.speed           = asset.speed;
                        bullet.radius          = asset.radius;
                        bullet.damage          = asset.damage;
                        bullet.canDamageSender = asset.canDamageSender;   
                        
                        temp.timeToLive = asset.timeToLive;
                        temp.timePassed = 0;
                        
                        movable.velocity = direction * asset.speed;
                    }
                    break;
                    
                    case ProjectileType.Rocket:
                    {
                        
                    }
                    break;
                    
                    case ProjectileType.Laser:
                    {
                        
                    }
                    break;
                }
                
                CreateReference(entity, EntityType.Projectile, position, orientation, scale, prefab);
            }
            break;
        }
        
        return entity;
    }

    public static void CreateReference(int entity, EntityType type, Vector3 position, float orientation, Vector3 scale, Entity prefab)
    {
        ref var transform = ref TransformPool.Add(entity);
        ref var goRef = ref GoReferencePool.Add(entity);
        
        transform.position = position;
        transform.orientation = orientation;
        transform.scale = scale;
        
        var go = Object.Instantiate(prefab, position, Quaternion.AngleAxis(orientation, Vector3.forward));
        
        go.Id = entity;
        go.Type = type;
        goRef.go = go;
        goRef.transform = go.transform;
    }
    
    public static void DestroyEntity(int entity)
    {
        if(DestroyPool.Has(entity) == false)
            DestroyPool.Add(entity);
    }
    
    public static void DestroyQueuedEntities()
    {
        foreach(var entity in DestroyRefQuery)
        {
            ref var goRef = ref GoReferencePool.Get(entity);
            goRef.go.Destroy();
            MainWorld.DelEntity(entity);
        }
    }
    
    public static void ApplyDamageToEntity(int entity, int damageAmount)
    {
        if(DamagePool.Has(entity))
        {
            ref var damage = ref DamagePool.Get(entity);
            damage.amount += damageAmount;
        }else
        {
            ref var damage = ref DamagePool.Add(entity);
            damage.amount = damageAmount;
        }
    }
    
    public static void UpdateHealth()
    {
        foreach(var entity in DamageQuery)
        {
            ref var health = ref HealthPool.Get(entity);
            ref var damage = ref DamagePool.Get(entity);
            
            health.current -= damage.amount;
            
            DamagePool.Del(entity);
            
            if(health.current <= 0)
            {
                DestroyEntity(entity);
            }
        }
    }    
    
    public static void UpdateTemp(float dt)
    {
        foreach(var entity in TempQuery)
        {
            ref var temp = ref TempPool.Get(entity);
            
            temp.timePassed += dt;
            
            if(temp.timePassed >= temp.timeToLive)
            {
                DestroyEntity(entity);
            }
        }
    }
    
    public static void MoveEntities(float dt)
    {
        foreach(var entity in MoveQuery)
        {
            ref var movable   = ref MovablePool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            
            transform.position += movable.velocity * dt;
        }
    }
}
