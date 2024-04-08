using System.IO;
using UnityEngine;
using System;
using System.Reflection;

public static class Vars
{    
    public static bool  PlayerInvisible = false;
    public static float CameraSpeed;
    
    public static void ParseVars(TextAsset asset)
    {
        var text   = asset.text;
        var lines  = text.Split('\n');
        var fields = typeof(Vars).GetFields();
        
        foreach(var line in lines)
        {
            if(String.IsNullOrEmpty(line))
            {
                continue;
            }

            var words = line.Split(' ');
            
            foreach(var field in fields)
            {
                if(field.Name == words[0])
                {
                    SetValueByType(field, words[1]);
                    break;
                }
            }
        }
    }
    
    private static void SetValueByType(FieldInfo field, string value)
    {
        switch(field.FieldType.ToString())
        {
            case "System.Boolean":
            {
                field.SetValue(null, Convert.ToBoolean(Int32.Parse(value)));
            }
            break;
            
            case "System.Single":
            {
                field.SetValue(null, Single.Parse(value));
            }
            break;
            
            default:
                Debug.LogError($"Cannot set field with type: {field.FieldType.ToString()}");
                break;
        }
    }
}