using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CClearCoolDownTrifle:ITrifle
{
    private string ChainName { get; set; }
    public CClearCoolDownTrifle()
        : base(false)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        ChainName = kXmlRead.Str("chainName");
    }

    public override void DoTrifle(CSkillEvent ev)
    {
        CSkillChain chain = ev.Owner.SkillComp.GetSkillChainByName(ChainName);
        chain.ClearCoolDown();
    }


#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("¼¼ÄÜÁ´Ãû:", GUILayout.Width(60));
        ChainName = EditorGUILayout.TextField(ChainName);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }

    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "ClearCoolDown");
        trifle.SetAttribute("chainName", ChainName);
        parent.AppendChild(trifle);
    }
#endif
}
