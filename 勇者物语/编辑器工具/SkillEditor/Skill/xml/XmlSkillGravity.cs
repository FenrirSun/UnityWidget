using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillGravity
{
    public float R { get; set; }
    public float V { get; set; }
    public float T { get; set; }
    public CXmlSkillPos XmlPos { get; set; }

    public CXmlSkillGravity() { }

    public void Init(XmlElement ele)
    {
        //Debug.LogError("gravity init");
        CXmlRead kXmlRead = new CXmlRead(ele);
        R = kXmlRead.Float("r");
        V = kXmlRead.Float("v");
        T = kXmlRead.Float("t");

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "pos":
                    {
                        string[] szPermission = { "self", "target", "ground", "event" };
                        XmlPos = new CXmlSkillPos();
                        XmlPos.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
            }
        }
        if (XmlPos == null)
        {
            Debug.LogError(" gravity has no pos");
        }
    }

    public void Gravity(CSkillEvent ev)
    {
        CharacterBase c = ev.Target;
        if (c != null)
        {
            if (c is Monster)
            {
                Vector3 pos = XmlPos.GetPosAfterCreate(ev);
                ML2Event tempEvent = new ML2Event((int)GameEventID.FMS_EVENT_ATTRACTED);
                tempEvent.PushUserData<Vector3>(pos);
                tempEvent.PushUserData<float>(R);
                tempEvent.PushUserData<float>(V);
                tempEvent.PushUserData<float>(T);
                GameEvent.DispatchEvent(c, tempEvent);
            }
            else//player or netplayer
            {
                Vector3 pos = XmlPos.GetPosAfterCreate(ev);
                //Vector3 dir = pos - c.MoveComp.transform.position;
                //dir.Normalize();
                //dir *= V;
                c.MoveComp.VelDir.AddGravity(pos,V, T);
            }
        }
    }
#if UNITY_EDITOR
    private string[] m_Where = new string[3] { "land", "air", "fall" };
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("重力:", GUILayout.Width(30));

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("重力场半径:", GUILayout.Width(80));
        R = EditorGUILayout.FloatField(R);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("重力场速度:", GUILayout.Width(80));
        V = EditorGUILayout.FloatField(V);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("速度量增加的时间:", GUILayout.Width(80));
        T = EditorGUILayout.FloatField(T);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();

        if (XmlPos == null)
        {
            XmlPos = new CXmlSkillPos();
        }
        XmlPos.Draw();
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "gravity")
    {
        XmlElement gravity = doc.CreateElement(name);

        gravity.SetAttribute("r",R.ToString());
        gravity.SetAttribute("v",V.ToString());
        gravity.SetAttribute("t", T.ToString());

        if (XmlPos != null)
        {
            XmlPos.Export(doc,gravity);
        }

        parent.AppendChild(gravity);
    }
#endif
}