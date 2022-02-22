using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillPosCircle
{
    public CXmlSkillPos XmlPos { get; private set; }
    public CXmlSkillDir XmlDir { get; private set; }
    public float R { get; private set; }
    public float AngleV0 { get; private set; }
    public float AngleOffset { get; private set; }
    public float LifeSeconds { get; private set; }

    public CXmlSkillPosCircle() { }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        R = kXmlRead.Float("r");
        AngleV0 = kXmlRead.Float("angleV0");
        AngleOffset = kXmlRead.Float("angleOffset");
        LifeSeconds = kXmlRead.Float("lifeSeconds");

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "pos":
                    {
                        string[] szPermission = { "self", "target", "ground", "event" ,"p2pPos"};
                        XmlPos = new CXmlSkillPos();
                        XmlPos.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
                case "dir":
                    {
                        string[] szPermission = { "self", "target", "ground", "twoPoint", "event" };
                        XmlDir = new CXmlSkillDir();
                        XmlDir.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
            }
        }

    }

#if UNITY_EDITOR
    private bool m_Draw = false;
    public bool m_Effective = false;

    public void Draw()
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        m_Effective = EditorGUILayout.Toggle(m_Effective, GUILayout.Width(10));
        m_Draw = EditorGUILayout.Foldout(m_Draw, "PosCircle");
        GUILayout.EndHorizontal();

        if (m_Draw)
        {
            if (XmlPos == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos" };
                XmlPos = new CXmlSkillPos();
                XmlPos.InitEditor(szPermission, "圆心");
            }

            if (XmlDir == null)
            {
                string[] szPermission = { "self", "target", "ground", "twoPoint", "event" };
                XmlDir = new CXmlSkillDir();
                XmlDir.InitEditor(szPermission, "旋转起始方向");
            }

            XmlPos.Draw();
            XmlDir.Draw();

            GUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField("半径:", GUILayout.Width(40));
            R = EditorGUILayout.FloatField(R);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("角速度:", GUILayout.Width(40));
            AngleV0 = EditorGUILayout.FloatField(AngleV0);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("角度偏移:", GUILayout.Width(50));
            AngleOffset = EditorGUILayout.FloatField(AngleOffset);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("生存时间:", GUILayout.Width(50));
            LifeSeconds = EditorGUILayout.FloatField(LifeSeconds);
            EditorGUILayout.Space();

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "circlePos")
    {
        XmlElement circlePos = doc.CreateElement(name);

        circlePos.SetAttribute("r", R.ToString());
        circlePos.SetAttribute("angleV0", AngleV0.ToString());
        circlePos.SetAttribute("angleOffset", AngleOffset.ToString());
        circlePos.SetAttribute("lifeSeconds", LifeSeconds.ToString());

        if (XmlPos != null) { XmlPos.Export(doc, circlePos); }
        if (XmlDir != null) { XmlDir.Export(doc, circlePos); }
        parent.AppendChild(circlePos);
    }
#endif
}