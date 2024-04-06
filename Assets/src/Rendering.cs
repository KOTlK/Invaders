using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using static Queries;
using static Pools;
using static Assets;

public static class Rendering
{
    public static void DrawBullets()
    {
        var instancesCount = InstancedBulletsQuery.GetEntitiesCount();
        
        // Debug.Log(instancesCount);
        
        if(instancesCount == 0)
            return;
            
        var output         = new NativeArray<Instanced>(instancesCount, Allocator.TempJob);
        var entities       = new NativeArray<int>(InstancedBulletsQuery.GetRawEntities(), Allocator.TempJob);
        var indices        = new NativeArray<int>(InstancedPool.GetRawSparseItems(), Allocator.TempJob);
        var components     = new NativeArray<Instanced>(InstancedPool.GetRawDenseItems(), Allocator.TempJob);
        
        
        var job = new GetInstancesJob
        {
            Entities   = entities,
            Indices    = indices,
            Components = components,
            Output     = output
        };
        
        var handle = job.Schedule(instancesCount, 32);
        
        handle.Complete();
        
        for(var i = 0; i < instancesCount; ++i)
        {
            var rp = new RenderParams(MaterialTable[output[i].material]);
            
            Graphics.RenderMesh(rp, MeshTable[output[i].mesh], 0, output[i].objectToWorld);
        }
        
        output.Dispose();
        entities.Dispose();
        indices.Dispose();
        components.Dispose();
    }
    
}

[BurstCompile]
public struct GetInstancesJob : IJobParallelFor
{
    [ReadOnly]  public NativeArray<int>       Entities;
    [ReadOnly]  public NativeArray<int>       Indices;
    [ReadOnly]  public NativeArray<Instanced> Components;
    [WriteOnly] public NativeArray<Instanced> Output;
    
    public void Execute(int index)
    {
        Output[index] = Components[Indices[Entities[index]]];
    }
}
