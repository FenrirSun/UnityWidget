using UnityEngine;
using System.Collections;
using UnityEditor;
using Code.External.Engine;

namespace Code.External.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AnimationCurveAsset))]
    public class AnimationCurveAssetEditor : UnityEditor.Editor
    {

        [MenuItem("Assets/Create/Animation Curve")]
        public static void CreateAsset()
        {
            AssetUtility.CreateAsset<AnimationCurveAsset>();
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            SerializedProperty sp = serializedObject.FindProperty("curve");

            GUILayout.BeginHorizontal();
            GUILayout.Space(-10);
            EditorGUILayout.PropertyField(sp, new GUIContent(""), false, GUILayout.Height(Mathf.Min(Screen.width * .5f, Screen.height)));
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }



    }
}