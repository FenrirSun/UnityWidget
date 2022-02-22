using System;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
public class CTerminateSkillTrifle : ITrifle
{
    private int SkillID{get;set;}
    public CTerminateSkillTrifle():base(true)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        SkillID = kXmlRead.Int("skillID");
    }

    public override void DoTrifle(CSkillEvent ev)
    {
        //ev.Skill.Chain.Comp.TerminateSkill(SkillID);
    }

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("中止技能:", GUILayout.Width(120));

        EditorGUILayout.LabelField("中止技能编号:", GUILayout.Width(80));
        SkillID = EditorGUILayout.IntField(SkillID);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }
    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "TerminateSkill");
        trifle.SetAttribute("skillID", SkillID.ToString());
        parent.AppendChild(trifle);
    }
#endif
}