using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理文字渐变图和材质球
/// </summary>
public class TMPColorGradientManager
{
    public static TMPColorGradientData gradientData;

    // 缓存新建的贴图和材质球，gradientId, mat name, mat
    //private static Dictionary<string, Dictionary<string, Material>> gradientMaterialDic = new Dictionary<string, Dictionary<string, Material>>();
    private static Dictionary<string, Texture2D> gradientTexDic = new Dictionary<string, Texture2D>();

    public static int ColorGradientTexID = Shader.PropertyToID("_ColorGradientTex");
    public static int UseGradientID = Shader.PropertyToID("_UseColorGradient");

    public static List<TMPColorGradientData.GradientInfo> GetColorGradients()
    {
        if (!gradientData) {
            gradientData = Resources.Load<TMPColorGradientData>("Data/TMPColorGradientData");
        }
        
        if (!gradientData)        {
            return new List<TMPColorGradientData.GradientInfo>();
        }
        
        return gradientData.colorGradients;
    }

    // public static Material GetGradientMat(string gradientId, Material oriMat)
    // {
    //     if (!gradientMaterialDic.ContainsKey(gradientId))
    //     {
    //         gradientMaterialDic.Add(gradientId, new Dictionary<string, Material>());
    //     }
    //
    //     if (!gradientMaterialDic[gradientId].ContainsKey(oriMat.name) || !gradientMaterialDic[gradientId][oriMat.name])
    //     {
    //         var newMat = new Material(oriMat);
    //         newMat.SetTexture(ColorGradientTexID, GetTexture(gradientId));
    //         newMat.SetFloat(UseGradientID, 1);
    //         gradientMaterialDic[gradientId][oriMat.name] = newMat;
    //     }
    //
    //     return gradientMaterialDic[gradientId][oriMat.name];
    // }
    
    const int width = 1;
    const int height = 128;
    public static Texture2D GetTexture(string gradientId)
    {
        if (gradientTexDic.ContainsKey(gradientId))
        {
            if (gradientTexDic[gradientId])
                return gradientTexDic[gradientId];
            else
                gradientTexDic.Remove(gradientId);
        }

        var gradient = GetColorGradients().Find(g => g.id == gradientId);
        if (gradient != null)
        {
            Texture2D gradientTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            gradientTex.wrapMode = TextureWrapMode.Clamp;
            gradientTex.filterMode = FilterMode.Bilinear;
            
            for (int i = 0; i < height; i++)
            {
                gradientTex.SetPixel(0, i, gradient.color.Evaluate((float) i / (float) height));
            }
            gradientTex.Apply(false);
            
            gradientTexDic.Add(gradientId, gradientTex);
            return gradientTex;
        }

        return null;
    }
    
}