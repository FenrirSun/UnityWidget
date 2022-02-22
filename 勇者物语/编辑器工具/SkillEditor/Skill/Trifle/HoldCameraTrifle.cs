using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CHoldCameraTrifle:ITrifle
{
    private bool Value = true;
    public CHoldCameraTrifle()
        : base(true)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Value = kXmlRead.Bool("value");
    }

    public override void DoTrifle(CSkillEvent ev)
    {
        GameApp.GetCameraManager().CanUpdateCamera(!Value);
    }


#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("固定相机:", GUILayout.Width(120));

        EditorGUILayout.LabelField("是否固定:", GUILayout.Width(60));
        Value = EditorGUILayout.Toggle(Value);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }
    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "HoldCamera");
        trifle.SetAttribute("value", Value.ToString());
        parent.AppendChild(trifle);
    }
#endif
}