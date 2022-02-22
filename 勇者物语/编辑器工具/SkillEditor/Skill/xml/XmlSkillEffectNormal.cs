using System;
using System.Xml;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif
public class CXmlSkillEffectNormal
{
    //public string Name { get; private set; }
    public CXmlSkillValueOfLvl<string> Name;
    public bool IsLoop { get; private set; }
    public float DelaySeconds { get; private set; }
    //public float Scale { get; private set; }
    public CXmlSkillValueOfLvl<float> Scale;
    //public float LifeSeconds { get; private set; }
    public CXmlSkillValueOfLvl<float> LifeSeconds;
    public CXmlSkillPos XmlPos { get; private set; }
    public CXmlSkillDir XmlDir { get; private set; }

    public CXmlSkillEffectNormal(){}
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        //Name = kXmlRead.Str("name");
        Name = new CXmlSkillValueOfLvl<string>(ele, "name", "");
        IsLoop = kXmlRead.Bool("loop");
        DelaySeconds = kXmlRead.Float("delaySeconds");
        //Scale = kXmlRead.Float("delaySeconds");
        Scale = new CXmlSkillValueOfLvl<float>(ele, "scale", 1.0f);
        //LifeSeconds = kXmlRead.Float("lifeSeconds", 999.0f);
        LifeSeconds = new CXmlSkillValueOfLvl<float>(ele, "lifeSeconds", 999.0f);

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "pos":
                    {
                        string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
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
    public void Draw()
    {
        GUILayout.BeginVertical("box");
        m_Draw = EditorGUILayout.Foldout(m_Draw, "EffectNormal");

        if (m_Draw)
        {
            if (XmlPos == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                XmlPos = new CXmlSkillPos();
                XmlPos.InitEditor(szPermission, "特效位置");
            }
            else
            {
                XmlPos.SetName("特效位置");
            }
            if (XmlDir == null)
            {
                string[] szPermission = { "self", "target", "ground", "twoPoint", "event" };
                XmlDir = new CXmlSkillDir();
                XmlDir.InitEditor(szPermission, "特效方向");
            }
            else
            {
                XmlDir.SetName("特效方向");
            }
            if (Name == null)
            {
                Name = new CXmlSkillValueOfLvl<string>("特效名字","");
            }
            Name.SetName("特效名字");

            if (Scale == null)
            {
                Scale = new CXmlSkillValueOfLvl<float>("特效大小", 1.0f);
            }
            Scale.SetName("特效大小");

            if (LifeSeconds == null) 
            {
                LifeSeconds = new CXmlSkillValueOfLvl<float>("生存时间", 999.0f);
            }
            LifeSeconds.SetName("生存时间");

            Name.Draw();
            Scale.Draw();
            LifeSeconds.Draw();

            XmlPos.Draw();
            XmlDir.Draw();

        }
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "effect")
    {
        XmlElement effect = doc.CreateElement(name);

        effect.SetAttribute("loop", IsLoop.ToString());
        effect.SetAttribute("delaySeconds", DelaySeconds.ToString());

        if(Name != null)
        {
            Name.Export(doc, effect, "name");
        }
        if (Scale != null)
        {
            Scale.Export(doc, effect, "scale");
        }
        if (LifeSeconds != null)
        {
            LifeSeconds.Export(doc, effect, "lifeSeconds");
        }
        if (XmlPos != null)
        {
            XmlPos.Export(doc, effect);
        }
        if(XmlDir != null)
        {
            XmlDir.Export(doc, effect);
        }
        parent.AppendChild(effect);
    }
#endif
}