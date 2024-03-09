using static Queries;
using static Pools;
using static Globals;
using UnityEngine;

public static class GameInput
{
    public static void UpdateInput()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var shooting = Input.GetKey(KeyCode.Mouse0);
        var mousePosition = MainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
            
        foreach(var entity in PlayerShipQuery)
        {
            ref var transform = ref TransformPool.Get(entity);
            ref var ship      = ref ShipPool.Get(entity);
            ref var movable   = ref MovablePool.Get(entity);
            
            var direction = mousePosition - transform.position;
        
            if(direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }
            
            movable.direction  = new Vector3(horizontal, vertical, 0);
            ship.lookDirection = direction;
            ship.shooting      = shooting;
        }
    }
}
