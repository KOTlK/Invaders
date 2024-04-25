using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System;
using static Queries;
using static Pools;
using static Assets;

public static class Rendering
{
    private static Laser[] laserQueue = new Laser[128];
    private static int     laserCount;
    
    private static Mesh[] meshPool = new Mesh[32];
    
    public static void DrawBullets()
    {
        var instancesCount = InstancedBulletsQuery.GetEntitiesCount();
        
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
    
    public static void DrawLaser(Vector3 start, Vector3 end, int materialIndex, float thickness)
    {
        laserCount++;
        laserQueue[laserCount].start         = start;
        laserQueue[laserCount].end           = end;
        laserQueue[laserCount].materialIndex = materialIndex;
        laserQueue[laserCount].thickness     = thickness;
        
        if(laserCount >= laserQueue.Length){
            Array.Resize(ref laserQueue, laserCount << 1);
        }
    }
    
    public static void DrawLasers()
    {
        for(var i = 0; i < laserCount; ++i){
            var material      = MaterialTable[laserQueue[i].materialIndex];
            var mesh          = GetMesh(i);
            var direction     = laserQueue[i].end - laserQueue[i].start;
            var length        = Vector3.Distance(laserQueue[i].end, laserQueue[i].start);
            var halfLength    = length * 0.5f;
            var halfThick     = laserQueue[i].thickness * 0.5f;
            var rightTop      = new Vector3(halfLength, halfThick, 0);
            var rightBottom   = new Vector3(halfLength, -halfThick, 0);
            var leftBottom    = new Vector3(-halfLength, -halfThick, 0);
            var leftTop       = new Vector3(-halfLength, halfThick, 0);
            var angle         = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var objectToWorld = Matrix4x4.TRS(laserQueue[i].start + direction * 0.5f, 
                                              Quaternion.AngleAxis(angle, Vector3.forward), 
                                              Vector3.one);
                                              
            mesh.SetVertices(new Vector3[]{
                leftTop,
                rightTop,
                rightBottom,
                leftBottom
            });
            mesh.SetTriangles(new int[]{
                0,
                1,
                2,
                0,
                2,
                3
            }, 0);
            mesh.SetUVs(0, new Vector2[]{
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            });
            mesh.SetNormals(new Vector3[]{
                Vector3.back,
                Vector3.back,
                Vector3.back,
                Vector3.back
            });
            
            
            var rp = new RenderParams(material);
            Graphics.RenderMesh(rp, mesh, 0, objectToWorld);
        }
    }
    
    public static void FlushLasers(){
        laserCount = 0;
    }
    
    private static Mesh GetMesh(int index){
        if(index >= meshPool.Length){
            Array.Resize(ref meshPool, index << 1);
        }
        
        if(meshPool[index] == null){
            meshPool[index] = new Mesh();
        }
        
        return meshPool[index];
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
