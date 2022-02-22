using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillEffectP2P
{
    public float V0{get;private set;}
    public float A{get;private set;}
    public int EndEventIndex{get;private set;}
    public string Name{get;private set;}
    public float DelaySeconds{get;private set;}
    public CXmlSkillValueOfLvl<float> Scale;
    public CXmlSkillPos XmlPos1 { get; private set; }
    public CXmlSkillPos XmlPos2{get;private set;}

    public CXmlSkillEffectP2P(){}
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Name = kXmlRead.Str("name");
        DelaySeconds = kXmlRead.Float("delaySeconds");
        Scale = new CXmlSkillValueOfLvl<float>(ele, "scale", 1.0f);
        V0 = kXmlRead.Float("v0");
        A = kXmlRead.Float("a");
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
            Debug.LogError("p2peffect should have two pos!!");
        }
    }

#if UNITY_EDITOR
    private bool m_Draw = false;
    public void Draw()
    {
        GUILayout.BeginVertical("box");
        m_Draw = EditorGUILayout.Foldout(m_Draw, "EffectP2P");

        if (m_Draw)
        {
            if (XmlPos1 == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                XmlPos1 = new CXmlSkillPos();
                XmlPos1.InitEditor(szPermission, "初始位置");
            }
            XmlPos1.SetName("初始位置");
            if (XmlPos2 == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                XmlPos2 = new CXmlSkillPos();
                XmlPos2.InitEditor(szPermission, "目标位置");
            }
            XmlPos2.SetName("目标位置");
            if(Scale == null)
            {
                Scale = new CXmlSkillValueOfLvl<float>("scale", 1.0f);
            }
            GUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField("特效名:", GUILayout.Width(40));
            Name = EditorGUILayout.TextField(Name);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("初速度:", GUILayout.Width(40));
            V0 = EditorGUILayout.FloatField(V0);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("加速度:", GUILayout.Width(40));
            A = EditorGUILayout.FloatField(A);
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
    public void Export(XmlDocument doc, XmlNode parent, string name = "p2pEffect")
    {
        XmlElement p2pEffect = doc.CreateElement(name);

        p2pEffect.SetAttribute("name", Name);
        p2pEffect.SetAttribute("v0", V0.ToString());
        p2pEffect.SetAttribute("a", A.ToString());
        p2pEffect.SetAttribute("endEventIndex", EndEventIndex.ToString());
        p2pEffect.SetAttribute("delaySeconds", DelaySeconds.ToString());

        if (Scale != null) 
        { 
            Scale.Export(doc,p2pEffect,"scale");
        }
        if (XmlPos1 != null)
        {
            XmlPos1.Export(doc,p2pEffect,"pos1");
        }
        if (XmlPos2 != null)
        {
            XmlPos2.Export(doc, p2pEffect, "pos2");
        }
        parent.AppendChild(p2pEffect);
    }
#endif
}