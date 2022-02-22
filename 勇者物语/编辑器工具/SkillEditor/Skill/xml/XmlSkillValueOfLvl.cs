using System.Collections.Generic;
using System;
using System.Xml;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Text;
#endif
public class CXmlSkillValueOfLvl<T>
{
    public string Affect { get; private set; }
    public int GiftID { get; private set; }
    private List<T> datas;
    private T data;

    public CXmlSkillValueOfLvl(XmlElement ele, string attr, T def)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        XmlNode node = kXmlRead.GetChild(attr);
        if (node != null)
        {
            kXmlRead.SetXmlElement(node as XmlElement);
            datas = new List<T>();
            kXmlRead.Vec("value", datas);
            //Debug.LogWarning("!!!!pyj!!!!!!!!!!!!!!!!!!!!!!attr=" + attr + "count = " + datas.Count);
            GiftID = kXmlRead.Int("giftID");
            Affect = kXmlRead.Str("affect");
#if UNITY_EDITOR
            m_Name = attr;
            datasStr = ListToString(datas);
            m_SetByGiftID = true;
            data = kXmlRead.Attr<T>(attr, def);
#endif
        }
        else
        {
            //string str = kXmlRead.Str(attr);
            //data = (T)Convert.ChangeType(str, typeof(T));
            data = kXmlRead.Attr<T>(attr, def);
        }
    }

    public T GetValue(CharacterBase character)
    {
        if (datas == null)
        {
            return data;
        }
        else
        {
			int lvl = 0;
			if(character.SkillInfo != null)
            	lvl = character.SkillInfo.GetGiftLvl(GiftID);
            return datas[lvl];
        }
    }

    public T GetValue(ServerCreature creature)
    {
        if (datas == null)
        {
          //  Debug.LogWarning("datas == null" );
            return data;
        }
        else
        {
          // Debug.LogWarning("datas != null" + GiftID);
            CXmlSkillGift xmlGift = GameApp.GetSkillManager().GetXmlSkillGift(GiftID);
            int giftLevel = 0;
            if (creature.objType == (uint)SceneObject.ObjectType.OBJECT_PLAYER)
            {
                foreach (var info in ((ServerPlayer)creature).skillInfoList)
                {
                    if (info.GiftID == GiftID)
                    {
                        giftLevel = info.GiftLevel;
                       // Debug.LogWarning(giftLevel + " ID " + GiftID);
                        break;
                    }
                }
            }

            if (giftLevel < 0 || giftLevel >= datas.Count)
            {
                Debug.LogError("Current Lvl error!" + xmlGift.CurLvl);
            }
         //   Debug.Log(xmlGift.CurLvl + "," + giftLevel);
         //   Debug.LogWarning("!!!! Getvalue" + datas[giftLevel]);
            return datas[giftLevel];
            
        }
    }

    public KeyValuePair<T,int> GetskillBuffValue(ServerCreature creature)//如果技能存在Buff；
    {
        
        if (datas == null)
        {
            //  Debug.LogWarning("datas == null" );
            return new KeyValuePair<T, int>(data, 0);
        }
        else
        {
            // Debug.LogWarning("datas != null" + GiftID);
            CXmlSkillGift xmlGift = GameApp.GetSkillManager().GetXmlSkillGift(GiftID);
            int giftLevel = 0;
            int max_level = 0;
            if (creature.objType == (uint)SceneObject.ObjectType.OBJECT_PLAYER)
            {
                foreach (var info in ((ServerPlayer)creature).skillInfoList)
                {
                    if (info.GiftID == GiftID)
                    {
                        
                        giftLevel = info.GiftLevel;
                        // Debug.LogWarning(giftLevel + " ID " + GiftID);
                        if (ShenQiBuff.Intance().skillbuff.ContainsKey(GiftID))
                        {
                            giftLevel = ShenQiBuff.Intance().skillbuff[GiftID] + giftLevel;
                            if (giftLevel >= datas.Count)
                            {
                                giftLevel = datas.Count - 1;
                                max_level = giftLevel - datas.Count + 1;
                            }
                        }
                        break;
                    }
                }
            }

            if (giftLevel < 0 || giftLevel >= datas.Count)
            {
                Debug.LogError("Current Lvl error!" + xmlGift.CurLvl);
            }
            //   Debug.Log(xmlGift.CurLvl + "," + giftLevel);
            //   Debug.LogWarning("!!!! Getvalue" + datas[giftLevel]);
            return new KeyValuePair<T,int>(datas[giftLevel],max_level);

        }
    }


#if UNITY_EDITOR
    public bool m_SetByGiftID = false;
    private string datasStr = "";
    private string m_Name = "name";
    
    public enum AffectType
    {
        giftlvl,
        playerlvl,
        petSkilllvl,
    }

    public void SetName(string name)
    {
        m_Name = name;
    }
    public void Draw()
    {
        GUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField(m_Name + ":", GUILayout.Width(80));
        EditorGUILayout.LabelField("是否受其它影响:", GUILayout.Width(90));
        m_SetByGiftID = EditorGUILayout.Toggle(m_SetByGiftID);
        if (m_SetByGiftID)
        {
            EditorGUILayout.LabelField("天赋id:", GUILayout.Width(60));
            GiftID = EditorGUILayout.IntField(GiftID);
            if (string.IsNullOrEmpty(Affect))
            {
                Affect = AffectType.giftlvl.ToString();
            }
            EditorGUILayout.LabelField("影响类型:", GUILayout.Width(70));
            Affect = EditorGUILayout.EnumPopup((AffectType)System.Enum.Parse(typeof(AffectType), Affect)).ToString();


            Color color = GUI.color;
            if (datasStr.Contains("  ") || datasStr.Contains(",,"))
            {
                GUI.color = Color.red;
            }
            string ty = "";
            if (datas != null && datas.Count > 0)
            {
                
                if (datas[0] is int)
                {
                    ty = "整数";
                }
                else if (datas[0] is float)
                {
                    ty = "小数";
                }
                else if (datas[0] is string)
                {
                    ty = "字符串";
                }
                else if (datas[0] is bool)
                {
                    ty = "Bool";
                }
            }

            EditorGUILayout.LabelField("Value(" + ty + "):", GUILayout.Width(90));
            datasStr = EditorGUILayout.TextField(datasStr);
            GUI.color = color;
            //StringToList(datasStr);

        }
        else
        {
            if (data is int)
            {
                EditorGUILayout.LabelField("Value(int):", GUILayout.Width(70));
                data = (T)Convert.ChangeType(EditorGUILayout.IntField(int.Parse(data.ToString())), typeof(T));
            }
            else if (data is float)
            {
                EditorGUILayout.LabelField("Value(float):", GUILayout.Width(70));
                data = (T)Convert.ChangeType(EditorGUILayout.FloatField(float.Parse(data.ToString())), typeof(T));
            }
            else if (data is string)
            {
                EditorGUILayout.LabelField("Value(String):", GUILayout.Width(70));
                data = (T)Convert.ChangeType(EditorGUILayout.TextField(data.ToString()), typeof(T));
            }
            else if (data is bool)
            {
                EditorGUILayout.LabelField("Value(Bool):", GUILayout.Width(70));
                data = (T)Convert.ChangeType(EditorGUILayout.Toggle((bool)Convert.ChangeType(data.ToString(), typeof(bool))), typeof(T));
            }
        }
        GUILayout.EndHorizontal();

    }

    public CXmlSkillValueOfLvl(string attr, T def)
    {
        m_Name = attr;
        data = def;
    }

    public void Export(XmlDocument doc, XmlNode parent,string name)
    {
        if (m_SetByGiftID)
        {
            if(datas != null)
            {
                XmlElement level = doc.CreateElement(name);
                level.SetAttribute("value", ListToString(datas));
                level.SetAttribute("affect", Affect);
                level.SetAttribute("giftID", GiftID.ToString());

                parent.AppendChild(level);
            }
        }
        else
        {
            if (data != null)
            {
                ((XmlElement)parent).SetAttribute(name, data.ToString());
            }
        }
    }

    private string ListToString(List<T> list)
    {
        StringBuilder str = new StringBuilder();
        foreach (var it in list)
        {
            str.Append(it.ToString());
            str.Append(" ");
        }
        return str.ToString();
    }

    private void StringToList(string strs)
    {
        if (strs.Contains("  ") || strs.Contains(",,"))
        {
            strs = "";
        }
        if (strs.Length > 0)
        {
            datas = new List<T>();
            string[] result = strs.Split(new char[] { ' ', ',' });
            foreach (string str in result)
            {
                datas.Add((T)Convert.ChangeType(str, typeof(T)));
            }
        }
    }
#endif

}