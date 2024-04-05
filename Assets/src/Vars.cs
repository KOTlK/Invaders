using System.IO;
using UnityEngine;
using System;

public static class Vars
{    
    public static bool  PlayerInvisible = false;
    public static float CameraSpeed;
    
    public static void ParseVars(TextAsset asset)
    {
        var text = asset.text;
        
        var lines = text.Split('\n');
        
        foreach(var line in lines)
        {
            if(String.IsNullOrEmpty(line))
            {
                continue;
            }

            var words = line.Split(' ');
            
            switch(words[0])
            {
                case nameof(PlayerInvisible):
                {
                    PlayerInvisible = Convert.ToBoolean(Int32.Parse(words[1]));
                }
                break;
                
                case nameof(CameraSpeed):
                {
                    CameraSpeed = Single.Parse(words[1]);
                }
                break;
            }
        }
    }
}