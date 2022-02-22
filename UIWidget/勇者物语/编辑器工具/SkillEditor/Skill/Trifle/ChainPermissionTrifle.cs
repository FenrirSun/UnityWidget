// SHANDA GAMES PROPRIETARY INFORMATION
//
// This software is supplied under the terms of a license agreement or
using System;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
public class CChainPermissionTrifle:ITrifle
{
    private float DurationSeconds{get;set;}

    public CChainPermissionTrifle():base(false)
    {

    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        DurationSeconds = kXmlRead.Float("durationSeconds");
    }

    public override void DoTrifle(CSkillEvent ev)
    {
        ev.Skill.Chain.PhaseAck(ev.Skill.Xml.PhaseIndex, DurationSeconds);
    }

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("下一技能持续有效时间:", GUILayout.Width(120));
        EditorGUILayout.LabelField("持续时间:", GUILayout.Width(60));
        DurationSeconds = EditorGUILayout.FloatField(DurationSeconds);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }
    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "ChainPermission");
        trifle.SetAttribute("durationSeconds", DurationSeconds.ToString());
        parent.AppendChild(trifle);
    }
#endif
}