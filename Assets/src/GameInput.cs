using static Queries;
using static Pools;
using static Globals;
using UnityEngine;

public static class GameInput
{
    public static void UpdateInput(float dt)
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
            ref var player    = ref PlayerPool.Get(entity);
            
            var direction = mousePosition - transform.position;
        
            if(direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }
            
            player.moveDirection = new Vector3(horizontal, vertical);
            player.lookDirection = direction;
            
            for(var i = 0; i < ship.weapons.Length; ++i)
            {
                ref var weapon  = ref WeaponPool.Get(ship.weapons[i]);
                weapon.shooting = shooting;
            }
        }
    }
}
