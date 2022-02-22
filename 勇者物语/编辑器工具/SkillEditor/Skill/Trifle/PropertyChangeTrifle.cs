using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CPropertyChangeTrifle:ITrifle
{
    private string Property{get;set;}
    private string Value { get; set; }

    public CPropertyChangeTrifle()
        : base(true)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Property = kXmlRead.Str("property");
        Value = kXmlRead.Str("value");
    }

    public override void DoTrifle(CSkillEvent ev)
    {
        switch (Property)
        {
            case "visible":
                {
                    ev.Owner.SetVisible(Convert.ToByte(Value));
                }
                break;
            case "beAttack":
                {
                    ev.Owner.Property.BeAttacked = Convert.ToByte(Value);
                }
                break;

        }
    }

#if UNITY_EDITOR

    private string[] m_Property = new string[2] { "visible", "beAttack" };
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");

        int index = -1;

        for (int i = 0; i < m_Property.Length; i++ )
        {
            if(m_Property[i].Equals(Property))
            {
                index = i;
                break;
            }
        }

        if(index == -1)
        {
            Property = m_Property[0];
            index = 0;
        }

        EditorGUILayout.LabelField("属性改变:", GUILayout.Width(120));

        EditorGUILayout.LabelField("属性类型:", GUILayout.Width(70));
        Property = m_Property[EditorGUILayout.Popup(index,m_Property)];
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("属性值:", GUILayout.Width(60));
        Value = EditorGUILayout.TextField(Value);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }
    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "PropertyChange");
        trifle.SetAttribute("property", Property.ToString());
        trifle.SetAttribute("value", Value.ToString());
        parent.AppendChild(trifle);
    }
#endif
}