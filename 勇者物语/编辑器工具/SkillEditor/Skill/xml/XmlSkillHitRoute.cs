using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillHitRoute
{
    public string Where { get; private set; }
    public float H0 { get; private set; }
    public float V0 { get; private set; }
    public float T { get; private set; }
    //public float Prob { get; private set; }
    public CXmlSkillValueOfLvl<float> Prob;

    public CXmlSkillHitRoute() { }

    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Where = kXmlRead.Str("where");
        H0 = kXmlRead.Float("h0");
        V0 = kXmlRead.Float("v0");
        T = kXmlRead.Float("t");
        //Prob = kXmlRead.Float("prob");
        Prob = new CXmlSkillValueOfLvl<float>(ele, "prob", 100.0f);
    }

#if UNITY_EDITOR
    private string[] m_Where = new string[3] { "land", "air", "fall" };
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("受击效果:", GUILayout.Width(60));

        GUILayout.BeginHorizontal();

        int index = -1;
        for (int i = 0; i < m_Where.Length; i++ )
        {
            if (m_Where[i].Equals(Where))
            {
                index = i;
                break;
            }
        }

        if(index == -1)
        {
            index = 0;
            Where = m_Where[0];
        }

        EditorGUILayout.LabelField("保留方向信息的EVENT下标:", GUILayout.Width(60));
        Where = m_Where[EditorGUILayout.Popup(index,m_Where)];
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("垂直速度:", GUILayout.Width(60));
        H0 = EditorGUILayout.FloatField(H0);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("水平速度:", GUILayout.Width(60));
        V0 = EditorGUILayout.FloatField(V0);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("硬直时间:", GUILayout.Width(60));
        T = EditorGUILayout.FloatField(T);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();

        if (Prob == null)
        {
            Prob = new CXmlSkillValueOfLvl<float>("成功概率", 100.0f);
        }
        Prob.SetName("成功概率");
        Prob.Draw();
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "hitRoute")
    {
        XmlElement hitRoute = doc.CreateElement(name);

        hitRoute.SetAttribute("where", Where);
        hitRoute.SetAttribute("h0", H0.ToString());
        hitRoute.SetAttribute("v0", V0.ToString());
        hitRoute.SetAttribute("t", T.ToString());

        if(Prob != null)
        {
            Prob.Export(doc, hitRoute, "prob");
        }

        parent.AppendChild(hitRoute);
    }

    public void SetWhere(string where)
    {
        Where = where;
    }
#endif
}