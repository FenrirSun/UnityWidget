using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillPos
{
    public bool IsAutoChange{get;set;}
    public string Value{get;set;}
    public string NodeName{get;set;}
    public Vector3 Offset { get; set; }
    public Vector3 Random { get; set; }
    public int PosIndex{get;set;}

    public CXmlSkillPos(){}
    public void Init(XmlElement ele, string[] szPermission, int num)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Value = kXmlRead.Str("value");
        NodeName = kXmlRead.Str("node");
        Offset = kXmlRead.Point3("offset");
        Random = kXmlRead.Point3("random");
        IsAutoChange = kXmlRead.Bool("autoChange");
        PosIndex = kXmlRead.Int("posIndex");
        bool bPermission = false;
        for (int i = 0; i < num; ++ i)
        {
            if (Value == szPermission[i])
            {
                bPermission = true;
                break;
            }
        }
        if (!bPermission)
        {
            
            Debug.LogError(ele.Name + "pos value = " + Value + "not defined!");
        }
    }

    public Vector3 GetPosBeforeCreate(CSkill skill, CSkillEvent preEv, CharacterBase character)
    {
        Vector3 ptPosRet = Vector3.zero;
        switch (Value)
        {
            case "preEvent":
                {
                    ptPosRet = preEv.Pos;
                    //if (Offset.x != 0.0f || Offset.z != 0.0f)
                    if (preEv.SkillDir != null)
                    {
                        OffsetPoint(ref ptPosRet, preEv.Dir);
                    }
                    ptPosRet.y += Offset.y;
                }
                break;
            case "preEffect":
                {
                    ptPosRet = preEv.EffectPos;
                    OffsetPoint(ref ptPosRet, preEv.EffectDir);
                    ptPosRet.y += Offset.y;
                }
                break;
            case "effect":
                {
                    ptPosRet = preEv.EffectPos;//其实不是pre，是当前的event
                    OffsetPoint(ref ptPosRet, preEv.EffectDir);
                    ptPosRet.y += Offset.y;
                }
                break;
            case "save":
                {
                    if (!skill.OwnerIsPlayer())
                    {
                        Debug.LogError("非自己不能取得save位置，请转发！");
                    }
                    ptPosRet = skill.Chain.GetPos(PosIndex);
                    if (Offset.x != 0.0f || Offset.z != 0.0f)
                    {
                        OffsetPoint(ref ptPosRet, skill.Chain.GetDir(PosIndex));
                    }
                    ptPosRet.y += Offset.y;
                }
                break;
            case "self":
                {
                    CharacterBase role = null;
                    if (skill != null)
                    {
                        role = skill.Owner;
                    }
                    else
                    {
                          role = character;
                    }
                    if (NodeName == "ground")
                    {
                        Transform tran = role.GetTransformByDummy("Dummy_Hit");
                        ptPosRet = tran.position;
                        OffsetPoint(ref ptPosRet, role.MoveComp.FaceDir.GetDestFaceDirVector());
                        ptPosRet.y = SkillUtility.GetGroundHeight(ptPosRet) + Offset.y;
                    }
                    else
                    {
                        //Transform tran = role.m_ObjInstance.transform;
                        Transform node = role.GetTransformByDummy(NodeName);
                        if (node != null)
                        {
                            ptPosRet = node.position;
                            OffsetPoint(ref ptPosRet, role.MoveComp.FaceDir.GetDestFaceDirVector());
                            ptPosRet.y += Offset.y;
                        }
                        else
                        {
                            Debug.LogError(NodeName + " not exist!");
                        }
                    }
                }
                break;
            case "ground":
                {
                    CharacterBase role = null;
                    if (skill != null)
                    {
                        role = skill.Owner;
                    }
                    else
                    {
                            role = character;
                     }
                    Transform tran = role.GetTransformByDummy("Dummy_Hit");
                    ptPosRet = tran.position;
                    OffsetPoint(ref ptPosRet, role.MoveComp.FaceDir.GetDestFaceDirVector());
                    ptPosRet.y = SkillUtility.GetGroundHeight(ptPosRet) + Offset.y;
                }
                break;
            case "target":
                break;
            case "event":
                break;
            case "p2pPos":
                break;
            case "circlePos":
                break;
            default:
                {
                    Debug.LogError("pos " + Value + " not defined!");
                }
                break;
        }
        return ptPosRet;
    }

    public Vector3 GetPosAfterCreate(CSkillEvent ev)
    {
        Vector3 ptPosRet = Vector3.zero;
        switch (Value)
        {
            case "self":
                {
                    CharacterBase role = ev.Owner;
                    if (NodeName == "ground")
                    {
                        Transform tran = role.GetTransformByDummy("Dummy_Hit");
                        ptPosRet = tran.position;
                        OffsetPoint(ref ptPosRet, role.MoveComp.FaceDir.GetDestFaceDirVector());
                        ptPosRet.y = SkillUtility.GetGroundHeight(ptPosRet) + Offset.y;
                    }
                    else
                    {
                        //Transform tran = role.m_ObjInstance.transform;
                        string nodeName = (ev.Owner is Pet) ? "Dummy_Hit" : NodeName;
                        Transform node = role.GetTransformByDummy(nodeName);
                        if (node != null)
                        {
                            ptPosRet = node.position;
                            OffsetPoint(ref ptPosRet, role.MoveComp.FaceDir.GetDestFaceDirVector());
                            ptPosRet.y += Offset.y;
                        }
                        else
                        {
                            Debug.LogError(nodeName + " not exist!");
                        }
                    }
                }
                break;
            case "target":
            {
                ulong nTargetID = ev.TargetID;
                CharacterBase target = GameApp.GetWorldManager().GetObject(nTargetID);
                if (target != null)
                {
                    if (NodeName == "ground")
                    {
                        Transform tran = target.GetTransformByDummy("Dummy_Hit");
                        ptPosRet = tran.position;
                        OffsetPoint(ref ptPosRet, target.MoveComp.FaceDir.GetDestFaceDirVector());
                        ptPosRet.y = SkillUtility.GetGroundHeight(ptPosRet) + Offset.y;
                    }
                    else
                    {
                        Transform node = target.GetTransformByDummy(NodeName);
                        if (node != null)
                        {
                            ptPosRet = node.position;
                            OffsetPoint(ref ptPosRet, target.MoveComp.FaceDir.GetDestFaceDirVector());
                            ptPosRet.y += Offset.y;
                        }
                        else
                        {
                            Debug.LogError(NodeName + " not exist!");
                        }
                    }
                }
            }
                break;
            case "save":
                {
                    ptPosRet = ev.Skill.Chain.GetPos(PosIndex);
                    if (Offset.y != 0.0f)
                    {
                        ptPosRet.y += Offset.y;
                    }
                    if (Offset.x != 0.0f || Offset.z != 0.0f)
                    {
                        OffsetPoint(ref ptPosRet, ev.Skill.Chain.GetDir(PosIndex));
                        ptPosRet.y += Offset.y;
                    }
                }
                break;
            case "event":
                {
                    ptPosRet = ev.Pos;
                    if (ev.SkillDir != null)
                    {
                        OffsetPoint(ref ptPosRet, ev.Dir);
                    }
                    ptPosRet.y += Offset.y;
                }
                break;
            case "p2pPos":
                {
                    ptPosRet = ev.PosP2P;
                    if (ev.SkillDir != null)
                    {
                        OffsetPoint(ref ptPosRet, ev.Dir);
                    }
                    ptPosRet.y += Offset.y;
                }
                break;
            case "circlePos":
                {
                    ptPosRet = ev.PosCircle;
                    if (ev.SkillDir != null)
                    {
                        OffsetPoint(ref ptPosRet, ev.Dir);
                    }
                    ptPosRet.y += Offset.y;
                }
                break;
            case "ground":
                {
                    CharacterBase role = ev.Owner;
                    Transform tran = role.GetTransformByDummy("Dummy_Hit");
                    ptPosRet = tran.position;
                    OffsetPoint(ref ptPosRet, role.MoveComp.FaceDir.GetDestFaceDirVector());
                    ptPosRet.y = SkillUtility.GetGroundHeight(ptPosRet) + Offset.y;
                }
                break;
            case "preEffect":
                {
                }
                break;
            case "preEvent":
                {
                }
                break;
            default:
                {
                    Debug.LogError("pos " + Value + " not defined!");
                }
                break;
        }

        return ptPosRet;
    }

    private void OffsetPoint(ref Vector3 ptPos, Vector3 ptDir)
    {
        Vector3 off = Offset;
        if (Random.x != 0f)
        {
            off.x += UnityEngine.Random.RandomRange(-Random.x, Random.x);
        }
        if (Random.z != 0f)
        {
            off.z += UnityEngine.Random.RandomRange(-Random.z, Random.z);
        }
        if (Random.y != 0f)
        {
            Debug.LogError("not support random.y");
        }
        if (off.x != 0.0f)
        {
            if (ptDir!= Vector3.zero)
            {
                Vector3 ptOne = ptDir;
                ptOne.Normalize();
                ptOne = Vector3.Cross(ptOne,Vector3.up);
                ptPos += ptOne * off.x;
            }
        }
        if (off.z != 0.0f)
        {
            Vector3 ptOne = ptDir;
            ptOne.Normalize();
            ptPos += ptOne * off.z;
        }
    }


#if UNITY_EDITOR
    private bool m_Draw = false;
    private string[] m_EnumSelects;
    private string m_Name;
    public bool m_Effective = false;
    public bool m_IsDrawEffectToggle = false;
    public void InitEditor(string[] enumArray,string name = "Pos")
    {
        m_EnumSelects = enumArray;
        m_Name = name;
    }

    public void SetName(string name)
    {
        m_Name = name;
    }
    public void Draw()
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        if (m_IsDrawEffectToggle)
        {
            m_Effective = EditorGUILayout.Toggle(m_Effective, GUILayout.Width(10));
        }
        m_Draw = EditorGUILayout.Foldout(m_Draw, m_Name);

        GUILayout.EndHorizontal();

        if (m_Draw)
        {

            GUILayout.BeginHorizontal();
            if (m_EnumSelects != null && m_EnumSelects.Length > 0)
            {
                if (string.IsNullOrEmpty(Value))
                {
                    Value = m_EnumSelects[0];
                }
                EditorGUILayout.LabelField("位置类型:", GUILayout.Width(50));
                int index = -1;
                for (int i = 0; i < m_EnumSelects.Length; i++)
                {
                    if (Value == m_EnumSelects[i])
                    {
                        index = i;
                        break;
                    }
                }
                if (index != -1)
                {
                    Value = m_EnumSelects[EditorGUILayout.Popup(index, m_EnumSelects)];
                }
            }
            
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("下标:", GUILayout.Width(30));
            PosIndex = EditorGUILayout.IntField(PosIndex);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("节点名:", GUILayout.Width(40));
            NodeName = EditorGUILayout.TextField(NodeName);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("偏移:", GUILayout.Width(30));
            Offset = EditorGUILayout.Vector3Field("",Offset);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("随机范围:", GUILayout.Width(50));
            Random = EditorGUILayout.Vector3Field("", Random);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("跟随移动:", GUILayout.Width(50));
            IsAutoChange = EditorGUILayout.Toggle(IsAutoChange);
            EditorGUILayout.Space();

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    public void Export(XmlDocument doc, XmlNode parent,string name = "pos")
    {
        XmlElement pos = doc.CreateElement(name);

        pos.SetAttribute("value", Value);
        if (!string.IsNullOrEmpty(NodeName))
        {
            pos.SetAttribute("node", NodeName);
        }
        pos.SetAttribute("offset", ConstFunc.Vector3ToString(Offset));
        pos.SetAttribute("random", ConstFunc.Vector3ToString(Random));
        pos.SetAttribute("autoChange", IsAutoChange.ToString());
        pos.SetAttribute("posIndex", PosIndex.ToString());

        parent.AppendChild(pos);
    }
#endif
}