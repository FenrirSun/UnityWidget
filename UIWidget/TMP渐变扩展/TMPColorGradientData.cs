using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = FileName, menuName = "ScriptableObjects/TMPColorGradientData", order = 5)]
public class TMPColorGradientData: ScriptableObject
{
    public const string FileName = "TMPColorGradientData";
    
    [ListDrawerSettings(CustomAddFunction = "CreateNewGradient")]
    public List<GradientInfo> colorGradients = new List<GradientInfo>();
    
    private GradientInfo CreateNewGradient()
    {
        GradientInfo result = new GradientInfo();
        result.id = "gradient name";
        result.color = new Gradient();
        
        return result;
    }

    [Serializable]
    public class GradientInfo
    {
        public string id;
        public Gradient color;
    }
}
