using UnityEngine;
using System.Runtime.CompilerServices;

public static class World
{
    public static Vector3 Center = Vector3.zero;
    public static Vector3 Size;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 RandomPointInsideWorldBounds()
    {
        var halfSize = Size * 0.5f;
        var x = Random.Range(Center.x - halfSize.x, Center.x + halfSize.x);
        var y = Random.Range(Center.y - halfSize.y, Center.y + halfSize.y);
        
        return new Vector3(x, y, 0);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsideWorldBounds(Vector3 point)
    {
        var halfSize = Size * 0.5f;
        var max      = Center + halfSize;
        var min      = Center - halfSize;
        
        if(point.x > max.x || point.x < min.x)
           return false;
           
        if(point.y > max.y || point.y < min.y)
            return false;
            
        return true;
    }
}
