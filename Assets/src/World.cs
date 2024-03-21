using UnityEngine;
using System.Runtime.CompilerServices;

public static class World
{
    public static Vector3 Center = Vector3.zero;
    public static Vector3 Size;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 RandomPointInsideWorldBounds()
    {
        var x = Random.Range(Center.x - Size.x, Center.x + Size.x);
        var y = Random.Range(Center.y - Size.y, Center.y + Size.y);
        
        return new Vector3(x, y, 0);
    }
}
