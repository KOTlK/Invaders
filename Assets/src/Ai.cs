using static Pools;
using static Queries;
using static Entities;
using static World;
using UnityEngine;
using static Vars;
using static Assets;
using static UnityEngine.Mathf;

public static class Ai
{
    public static void UpdateAi(float dt)
    {
        UpdatePatrol(dt);
        UpdateFollowers(dt);
        UpdateEngage(dt);
    }


    //Angle deviations in radians.
    //Less MinAngleDeviation means that agent will roll counterclockwise around target. 
    //Less MaxAngleDeviation means that agent will roll clockwise around target.
    private const float MinAngleDeviation = -0.5f;
    private const float MaxAngleDeviation = 1.5f;
    
    public static void UpdateEngage(float dt)
    {
        foreach(var entity in EngageQuery)
        {
            ref var target          = ref TargetPool.Get(entity);
            ref var engage          = ref EngagePool.Get(entity);
            ref var holdDistance    = ref HoldDistancePool.Get(entity);
            ref var transform       = ref TransformPool.Get(entity);
            ref var movement        = ref MovementPool.Get(entity);
            ref var ship            = ref ShipPool.Get(entity);
            ref var ai              = ref AiPool.Get(entity);
            ref var targetTransform = ref TransformPool.Get(target.targetEntity);
            
            var targetVelocity = Vector3.zero;
            
            if(PlayerPool.Has(target.targetEntity))
            {
                targetVelocity = PlayerPool.Get(target.targetEntity).velocity;
            }else
            {
                targetVelocity = MovementPool.Get(target.targetEntity).velocity;
            }
            
            //move around target
            var direction = transform.position - targetTransform.position;
            var angle     = Atan2(direction.y, direction.x);
            
            angle += Random.Range(MinAngleDeviation, MaxAngleDeviation);
            
            var orientation          = new Vector3(Cos(angle), Sin(angle), 0) * holdDistance.distance;
            var position             = targetTransform.position + orientation;
            var directionToTargetPos = position - transform.position;
            var steering             = new Steering(); 
            var velocity             = directionToTargetPos.normalized * (ship.acceleration);
            
            steering.linear = velocity - movement.velocity;
            
            //aim at target
            //TODO: aim at future target position not in current
            var targetDirection = targetTransform.position - transform.position;
            angle = Atan2(targetDirection.y, targetDirection.x) * Rad2Deg;
            var angularRotation = MoveTowardsAngle(transform.orientation, angle, ship.rotationSpeed * dt);
            
            steering.angular  = angularRotation - transform.orientation;
            movement.steering = steering;
            
            //shoot at target
            
            ship.shooting = true;
        }
    }
    
    public static void UpdateFollowers(float dt)
    {
        foreach(var entity in FollowQuery)
        {
            ref var follow    = ref FollowPool.Get(entity);
            ref var hasTarget = ref TargetPool.Get(entity);
            
            if(DestroyPool.Has(hasTarget.targetEntity))
            {
                FollowPool.Del(entity);
                TargetPool.Del(entity);
                continue;
            }
                
            ref var ship            = ref ShipPool.Get(entity);
            ref var transform       = ref TransformPool.Get(entity);
            ref var targetTransform = ref TransformPool.Get(hasTarget.targetEntity);
            ref var movement        = ref MovementPool.Get(entity);
            
            var directionToTarget = targetTransform.position - transform.position;
            var direction         = directionToTarget;
            var distance          = direction.magnitude;
            
            if(distance > follow.maxDistance)
            {
                ref var patrol = ref PatrolPool.Add(entity);
                ref var ai     = ref AiPool.Get(entity);
                
                patrol.destination  = RandomPointInsideWorldBounds();
                patrol.searchRadius = ai.searchRadius;
                
                FollowPool.Del(entity);
                TargetPool.Del(entity);
                continue;
            }
            
            if(distance <= follow.distance)
            {
                ref var holdDistance = ref HoldDistancePool.Add(entity);
                ref var engage       = ref EngagePool.Add(entity);
                ref var ai           = ref AiPool.Get(entity);
                
                holdDistance.distance = ai.holdDistance;
                holdDistance.max      = ai.maxHoldDistance;
                engage.weaponRange    = ship.weaponRange;
                
                FollowPool.Del(entity);
                continue;
            }
            
            var steering          = new Steering();
            
            direction.Normalize();
            
            var targetPosition = targetTransform.position - direction * follow.distance;

            direction = targetPosition - transform.position;
            
            direction.Normalize();
            
            var angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            
            var target = Mathf.MoveTowardsAngle(transform.orientation, angle, ship.rotationSpeed * dt);
            
            steering.linear = direction * ship.acceleration - movement.velocity;
            steering.angular = target - transform.orientation;
            
            movement.steering = steering;
        }
    }
    
    private const float DestinationEpsilon = 0.01f;
    private const float SlowRadius         = 2f;
    private const float TimeToTarget       = 10f;
    
    private static Collider2D[] CollisionBuffer = new Collider2D[64];
    
    public static void UpdatePatrol(float dt)
    {
        foreach(var entity in PatrolQuery)
        {
            ref var ship      = ref ShipPool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            ref var patrol    = ref PatrolPool.Get(entity);
            ref var movement  = ref MovementPool.Get(entity);
            ref var ai        = ref AiPool.Get(entity);
            

            var collCount = Physics2D.OverlapCircleNonAlloc(transform.position, ai.searchRadius, CollisionBuffer);
            
            var exit = false;
            for(var i = 0; i < collCount; ++i)
            {
                var coll = CollisionBuffer[i];
                
                if(coll.TryGetComponent(out Entity targetEntity))
                {
                    if(targetEntity.Id == entity)
                        continue;
                        
                    if(targetEntity.Type == EntityType.Ship || targetEntity.Type == EntityType.Player)
                    {
                        ref var hasTarget = ref TargetPool.Add(entity);
                        ref var follow    = ref FollowPool.Add(entity);
                        
                        hasTarget.targetEntity = targetEntity.Id;
                        follow.distance        = ai.followDistance;
                        follow.maxDistance     = ai.maxFollowDistance;
                        
                        PatrolPool.Del(entity);
                        exit = true;
                        break;
                    }
                }
            }
            
            if(exit)
                continue;
            
            var direction   = patrol.destination - transform.position;
            var distance    = direction.magnitude;
            var targetSpeed = 0f;
            var steering    = new Steering();
            
            if(distance <= DestinationEpsilon)
            {
                patrol.destination = RandomPointInsideWorldBounds();
                continue;
            }
            
            if(distance > SlowRadius)
            {
                targetSpeed = ship.acceleration;
            }else
            {
                targetSpeed = ship.acceleration * distance / SlowRadius;
            }
            
            var targetVelocity = direction.normalized * targetSpeed;
            
            steering.linear = targetVelocity - movement.velocity;
            steering.linear *= TimeToTarget;
            
            if(steering.linear.sqrMagnitude > ship.acceleration * ship.acceleration)
            {
                steering.linear.Normalize();
                steering.linear *= ship.acceleration;
            }
            
            
            var angle = Mathf.Atan2(movement.velocity.y, movement.velocity.x) * Mathf.Rad2Deg;
            
            var target = Mathf.MoveTowardsAngle(transform.orientation, angle, ship.rotationSpeed * dt);
            
            steering.angular = target - transform.orientation;
            
            movement.steering = steering;
        }
    } 
}
