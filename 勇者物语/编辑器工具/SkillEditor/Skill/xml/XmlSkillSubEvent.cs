using System;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
public class CXmlSkillSubEvent
{
    //public int EventIndexWhenHasTarget{get;private set;}
    //public int EventIndexWhenHasNotTarget { get; private set; }
    //public float Seconds { get; private set; }
    public CXmlSkillValueOfLvl<int> EventIndexWhenHasTarget;
    public CXmlSkillValueOfLvl<int> EventIndexWhenHasNotTarget;
    public CXmlSkillValueOfLvl<float> Seconds;
    public CXmlSkillValueOfLvl<float> Prob;
    public int Num;
    public float IntervalSeconds;

    public CXmlSkillSubEvent(){}
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        //EventIndexWhenHasTarget = kXmlRead.Int("eventIndexWhenHasTarget");
        //EventIndexWhenHasNotTarget = kXmlRead.Int("eventIndexWhenHasNotTarget");
        //Seconds = kXmlRead.Float("seconds");
        EventIndexWhenHasTarget = new CXmlSkillValueOfLvl<int>(ele, "eventIndexWhenHasTarget",0);
        EventIndexWhenHasNotTarget = new CXmlSkillValueOfLvl<int>(ele, "eventIndexWhenHasNotTarget", 0);
        Seconds = new CXmlSkillValueOfLvl<float>(ele, "seconds", 0.0f);
        Prob = new CXmlSkillValueOfLvl<float>(ele, "prob", 100.0f);
        Num = kXmlRead.Int("num", 1);
        IntervalSeconds = kXmlRead.Float("intervalSeconds");
    }

#if UNITY_EDITOR
    private bool m_Draw = false;
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        m_Draw = EditorGUILayout.Foldout(m_Draw,"SubEvent");

        if(m_Draw)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("连续触发数量:", GUILayout.Width(80));
            Num = EditorGUILayout.IntField(Num);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("连续触发间隔:", GUILayout.Width(80));
            IntervalSeconds = EditorGUILayout.FloatField(IntervalSeconds);
            EditorGUILayout.Space();

            GUILayout.EndHorizontal();

            if (EventIndexWhenHasTarget == null)
            {
                EventIndexWhenHasTarget = new CXmlSkillValueOfLvl<int>("下一事件(有)", 0);
            }
            EventIndexWhenHasTarget.SetName("下一事件(有)");
            if (EventIndexWhenHasNotTarget == null)
            {
                EventIndexWhenHasNotTarget = new CXmlSkillValueOfLvl<int>("下一事件(无)", 0);
            }
            EventIndexWhenHasNotTarget.SetName("下一事件(无)");
            if (Seconds == null)
            {
                Seconds = new CXmlSkillValueOfLvl<float>("延时", 0.0f);
            }
            Seconds.SetName("延时");
            if (Prob == null)
            {
                Prob = new CXmlSkillValueOfLvl<float>("触发概率", 100f);
            }
            Prob.SetName("触发概率");

            EventIndexWhenHasTarget.Draw();
            EventIndexWhenHasNotTarget.Draw();
            Seconds.Draw();
            Prob.Draw();
        }

        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "subEvent")
    {
        XmlElement subEvent = doc.CreateElement(name);

        subEvent.SetAttribute("num", Num.ToString());
        subEvent.SetAttribute("intervalSeconds", IntervalSeconds.ToString());

        if(EventIndexWhenHasTarget != null)
        {
            EventIndexWhenHasTarget.Export(doc, subEvent, "eventIndexWhenHasTarget");
        }

        if (EventIndexWhenHasNotTarget != null)
        {
            EventIndexWhenHasNotTarget.Export(doc, subEvent, "eventIndexWhenHasNotTarget");
        }

        if (Seconds != null)
        {
            Seconds.Export(doc, subEvent, "seconds");
        }
        if (Prob != null)
        {
            Prob.Export(doc, subEvent, "prob");
        }
        parent.AppendChild(subEvent);
    }
#endif
}