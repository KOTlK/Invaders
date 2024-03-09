using Leopotam.EcsLite;
using UnityEngine;
using static Pools;
using static Queries;
using static Assets;
using static Globals;
using static Entities;
using static Projectiles;

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

public static class Ships
{
    public static void CreateMultipleShipsRandomly(int count, float radius)
    {
        for(var i = 0; i < count; ++i)
        {
            var entity = CreateEntity(EntityType.Ship, Random.insideUnitCircle * radius, Random.Range(0, ShipAssetTable.Length));
            
            ref var movable = ref MovablePool.Get(entity);
            ref var follow   = ref FollowPool.Add(entity);
            
            follow.target = 0;
            follow.distance = 5f;
                        
            movable.direction = Random.insideUnitCircle.normalized;
        }
    }
    
    private static Collider2D[] results = new Collider2D[32];
    public static void UpdateShips(float dt)
    {
        foreach(var entity in ShipsQuery)
        {
            ref var ship      = ref ShipPool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            ref var movable   = ref MovablePool.Get(entity);
            
            movable.velocity = Vector3.ClampMagnitude(movable.velocity + movable.direction * ship.acceleration * dt, ship.maxSpeed);
            
            //Rotate
            
            var angle = Mathf.Atan2(ship.lookDirection.y, ship.lookDirection.x) * Mathf.Rad2Deg - 90f;
            
            transform.orientation = Mathf.MoveTowardsAngle(transform.orientation, angle, ship.rotationSpeed * dt);
            
            //handle collisions
            var collisionsCount = Physics2D.OverlapBoxNonAlloc(transform.position, ship.size, transform.orientation, results);
            
            for(var i = 0; i < collisionsCount; ++i)
            {
                var coll = results[i];
                
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
                var orientation = (transform.orientation + 90f) * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation), 0);
                CreateProjectile(transform.position + direction * 1f, direction, entity, ship.projectileId);
                ship.reloadProgress = 0f;
            }
        }
    }
}