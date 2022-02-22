using System;
using System.Xml;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif
public class CProduceBubbleTrifle: ITrifle
{
    private float DurationSeconds { get; set; }
    private int TextID { get; set; }

    public CProduceBubbleTrifle()
        : base(false)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        DurationSeconds = kXmlRead.Float("durationSeconds");
        TextID = kXmlRead.Int("textID");
   }

    public override void DoTrifle(CSkillEvent ev)
    {
        GameApp.GetUIManager().ProduceBubble(O_Localization.instance.GetText("PyjUILocal", TextID), ev.Owner.MoveComp.transform,DurationSeconds);
    }

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("文字tips:", GUILayout.Width(120));

        EditorGUILayout.LabelField("冒泡文字ID:", GUILayout.Width(60));
        TextID = EditorGUILayout.IntField(TextID);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("冒泡文字时间:", GUILayout.Width(80));
        DurationSeconds = EditorGUILayout.FloatField(DurationSeconds);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }
    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "ProduceBubble");
        trifle.SetAttribute("durationSeconds", DurationSeconds.ToString());
        trifle.SetAttribute("textID", TextID.ToString());
        parent.AppendChild(trifle);
    }
#endif
}