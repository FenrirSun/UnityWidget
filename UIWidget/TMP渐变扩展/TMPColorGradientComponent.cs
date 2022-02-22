using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPColorGradientComponent : MonoBehaviour
{
    [OnValueChanged("OnChangeGradientInfo")]
    [ValueDropdown("GetListGradientInfo")]
    public string gradientId;

    [OnValueChanged("UpdateTexture")]
    public Gradient gradient;
    
    private TextMeshProUGUI _TMPUgui;

    private TextMeshProUGUI TMPUgui
    {
        get
        {
            if (!_TMPUgui) {
                _TMPUgui = GetComponent<TextMeshProUGUI>();
            }
            return _TMPUgui;
        }
    }
    private Material mat;

    void Update()
    {
        if (mat) return;
        SetGradientTexture();
    }

    private void SetGradientTexture()
    {
        if (Application.isPlaying)
        {
            // var canvasRenderer = TMPUgui.canvasRenderer;
            if (TMPUgui.fontSharedMaterial)
            {
                TMPUgui.enableVertexGradient = true;
                VertexGradient gradient = new VertexGradient();
                gradient.bottomLeft = Color.black;
                gradient.bottomRight = Color.black;
                gradient.topLeft = Color.white;
                gradient.topRight = Color.white;
                TMPUgui.colorGradient = gradient;

                mat = new Material(TMPUgui.fontSharedMaterial);
                TMPUgui.fontSharedMaterial = mat;
                // canvasRenderer.SetMaterial(mat, 0);

                OnChangeFont();
            }
        }
    }

    public void OnChangeFont()
    {
        if (!TMPUgui || !mat)
            return;

        mat.SetTexture(TMPColorGradientManager.ColorGradientTexID, TMPColorGradientManager.GetTexture(gradientId));
        mat.SetFloat(TMPColorGradientManager.UseGradientID, 1);
        OnSetSubMesh();
    }

    private void OnSetSubMesh()
    {
        var childNum = transform.childCount;
        for (int i = 0; i < childNum; ++i)
        {
            var child = transform.GetChild(i);
            if (child.name.Contains("TMP SubMeshUI"))
            {
                var subMesh = child.GetComponent<TMP_SubMeshUI>();
                if (subMesh && subMesh.sharedMaterial)
                {
                    subMesh.sharedMaterial.SetTexture(TMPColorGradientManager.ColorGradientTexID, TMPColorGradientManager.GetTexture(gradientId));
                    subMesh.sharedMaterial.SetFloat(TMPColorGradientManager.UseGradientID, 1);
                }
            }
        }
    }

    private void UpdateTexture()
    {
        if (!Application.isPlaying || !mat) return;
        int height = 128;
        Texture2D gradientTex = (Texture2D)mat.GetTexture(TMPColorGradientManager.ColorGradientTexID);
        if (gradientTex)
        {
            for (int i = 0; i < height; i++)
            {
                gradientTex.SetPixel(0, i, gradient.Evaluate((float) i / (float) height));
            }
            gradientTex.Apply(false);
        }
    }

    private void OnDestroy()
    {
        if(mat)
            Destroy(mat);
        mat = null;
    }
    
#if UNITY_EDITOR

    private string lastSelectId;
    private void OnValidate()
    {
        if (!Application.isPlaying) {
            if (lastSelectId != gradientId)
            {
                OnChangeGradientInfo();
                lastSelectId = gradientId;
            }
        }
        else {
            UpdateTexture();
        }
    }
    
    private void OnChangeGradientInfo()
    {
        var colorInfos = TMPColorGradientManager.GetColorGradients();
        if (colorInfos != null)
        {
            foreach (var info in colorInfos)
            {
                if (info.id == gradientId)
                {
                    gradient = info.color;
                    break;
                }
            }
        }
    }
    
    [Button("保存颜色", ButtonSizes.Medium)]
    public void SaveColor()
    {
        if (TMPColorGradientManager.gradientData != null)
        {
            foreach (var info in TMPColorGradientManager.gradientData.colorGradients)
            {
                if (info.id == gradientId)
                {
                    info.color = gradient;
                    break;
                }
            }
            EditorUtility.SetDirty(TMPColorGradientManager.gradientData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            //Debug.LogError("save tmp gradient color");
        }
    }
    
    [NonSerialized]
    [ShowInInspector]
    [FoldoutGroup("创建新颜色")]
    public string newColorName;
    
    [FoldoutGroup("创建新颜色")]
    [Button("创建", ButtonSizes.Medium)]
    public void CreateNewColor()
    {
        if (string.IsNullOrEmpty(newColorName))
        {
            EditorUtility.DisplayDialog("提示", "请出入新颜色名称", "OK");
            return;
        }
        
        var colorInfos = TMPColorGradientManager.GetColorGradients();
        if (colorInfos != null)
        {
            foreach (var info in colorInfos)
            {
                if (info.id == newColorName)
                {
                    EditorUtility.DisplayDialog("提示", "颜色名称重复", "OK");
                    return;
                }
            }
            
            TMPColorGradientData.GradientInfo gradInfo = new TMPColorGradientData.GradientInfo();
            gradInfo.id = newColorName;
            var newGrad = new Gradient();
            newGrad.mode = gradient.mode;
            newGrad.SetKeys(gradient.colorKeys, gradient.alphaKeys);
            gradient = newGrad;
            gradInfo.color = gradient;
            colorInfos.Add(gradInfo);
            gradientId = gradInfo.id;
            newColorName = string.Empty;
            
            EditorUtility.SetDirty(TMPColorGradientManager.gradientData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    
    [Button("查看颜色库", ButtonSizes.Medium)]
    public void OpenSerializeData()
    {
        var data = Resources.Load<TMPColorGradientData>("Data/" + TMPColorGradientData.FileName);
        EditorGUIUtility.PingObject(data.GetInstanceID());
    }
    
    // inspector 用
    private IEnumerable<string> GetListGradientInfo()
    {
        List<string> result = new List<string>();
        var colorInfos = TMPColorGradientManager.GetColorGradients();
        if (colorInfos != null)
        {
            foreach (var info in colorInfos)
            {
                result.Add(info.id);
            }
        }
    
        return result;
    }
    
#endif
    
}