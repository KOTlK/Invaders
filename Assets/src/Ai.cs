using static Pools;
using static Queries;
using static Entities;
using UnityEngine;

public static class Ai
{
    public static void UpdateAi()
    {
        UpdateFollowers();
        // UpdateSteering();
    }
    
    public static void UpdateFollowers()
    {
        foreach(var entity in FollowQuery)
        {
            ref var follow    = ref FollowPool.Get(entity);
            
            if(DestroyPool.Has(follow.target))
            {
                FollowPool.Del(entity);
                continue;
            }
                
            ref var movable         = ref MovablePool.Get(entity);
            ref var transform       = ref TransformPool.Get(entity);
            ref var targetTransform = ref TransformPool.Get(follow.target);
            
            var direction = targetTransform.position - transform.position;
            
            direction.Normalize();
            
            var targetPosition = targetTransform.position - direction * follow.distance;

            direction = targetPosition - transform.position;
            
            direction.Normalize();
            
            movable.direction = direction;
        }
    }
    
    private static Collider2D[] obstacleColliders = new Collider2D[64];
    public static void UpdateObstacleAvoidance()
    {
        foreach(var entity in AiAgentsQuery)
        {
            ref var transform = ref TransformPool.Get(entity);
            ref var movable   = ref MovablePool.Get(entity);
            ref var agent     = ref AiAgentPool.Get(entity);
            
            var direction = movable.direction;
            
            var obstaclesCount = Physics2D.OverlapCircleNonAlloc(transform.position, agent.radius, obstacleColliders);
            
            for(var i = 0; i < obstaclesCount; ++i)
            {
                var coll = obstacleColliders[i];
                
                if(coll.TryGetComponent(out Entity obstacleEntity))
                {
                    if(obstacleEntity.Id == entity)
                        continue;
                        
                    ref var obstacleMovable   = ref MovablePool.Get(entity);
                    ref var obstacleTransform = ref TransformPool.Get(entity);
                    
                    
                }
            }
        }
    }
}
