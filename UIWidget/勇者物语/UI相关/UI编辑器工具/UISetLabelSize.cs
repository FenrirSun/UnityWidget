using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class UISetLabelSize : EditorWindow
{
    private float Size = 1.0f;

    [MenuItem("UITool/批量设置UI大小")]
    static void SetColor()
    {
        EditorWindow.GetWindow<UISetLabelSize>();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        Size = EditorGUILayout.Slider("缩放倍数:", Size, 0f, 10f);

        if (GUILayout.Button("Set scale"))
        {
            foreach (Transform tran in Selection.transforms)
            {
                tran.localScale = new Vector3(tran.localScale.x * Size,
                     tran.localScale.y * Size, tran.localScale.z);
            }
        }

        if (GUILayout.Button("Set Widget Size"))
        {
            foreach (Transform tran in Selection.transforms)
            {
                UIWidget widget = tran.GetComponent<UIWidget>();
                if (widget != null)
                {
                    widget.width = (int)(widget.width * Size);
                    widget.height = (int)(widget.height * Size);
                }
            }
        }

        if (GUILayout.Button("Set Default"))
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                go.GetComponent<UISprite>().MakePixelPerfect();
            }
        }
        EditorGUILayout.EndVertical();

    }

}
