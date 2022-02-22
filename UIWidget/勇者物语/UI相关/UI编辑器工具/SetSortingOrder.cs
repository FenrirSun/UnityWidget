using UnityEngine;
using System.Collections;
using UnityEditor;
public class SetSortingOrder : EditorWindow
{

    [MenuItem("TNN/设置SortingOrder渲染层次")]
    static void SetRendererLayer()
    {
        EditorWindow.GetWindow<SetSortingOrder>();
    }

    int layer = 3000;
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        layer = EditorGUILayout.IntField("设置渲染层", layer);
        if (GUILayout.Button("设置"))
        {
            if (Selection.activeGameObject != null)
            {
                //ParticleSystem[] particles = Selection.activeGameObject.GetComponentsInChildren<ParticleSystem>();
                //for (int i = 0; i < particles.Length; i++)
                //{
                //    particles[i].renderer.sharedMaterial.renderQueue = layer;
                //}
                var list_render = Selection.activeGameObject.GetComponentsInChildren<Renderer>(true);
                foreach (var render in list_render)
                {
                    render.sortingOrder = layer;
                }

                var list_panel = Selection.activeGameObject.GetComponentsInChildren<O_UIPanel>(true);
                foreach (var panel in list_panel)
                {
                    //panel.sortingOrder = layer;
                    foreach (O_UIDrawCall drawcall in panel.drawCalls)
                    {
                        drawcall.renderer.sortingOrder = layer;
                    }
                }
            }
        }
        GUILayout.EndHorizontal();

        int nowLayer = 0;
        if (Selection.activeGameObject != null)
        {
            if (Selection.activeGameObject.GetComponent<Renderer>() != null)
            {
                nowLayer = Selection.activeGameObject.GetComponent<Renderer>().sortingOrder;
            }
            if (Selection.activeGameObject.GetComponent<O_UIPanel>() != null)
            {
                if (Selection.activeGameObject.GetComponent<O_UIPanel>().drawCalls.Count > 0)
                {
                    nowLayer = Selection.activeGameObject.GetComponent<O_UIPanel>().drawCalls[0].renderer.sortingOrder;
                }
            }
        }

        GUILayout.Label("当前层次：" + nowLayer.ToString());
    }
    
    
}
