using Leopotam.EcsLite;
using static Assets;
using static Pools;
using static Queries;
using UnityEngine;
using static Globals;
using static World;
using static Vars;
using static Rendering;

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
    public Collider2D collider;
    public float      rotationSpeed;
    public float      maxSpeed;
    public float      acceleration;
    public float      damping;
    public int[]      weapons; //weaponEntities;
}

[System.Serializable]
public struct ShipConfig
{
    public Entity    prefab;
    public Weapon[]  weapons;
    public float     rotationSpeed;
    public float     maxSpeed;
    public float     acceleration;
    public float     damping;
    public int       maxHp;
    
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
    public Vector3    muzzleOffset;
    public float      range;
    public float      bps;
    public float      reloadTime;
    public float      reloadProgress;
    public float      laserThickness;
    public int        projectileId;
    public int        owner;
    public int        materialIndex;
    public int        laserDamage;
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
    Missile
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

public struct Missile
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

public struct Laser
{
    public Vector3 start;
    public Vector3 end;
    public int     materialIndex;
    public float   thickness;
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
    Missile,
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
    
    public static void CreateReferenceFromInstance(int entity, EntityType type, Vector3 position, float orientation, Vector3 scale, Entity instance)
    {
        ref var transform = ref TransformPool.Add(entity);
        ref var goRef     = ref GoReferencePool.Add(entity);
        
        transform.position    = position;
        transform.orientation = orientation;
        transform.scale       = scale;
        
        instance.Id     = entity;
        instance.Type   = type;
        goRef.go        = instance;
        goRef.transform = instance.transform;
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
        
        ref var ship     = ref ShipPool.Add(entity);
        ref var hp       = ref HealthPool.Add(entity);
        var     go       = Object.Instantiate(asset.prefab, 
                                    position, 
                                    Quaternion.AngleAxis(orientation, Vector3.forward));
        PlayerPool.Add(entity);
        
        ship.weapons = new int[asset.weapons.Length];
        for(var i = 0; i < asset.weapons.Length; ++i)
        {
            var weaponEntity = CreateWeapon(ref asset.weapons[i], entity, EntityType.Player);
            ship.weapons[i] = weaponEntity;
        }
        
        ship.collider       = go.GetComponent<Collider2D>();
        ship.rotationSpeed  = asset.rotationSpeed;
        ship.maxSpeed       = asset.maxSpeed;
        ship.acceleration   = asset.acceleration;
        ship.damping        = asset.damping;
        
        hp.current = asset.maxHp;
        hp.max     = asset.maxHp;
        
        CreateReferenceFromInstance(entity, EntityType.Player, position, orientation, scale, go);
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
        var     asset    = ShipAssetTable[assetId];
        ref var ship     = ref ShipPool.Add(entity);
        ref var hp       = ref HealthPool.Add(entity);
        ref var movement = ref MovementPool.Add(entity);
        ref var ai       = ref AiPool.Add(entity);
        ref var sm       = ref StateMachinePool.Add(entity);
        ref var patrol   = ref PatrolPool.Add(entity);
        var     go       = Object.Instantiate(asset.prefab, 
                                    position, 
                                    Quaternion.AngleAxis(orientation, Vector3.forward));
        
        ship.weapons = new int[asset.weapons.Length];
        for(var i = 0; i < asset.weapons.Length; ++i)
        {
            var weaponEntity = CreateWeapon(ref asset.weapons[i], entity, EntityType.Player);
            ship.weapons[i] = weaponEntity;
        }
        
        ship.collider       = go.GetComponent<Collider2D>();
        ship.rotationSpeed  = asset.rotationSpeed;
        ship.maxSpeed       = asset.maxSpeed;
        ship.acceleration   = asset.acceleration;
        ship.damping        = asset.damping;
        
        hp.current          = asset.maxHp;
        hp.max              = asset.maxHp;
        
        movement.velocity   = Vector3.zero;
        movement.steering   = new Steering{linear = Vector3.zero, angular = 0f};
        
        ai = asset.aiSettings.shipSettings;
        
        sm.currentState     = EnemyState.Patrolling;
        patrol.destination  = RandomPointInsideWorldBounds();    
        
        CreateReferenceFromInstance(entity, EntityType.Ship, position, orientation, scale, go);
        
        return entity;
    }
    
    public static int CreateWeapon(ref Weapon config, int owner, EntityType ownerType)
    {
        var entity     = CreateEntity();
        ref var weapon = ref WeaponPool.Add(entity);
        
        weapon = config;
        
        weapon.owner          = owner;
        weapon.reloadTime     = 1 / weapon.bps;
        weapon.reloadProgress = weapon.reloadTime;
        
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
    
    public static int CreateMissile(Vector3 position, Vector3 direction, int owner, int assetId)
    {
        var entity        = CreateEntity();
        var asset         = ProjectileTable[assetId];
        var prefab        = PrefabTable[asset.prefabId];
        var orientation   = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        ref var missile   = ref MissilePool.Add(entity);
        ref var movement  = ref MovementPool.Add(entity);
        ref var health    = ref HealthPool.Add(entity);
        
        direction.Normalize();
        
        missile.speed                     = asset.speed;
        missile.angularSpeed              = asset.angularSpeed;
        missile.accelerationNoTarget      = asset.accelerationNoTarget;
        missile.accelerationWithTarget    = asset.accelerationWithTarget;
        missile.radius                    = asset.radius;
        missile.splashRadius              = asset.splashRadius;
        missile.searchRadius              = asset.searchRadius;
        missile.fov                       = asset.fov;
        missile.flightDistance            = asset.flightDistance;
        missile.damage                    = asset.damage;
        missile.explosionDamage           = asset.explosionDamage;
        missile.sender                    = owner;
        missile.target                    = -1;
        
        movement.velocity         = direction * (asset.speed * 0.7f);
        movement.steering.angular = 0f;
        
        health.max     = asset.health;
        health.current = asset.health;
        
        CreateReference(entity, EntityType.Missile, position, orientation, Vector3.one, prefab);
        return entity;
    }
    
    public static void DestroyShip(int entity)
    {
        ref var ship = ref ShipPool.Get(entity);
        foreach(var weaponId in ship.weapons)
        {
            DestroyEntity(weaponId);
        }
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
                if (ShipPool.Has(entity))
                {
                    DestroyShip(entity);
                }else
                {
                    DestroyEntity(entity);
                }
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
    
    private static Collider2D[]    shipCollisionResults = new Collider2D[32];
    private static ContactFilter2D shipContactFilter = new ContactFilter2D(){
                                                        layerMask      = Physics2D.AllLayers,
                                                        minDepth       = Mathf.Infinity,
                                                        maxDepth       = Mathf.Infinity,
                                                        minNormalAngle = Mathf.Infinity,
                                                        maxNormalAngle = Mathf.Infinity};
    public static void UpdateShips(float dt)
    {
        foreach(var entity in ShipsQuery)
        {
            ref var ship      = ref ShipPool.Get(entity);
            
            //handle collisions
            var collisionsCount = ship.collider.OverlapCollider(shipContactFilter, shipCollisionResults);
            
            for(var i = 0; i < collisionsCount; ++i)
            {
                var coll = shipCollisionResults[i];
                
                if(coll.TryGetComponent(out Entity collidedEntity))
                {
                    if(collidedEntity.Id == entity)
                        continue;
                        
                    if(collidedEntity.Type == EntityType.Ship)
                    {
                        //TODO: Handle collisions properly
                        if((PlayerPool.Has(collidedEntity.Id) || 
                            PlayerPool.Has(entity)) && PlayerInvisible)
                        {
                            
                        }else
                        {
                            DestroyShip(entity);
                            DestroyShip(collidedEntity.Id);
                            continue;
                        }
                    }
                }
            }
        }
    }
    
    private static RaycastHit2D[] laserCollisionBuffer = new RaycastHit2D[32];
    public static void UpdateWeapons(float dt)
    {
        foreach(var entity in WeaponsQuery)
        {
            ref var weapon    = ref WeaponPool.Get(entity);
            
            //fuck you, leopotam
            try
            {
                if(DestroyPool.Has(weapon.owner))
                {
                    continue;
                }
            }catch
            {
                continue;
            }
            
            
            ref var transform = ref TransformPool.Get(weapon.owner);
            
            //tick reloading
            weapon.reloadProgress += dt;
            
            //shoot if can            
            if(weapon.shooting && weapon.type == WeaponType.Laser) //if laser
            {
                var orientation = transform.orientation * Mathf.Deg2Rad;
                var sin         = Mathf.Sin(orientation);
                var cos         = Mathf.Cos(orientation);
                var direction   = new Vector3(cos, sin, 0).normalized;
                var offset = new Vector3(cos * weapon.muzzleOffset.x - sin * weapon.muzzleOffset.y,
                                         sin * weapon.muzzleOffset.x + cos * weapon.muzzleOffset.y);
                var position = transform.position + offset;
                var hitCount = Physics2D.Raycast(position, 
                                                 direction,
                                                 shipContactFilter,
                                                 laserCollisionBuffer,
                                                 weapon.range);
                
                float   closestDistance = 1000f;
                Entity  hitEntity       = null;
                Vector3 hitPoint        = Vector3.zero;
                
                for(var i = 0; i < hitCount; ++i)
                {
                    if(laserCollisionBuffer[i].collider.TryGetComponent(out Entity hittedEntity))
                    {
                        if(hittedEntity.Id != weapon.owner)
                        {
                            var distance = Vector3.Distance(laserCollisionBuffer[i].point, position);
                            if(distance < closestDistance)
                            {
                                hitEntity       = hittedEntity;
                                closestDistance = distance;
                                hitPoint        = laserCollisionBuffer[i].point;
                            }
                        }
                    }
                }
                
                if(hitEntity == null)
                {
                    DrawLaser(position, 
                              transform.position + direction * weapon.range, 
                              weapon.materialIndex, 
                              weapon.laserThickness);
                }else
                {
                    DrawLaser(position, 
                              hitPoint, 
                              weapon.materialIndex,
                              weapon.laserThickness);
                }
                
                if(weapon.reloadProgress >= weapon.reloadTime)
                {
                    if(hitEntity != null)
                    {
                        switch(hitEntity.Type)
                        {
                            case EntityType.Player:
                            {
                                if(PlayerInvisible == false)
                                    ApplyDamageToEntity(hitEntity.Id, weapon.laserDamage);
                            }
                            break;
                            
                            case EntityType.Ship:
                            {
                                ApplyDamageToEntity(hitEntity.Id, weapon.laserDamage);
                            }
                            break;
                            
                            case EntityType.Missile:
                            {
                                ApplyDamageToEntity(hitEntity.Id, weapon.laserDamage);
                            }
                            break;
                        }
                    }
                    weapon.reloadProgress = 0f;
                }
            }else if(weapon.shooting && weapon.reloadProgress >= weapon.reloadTime)
            {
                var orientation = transform.orientation * Mathf.Deg2Rad;
                var sin         = Mathf.Sin(orientation);
                var cos         = Mathf.Cos(orientation);
                var direction   = new Vector3(cos, sin, 0);
                var projectile  = ProjectileTable[weapon.projectileId];
                
                switch(projectile.type)
                {
                    case ProjectileType.Bullet:
                    {
                        var offset = new Vector3(cos * weapon.muzzleOffset.x - sin * weapon.muzzleOffset.y,
                                                 sin * weapon.muzzleOffset.x + cos * weapon.muzzleOffset.y);
                        CreateBullet(transform.position + offset,
                                     direction, 
                                     weapon.range, 
                                     weapon.owner, 
                                     weapon.projectileId);
                    }
                    break;
                    
                    case ProjectileType.Missile:
                    {
                        var offset = new Vector3(cos * weapon.muzzleOffset.x - sin * weapon.muzzleOffset.y,
                                                 sin * weapon.muzzleOffset.x + cos * weapon.muzzleOffset.y);
                        CreateMissile(transform.position + offset, 
                                     direction, 
                                     weapon.owner, 
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
    
    private static Collider2D[] missilesCollisionBuffer = new Collider2D[32];
    public static void UpdateMissiles(float dt)
    {
        foreach(var entity in MissilesQuery)
        {
            ref var missile   = ref MissilePool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            ref var movement  = ref MovementPool.Get(entity);
            var orientation   = transform.orientation * Mathf.Deg2Rad;
            
            if(missile.target != -1)
            {
                //madness bcs there is no way to check if entity is alive in leoecs
                try
                {
                    if(DestroyPool.Has(missile.target))
                    {
                        missile.target = -1;
                    }
                }catch
                {
                    missile.target = -1;
                }
            }
            
            //resolve collisions
            var collCount = Physics2D.OverlapCircleNonAlloc(transform.position,
                                                            missile.radius,
                                                            missilesCollisionBuffer);

            var destroyed = false;
                                                                              
            for(var i = 0; i < collCount; ++i)
            {
                var coll = missilesCollisionBuffer[i];
                
                if(coll.TryGetComponent(out Entity collidedEntity))
                {
                    if(collidedEntity.Id == entity)
                        continue;
                        
                    if(collidedEntity.Id == missile.sender)
                        continue;
                        
                    switch(collidedEntity.Type)
                    {
                        case EntityType.Ship:
                        {
                            ApplyDamageToEntity(collidedEntity.Id, missile.damage);
                            destroyed = true;
                        }
                        break;
                        
                        case EntityType.Player:
                        {
                            if(PlayerInvisible == false)
                            {
                                ApplyDamageToEntity(collidedEntity.Id, missile.damage);
                                destroyed = true;
                            }
                        }
                        break;
                        
                        default:
                            break;
                    }
                    
                    break;
                }
            }
            
            //create explosion and damage nearby entities
            if(destroyed)
            {
                collCount = Physics2D.OverlapCircleNonAlloc(transform.position, 
                                                            missile.splashRadius,
                                                            missilesCollisionBuffer);
                                                            
                for(var i = 0; i < collCount; ++i)
                {
                    var coll = missilesCollisionBuffer[i];
                    
                    if(coll.TryGetComponent(out Entity collidedEntity))
                    {
                        if(collidedEntity.Id == entity)
                            continue;
                        
                        if(collidedEntity.Id == missile.sender)
                            continue;
                            
                        switch(collidedEntity.Type)
                        {
                            case EntityType.Ship:
                            {
                                ApplyDamageToEntity(collidedEntity.Id, missile.explosionDamage);
                            }
                            break;
                            
                            case EntityType.Player:
                            {
                                if(PlayerInvisible == false)
                                    ApplyDamageToEntity(collidedEntity.Id, missile.explosionDamage);
                            }
                            break;
                            
                            case EntityType.Missile:
                                ApplyDamageToEntity(collidedEntity.Id, missile.explosionDamage);
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
            if(missile.target == -1)
            {
                collCount = Physics2D.OverlapCircleNonAlloc(
                                        transform.position,
                                        missile.searchRadius, 
                                        missilesCollisionBuffer);
            
                var closestDistance   = 10000f;
                var closestDifference = 10000f;
                var closestEntity     = -1;
                var closestDirection  = Vector3.zero;
                var lookDirection     = new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation), 0);
                lookDirection.Normalize();

                
                for(var i = 0; i < collCount; ++i)
                {
                    var coll = missilesCollisionBuffer[i];
                    
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
                    
                    if(fov <= missile.fov)
                    {
                        missile.target = closestEntity;
                    }
                }
            }
            
            //rotate
            var steering = new Steering();

            if(missile.target == -1)
            {
                steering.angular = 0f;
            }else
            {
                ref var targetTransform = ref TransformPool.Get(missile.target);
                var directionToTarget   = targetTransform.position - transform.position;
                var targetOrientation   = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                var angularRotation = Mathf.MoveTowardsAngle(transform.orientation, 
                                                             targetOrientation, 
                                                             missile.angularSpeed * dt);

                steering.angular = angularRotation - transform.orientation;
            }
            
            
            //move
            var direction = new Vector3(Mathf.Cos(orientation), 
                                        Mathf.Sin(orientation));
            
            if(missile.target == -1)
            {
                steering.linear = (direction.normalized * missile.accelerationNoTarget) - movement.velocity;
            }else
            {
                steering.linear = (direction.normalized * missile.accelerationWithTarget) - movement.velocity;
            }
            
            movement.steering = steering;
            
            missile.flightDistance -= movement.velocity.magnitude * dt;
            
            if(missile.flightDistance <= 0)
            {
                DestroyEntity(entity);
            }
        }
    }
}






// line to fix code editor bug :)