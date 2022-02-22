using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillEffectParabola
{
    public float H0{get;private set;}
    public float G{get;private set;}
    public int EndEventIndex{get;private set;}
    public string Name{get;private set;}
    public float DelaySeconds{get;private set;}
    public CXmlSkillValueOfLvl<float> Scale;
    public CXmlSkillPos XmlPos1 { get; private set; }
    public CXmlSkillPos XmlPos2 { get; private set; }

    public CXmlSkillEffectParabola() { }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Name = kXmlRead.Str("name");
        DelaySeconds = kXmlRead.Float("delaySeconds");
        Scale = new CXmlSkillValueOfLvl<float>(ele, "scale", 1.0f);
        H0 = kXmlRead.Float("h0");
        G = kXmlRead.Float("g");
        EndEventIndex = kXmlRead.Int("endEventIndex",-1);

        foreach(XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
            case "pos1":
                {
                    string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                    XmlPos1 = new CXmlSkillPos();
                    XmlPos1.Init(node as XmlElement,szPermission,szPermission.Length);
                }
                    break;
            case "pos2":
                {
                    string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                    XmlPos2 = new CXmlSkillPos();
                    XmlPos2.Init(node as XmlElement,szPermission,szPermission.Length);
                }
                    break;
            }
        }
        if (XmlPos1 == null || XmlPos2 == null)
        {
            Debug.LogError("Parabola effect should have two pos!!");
        }
    }

#if UNITY_EDITOR
    private bool m_Draw = false;
    public void Draw()
    {
        GUILayout.BeginVertical("box");
        m_Draw = EditorGUILayout.Foldout(m_Draw, "EffectParabola");

        if (m_Draw)
        {
            if (XmlPos1 == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                XmlPos1 = new CXmlSkillPos();
                XmlPos1.InitEditor(szPermission, "初始位置");
            }
            else
            {
                XmlPos1.SetName("初始位置");
            }
            if (XmlPos2 == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                XmlPos2 = new CXmlSkillPos();
                XmlPos2.InitEditor(szPermission, "目标位置");
            }
            else
            {
                XmlPos2.SetName("目标位置");
            }
            if (Scale == null)
            {
                Scale = new CXmlSkillValueOfLvl<float>("scale", 1.0f);
            }

            Scale.SetName("特效大小");
            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("特效名:", GUILayout.Width(40));
            Name = EditorGUILayout.TextField(Name);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("初速度:", GUILayout.Width(40));
            H0 = EditorGUILayout.FloatField(H0);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("重力:", GUILayout.Width(30));
            G = EditorGUILayout.FloatField(G);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("延时:", GUILayout.Width(50));
            DelaySeconds = EditorGUILayout.FloatField(DelaySeconds);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("结束事件ID:", GUILayout.Width(65));
            EndEventIndex = EditorGUILayout.IntField(EndEventIndex);
            EditorGUILayout.Space();

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            Scale.Draw();
            XmlPos1.Draw();
            XmlPos2.Draw();
        }
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "parabolaEffect")
    {
        XmlElement parabolaEffect = doc.CreateElement(name);

        parabolaEffect.SetAttribute("name", Name);
        parabolaEffect.SetAttribute("h0", H0.ToString());
        parabolaEffect.SetAttribute("g", G.ToString());
        parabolaEffect.SetAttribute("endEventIndex", EndEventIndex.ToString());
        parabolaEffect.SetAttribute("delaySeconds", DelaySeconds.ToString());

        if (Scale != null)
        {
            Scale.Export(doc, parabolaEffect, "scale");
        }
        if (XmlPos1 != null)
        {
            XmlPos1.Export(doc, parabolaEffect, "pos1");
        }
        if (XmlPos2 != null)
        {
            XmlPos2.Export(doc, parabolaEffect, "pos2");
        }
        parent.AppendChild(parabolaEffect);
    }
#endif
}