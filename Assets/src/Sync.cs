using Leopotam.EcsLite;
using UnityEngine;
using static Globals;
using static Pools;
using static Queries;

public static class Sync
{
    public static void SyncReferences()
    {
        foreach(var entity in TransformReferencesQuery)
        {
            ref var transform = ref TransformPool.Get(entity);
            ref var reference = ref GoReferencePool.Get(entity);
            
            reference.transform.SetPositionAndRotation(transform.position, Quaternion.AngleAxis(transform.orientation, Vector3.forward));
            reference.transform.localScale = transform.scale;
        }
    }
}