using System;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
public class CSavePosTrifle : ITrifle
{
    public int Index { get; private set; }
    public CSavePosTrifle()
        : base(true)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Index = kXmlRead.Int("posIndex");
    }

    public override void DoTrifle(CSkillEvent ev)
    {
        ev.Skill.Chain.SavePos(Index, ev.Pos);
    }

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("保存事件位置:", GUILayout.Width(120));

        EditorGUILayout.LabelField("保留位置信息的EVENT下标:", GUILayout.Width(60));
        Index = EditorGUILayout.IntField(Index);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }
    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "SaveDir");
        trifle.SetAttribute("dirIndex", Index.ToString());
        parent.AppendChild(trifle);
    }
#endif
}