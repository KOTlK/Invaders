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
        UpdateStateMachines(dt);
        UpdatePatrolSteering(dt);
        UpdateFollowersSteering(dt);
        UpdateEngageSteering(dt);
    }

    private static Collider2D[] CollisionBuffer = new Collider2D[64];
    
    public static void UpdateStateMachines(float dt)
    {
        foreach(var entity in EnemyStateMachineQuery)
        {
            ref var stateMachine = ref StateMachinePool.Get(entity);
            ref var ai           = ref AiPool.Get(entity);
            
            switch(stateMachine.currentState)
            {
                case EnemyState.Patrolling:
                {
                    ref var transform = ref TransformPool.Get(entity);

                    var collCount = Physics2D.OverlapCircleNonAlloc(
                        transform.position, 
                        ai.searchRadius, 
                        CollisionBuffer);
            
                    for(var i = 0; i < collCount; ++i)
                    {
                        var coll = CollisionBuffer[i];
                        
                        if(coll.TryGetComponent(out Entity targetEntity))
                        {
                            if(targetEntity.Id == entity)
                                continue;
                                
                            if(targetEntity.Type == EntityType.Ship || 
                               targetEntity.Type == EntityType.Player)
                            {
                                ToFollowingTarget(entity, ref ai, ref stateMachine, targetEntity.Id);
                                break;
                            }
                        }
                    }
                }
                break;
                
                case EnemyState.FollowingTarget:
                {
                    ref var target          = ref TargetPool.Get(entity);             
                    ref var transform       = ref TransformPool.Get(entity);
                    ref var targetTransform = ref TransformPool.Get(target.entity);
                    ref var follow          = ref FollowPool.Get(entity);
                    
                    if(DestroyPool.Has(target.entity))
                    {
                        ToPatrolling(entity, ref ai, ref stateMachine);
                        break;
                    }
                    
                    var directionToTarget = targetTransform.position - transform.position;
                    var direction         = directionToTarget;
                    var distance          = direction.magnitude;
                    
                    if(distance > follow.maxDistance)
                    {
                        ToPatrolling(entity, ref ai, ref stateMachine);
                        break;
                    }
                    
                    if(distance <= follow.distance)
                    {
                        ToFighting(entity, ref ai, ref stateMachine);
                        break;
                    }
                }
                break;
                
                case EnemyState.Fighting:
                {
                    ref var target          = ref TargetPool.Get(entity);
                    ref var transform       = ref TransformPool.Get(entity);
                    ref var targetTransform = ref TransformPool.Get(target.entity);
                    ref var holdDistance    = ref HoldDistancePool.Get(entity);
                    ref var ship            = ref ShipPool.Get(entity);
                    
                    ship.shooting = true;
                    
                    if(DestroyPool.Has(target.entity))
                    {
                        ship.shooting = false;
                        
                        ToPatrolling(entity, ref ai, ref stateMachine);
                        break;
                    }
                    
                    var sqrDistanceToTarget = (targetTransform.position - transform.position).sqrMagnitude;
                    
                    if(sqrDistanceToTarget >= holdDistance.max * holdDistance.max)
                    {
                        ship.shooting = false;
                        
                        ToFollowingTarget(entity, ref ai, ref stateMachine, target.entity);
                        break;
                    }
                }
                break;
            }
        }
    }
    
    public static void ToPatrolling(int entity, ref AiShip ai, ref EnemyStateMachine sm)
    {
        ref var patrol = ref PatrolPool.Add(entity);
                        
        patrol.destination  = RandomPointInsideWorldBounds();
        patrol.searchRadius = ai.searchRadius;
        
        DelComponentIfExist(FollowPool, entity);
        DelComponentIfExist(TargetPool, entity);
        DelComponentIfExist(EngagePool, entity);
        DelComponentIfExist(HoldDistancePool, entity);
        
        sm.currentState = EnemyState.Patrolling;
    }
    
    public static void ToFollowingTarget(int entity, ref AiShip ai, ref EnemyStateMachine sm, int targetEntity)
    {
        ref var target    = ref AddComponentIfNotExist(TargetPool, entity);
        ref var follow    = ref FollowPool.Add(entity);
        
        target.entity          = targetEntity;
        follow.distance        = ai.followDistance;
        follow.maxDistance     = ai.maxFollowDistance;
        
        DelComponentIfExist(EngagePool, entity);
        DelComponentIfExist(HoldDistancePool, entity);
        DelComponentIfExist(PatrolPool, entity);
        
        sm.currentState = EnemyState.FollowingTarget;
    }
    
    public static void ToFighting(int entity, ref AiShip ai, ref EnemyStateMachine sm)
    {
        ref var holdDistance = ref HoldDistancePool.Add(entity);
        ref var engage       = ref EngagePool.Add(entity);
        ref var ship         = ref ShipPool.Get(entity);
        
        holdDistance.distance = ai.holdDistance;
        holdDistance.max      = ai.maxHoldDistance;
        engage.weaponRange    = ship.weaponRange;
        
        DelComponentIfExist(FollowPool, entity);
        DelComponentIfExist(PatrolPool, entity);
        
        sm.currentState = EnemyState.Fighting;
    }


    //Angle deviations in radians.
    //Less MinAngleDeviation means that agent will roll counterclockwise around target. 
    //Less MaxAngleDeviation means that agent will roll clockwise around target.
    private const float MinAngleDeviation = -0.5f;
    private const float MaxAngleDeviation = 1.5f;
    
    public static void UpdateEngageSteering(float dt)
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
            ref var targetTransform = ref TransformPool.Get(target.entity);
            
            var targetVelocity = Vector3.zero;
            
            if(PlayerPool.Has(target.entity))
            {
                targetVelocity = PlayerPool.Get(target.entity).velocity;
            }else
            {
                targetVelocity = MovementPool.Get(target.entity).velocity;
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
            var distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            var time             = distanceToTarget / ProjectileTable[ship.projectileId].speed;
            var targetPosition   = targetTransform.position + targetVelocity * time;
            var targetDirection  = targetPosition - transform.position;
            
            angle = Atan2(targetDirection.y, targetDirection.x) * Rad2Deg;
            var angularRotation = MoveTowardsAngle(transform.orientation, angle, ship.rotationSpeed * dt);
            
            steering.angular  = angularRotation - transform.orientation;
            movement.steering = steering;            
        }
    }
    
    public static void UpdateFollowersSteering(float dt)
    {
        foreach(var entity in FollowQuery)
        {
            ref var follow    = ref FollowPool.Get(entity);
            ref var target    = ref TargetPool.Get(entity);
                
            ref var ship            = ref ShipPool.Get(entity);
            ref var transform       = ref TransformPool.Get(entity);
            ref var targetTransform = ref TransformPool.Get(target.entity);
            ref var movement        = ref MovementPool.Get(entity);
            
            var directionToTarget = targetTransform.position - transform.position;
            var direction         = directionToTarget;
            
            var steering          = new Steering();
            
            direction.Normalize();
            
            var targetPosition = targetTransform.position - direction * follow.distance;

            direction = targetPosition - transform.position;
            
            direction.Normalize();
            
            var angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            
            var targetAngle = Mathf.MoveTowardsAngle(transform.orientation, angle, ship.rotationSpeed * dt);
            
            steering.linear = direction * ship.acceleration - movement.velocity;
            steering.angular = targetAngle - transform.orientation;
            
            movement.steering = steering;
        }
    }
    
    private const float DestinationEpsilon = 0.01f;
    private const float SlowRadius         = 2f;
    private const float TimeToTarget       = 10f;
    
    public static void UpdatePatrolSteering(float dt)
    {
        foreach(var entity in PatrolQuery)
        {
            ref var ship      = ref ShipPool.Get(entity);
            ref var transform = ref TransformPool.Get(entity);
            ref var patrol    = ref PatrolPool.Get(entity);
            ref var movement  = ref MovementPool.Get(entity);
            
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
