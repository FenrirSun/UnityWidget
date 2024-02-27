using System.Reflection;
using UnityEditor;
using UnityEngine;
using ZLib;
using UnityEngine.UI;
using System.Collections.Generic;


[CanEditMultipleObjects]
[CustomEditor(typeof(UIObject))]
public class UIObjectEditor : Editor
{

    UIObject m_object;
    BaseUIWindowData uiData;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        m_object = target as UIObject;
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("读取脚本"))
        {
            if (m_object.target == null)
            {
                Debug.Log("请拖拽要读取的脚本到target!!!!");
                return;
            }

            //设置依赖关系
            uiData = m_object.transform.GetComponent<BaseUIWindowData>();
            if (uiData!=null)
            {
                uiData.ImageDependenciesPathName.Clear();
                uiData.ImageDependenciesOverrideSpriteName.Clear();
                uiData.ImageDependenciesImage.Clear();
                
                uiData.RawImageDependenciesPathName.Clear();
                uiData.RawImageDependenciesImage.Clear();

                chishiCount = 0;
                getTransform(m_object.transform);
            }

            m_object.ParseConection(m_object.target);
        }
        if (GUILayout.Button("还原脚本"))
        {
            if (m_object.target == null)
            {
                m_object.target = m_object.gameObject.GetComponent(m_object.targetClassName);
            }
            if (m_object.target == null)
            {
                Assembly assembly = Assembly.Load(m_object.targetAssemblyName);
                m_object.target = m_object.gameObject.AddComponent(assembly.GetType(m_object.targetName));
                //m_object.target = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(m_object.gameObject, "Assets/Built-in/Tools/Editor/UIObject/UIObjectEditor.cs (27,30)", m_object.targetName);
                //m_bject.target = m_bject.gameObject.AddComponent(m_bject.targetType);
            }
            if (m_object.target == null)
            {
                Debug.LogError("请拖拽要原来的脚本到target!!!!");
                return;
            }

            
            

            m_object.CreateConnection(m_object.target);

            //测试依赖关系
            uiData = m_object.transform.GetComponent<BaseUIWindowData>();
            //int k = 0;
            if (uiData != null)
            {

                for (int i = 0; i < uiData.ImageDependenciesPathName.Count; i++)
                {
                    //Debug.LogError(uiData.ImageDependenciesImage[i] + "   " + uiData.ImageDependenciesPathName[i] + "   " + uiData.ImageDependenciesOverrideSpriteName[i] + "  ===============" + i);
                }

                for (int i = 0; i < uiData.RawImageDependenciesPathName.Count; i++)
                {
                    //Debug.LogError(uiData.RawImageDependenciesImage[i] + "   " + uiData.RawImageDependenciesPathName[i] +  "  -----------" + i);
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    int chishiCount = 0;
    void getTransform(Transform tr)
    {
        Image image = tr.GetComponent<Image>();
        if (image != null && image.overrideSprite != null)
        {
            uiData.ImageDependenciesPathName.Add(image.mainTexture.name);
            uiData.ImageDependenciesOverrideSpriteName.Add(image.overrideSprite.name);
            uiData.ImageDependenciesImage.Add(image);
        }

        RawImage rawImage = tr.GetComponent<RawImage>();
        if (rawImage != null && rawImage.mainTexture != null && rawImage.texture!=null)
        {
            uiData.RawImageDependenciesPathName.Add(rawImage.texture.name);
            uiData.RawImageDependenciesImage.Add(rawImage);
            //Debug.LogError(rawImage);
        }

        chishiCount++;
        //Debug.LogError(chishiCount + "   " + tr.name);
        for (int i = 0; i < tr.childCount; i++)
        {
            Transform tt = tr.GetChild(i);
            if (tt != null)
            {
                getTransform(tt);
            }
        }
    }

}
