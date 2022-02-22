using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

public class CXmlSkillEvent
{
    public int Index { get; private set; }
    public CXmlSkillValueOfLvl<int> Num;
    public int ZeroTargetEventIndex { get; private set; }
    public bool IsServerNeed { get; private set; }
    public bool IsClientNeed { get; private set; }
    public bool IsBreakable { get; private set; }
    public CXmlSkillInfluence XmlInfluence { get; private set; }
    public CXmlSkillEffectNormal XmlEffect { get; private set; }
    public CXmlSkillEffectP2P XmlP2PEffect { get; private set; }
    public CXmlSkillEffectParabola XmlParabolaEffect { get; private set; }
    public CXmlSkillEffectCollision XmlCollisionEffect { get; private set; }
    public CXmlSkillAction XmlAction { get; private set; }
    public CXmlSkillTarget XmlTarget { get; private set; }
    public CXmlSkillPos XmlPos { get; private set; }
    public CXmlSkillPosP2P XmlPosP2P { get; private set; }
    public CXmlSkillPosCircle XmlPosCircle { get; private set; }
    public CXmlSkillDir XmlDir { get; private set; }
    public CXmlSkillRange XmlRange { get; private set; }
    public List<CXmlSkillSubEvent> XmlSubEvents { get; private set; }
    private List<ITrifle> Trifles { get; set; }

    public CXmlSkillEvent()
    {
        XmlSubEvents = new List<CXmlSkillSubEvent>();
        Trifles = new List<ITrifle>();
    }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Index = kXmlRead.Int("index");
        //Num = kXmlRead.Int("num");
        Num = new CXmlSkillValueOfLvl<int>(ele, "num", 0);

        IsBreakable = kXmlRead.Bool("breakable");
        IsClientNeed = kXmlRead.Bool("clientNeed");
        IsServerNeed = kXmlRead.Bool("serverNeed");
        ZeroTargetEventIndex = kXmlRead.Int("zeroTargetEventIndex", -1);

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "target":
                    {
                        XmlTarget = new CXmlSkillTarget();
                        XmlTarget.Init(node as XmlElement);
                    }
                    break;
                case "pos":
                    {
                        string[] szPermission = { "preEvent", "self", "target", "ground", "save", "preEffect" };
                        XmlPos = new CXmlSkillPos();
                        XmlPos.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
                case "p2pPos":
                    {
                        XmlPosP2P = new CXmlSkillPosP2P();
                        XmlPosP2P.Init(node as XmlElement);
                    }
                    break;
                case "circlePos":
                    {
                        XmlPosCircle = new CXmlSkillPosCircle();
                        XmlPosCircle.Init(node as XmlElement);
                    }
                    break;
                case "dir":
                    {
                        string[] szPermission = { "preEvent", "self", "target", "ground", "twoPoint", "save", "preEffect" };
                        XmlDir = new CXmlSkillDir();
                        XmlDir.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
                case "effect":
                    {
                        XmlEffect = new CXmlSkillEffectNormal();
                        XmlEffect.Init(node as XmlElement);
                    }
                    break;
                case "p2pEffect":
                    {
                        XmlP2PEffect = new CXmlSkillEffectP2P();
                        XmlP2PEffect.Init(node as XmlElement);
                    }
                    break;
                case "parabolaEffect":
                    {
                        XmlParabolaEffect = new CXmlSkillEffectParabola();
                        XmlParabolaEffect.Init(node as XmlElement);
                    }
                    break;
                case "collisionEffect":
                    {
                        XmlCollisionEffect = new CXmlSkillEffectCollision();
                        XmlCollisionEffect.Init(node as XmlElement);
                    }
                    break;
                case "action":
                    {
                        XmlAction = new CXmlSkillAction();
                        XmlAction.Init(node as XmlElement);
                    }
                    break;
                case "range":
                    {
                        XmlRange = new CXmlSkillRange();
                        XmlRange.Init(node as XmlElement);
                    }
                    break;
                case "influence":
                    {
                        XmlInfluence = new CXmlSkillInfluence();
                        XmlInfluence.Init(node as XmlElement);
                    }
                    break;
                case "clientTrifles":
                    {
                        foreach (XmlNode nodeTrifle in node.ChildNodes)
                        {
                            if (nodeTrifle.Name == "trifle")
                            {
                                string szName = new CXmlRead(nodeTrifle as XmlElement).Str("name");
                                ITrifle trifle = Assembly.GetExecutingAssembly().CreateInstance("C" + szName + "Trifle") as ITrifle;
                                trifle.Init(nodeTrifle as XmlElement);
                                Trifles.Add(trifle);
                            }
                        }
                    }
                    break;
                case "subEvents":
                    {
                        foreach (XmlNode nodeSubEvent in node.ChildNodes)
                        {
                            if (nodeSubEvent.Name == "subEvent")
                            {
                                CXmlSkillSubEvent subEvent = new CXmlSkillSubEvent();
                                subEvent.Init(nodeSubEvent as XmlElement);
                                XmlSubEvents.Add(subEvent);
                            }
                        }
                    }
                    break;
            }
        }

#if UNITY_EDITOR
        if (XmlTarget != null)
        {
            XmlTarget.m_Effective = true;
        }
        if (XmlPos != null)
        {
            XmlPos.m_Effective = true;
        }

        if (XmlPosP2P != null)
        {
            XmlPosP2P.m_Effective = true;
        }

        if (XmlPosCircle != null)
        {
            XmlPosCircle.m_Effective = true;
        }
        if (XmlDir != null)
        {
            XmlDir.m_Effective = true;
        }

        if (XmlAction != null)
        {
            XmlAction.m_Effective = true;
        }

        if (XmlRange != null)
        {
            XmlRange.m_Effective = true;
        }

        if (XmlInfluence != null)
        {
            XmlInfluence.m_Effective = true;
        }

        if (string.IsNullOrEmpty(m_EffectSelectName))
        {
            if (XmlEffect != null)
            {
                m_EffectSelectName = m_EffectArray[1];
            }
            if (XmlP2PEffect != null)
            {
                m_EffectSelectName = m_EffectArray[2];
            }
            if (XmlParabolaEffect != null)
            {
                m_EffectSelectName = m_EffectArray[3];
            }
            if (XmlCollisionEffect != null)
            {
                m_EffectSelectName = m_EffectArray[4];
            }
        }
#endif
    }

    public void DoAllTrifles(CSkillEvent ev)
    {
        foreach (ITrifle trifle in Trifles)
        {
            if (ev.OwnerIsPlayer() ||
                ev.Owner.MoveComp.OwnerIsType(EOwnerType.MasterMonster) || 
                ev.OwnerIsAutoPlayer() || 
                trifle.IsNeedSimulate ||
                ev.Owner.MoveComp.OwnerIsType(EOwnerType.Pet))
            {
                trifle.DoTrifle(ev);
            }
        }
    }

    public void EndAllTrifles(CSkillEvent ev)
    {
        foreach (ITrifle trifle in Trifles)
        {
            if (ev.OwnerIsPlayer() ||
                 ev.Owner.MoveComp.OwnerIsType(EOwnerType.MasterMonster) ||
                 ev.OwnerIsAutoPlayer() ||
                 trifle.IsNeedSimulate ||
                 ev.Owner.MoveComp.OwnerIsType(EOwnerType.Pet))
            {
                trifle.EndTrifle(ev);
            }
        }
    }

#if UNITY_EDITOR
    public bool m_Draw = false;
    private bool m_DrawSubEvent = false;
    private bool m_DrawTrifles = false;
    private string[] m_EffectArray = new string[5] {"None", "EffectNormal", "EffectP2P", "EffectParabola", "EffectCollision" };
    private string m_EffectSelectName = "";
    private string[] m_TriflesType = new string[] { "SavePos", "SaveDir", "TerminateSkill", "ShakeCamera", "PropertyChange", "ChainPermission", "ProduceBubble", "Blur", "ClearCoolDown", "HoldCamera" };
    private string m_TriflesAdd = "";
    public void Draw()
    {
        GUILayout.BeginHorizontal("box");

        if (m_Draw)
        {
            GUI.color = Color.green;
        }
        m_Draw = EditorGUILayout.Foldout(m_Draw, "Event");

        EditorGUILayout.LabelField("ID:", GUILayout.Width(20));
        Index = EditorGUILayout.IntField(Index);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("服务器必须:", GUILayout.Width(65));
        IsServerNeed = EditorGUILayout.Toggle(IsServerNeed);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("客户端必须:", GUILayout.Width(65));
        IsClientNeed = EditorGUILayout.Toggle(IsClientNeed);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("是否可打断:", GUILayout.Width(65));
        IsBreakable = EditorGUILayout.Toggle(IsBreakable);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("无目标时ID:", GUILayout.Width(65));
        ZeroTargetEventIndex = EditorGUILayout.IntField(ZeroTargetEventIndex);

        EditorGUILayout.Space();

        if(Num == null)
        {
            Num = new CXmlSkillValueOfLvl<int>("Event数量",0);
        }
        Num.SetName("Event数量");
        Num.Draw();

        GUILayout.EndHorizontal();
        if (XmlTarget == null)
        {
            XmlTarget = new CXmlSkillTarget();
        }
        if (XmlPos == null)
        {
            string[] szPermission = { "preEvent", "self", "target", "ground", "save", "preEffect" };
            XmlPos = new CXmlSkillPos();
            XmlPos.InitEditor(szPermission);
        }
        if (XmlPosCircle == null)
        {
            XmlPosCircle = new CXmlSkillPosCircle();
        }
        if (XmlPosP2P == null)
        {
            XmlPosP2P = new CXmlSkillPosP2P();
        }
        if (XmlDir == null)
        {
            string[] szPermission = { "preEvent", "self", "target", "ground", "twoPoint", "save", "preEffect" };
            XmlDir = new CXmlSkillDir();
            XmlDir.InitEditor(szPermission);
        }
        if (m_Draw)
        {

            XmlTarget.m_IsDrawEffectToggle = true;
            XmlTarget.Draw();


            XmlPos.m_IsDrawEffectToggle = true;
            XmlPos.Draw();


            XmlPosP2P.Draw();


            XmlPosCircle.Draw();


            XmlDir.m_IsDrawEffectToggle = true;
            XmlDir.Draw();

            GUILayout.BeginVertical("box");

            int index_Effect = -1;

            for (int i = 0; i < m_EffectArray.Length; i++)
            {
                if (m_EffectArray[i].Equals(m_EffectSelectName))
                {
                    index_Effect = i;
                    break;
                }
            }

            if(index_Effect == -1)
            {
                index_Effect = 0;
                m_EffectSelectName = m_EffectArray[0];
            }

            m_EffectSelectName = m_EffectArray[EditorGUILayout.Popup("效果类型:", index_Effect, m_EffectArray)];

            switch (m_EffectSelectName)
            {
                case "EffectNormal":
                    {
                        if (XmlEffect == null)
                        {
                            XmlEffect = new CXmlSkillEffectNormal();
                        }
                        XmlEffect.Draw();
                        break;
                    }
                case "EffectP2P":
                    {
                        if (XmlP2PEffect == null)
                        {
                            XmlP2PEffect = new CXmlSkillEffectP2P();
                        }
                        XmlP2PEffect.Draw();
                        break;
                    }
                case "EffectParabola":
                    {
                        if (XmlParabolaEffect == null)
                        {
                            XmlParabolaEffect = new CXmlSkillEffectParabola();
                        }
                        XmlParabolaEffect.Draw();
                        break;
                    }
                case "EffectCollision":
                    {
                        if (XmlCollisionEffect == null)
                        {
                            XmlCollisionEffect = new CXmlSkillEffectCollision();
                        }
                        XmlCollisionEffect.Draw();
                        break;
                    }
            }
            
            GUILayout.EndVertical();

            if (XmlAction == null)
            {
                XmlAction = new CXmlSkillAction();
            }
            XmlAction.Draw();

            if (XmlRange == null)
            {
                XmlRange = new CXmlSkillRange();
            }

            XmlRange.m_IsDrawEffectToggle = true;
            XmlRange.Draw();

            if (XmlInfluence == null)
            {
                XmlInfluence = new CXmlSkillInfluence();
            }

            XmlInfluence.Draw();

            if (XmlSubEvents == null)
            {
                XmlSubEvents = new List<CXmlSkillSubEvent>();
            }

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            m_DrawSubEvent = EditorGUILayout.Foldout(m_DrawSubEvent, "SubEvent");
            if(GUILayout.Button("添加子事件"))
            {
                XmlSubEvents.Add(new CXmlSkillSubEvent());
            }
            GUILayout.EndHorizontal();
            int removeIndex = -1;
            if (m_DrawSubEvent)
            {
                for (int i = 0; i < XmlSubEvents.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    XmlSubEvents[i].Draw();
                    if(GUILayout.Button("Delete"))
                    {
                        removeIndex = i;
                    }
                    GUILayout.EndHorizontal();

                }
            }

            if (removeIndex != -1) { XmlSubEvents.RemoveAt(removeIndex); }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            m_DrawTrifles = EditorGUILayout.Foldout(m_DrawTrifles, "Trifles");

            int m_IndexTriflesAdd = -1;
            for (int i = 0;i < m_TriflesType.Length; i++)
            {
                if(m_TriflesType[i].Equals(m_TriflesAdd))
                {
                    m_IndexTriflesAdd = i;
                    break;
                }
            }
            if (m_IndexTriflesAdd == -1)
            {
                m_IndexTriflesAdd = 0;
                m_TriflesAdd = m_TriflesType[0];
            }

            m_TriflesAdd = m_TriflesType[EditorGUILayout.Popup(m_IndexTriflesAdd, m_TriflesType)];
            if (GUILayout.Button("添加Trifles"))
            {
                ITrifle trifle = Assembly.GetExecutingAssembly().CreateInstance("C" + m_TriflesAdd + "Trifle") as ITrifle;
                Trifles.Add(trifle);
            }
            GUILayout.EndHorizontal();

            removeIndex = -1;
            if (m_DrawTrifles)
            {
                for (int i = 0; i < Trifles.Count; i++ )
                {
                    GUILayout.BeginHorizontal();
                    Trifles[i].Draw();
                    if (GUILayout.Button("Delete"))
                    {
                        removeIndex = i;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            if (removeIndex != -1) { Trifles.RemoveAt(removeIndex); }
            GUILayout.EndVertical();
        }

        GUI.color = Color.white;
    }
    public CXmlSkillEvent(int _index)
    {
        Index = _index;
        XmlSubEvents = new List<CXmlSkillSubEvent>();
        Trifles = new List<ITrifle>();
        ZeroTargetEventIndex = -1;
    }

    public void Export(XmlDocument doc, XmlNode parent) 
    {
        XmlElement m_Event = doc.CreateElement("event");

        m_Event.SetAttribute("index", Index.ToString());
        m_Event.SetAttribute("serverNeed", IsServerNeed.ToString());
        m_Event.SetAttribute("clientNeed", IsClientNeed.ToString());
        m_Event.SetAttribute("breakable", IsBreakable.ToString());
        m_Event.SetAttribute("zeroTargetEventIndex", ZeroTargetEventIndex.ToString());
        
        if(Num != null)
        {
            Num.Export(doc,m_Event,"num");
        }

        if (XmlTarget != null && XmlTarget.m_Effective)
        {
            XmlTarget.Export(doc,m_Event);
        }

        if (XmlPos != null && XmlPos.m_Effective)
        {
            XmlPos.Export(doc,m_Event);
        }

        if (XmlPosP2P != null && XmlPosP2P.m_Effective)
        {
            XmlPosP2P.Export(doc,m_Event);
        }

        if (XmlPosCircle != null && XmlPosCircle.m_Effective)
        {
            XmlPosCircle.Export(doc, m_Event);
        }

        if (XmlDir != null && XmlDir.m_Effective)
        {
            XmlDir.Export(doc, m_Event);
        }

        switch (m_EffectSelectName)
        {
            case "EffectNormal":
                {
                    if (XmlEffect != null)
                    {
                        XmlEffect.Export(doc, m_Event);
                    }
                    break;
                }
            case "EffectP2P":
                {
                    if (XmlP2PEffect != null)
                    {
                        XmlP2PEffect.Export(doc, m_Event);
                    }
                    break;
                }
            case "EffectParabola":
                {
                    if (XmlParabolaEffect != null)
                    {
                        XmlParabolaEffect.Export(doc, m_Event);
                    }
                    break;
                }
            case "EffectCollision":
                {
                    if (XmlCollisionEffect != null)
                    {
                        XmlCollisionEffect.Export(doc, m_Event);
                    }
                    break;
                }
        }

        if (XmlAction != null && XmlAction.m_Effective)
        {
            XmlAction.Export(doc, m_Event);
        }

        if (XmlRange != null && XmlRange.m_Effective)
        {
            XmlRange.Export(doc, m_Event);
        }

        if (XmlInfluence != null && XmlInfluence.m_Effective)
        {
            XmlInfluence.Export(doc, m_Event);
        }

        //-----------------------------------------------------
        if(XmlSubEvents.Count > 0)
        {
            XmlElement subevents = doc.CreateElement("subEvents");

            foreach (var it in XmlSubEvents)
            {
                it.Export(doc, subevents);
            }

            m_Event.AppendChild(subevents);
        }
        //-----------------------------------------------------
        if (Trifles.Count > 0)
        {
            XmlElement clientTrifles = doc.CreateElement("clientTrifles");

            foreach (var it in Trifles)
            {
                it.Export(doc, clientTrifles);
            }

            m_Event.AppendChild(clientTrifles);
        }

        parent.AppendChild(m_Event);
    }
#endif
}