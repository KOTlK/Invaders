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
    public int     weapon; //weaponEntity;
}

[System.Serializable]
public struct ShipConfig
{
    public Vector2 size;
    public float   rotationSpeed;
    public float   maxSpeed;
    public float   acceleration;
    public float   damping;
    public int     maxHp;
    public int     prefabId;
    public int     weaponId;
    
    public AiSettings  aiSettings;
}

public enum WeaponType
{
    Kinematic,
    RocketLauncher,
    Laser
}

[System.Serializable]
public struct Weapon
{
    public WeaponType type;
    public float      range;
    public float      bps;
    public float      reloadTime;
    public float      reloadProgress;
    public int        projectileId;
    public int        owner;
    public bool       shooting;
}

public struct Player
{
    public Vector3 lookDirection;
    public Vector3 moveDirection;
    public Vector3 velocity;
}

public struct Destroy
{
    public int framesPassed;
}

public enum ProjectileType
{
    Bullet,
    Rocket
}

[System.Serializable] //TODO: maybe write custom inspector for this?
public struct UnitedProjectile
{
    public ProjectileType type;
    public bool           instanced;
    public int            mesh;
    public int            material;
    public int            prefabId; // prefab id in prefab table
    public int            damage;
    public int            health;
    public int            explosionDamage;
    public bool           canDamageSender;
    public float          speed;
    public float          angularSpeed;
    public float          accelerationNoTarget;
    public float          accelerationWithTarget;
    public float          radius;
    public float          timeToLive;
    public float          splashRadius;
    public float          searchRadius;
    public float          fov;
    public float          flightDistance;
    public float          maxLength;
    public float          thickness;
    public float          attackDelay;
}

public struct Bullet
{
    public float   speed;
    public float   radius;
    public int     damage;
    public int     sender;
    public bool    canDamageSender;
}

public struct Rocket
{
    public float speed;
    public float angularSpeed;
    public float accelerationNoTarget;
    public float accelerationWithTarget;
    public float radius;
    public float splashRadius;
    public float searchRadius;
    public float fov;
    public float flightDistance;
    public int   damage;
    public int   explosionDamage;
    public int   sender;
    public int   target;
}

public struct Instanced
{
    public Matrix4x4 objectToWorld;
    public int       mesh; // mesh index in table
    public int       material; // material index in table
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
    public int entity;
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

public enum EnemyState
{
    Patrolling,
    FollowingTarget,
    Fighting
}

public struct EnemyStateMachine
{
    public EnemyState currentState;
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



public enum EntityType
{
    Ship,
    Bullet,
    Rocket,
    Player
}

public static class Entities
{
    public static int CreateEntity()
    {
        var entity = MainWorld.NewEntity();
        
        return entity;
    }

    public static void CreateReference(int entity, EntityType type, Vector3 position, float orientation, Vector3 scale, Entity prefab)
    {
        ref var transform = ref TransformPool.Add(entity);
        ref var goRef     = ref GoReferencePool.Add(entity);
        
        transform.position    = position;
        transform.orientation = orientation;
        transform.scale       = scale;
        
        var go = Object.Instantiate(prefab, 
                                    position, 
                                    Quaternion.AngleAxis(orientation, Vector3.forward));
        
        go.Id           = entity;
        go.Type         = type;
        goRef.go        = go;
        goRef.transform = go.transform;
    }
    
    public static void DestroyEntity(int entity)
    {
        if(DestroyPool.Has(entity) == false)
        {
            ref var destroy = ref DestroyPool.Add(entity);
            destroy.framesPassed = 0;
        }
    }

    public static void CreatePlayer(Vector3 position, float orientation, Vector3 scale, int assetId)
    {
        var entity       = CreateEntity();
        var asset        = ShipAssetTable[assetId];
        var prefab       = PrefabTable[asset.prefabId];
        
        ref var ship     = ref ShipPool.Add(entity);
        ref var hp       = ref HealthPool.Add(entity);
        PlayerPool.Add(entity);
        var weaponEntity = CreateWeapon(asset.weaponId, entity, EntityType.Player);
        
        ship.size           = asset.size;
        ship.rotationSpeed  = asset.rotationSpeed;
        ship.maxSpeed       = asset.maxSpeed;
        ship.acceleration   = asset.acceleration;
        ship.damping        = asset.damping;
        
        hp.current = asset.maxHp;
        hp.max     = asset.maxHp;
        
        CreateReference(entity, EntityType.Player, position, orientation, scale, prefab);
    }
    
    public static void CreateMultipleShipsRandomly(int count)
    {
        for(var i = 0; i < count; ++i)
        {
            var entity = CreateEntity();
            CreateAiShip(entity, Random.Range(0, ShipAssetTable.Length), RandomPointInsideWorldBounds(),  0f, Vector3.one);
        }
    }
    
    public static int CreateAiShip(int entity, int assetId, Vector3 position, float orientation, Vector3 scale)
    {
        var asset        = ShipAssetTable[assetId];
        var prefab       = PrefabTable[asset.prefabId];
        
        ref var ship     = ref ShipPool.Add(entity);
        ref var hp       = ref HealthPool.Add(entity);
        ref var movement = ref MovementPool.Add(entity);
        ref var ai       = ref AiPool.Add(entity);
        ref var sm       = ref StateMachinePool.Add(entity);
        ref var patrol   = ref PatrolPool.Add(entity);
        
        var weaponEntity = CreateWeapon(asset.weaponId, entity, EntityType.Ship);
        
        ship.size           = asset.size;
        ship.rotationSpeed  = asset.rotationSpeed;
        ship.maxSpeed       = asset.maxSpeed;
        ship.acceleration   = asset.acceleration;
        ship.damping        = asset.damping;
        ship.weapon         = weaponEntity;
        
        hp.current          = asset.maxHp;
        hp.max              = asset.maxHp;
        
        movement.velocity   = Vector3.zero;
        movement.steering   = new Steering{linear = Vector3.zero, angular = 0f};
        
        ai = asset.aiSettings.shipSettings;
        
        sm.currentState     = EnemyState.Patrolling;
        patrol.destination  = RandomPointInsideWorldBounds();    
        
        CreateReference(entity, EntityType.Ship, position, orientation, scale, prefab);
        
        return entity;
    }
    
    public static int CreateWeapon(int assetId, int owner, EntityType ownerType)
    {
        var asset      = WeaponTable[assetId];
        var entity     = CreateEntity();
        ref var weapon = ref WeaponPool.Add(entity);
        
        weapon = asset;
        
        weapon.owner          = owner;
        weapon.reloadTime     = 1 / weapon.bps;
        weapon.reloadProgress = weapon.reloadTime;
        
        switch(ownerType)
        {
            case EntityType.Ship:
            {
                ref var ship = ref ShipPool.Get(owner);
                
                ship.weapon = entity;
            }
            break;
            
            case EntityType.Player:
            {
                ref var ship = ref ShipPool.Get(owner);
                
                ship.weapon = entity;
            }
            break;
        }
        
        return entity;
    }
    
    public static void CreateBullet(Vector3 position, Vector3 direction, float maxDistance, int owner, int assetId)
    {        
        var orientation = Mathf.Atan2(direction.y, direction.x);
        var asset       = ProjectileTable[assetId];
        var entity      = CreateEntity();
        
        if(asset.instanced)
        {
            ref var instance  = ref InstancedPool.Add(entity);
            ref var transform = ref TransformPool.Add(entity);
            
            transform.position    = position;
            transform.orientation = orientation * Mathf.Rad2Deg;
            transform.scale       = Vector3.one;
            
            instance.mesh     = asset.mesh;
            instance.material = asset.material;
        }else
        {
            var prefab      = PrefabTable[asset.prefabId];

            CreateReference(entity, EntityType.Bullet, position, orientation, Vector3.one, prefab);
        }
        
        ref var bullet   = ref BulletPool.Add(entity);
        ref var temp     = ref TempPool.Add(entity);
        ref var movement = ref MovementPool.Add(entity);
        
        direction.Normalize();
        
        bullet.speed           = asset.speed;
        bullet.radius          = asset.radius;
        bullet.damage          = asset.damage;
        bullet.sender          = owner;
        bullet.canDamageSender = asset.canDamageSender;   
        
        temp.timePassed = 0;
        temp.timeToLive = maxDistance / bullet.speed;
        
        movement.velocity = direction * asset.speed;
        movement.steering.angular = 0f;
    }
    
    public static int CreateRocket(Vector3 position, Vector3 direction, int owner, int assetId)
    {
        var entity      = CreateEntity();
        var asset       = ProjectileTable[assetId];
        var prefab      = PrefabTable[asset.prefabId];
        var orientation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        ref var rocket   = ref RocketPool.Add(entity);
        ref var movement = ref MovementPool.Add(entity);
        ref var health   = ref HealthPool.Add(entity);
        
        direction.Normalize();
        
        rocket.speed                     = asset.speed;
        rocket.angularSpeed              = asset.angularSpeed;
        rocket.accelerationNoTarget      = asset.accelerationNoTarget;
        rocket.accelerationWithTarget    = asset.accelerationWithTarget;
        rocket.radius                    = asset.radius;
        rocket.splashRadius              = asset.splashRadius;
        rocket.searchRadius              = asset.searchRadius;
        rocket.fov                       = asset.fov;
        rocket.flightDistance            = asset.flightDistance;
        rocket.damage                    = asset.damage;
        rocket.explosionDamage           = asset.explosionDamage;
        rocket.sender                    = owner;
        rocket.target                    = -1;
        
        movement.velocity         = direction * (asset.speed * 0.7f);
        movement.steering.angular = 0f;
        
        health.max     = asset.health;
        health.current = asset.health;
        
        CreateReference(entity, EntityType.Rocket, position, orientation, Vector3.one, prefab);
        return entity;
    }
    
    public static void DestroyShip(int entity)
    {
        ref var ship = ref ShipPool.Get(entity);
        DestroyEntity(ship.weapon);
        DestroyEntity(entity);
    }
    
    public static void DestroyQueuedEntities()
    {
        foreach(var entity in DestroyRefQuery)
        {
            ref var destroy = ref DestroyPool.Get(entity);
            ref var goRef   = ref GoReferencePool.Get(entity);
            
            if(destroy.framesPassed <= 2)
            {
                destroy.framesPassed++;
                continue;
            }else
            {
                goRef.go.Destroy();
                MainWorld.DelEntity(entity);
            }
        }
        
        foreach(var entity in DestroyQuery)
        {
            ref var destroy = ref DestroyPool.Get(entity);
            
            if(destroy.framesPassed <= 2)
            {
                destroy.framesPassed++;
                continue;
            }else
            {
                MainWorld.DelEntity(entity);
            }
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
            
            var nextPosition = transform.position + movement.velocity * dt;
            
            var halfSize = Size * 0.5f;
            var max      = Center + halfSize;
            var min      = Center - halfSize;
            
            if(nextPosition.x < min.x || nextPosition.x > max.x)
            {
                movement.velocity.x = 0;
            }
            
            if(nextPosition.y < min.y || nextPosition.y > max.y)
            {
                movement.velocity.y = 0;
            }
            
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
            
            var nextPosition = transform.position + player.velocity * dt;
            
            var halfSize = Size * 0.5f;
            var max      = Center + halfSize;
            var min      = Center - halfSize;
            
            if(nextPosition.x < min.x || nextPosition.x > max.x)
            {
                player.velocity.x = 0;
            }
            
            if(nextPosition.y < min.y || nextPosition.y > max.y)
            {
                player.velocity.y = 0;
            }
            
            transform.position    += player.velocity * dt;
            transform.orientation += targetOrientation;
            
            FollowCamera.FollowTarget(transform.position);
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
            
            ref var weapon = ref WeaponPool.Get(ship.weapon);
            
            //tick reloading
            weapon.reloadProgress += dt;
            
            //shoot if can            
            if(weapon.shooting && weapon.reloadProgress >= weapon.reloadTime)
            {
                var orientation = transform.orientation * Mathf.Deg2Rad;
                var direction   = new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation), 0);
                var projectile  = ProjectileTable[weapon.projectileId];
                
                switch(projectile.type)
                {
                    case ProjectileType.Bullet:
                    {
                        CreateBullet(transform.position + direction * 1f,
                                     direction, 
                                     weapon.range, 
                                     entity, 
                                     weapon.projectileId);
                    }
                    break;
                    
                    case ProjectileType.Rocket:
                    {
                        CreateRocket(transform.position + direction * 1f, 
                                     direction, 
                                     entity, 
                                     weapon.projectileId);
                    }
                    break;
                }
                weapon.reloadProgress = 0f;
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
    
    private static Collider2D[] rocketsCollisionBuffer = new Collider2D[32];
    public static void UpdateRockets(float dt)
    {
        foreach(var entity in RocketsQuery)
        {
            ref var rocket    = ref RocketPool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            ref var movement  = ref MovementPool.Get(entity);
            var orientation   = transform.orientation * Mathf.Deg2Rad;
            
            if(rocket.target != -1)
            {
                //madness bcs there is no way to check if entity is alive in leoecs
                try
                {
                    if(DestroyPool.Has(rocket.target))
                    {
                        rocket.target = -1;
                    }
                }catch
                {
                    rocket.target = -1;
                }
            }
            
            if(InsideWorldBounds(transform.position) == false)
            {
                DestroyEntity(entity);
                continue;
            }
            
            //resolve collisions
            var collCount = Physics2D.OverlapCircleNonAlloc(transform.position,
                                                            rocket.radius,
                                                            rocketsCollisionBuffer);

            var destroyed = false;
                                                                              
            for(var i = 0; i < collCount; ++i)
            {
                var coll = rocketsCollisionBuffer[i];
                
                if(coll.TryGetComponent(out Entity collidedEntity))
                {
                    if(collidedEntity.Id == entity)
                        continue;
                        
                    if(collidedEntity.Id == rocket.sender)
                        continue;
                        
                    switch(collidedEntity.Type)
                    {
                        case EntityType.Ship:
                        {
                            ApplyDamageToEntity(collidedEntity.Id, rocket.damage);
                        }
                        break;
                        
                        case EntityType.Player:
                        {
                            if(PlayerInvisible == false)
                                ApplyDamageToEntity(collidedEntity.Id, rocket.damage);
                        }
                        break;
                        
                        case EntityType.Rocket:
                        {
                            ApplyDamageToEntity(collidedEntity.Id, rocket.damage);
                        }
                        break;
                        
                        default:
                            break;
                    }
                    
                    destroyed = true;
                    break;
                }
            }
            
            //create explosion and damage nearby entities
            if(destroyed)
            {
                collCount = Physics2D.OverlapCircleNonAlloc(transform.position, 
                                                            rocket.splashRadius,
                                                            rocketsCollisionBuffer);
                                                            
                for(var i = 0; i < collCount; ++i)
                {
                    var coll = rocketsCollisionBuffer[i];
                    
                    if(coll.TryGetComponent(out Entity collidedEntity))
                    {
                        if(collidedEntity.Id == entity)
                            continue;
                        
                        if(collidedEntity.Id == rocket.sender)
                            continue;
                            
                        switch(collidedEntity.Type)
                        {
                            case EntityType.Ship:
                            {
                                ApplyDamageToEntity(collidedEntity.Id, rocket.explosionDamage);
                            }
                            break;
                            
                            case EntityType.Player:
                            {
                                if(PlayerInvisible == false)
                                    ApplyDamageToEntity(collidedEntity.Id, rocket.explosionDamage);
                            }
                            break;
                            
                            case EntityType.Rocket:
                                ApplyDamageToEntity(collidedEntity.Id, rocket.explosionDamage);
                            break;
                            
                            default:
                                break;
                        }
                    }
                }
                
                DestroyEntity(entity);
                continue;
            }
            
            //search for target
            if(rocket.target == -1)
            {
                collCount = Physics2D.OverlapCircleNonAlloc(
                                        transform.position,
                                        rocket.searchRadius, 
                                        rocketsCollisionBuffer);
            
                var closestDistance   = 10000f;
                var closestDifference = 10000f;
                var closestEntity     = -1;
                var closestDirection  = Vector3.zero;
                var lookDirection     = new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation), 0);
                lookDirection.Normalize();

                
                for(var i = 0; i < collCount; ++i)
                {
                    var coll = rocketsCollisionBuffer[i];
                    
                    if(coll.TryGetComponent(out Entity collidedEntity))
                    {
                        if(collidedEntity.Id == entity)
                            continue;
                            
                        if(collidedEntity.Type == EntityType.Ship || 
                           collidedEntity.Type == EntityType.Player)
                        {
                            ref var targetTransform = ref TransformPool.Get(collidedEntity.Id);
                            var directionToTarget   = targetTransform.position - transform.position;
                            directionToTarget.Normalize();
                            var distance            = Vector3.Dot(directionToTarget, lookDirection);
                            var difference          = 1 - distance;
                            
                            if(difference < closestDifference)
                            {
                                closestDistance   = distance;
                                closestDifference = difference;
                                closestEntity     = collidedEntity.Id;
                                closestDirection  = directionToTarget;
                            }
                        }
                    }
                }
                
                if(closestEntity > -1)
                {
                    var fov = Mathf.Acos(closestDistance) * Mathf.Rad2Deg;
                    
                    if(fov <= rocket.fov)
                    {
                        rocket.target = closestEntity;
                    }
                }
            }
            
            //rotate
            var steering = new Steering();

            if(rocket.target == -1)
            {
                steering.angular = 0f;
            }else
            {
                ref var targetTransform = ref TransformPool.Get(rocket.target);
                var directionToTarget   = targetTransform.position - transform.position;
                var targetOrientation   = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                var angularRotation = Mathf.MoveTowardsAngle(transform.orientation, 
                                                             targetOrientation, 
                                                             rocket.angularSpeed * dt);

                steering.angular = angularRotation - transform.orientation;
            }
            
            
            //move
            var direction = new Vector3(Mathf.Cos(orientation), 
                                        Mathf.Sin(orientation));
            
            if(rocket.target == -1)
            {
                steering.linear = (direction.normalized * rocket.accelerationNoTarget) - movement.velocity;
            }else
            {
                steering.linear = (direction.normalized * rocket.accelerationWithTarget) - movement.velocity;
            }
            
            movement.steering = steering;
            
            rocket.flightDistance -= movement.velocity.magnitude * dt;
            
            if(rocket.flightDistance <= 0)
            {
                DestroyEntity(entity);
            }
        }
    }
}






// line to fix code editor bug :)