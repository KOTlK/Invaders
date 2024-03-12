using Leopotam.EcsLite;
using static Assets;
using static Pools;
using static Queries;
using UnityEngine;
using static Globals;

public struct Transform
{
    public float      orientation;
    public Vector3    position;
    public Vector3    scale;
}

public struct GameObjectReference
{
    public Entity                 go;
    public UnityEngine.Transform  transform;
}

public struct Movable
{
    public Vector3 direction;
    public Vector3 velocity;
}

public struct Ship
{
    public Vector3 lookDirection;
    public Vector2 size;
    public float   rotationSpeed;
    public float   maxSpeed;
    public float   acceleration;
    public float   reloadTime;
    public float   reloadProgress;
    public bool    shooting;
    public int     projectileId; // projectileId in projectiles table    
}

public struct Player
{
    
}

public struct Destroy
{
    
}

public struct Bullet
{
    public float   speed;
    public float   radius;
    public int     damage;
    public int     sender;
    public bool    canDamageSender;
}

public struct Temporary
{
    public float timeToLive;
    public float timePassed;
}

public struct Health
{
    public int current;
    public int max;
}

public struct Damage
{
    public int amount;
}


//AI

public struct AiAgent
{
    public Vector3 velocity;
    public Vector3 steering;
    public float   acceleration;
    public float   maxSpeed;
    public float   radius;
    public float   fov;
}

public struct FollowTarget
{
    public int   target;
    public float distance;
}

public enum EntityType
{
    Ship,
    Projectile
}

[System.Serializable]
public struct ShipConfig
{
    public Vector2 size;
    public float   rotationSpeed;
    public float   maxSpeed;
    public float   acceleration;
    public float   reloadTime;
    public int     maxHp;
    public int     projectileId;
    public int     prefabId;
}

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

public static class Entities
{
    public static void CreatePlayer(Vector3 position, int assetId)
    {
        var entity = CreateEntity(EntityType.Ship, position, assetId);
        
        PlayerPool.Add(entity);
    }
    
    public static void CreateMultipleShipsRandomly(int count, float radius)
    {
        for(var i = 0; i < count; ++i)
        {
            var entity = CreateEntity(EntityType.Ship, Random.insideUnitCircle * radius, Random.Range(0, ShipAssetTable.Length));
            
            ref var movable  = ref MovablePool.Get(entity);
            ref var follow   = ref FollowPool.Add(entity);
            
            follow.target   = 0;
            follow.distance = 5f;

            movable.direction = Random.insideUnitCircle.normalized;
        }
    }
    
    public static void CreateProjectile(Vector3 position, Vector3 direction, int owner, int assetId)
    {        
        var orientation = Mathf.Atan2(direction.y, direction.x);
        var entity      = CreateEntity(EntityType.Projectile, position, orientation, Vector3.one, assetId);
        ref var bullet = ref BulletPool.Get(entity);
        bullet.sender = owner;
    }
    
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
        ref var goRef     = ref GoReferencePool.Add(entity);
        
        transform.position    = position;
        transform.orientation = orientation;
        transform.scale       = scale;
        
        var go = Object.Instantiate(prefab, position, Quaternion.AngleAxis(orientation, Vector3.forward));
        
        go.Id           = entity;
        go.Type         = type;
        goRef.go        = go;
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
    
    private static Collider2D[] shipColisionResults = new Collider2D[32];
    public static void UpdateShips(float dt)
    {
        foreach(var entity in ShipsQuery)
        {
            ref var ship      = ref ShipPool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            ref var movable   = ref MovablePool.Get(entity);
            
            movable.velocity = Vector3.ClampMagnitude(movable.velocity + movable.direction * ship.acceleration * dt, ship.maxSpeed);
            
            //Rotate
            
            var angle = Mathf.Atan2(ship.lookDirection.y, ship.lookDirection.x) * Mathf.Rad2Deg;
            
            transform.orientation = Mathf.MoveTowardsAngle(transform.orientation, angle, ship.rotationSpeed * dt);
            
            //handle collisions
            var collisionsCount = Physics2D.OverlapBoxNonAlloc(transform.position, ship.size, transform.orientation, shipColisionResults);
            
            for(var i = 0; i < collisionsCount; ++i)
            {
                var coll = shipColisionResults[i];
                
                if(coll.TryGetComponent(out Entity collidedEntity))
                {
                    if(collidedEntity.Id == entity)
                        continue;
                        
                    if(collidedEntity.Type == EntityType.Ship)
                    {
                        //TODO: Handle collisions properly
                        DestroyEntity(entity);
                        DestroyEntity(collidedEntity.Id);
                        return;
                    }
                }
            }
            
            //tick reloading
            ship.reloadProgress += dt;
            
            //shoot if can            
            if(ship.shooting && ship.reloadProgress >= ship.reloadTime)
            {
                var orientation = transform.orientation * Mathf.Deg2Rad;
                var direction   = new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation), 0);
                CreateProjectile(transform.position + direction * 1f, direction, entity, ship.projectileId);
                ship.reloadProgress = 0f;
            }
        }
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






// line to fix code editor bug :)