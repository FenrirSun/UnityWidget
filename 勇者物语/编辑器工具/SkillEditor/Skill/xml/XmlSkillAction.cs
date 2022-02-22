using System;
using System.Xml;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillActionData
{
    public List<string> Names = new List<string>();
    public Vector3 Vel;
    public bool End;                // 暂不使用
    public bool Movable;
    public float T;                 // 动作结束时间
    public float TForV;             // 速度结束时间
    public float BreakTime;
    public string AutoAim ;
}

public class CXmlSkillAction
{
    public CXmlSkillActionData m_Data = new CXmlSkillActionData();
    public CXmlSkillValueOfLvl<bool> Movable;
    public CXmlSkillAction() { }

    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        kXmlRead.Vec("name", m_Data.Names);
        m_Data.End = kXmlRead.Bool("end", true);                        //默认是没有下一个动作了
        Movable = new CXmlSkillValueOfLvl<bool>(ele, "movable", true);
        m_Data.T = kXmlRead.Float("t");
        m_Data.TForV = kXmlRead.Float("tForV");
        m_Data.BreakTime = kXmlRead.Float("breakTime");
        m_Data.Vel = kXmlRead.Point3("velocity");
        if (m_Data.Vel.y != 0.0f)                                       //向上速度暂不支持
        {
            Debug.LogError("Velocity.y not supported");
        }
        m_Data.AutoAim = kXmlRead.Str("autoAim", "never");
    }

    public bool IsAutoAimOnce() { return m_Data.AutoAim == "once"; }

    public bool IsAutoAimAllways() { return m_Data.AutoAim == "always"; }

#if UNITY_EDITOR
    private bool m_Draw = false;
    private string[] m_Rotate = new string[3] { "never", "once", "always" };
    private string m_ActionAnis = "";
    public bool m_Effective = false;
    public void Draw()
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        m_Effective = EditorGUILayout.Toggle(m_Effective, GUILayout.Width(10));
        m_Draw = EditorGUILayout.Foldout(m_Draw, "Action");
        GUILayout.EndHorizontal();
        if (m_Draw)
        {
            GUILayout.BeginHorizontal();

            //EditorGUILayout.LabelField("动画名称:", GUILayout.Width(75));
            //DeadEventIndex = EditorGUILayout.IntField(DeadEventIndex);
            //EditorGUILayout.Space();
            if (string.IsNullOrEmpty(m_ActionAnis) && m_Data.Names != null)
            {
                m_ActionAnis = ConstFunc.ListToString(m_Data.Names);
            }
            Color color = GUI.color;
            if (m_ActionAnis.Contains("  ") || m_ActionAnis.Contains(",,"))
            {
                GUI.color = Color.red;
            }
            EditorGUILayout.LabelField("动画名字:", GUILayout.Width(50));
            m_ActionAnis = EditorGUILayout.TextField(m_ActionAnis);
            EditorGUILayout.Space();
            if (!m_ActionAnis.Contains("  ") || m_ActionAnis.Contains(",,"))
            {
                m_Data.Names = ConstFunc.StringToList<string>(m_ActionAnis);
            }
            
            GUI.color = color;

            EditorGUILayout.LabelField("动画时间:", GUILayout.Width(50));
            m_Data.T = EditorGUILayout.FloatField(m_Data.T);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("移动速度", GUILayout.Width(50));
            m_Data.Vel = EditorGUILayout.Vector3Field("",m_Data.Vel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("移动时间:", GUILayout.Width(50));
            m_Data.TForV = EditorGUILayout.FloatField(m_Data.TForV);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("动画取消时间:", GUILayout.Width(75));
            m_Data.BreakTime = EditorGUILayout.FloatField(m_Data.BreakTime);
            EditorGUILayout.Space();

            if(string.IsNullOrEmpty(m_Data.AutoAim))
            {
                m_Data.AutoAim = m_Rotate[0];
            }

            int index = -1;

            for (int i = 0; i < m_Rotate.Length; i++ )
            {
                if(m_Rotate[i].Equals(m_Data.AutoAim))
                {
                    index = i;
                    break;
                }
            }

            if(index == -1)
            {
                m_Data.AutoAim = m_Rotate[0];
            }
            EditorGUILayout.LabelField("朝向:", GUILayout.Width(75));
            m_Data.AutoAim = m_Rotate[EditorGUILayout.Popup(index, m_Rotate)];
            EditorGUILayout.Space();

            //EditorGUILayout.LabelField("移动响应指令:", GUILayout.Width(75));
            //m_Data.Movable = EditorGUILayout.Toggle(m_Data.Movable);
            //EditorGUILayout.Space();

            GUILayout.EndHorizontal();
            if (Movable == null)
            {
                Movable = new CXmlSkillValueOfLvl<bool>("移动响应指令", true);
            }
            Movable.SetName("移动响应指令");

            Movable.Draw();
        }
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "action")
    {
        XmlElement action = doc.CreateElement(name);

        action.SetAttribute("name", ConstFunc.ListToString(m_Data.Names));
        action.SetAttribute("t", m_Data.T.ToString());
        action.SetAttribute("velocity", ConstFunc.Vector3ToString(m_Data.Vel));
        action.SetAttribute("tForV", m_Data.TForV.ToString());
        action.SetAttribute("breakTime", m_Data.BreakTime.ToString());
        action.SetAttribute("autoAim", m_Data.AutoAim);

        if (Movable != null) 
        {
            Movable.Export(doc, action, "movable");
        }

        parent.AppendChild(action);
    }
#endif
}