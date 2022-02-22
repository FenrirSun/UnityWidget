using System;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
public class CSaveDirTrifle: ITrifle
{
    public int Index{get;private set;}

    public CSaveDirTrifle():base(true)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Index = kXmlRead.Int("dirIndex");
   }

    public override void DoTrifle(CSkillEvent ev)
    {
        ev.Skill.Chain.SaveDir(Index, ev.Dir);
    }

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("����ʱ�䷽��:", GUILayout.Width(120));

        EditorGUILayout.LabelField("����������Ϣ��EVENT�±�:", GUILayout.Width(60));
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