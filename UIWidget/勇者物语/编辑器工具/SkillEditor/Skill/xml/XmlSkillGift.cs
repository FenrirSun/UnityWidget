using System.Collections.Generic;
using System;
using System.Xml;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillGift
{
    public int ID { get; private set; }
    public string Name
    {
        get
        {
            return O_Localization.instance.GetText("PlayerGiftNameLocal", ID);
        }
    }
    public string Sprite { get; private set; }
    public string Atlas { get; private set; }
    public int CurLvl { get; private set; }
    private List<int> SPs;
    private List<int> lvls;
    private List<int> Monenys;
    private List<float> Value0;
    private List<float> Value1;
    public CXmlSkillGiftGroup XmlGroup;
    public List<int> NeedLevels = new List<int>();

    //天赋的最高等级
    public int MaxlvlForGift
    {
        get
        {
            return SPs.Count - 1;
        }
    }

    //根据人物等级决定主技能的最高等级
    public int MaxlvlForPlayerLevel
    {
        get
        {
            //if (ID % 10 != 1)//约定个位数是1的天赋是主技能
            //{
            //    return SPs.Count - 1;
            //}
            //else
            //{
            //    return XmlGroup.GetMainGiftMaxLvl();
            //}
            
            int lvl = (int)GameApp.GetWorldManager().MainPlayer.GetProperty().Level;
            for (int i = 0; i < NeedLevels.Count; ++i)
            {
                if (NeedLevels[i] > lvl)
                {
                    return i - 1;
                }
            }
            return NeedLevels.Count - 1;

        }
    }

    public CXmlSkillGift(CXmlSkillGiftGroup xmlGroup)
    {
        XmlGroup = xmlGroup;
    }

    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        ID = kXmlRead.Int("id");
        CurLvl = kXmlRead.Int("curLvl");
        //Name = kXmlRead.Str("name");
        Sprite = kXmlRead.Str("sprite", "leiguanglao");
        Atlas = kXmlRead.Str("atlas", "leiguanglao");
        SPs = new List<int>();
        Monenys = new List<int>();
        lvls = new List<int>();

        kXmlRead.Vec("sp", SPs);
        kXmlRead.Vec("money",Monenys);
        kXmlRead.Vec("needLevel",lvls);
      
        kXmlRead.Vec("needLevel", NeedLevels);
   
        if (SPs.Count != NeedLevels.Count)
        {
            Debug.LogError("Error!!gift " + ID + " SP count = " + SPs.Count + ", needLevel count = " + NeedLevels.Count);
        }
   
        if (kXmlRead.HasAttribute("value0"))
        {
            Value0 = new List<float>();
            kXmlRead.Vec("value0", Value0);
            if (Value0.Count != SPs.Count)
            {
                Debug.LogError("Error!!gift " + ID + " SP count = " + SPs.Count + ", value0 count = " + Value0.Count);
            }
        }
        if (kXmlRead.HasAttribute("value1"))
        {
            Value1 = new List<float>();
            kXmlRead.Vec("value1", Value1);
            if (Value1.Count != SPs.Count)
            {
                Debug.LogError("Error!!gift " + ID + " SP count = " + SPs.Count + ", value1 count = " + Value1.Count);
            }
        }

        //foreach (XmlNode node in ele.ChildNodes)
        //{
        //    switch (node.Name)
        //    {
        //        case "currentLevel":
        //            {
        //                CXmlRead kXmlRead1 = new CXmlRead(node as XmlElement);
        //                CurLevelNote = kXmlRead1.Str("note");
        //            }
        //            break;
        //        case "nextLevel":
        //            {
        //                CXmlRead kXmlRead2 = new CXmlRead(node as XmlElement);
        //                NextLevelNote = kXmlRead2.Str("note");
        //            }
        //            break;
        //    }
        //}
#if UNITY_EDITOR
        if(SPs != null)
        {
            m_Sp = ConstFunc.ListToString(SPs);
        }
        if (lvls != null)
        {
            m_Lvs = ConstFunc.ListToString(lvls);
        }
        if (Monenys != null)
        {
            m_Money = ConstFunc.ListToString(Monenys);
        }
        if (Value0 != null)
        {
            m_Value0 = ConstFunc.ListToString(Value0);
        }
        if (Value1 != null)
        {
            m_Value1 = ConstFunc.ListToString(Value1);
        }
        if (NeedLevels != null)
        {
            m_NeedLevel = ConstFunc.ListToString(NeedLevels);
        }
        if (string.IsNullOrEmpty(m_Name))
        {
            m_Name = O_Localization.instance.GetText("PlayerGiftNameLocal", ID);
        }
        if (string.IsNullOrEmpty(m_CurrectNote))
        {
            m_CurrectNote = O_Localization.instance.GetText("PlayerGiftCurrentLevelNoteLocal", ID);
        }
        if (string.IsNullOrEmpty(m_NextNote))
        {
            m_NextNote = O_Localization.instance.GetText("PlayerGiftNextLevelNoteLocal", ID);
        }
#endif
    }

    //lvl当前等级
    public int GetCurNeedSP(int lvl)
    {
        return SPs[lvl+1];
    }
    public int GetCurNeedMoneny(int lvl)
    {
        return Monenys[lvl+1];
    }
    public int GetCurNeedLel(int lvl)
    {
        return lvls[lvl+1];
    }
    //lvl当前等级
    public string GetCurLevelNote(int lvl)
    {
        string CurLevelNote = O_Localization.instance.GetText("PlayerGiftCurrentLevelNoteLocal", ID); ;
        string NextLevelNote = O_Localization.instance.GetText("PlayerGiftNextLevelNoteLocal", ID); ;
        string str = null;
        if (Value1 != null)
        {
            str = string.Format(CurLevelNote, Value0[lvl], Value1[lvl]);
        }
        else if (Value0 != null)
        {
            str = string.Format(CurLevelNote, Value0[lvl]);
        }
        else
        {
            if (lvl == 0)
            {
                str = CurLevelNote;
            }
            else
            {
                str = NextLevelNote;
            }
        }
        return str;
    }

    //lvl当前等级
    public string GetNextLevelNote(int lvl)
    {
        string NextLevelNote = O_Localization.instance.GetText("PlayerGiftNextLevelNoteLocal", ID); ;
        lvl++;
        string str = null;
        if (Value1 != null)
        {
            str = string.Format(NextLevelNote, Value0[lvl], Value1[lvl]);
        }
        else if (Value0 != null)
        {
            str = string.Format(NextLevelNote, Value0[lvl]);
        }
        else
        {
            //Debug.Log("!!!!!!!!!$$$$$$$$$$$$$$$$$$$lvl =" + lvl + " note = " + NextLevelNote);
            if (lvl == 1)
            {
                str = NextLevelNote;
            }
            else
            {
                str = "";
            }
        }
        return str;
    }



#if UNITY_EDITOR
    private string m_Sp = "";
    private string m_Money = "";
    private string m_Lvs = "";
    private string m_Value0 = "";
    private string m_Value1 = "";
    private string m_NeedLevel = "";
    private bool m_Draw = false;
    public string m_CurrectNote = "";
    public string m_NextNote = "";
    public string m_Name = "";
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();

        m_Draw = EditorGUILayout.Foldout(m_Draw, "Gift");

        EditorGUILayout.LabelField("ID:", GUILayout.Width(30));
        ID = EditorGUILayout.IntField(ID);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
        if(m_Draw)
        {
            //GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Name:", GUILayout.Width(80));
            m_Name = EditorGUILayout.TextField(m_Name);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("CurrectNote:", GUILayout.Width(80));
            m_CurrectNote = EditorGUILayout.TextField(m_CurrectNote);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("NextNote:", GUILayout.Width(80));
            m_NextNote = EditorGUILayout.TextField(m_NextNote);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Sprite:", GUILayout.Width(40));
            Sprite = EditorGUILayout.TextField(Sprite);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Atlas:", GUILayout.Width(40));
            Atlas = EditorGUILayout.TextField(Atlas);
            EditorGUILayout.Space();

            //EditorGUILayout.LabelField("CurLvl:", GUILayout.Width(30));
            //CurLvl = EditorGUILayout.IntField(CurLvl);
            //EditorGUILayout.Space();

            //GUILayout.EndHorizontal();

            Color old = GUI.color;
            if (m_Sp.Contains("  ") || m_Sp.Contains(",,"))
            {
                GUI.color = Color.red;
            }

            EditorGUILayout.LabelField("Sp:", GUILayout.Width(60));
            m_Sp = EditorGUILayout.TextField(m_Sp);
            EditorGUILayout.Space();
            GUI.color = old;
            if (!m_Sp.Contains("  ") && !m_Sp.Contains(",,"))
            {
                SPs = ConstFunc.StringToList<int>(m_Sp);
            }

            old = GUI.color;
            if (m_Money.Contains("  ") || m_Money.Contains(",,"))
            {
                GUI.color = Color.red;
            }
            EditorGUILayout.LabelField("Money:", GUILayout.Width(60));
            m_Money = EditorGUILayout.TextField(m_Money);
            EditorGUILayout.Space();
            GUI.color = old;
            if (!m_Money.Contains("  ") && !m_Money.Contains(",,"))
            {
                Monenys = ConstFunc.StringToList<int>(m_Money);
            }

            old = GUI.color;
            if (m_Lvs.Contains("  ") || m_Lvs.Contains(",,"))
            {
                GUI.color = Color.red;
            }
            EditorGUILayout.LabelField("LV:", GUILayout.Width(60));
            m_Lvs = EditorGUILayout.TextField(m_Lvs);
            EditorGUILayout.Space();
            GUI.color = old;
            if (!m_Lvs.Contains("  ") && !m_Lvs.Contains(",,"))
            {
                lvls = ConstFunc.StringToList<int>(m_Lvs);
            }

            old = GUI.color;
            if (m_Value0.Contains("  ") || m_Value0.Contains(",,"))
            {
                GUI.color = Color.red;
            }
            EditorGUILayout.LabelField("value0:", GUILayout.Width(60));
            m_Value0 = EditorGUILayout.TextField(m_Value0);
            EditorGUILayout.Space();
            GUI.color = old;
            if (!m_Value0.Contains("  ") && !m_Value0.Contains(",,"))
            {
                Value0 = ConstFunc.StringToList<float>(m_Value0);
            }


            old = GUI.color;
            if (m_Value1.Contains("  ") || m_Value1.Contains(",,"))
            {
                GUI.color = Color.red;
            }
            EditorGUILayout.LabelField("value1:", GUILayout.Width(60));
            m_Value1 = EditorGUILayout.TextField(m_Value1);
            EditorGUILayout.Space();
            GUI.color = old;
            if (!m_Value1.Contains("  ") && !m_Value1.Contains(",,"))
            {
                Value1 = ConstFunc.StringToList<float>(m_Value1);
            }


            old = GUI.color;
            if (m_NeedLevel.Contains("  ") || m_NeedLevel.Contains(",,"))
            {
                GUI.color = Color.red;
            }
            EditorGUILayout.LabelField("needLevel:", GUILayout.Width(80));
            m_NeedLevel = EditorGUILayout.TextField(m_NeedLevel);
            EditorGUILayout.Space();
            GUI.color = old;
            if (!m_NeedLevel.Contains("  ") && !m_NeedLevel.Contains(",,"))
            {
                NeedLevels = ConstFunc.StringToList<int>(m_NeedLevel);
            }

        }

        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "gift") 
    {
        XmlElement gift = doc.CreateElement(name);

        gift.SetAttribute("id", ID.ToString());
        gift.SetAttribute("name", Name);
        gift.SetAttribute("atlas", Atlas);
        gift.SetAttribute("sprite", Sprite);
        if (!string.IsNullOrEmpty(m_Sp))
        {
            gift.SetAttribute("sp", m_Sp);
        }
        if (!string.IsNullOrEmpty(m_Money))
        {
            gift.SetAttribute("money", m_Money);
        }
        if (!string.IsNullOrEmpty(m_NeedLevel))
        {
            gift.SetAttribute("needLevel", m_NeedLevel);
        }
        if (!string.IsNullOrEmpty(m_Value0))
        {
            gift.SetAttribute("value0", m_Value0);
        }

        if (!string.IsNullOrEmpty(m_Value1))
        {
            gift.SetAttribute("value1", m_Value1);
        }

        parent.AppendChild(gift);
    }
    public void SetID(int id)
    {
        ID = id;
    }
#endif
}