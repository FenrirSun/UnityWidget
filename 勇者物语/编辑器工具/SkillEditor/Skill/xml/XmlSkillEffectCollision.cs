using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillEffectCollision
{
    public float V0{get;private set;}
    public float A{get;private set;}
    public int EndEventIndex { get; private set; }
    public int CollisionEventIndex { get; private set; }//碰撞后触发的event
    public string Name{get;private set;}
    public float DelaySeconds { get; private set; }
    public float LifeSeconds { get; private set; }
    public int DeadEventIndex { get; private set; }//lifeSeconds结束时触发的event
    public CXmlSkillValueOfLvl<float> Scale;
    public CXmlSkillPos XmlPos1 { get; private set; }
    public CXmlSkillPos XmlPos2{get;private set;}
    public CXmlSkillRange XmlRange { get; private set; }

    public CXmlSkillEffectCollision() { }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Name = kXmlRead.Str("name");
        DelaySeconds = kXmlRead.Float("delaySeconds");
        LifeSeconds = kXmlRead.Float("lifeSeconds",10000.0f);
        Scale = new CXmlSkillValueOfLvl<float>(ele, "scale", 1.0f);
        V0 = kXmlRead.Float("v0");
        A = kXmlRead.Float("a");
        EndEventIndex = kXmlRead.Int("endEventIndex", -1);
        DeadEventIndex = kXmlRead.Int("deadEventIndex", -1);
        CollisionEventIndex = kXmlRead.Int("collisionEventIndex", -1);

        foreach(XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "range":
                    {
                        XmlRange = new CXmlSkillRange();
                        XmlRange.Init(node as XmlElement);
                    }
                    break;
                case "pos1":
                    {
                        string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                        XmlPos1 = new CXmlSkillPos();
                        XmlPos1.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
                case "pos2":
                    {
                        string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                        XmlPos2 = new CXmlSkillPos();
                        XmlPos2.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
            }
        }
        if (XmlPos1 == null || XmlPos2 == null)
        {
            Debug.LogError("collision effect should have two pos!!");
        }
    }

#if UNITY_EDITOR
    private bool m_Draw = false;
    public void Draw()
    {
        GUILayout.BeginVertical("box");
        m_Draw = EditorGUILayout.Foldout(m_Draw, "EffectCollision");

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
            if(XmlRange == null)
            {
                XmlRange = new CXmlSkillRange();
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
            V0 = EditorGUILayout.FloatField(V0);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("加速度:", GUILayout.Width(40));
            A = EditorGUILayout.FloatField(A);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("延时:", GUILayout.Width(50));
            DelaySeconds = EditorGUILayout.FloatField(DelaySeconds);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("生存时间:", GUILayout.Width(50));
            LifeSeconds = EditorGUILayout.FloatField(LifeSeconds);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("特效结束事件ID:", GUILayout.Width(75));
            EndEventIndex = EditorGUILayout.IntField(EndEventIndex);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("碰撞事件ID:", GUILayout.Width(75));
            CollisionEventIndex = EditorGUILayout.IntField(CollisionEventIndex);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Life结束事件ID:", GUILayout.Width(75));
            DeadEventIndex = EditorGUILayout.IntField(DeadEventIndex);
            EditorGUILayout.Space();

            

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            Scale.Draw();
            XmlPos1.Draw();
            XmlPos2.Draw();
            XmlRange.Draw();
        }
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "collisionEffect")
    {
        XmlElement collisionEffect = doc.CreateElement(name);

        collisionEffect.SetAttribute("name", Name);
        collisionEffect.SetAttribute("v0", V0.ToString());
        collisionEffect.SetAttribute("a", A.ToString());
        collisionEffect.SetAttribute("lifeSeconds", LifeSeconds.ToString());
        collisionEffect.SetAttribute("deadEventIndex", DeadEventIndex.ToString());
        collisionEffect.SetAttribute("collisionEventIndex", CollisionEventIndex.ToString());
        collisionEffect.SetAttribute("endEventIndex", EndEventIndex.ToString());
        collisionEffect.SetAttribute("delaySeconds", DelaySeconds.ToString());

        if(Scale != null)
        {
            Scale.Export(doc, collisionEffect, "scale");
        }
        if (XmlPos1 != null)
        {
            XmlPos1.Export(doc, collisionEffect, "pos1");
        }
        if (XmlPos2 != null)
        {
            XmlPos2.Export(doc, collisionEffect, "pos2");
        }
        if (XmlRange != null)
        {
            XmlRange.Export(doc,collisionEffect);
        }
        parent.AppendChild(collisionEffect);
    }
#endif
}