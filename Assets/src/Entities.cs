using Leopotam.EcsLite;
using static Assets;
using static Pools;
using static Queries;
using UnityEngine;
using static Globals;
using static World;
using static Vars;

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

public struct Movement
{
    public Vector3  velocity;
    public float    rotation;
    public Steering steering;
}

[System.Serializable]
public struct Steering
{
    public Vector3 linear;
    public float   angular;
}

public struct Ship
{
    public Vector2 size;
    public float   rotationSpeed;
    public float   maxSpeed;
    public float   acceleration;
    public float   damping;
    public float   reloadTime;
    public float   reloadProgress;
    public float   weaponRange;
    public bool    shooting;
    public int     projectileId; // projectileId in projectiles table    
}

public struct Player
{
    public Vector3 lookDirection;
    public Vector3 moveDirection;
    public Vector3 velocity;
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

public struct FollowTarget
{
    public float distance;
    public float maxDistance;
}

public struct Patrol
{
    public Vector3 destination;
    public float   searchRadius;
}

public struct HasTarget
{
    public int targetEntity;
}

public struct HoldDistance
{
    public float distance;
    public float max;
}

public struct Engage
{
    public float weaponRange;
}

[System.Serializable]
public struct AiShip
{
    public float followDistance;
    public float maxFollowDistance;
    public float holdDistance;
    public float maxHoldDistance;
    public float searchRadius;
}

[System.Serializable]
public struct AiSettings
{
    public AiShip shipSettings;
}

[System.Serializable]
public struct ShipConfig
{
    public Vector2 size;
    public float   rotationSpeed;
    public float   maxSpeed;
    public float   acceleration;
    public float   damping;
    public float   reloadTime;
    public float   weaponRange;
    public int     maxHp;
    public int     projectileId;
    public int     prefabId;
    
    public AiSettings  aiSettings;
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

public enum EntityType
{
    Ship,
    Projectile,
    Player
}

public static class Entities
{
    public static void CreatePlayer(Vector3 position, int assetId)
    {
        var entity = CreateEntity(EntityType.Player, position, assetId);        
    }
    
    public static void CreateMultipleShipsRandomly(int count)
    {
        for(var i = 0; i < count; ++i)
        {
            var entity = CreateEntity(EntityType.Ship, RandomPointInsideWorldBounds(), Random.Range(0, ShipAssetTable.Length));
            
            ref var patrol = ref PatrolPool.Add(entity);
            
            patrol.destination = RandomPointInsideWorldBounds();
        }
    }
    
    public static void CreateProjectile(Vector3 position, Vector3 direction, float maxDistance, int owner, int assetId)
    {        
        var orientation = Mathf.Atan2(direction.y, direction.x);
        var entity      = CreateEntity(EntityType.Projectile, position, orientation, Vector3.one, assetId);
        ref var bullet = ref BulletPool.Get(entity);
        ref var temp   = ref TempPool.Get(entity);
        
        temp.timeToLive = maxDistance / bullet.speed;
        
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
                
                ref var ship     = ref ShipPool.Add(entity);
                ref var hp       = ref HealthPool.Add(entity);
                ref var movement = ref MovementPool.Add(entity);
                ref var ai       = ref AiPool.Add(entity);
                
                ship.size           = asset.size;
                ship.reloadTime     = asset.reloadTime;
                ship.weaponRange    = asset.weaponRange;
                ship.projectileId   = asset.projectileId;
                ship.rotationSpeed  = asset.rotationSpeed;
                ship.maxSpeed       = asset.maxSpeed;
                ship.acceleration   = asset.acceleration;
                ship.damping        = asset.damping;
                
                hp.current          = asset.maxHp;
                hp.max              = asset.maxHp;
                
                movement.velocity   = Vector3.zero;
                movement.steering   = new Steering{linear = Vector3.zero, angular = 0f};
                
                ai = asset.aiSettings.shipSettings;
                
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
                        ref var bullet   = ref BulletPool.Add(entity);
                        ref var temp     = ref TempPool.Add(entity);
                        ref var movement = ref MovementPool.Add(entity);
                        //in this case orientation passed as radians, not degrees. otherwise there will be unnecessary translations.
                        var direction = new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation), 0);
                        
                        direction.Normalize();
                        
                        bullet.speed           = asset.speed;
                        bullet.radius          = asset.radius;
                        bullet.damage          = asset.damage;
                        bullet.canDamageSender = asset.canDamageSender;   
                        
                        temp.timePassed = 0;
                        
                        movement.velocity = direction * asset.speed;
                        movement.steering.angular = 0f;
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
            
            case EntityType.Player:
            {
                var asset  = ShipAssetTable[assetId];
                var prefab = PrefabTable[asset.prefabId];
                
                ref var ship     = ref ShipPool.Add(entity);
                ref var hp       = ref HealthPool.Add(entity);
                PlayerPool.Add(entity);
                
                ship.size          = asset.size;
                ship.rotationSpeed = asset.rotationSpeed;
                ship.weaponRange   = asset.weaponRange;
                ship.maxSpeed      = asset.maxSpeed;
                ship.acceleration  = asset.acceleration;
                ship.damping       = asset.damping;
                ship.reloadTime    = asset.reloadTime;
                ship.projectileId  = asset.projectileId;
                
                hp.current = asset.maxHp;
                hp.max     = asset.maxHp;
                
                CreateReference(entity, EntityType.Player, position, orientation, scale, prefab);
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
            ref var movement  = ref MovementPool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            
            transform.position    += movement.velocity * dt;
            transform.orientation += movement.steering.angular;
            
            movement.velocity += movement.steering.linear * dt;
        }
    }
    
    public static void UpdatePlayer(float dt)
    {
        foreach(var entity in PlayerShipQuery)
        {
            ref var ship      = ref ShipPool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            ref var player    = ref PlayerPool.Get(entity);
            
            if(player.moveDirection.x == 0)
            {
                player.velocity.x = Mathf.MoveTowards(player.velocity.x, 0, ship.damping * dt);
            } else
            {
                player.velocity.x += player.moveDirection.x * ship.acceleration * dt;
            }
            
            if(player.moveDirection.y == 0)
            {
                player.velocity.y = Mathf.MoveTowards(player.velocity.y, 0, ship.damping * dt);
            }else
            {
                player.velocity.y += player.moveDirection.y * ship.acceleration * dt;
            }
            
            player.velocity = Vector3.ClampMagnitude(player.velocity, ship.maxSpeed);
            
            var angle = Mathf.Atan2(player.lookDirection.y, player.lookDirection.x) * Mathf.Rad2Deg;
            var targetOrientation = Mathf.MoveTowardsAngle(transform.orientation, angle, ship.rotationSpeed * dt);
            targetOrientation -= transform.orientation;
            
            transform.position    += player.velocity * dt;
            transform.orientation += targetOrientation;
        }
    }
    
    private static Collider2D[] shipColisionResults = new Collider2D[32];
    public static void UpdateShips(float dt)
    {
        foreach(var entity in ShipsQuery)
        {
            ref var ship      = ref ShipPool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            
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
                        if((PlayerPool.Has(collidedEntity.Id) || PlayerPool.Has(entity)) && PlayerInvisible)
                        {
                            
                        }else
                        {
                            DestroyEntity(entity);
                            DestroyEntity(collidedEntity.Id);
                            continue;
                        }
                        
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
                CreateProjectile(transform.position + direction * 1f, direction, ship.weaponRange, entity, ship.projectileId);
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
                    
                    if(collidedEntity.Type == EntityType.Player)
                    {
                        if(PlayerInvisible)
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