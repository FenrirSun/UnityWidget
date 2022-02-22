using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class UISetLabelSize : EditorWindow
{
    private float Size = 1.0f;

    [MenuItem("TNN/设置UI缩放")]
    static void SetColor()
    {
        EditorWindow.GetWindow<UISetLabelSize>();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        Size = EditorGUILayout.Slider("设置缩放倍数:", Size, 0f, 100f);
        if (GUILayout.Button("设置"))
        {
            Selection.activeGameObject.transform.localScale = new Vector3(Selection.activeGameObject.transform.localScale.x * Size,
                Selection.activeGameObject.transform.localScale.y * Size, Selection.activeGameObject.transform.localScale.z);
        }
        if (GUILayout.Button("恢复默认大小"))
        {
            Selection.activeGameObject.GetComponent<O_UISprite>().MakePixelPerfect();
        }
        EditorGUILayout.EndVertical();

    }

}
