using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CBlurTrifle:ITrifle
{
    private float DurationSeconds { get; set; }
    public CBlurTrifle()
        : base(true)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        DurationSeconds = kXmlRead.Float("durationSeconds");
    }

    public override void DoTrifle(CSkillEvent ev)
    {
        Camera cam = Camera.main;
        if (cam.gameObject.GetComponent("RadialBlur") == null)
        {
            cam.gameObject.AddComponent("RadialBlur");
        }   
		 
		RadialBlur radialBlur = (RadialBlur)cam.gameObject.GetComponent(typeof(RadialBlur));
		if(radialBlur!=null)
		{	
			radialBlur.enabled = true;
            radialBlur.durationTime = DurationSeconds;
			radialBlur.Reset();
		}
    }

    public override void EndTrifle(CSkillEvent ev)
    {

    }

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("屏幕模糊:", GUILayout.Width(120));
        EditorGUILayout.LabelField("持续时间:", GUILayout.Width(60));
        DurationSeconds = EditorGUILayout.FloatField(DurationSeconds);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }
    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "Blur");
        trifle.SetAttribute("durationSeconds", DurationSeconds.ToString());
        parent.AppendChild(trifle);
    }
#endif
}
