using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillPosP2P
{
    public CXmlSkillPos XmlPos1 { get; private set; }
    public CXmlSkillPos XmlPos2 { get; private set; }
    public float V0 { get; private set; }
    public float A { get; private set; }
    public float LifeSeconds { get; private set; }

    public CXmlSkillPosP2P() { }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        V0 = kXmlRead.Float("v0");
        A = kXmlRead.Float("a");
        LifeSeconds = kXmlRead.Float("lifeSeconds");

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "pos1":
                    {
                        string[] szPermission = { "self", "target", "ground", "event" };
                        XmlPos1 = new CXmlSkillPos();
                        XmlPos1.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
                case "pos2":
                    {
                        string[] szPermission = { "self", "target", "ground", "event" };
                        XmlPos2 = new CXmlSkillPos();
                        XmlPos2.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
            }
        }
        if (XmlPos1 == null || XmlPos2 == null)
        {
            Debug.LogError("p2pPos should have two pos!!");
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
        m_Draw = EditorGUILayout.Foldout(m_Draw, "PosP2P");
        GUILayout.EndHorizontal();

        if (m_Draw)
        {
            if (XmlPos1 == null)
            {
                string[] szPermission = { "self", "target", "ground", "event" };
                XmlPos1 = new CXmlSkillPos();
                XmlPos1.InitEditor(szPermission,"初始位置");
            }

            if(XmlPos2 == null)
            {
                string[] szPermission = { "self", "target", "ground", "event" };
                XmlPos2 = new CXmlSkillPos();
                XmlPos2.InitEditor(szPermission, "目标位置");
            }
            EditorGUILayout.Separator();
            XmlPos1.Draw();
            XmlPos2.Draw();
            GUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField("初速度:", GUILayout.Width(40));
            V0 = EditorGUILayout.FloatField(V0);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("加速度:", GUILayout.Width(40));
            A = EditorGUILayout.FloatField(A);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("生存时间:", GUILayout.Width(50));
            LifeSeconds = EditorGUILayout.FloatField(LifeSeconds);
            EditorGUILayout.Space();

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    public void InitEditor()
    {

    }

    public void Export(XmlDocument doc, XmlNode parent, string name = "p2pPos")
    {
        XmlElement p2pPos = doc.CreateElement(name);

        p2pPos.SetAttribute("v0", V0.ToString());
        p2pPos.SetAttribute("a", A.ToString());
        p2pPos.SetAttribute("lifeSeconds", LifeSeconds.ToString());

        if(XmlPos1 != null)
        {
            XmlPos1.Export(doc, p2pPos, "pos1");
        }

        if(XmlPos2 != null)
        {
            XmlPos2.Export(doc, p2pPos, "pos2");
        }
        parent.AppendChild(p2pPos);
    }
#endif
}