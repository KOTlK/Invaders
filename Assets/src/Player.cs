using Leopotam.EcsLite;
using static Globals;
using static Assets;
using static Pools;
using static Entities;
using UnityEngine;

public static class Players
{
    public static void CreatePlayer(Vector3 position, int assetId)
    {
        var entity = CreateEntity(EntityType.Ship, position, assetId);
        
        PlayerPool.Add(entity);
    }
}