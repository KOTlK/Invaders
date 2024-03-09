using static Pools;
using static Queries;

public static class Ai
{
    public static void UpdateAi()
    {
        UpdateFollowers();
        UpdateSteering();
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
    
    public static void UpdateSteering()
    {
        //TODO: implement steering;
    }
}
