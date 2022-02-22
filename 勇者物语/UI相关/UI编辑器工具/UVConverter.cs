using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class UVConvertor : MonoBehaviour
{
    public static Vector2[] s_uv;

    [MenuItem("ML2Editor/UVConverter")]
    static void ConvertUIFromOldToNew()
    {
        GameObject asset = GameObject.Find("totempiller_02") as GameObject;
        int index = 0;
        foreach (MeshFilter meshFilter in asset.GetComponentsInChildren(typeof(MeshFilter)))
        {
            Vector2[] uvs = meshFilter.sharedMesh.uv2;
            for (int i = 0; i < uvs.Length; i++)
            {
                index++;
                Debug.Log("index = " + index + "uv2.<u, v> = " + uvs[i].x + " " + uvs[i].y);
                uvs[i].x = 0.5f;
                uvs[i].y = 0.5f;
            }
            meshFilter.sharedMesh.uv2 = uvs;
        }

    }

}
