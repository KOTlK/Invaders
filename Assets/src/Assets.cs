using UnityEngine;
using System;

public static class Assets
{
    public static ShipConfig[]       ShipAssetTable;
    public static Material[]         MaterialTable;
    public static Mesh[]             MeshTable;
    public static EffectConfig[]     EffectTable;
    public static EffectConfig[]     SortedEffects; //#Redundant It should be used to drop random effects, but now it's useless
    
    //#Redundant
    public static void SortEffects()
    {
        SortedEffects = new EffectConfig[EffectTable.Length];
        var max       = EffectTable[0].dropChance;
        
        for(var i = 1; i < EffectTable.Length; ++i)
        {
            if(EffectTable[i].dropChance > max)
            {
                max = EffectTable[i].dropChance;
            }
        }
        
        var count = new int[max + 1];
        
        for(var i = 0; i < EffectTable.Length; ++i)
        {
            count[EffectTable[i].dropChance]++;
        }
        
        for(var i = 1; i <= max; ++i)
        {
            count[i] += count[i - 1];
        }
        
        for(var i = 0; i < EffectTable.Length; ++i)
        {
            SortedEffects[count[EffectTable[i].dropChance] - 1] = EffectTable[i];
            count[EffectTable[i].dropChance]--;
        }
    }
}