using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillDir
{
    public string Value { get; private set; }
    public Vector3 Offset { get; private set; }
    public bool IsAutoChange { get; private set; }
    private int DirIndex { get; set; }
    public CXmlSkillPos XmlPos1 { get; private set; }
    public CXmlSkillPos XmlPos2 { get; private set; }

    public CXmlSkillDir(){}

    public void Init(XmlElement ele, string[] szPermission, int num)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Value = kXmlRead.Str("value");
        Offset = kXmlRead.Point3("offset");
        IsAutoChange = kXmlRead.Bool("autoChange");
        DirIndex = kXmlRead.Int("dirIndex");

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
            Debug.LogError("dir value = "+ Value + "not defined!");
        }

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "pos1":
                    {
                        string[] szPermission1 = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                        XmlPos1 = new CXmlSkillPos();
                        XmlPos1.Init(node as XmlElement, szPermission1, szPermission1.Length);
                    }
                    break;
                case "pos2":
                    {
                        string[] szPermission2 = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                        XmlPos2 = new CXmlSkillPos();
                        XmlPos2.Init(node as XmlElement, szPermission2, szPermission2.Length);
                    }
                    break;
            }
        }

    }

    public Vector3 GetDirBeforeCreate(CSkill skill, CSkillEvent evPre, CharacterBase character)
    {
        Vector3 ptDirRet = Vector3.zero;
        switch (Value)
        {
        case "preEvent":
            {
                ptDirRet = evPre.Dir;
            }
        	break;
        case "preEffect":
            {
                ptDirRet = evPre.EffectDir;
            }
        	break;
        case "save":
            {
                ptDirRet = skill.Chain.GetDir(DirIndex);
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
                    if (character != null)
                    {
                        role = character;
                    }
                    else
                    {
                        role = GameApp.GetWorldManager().MainPlayer;
                    }
                }
                ptDirRet = role.MoveComp.FaceDir.GetDestFaceDirVector(); 

            }
        	break;
        case "twoPoint":
            {
                ptDirRet = XmlPos2.GetPosBeforeCreate(skill, evPre, character) - XmlPos1.GetPosBeforeCreate(skill, evPre, character);
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
                    if (character != null)
                    {
                        role = character;
                    }
                    else
                    {
                        role = GameApp.GetWorldManager().MainPlayer;
                    }
                }

                Vector3 ptDir,ptPos;
                Transform tran = role.GetTransformByDummy("Dummy_Hit");
                ptDir = role.MoveComp.FaceDir.GetDestFaceDirVector();
                ptPos = tran.position;
                ptPos += ptDir*5.0f;
                //if (Offset != Vector3.zero)
                //{
                //    Debug.LogError("方向偏移未做！");
                //}
                ptPos.y = SkillUtility.GetGroundHeight(ptPos);
                ptDirRet = ptPos - tran.position;
            }
            break;
        case "target":
            {
            }
            break;
        case "event":
            {
            }
            break;
        default:
            {
                Debug.LogError("dir " + Value + " not defined!");
            }
            break;
        }
        if (Offset != Vector3.zero)
        {
            //float newDir = Mathf.Atan2(ptDirRet.z, ptDirRet.x) * Mathf.Rad2Deg;
            //newDir += Offset.y;
            //ptDirRet.x = Mathf.Cos(newDir * Mathf.Deg2Rad);
            //ptDirRet.z = Mathf.Cos(newDir * Mathf.Deg2Rad);
            Quaternion q = Quaternion.EulerAngles(-Offset);
            ptDirRet = q * ptDirRet;
        }
        return ptDirRet;
    }

    public Vector3 GetDirAfterCreate(CSkillEvent ev)
    {
        Vector3 ptDirRet = Vector3.zero;
        switch (Value)
        {
            case "self":
                {
                    CharacterBase role = ev.Owner;
                    ptDirRet = role.MoveComp.FaceDir.GetDestFaceDirVector();
                }
                break;
            case "twoPoint":
                {
                    ptDirRet = XmlPos2.GetPosAfterCreate(ev) - XmlPos1.GetPosAfterCreate(ev);
                }
                break;
            case "target":
                {
                    ulong nTargetID = ev.TargetID;
                    CharacterBase target = GameApp.GetWorldManager().GetObject(nTargetID);
                    if (target != null)
                    {
                        ptDirRet = target.MoveComp.FaceDir.GetDestFaceDirVector(); 
                    }
                }
                break;
            case "ground":
                {
                    CharacterBase role = ev.Owner;

                    Vector3 ptDir,ptPos;
                    Transform tran = role.GetTransformByDummy("Dummy_Hit");
                    ptDir = role.MoveComp.FaceDir.GetDestFaceDirVector();
                    ptPos = tran.position;
                    ptPos += ptDir*5.0f;
                    //if (Offset != Vector3.zero)
                    //{
                    //    Debug.LogError("方向偏移未做！");
                    //}
                    ptPos.y = SkillUtility.GetGroundHeight(ptPos);
                    ptDirRet = ptPos - tran.position;
                }
                break;
            case "event":
                {
                    ptDirRet = ev.Dir;
                }
            break;
            case "preEvent":
                {
                }
            break;
            case "preEffect":
                {
                }
            break;
            case "save":
                {
                }
            break;
            default:
                {
                    Debug.LogError("dir value = "+ Value + "not defined!");
                    //Common::Utility::ML2Assert(false, "skill id = %d,event id = %d, dir %s未定义！",pkSkillEvent->GetSkill()->GetXml()->GetID(),pkSkillEvent->GetXml()->GetIndex() ,m_strValue);
                }
                break;
        }

        if (Offset != Vector3.zero)
        {
            //float newDir = Mathf.Atan2(ptDirRet.z, ptDirRet.x) * Mathf.Rad2Deg;
            //newDir += Offset.y;
            //ptDirRet.x = Mathf.Cos(newDir * Mathf.Deg2Rad);
            //ptDirRet.z = Mathf.Cos(newDir * Mathf.Deg2Rad);
            Quaternion q = Quaternion.EulerAngles(-Offset);
            ptDirRet = q * ptDirRet;
        }
        return ptDirRet;
    }


#if UNITY_EDITOR
    private bool m_Draw = false;
    private string[] m_EnumSelects;
    private string m_Name = "Dir";
    public bool m_Effective = false;
    public bool m_IsDrawEffectToggle = false;

    public void InitEditor(string[] enumArray, string name = "Dir")
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

            if (XmlPos1 == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                XmlPos1 = new CXmlSkillPos();
                XmlPos1.InitEditor(szPermission, "初始位置");
            }

            if (XmlPos2 == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                XmlPos2 = new CXmlSkillPos();
                XmlPos2.InitEditor(szPermission, "目标位置");
            }

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

            EditorGUILayout.LabelField("方向编号:", GUILayout.Width(50));
            DirIndex = EditorGUILayout.IntField(DirIndex);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("偏移:", GUILayout.Width(30));
            Offset = EditorGUILayout.Vector3Field("", Offset);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("跟随移动:", GUILayout.Width(50));
            IsAutoChange = EditorGUILayout.Toggle(IsAutoChange);
            EditorGUILayout.Space();

            GUILayout.EndHorizontal();

            XmlPos1.Draw();
            XmlPos2.Draw();
        }
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent,string name = "dir")
    {
        XmlElement dir = doc.CreateElement(name);

        dir.SetAttribute("value", Value);
        dir.SetAttribute("offset", ConstFunc.Vector3ToString(Offset));
        dir.SetAttribute("autoChange", IsAutoChange.ToString());
        dir.SetAttribute("dirIndex", DirIndex.ToString());

        if(XmlPos1 == null)
        {
            //EditorUtility.DisplayDialog("注意", "请填写position", "知道");
        }
        else
        {
            XmlPos1.Export(doc, dir, "pos1");
        }
        if (XmlPos2 == null)
        {
            //EditorUtility.DisplayDialog("注意", "请填写position", "知道"); 
        }
        else
        {
            XmlPos2.Export(doc, dir, "pos2");
        }
        

        parent.AppendChild(dir);
    }
#endif
}