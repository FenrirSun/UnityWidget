using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;

public static class SkillEditorConfig {

    public static Rect skillListArea;
    public static Rect skillIntroduceArea;
    public static Rect chainIntroduceArea;
    public static Rect skillDetailInfoArea;
    public static Rect skillItemSize;
    public static Rect skillFunctionButtonArea;
    public static Rect skillRangeArea;
    public static Rect skillGiftArea;
    public static Vector2 skillEditorWindowSize;

    static SkillEditorConfig()
    {
        Init();
    }

    public static void Init()
    {
        XmlDocument xml = new XmlDocument();
        string path = Application.dataPath + "/EditResources/SkillEditorConfig.xml";
        xml.Load(path);
        if (xml.InnerXml != null)
        {
            XmlNode root = xml.SelectSingleNode("config") as XmlNode;
            Dictionary<string, object> data = Deserialize(root);


            skillListArea = (Rect)data["skillListArea"];
            skillIntroduceArea = (Rect)data["skillIntroduceArea"];
            skillDetailInfoArea = (Rect)data["skillDetailInfoArea"];
            skillItemSize = (Rect)data["skillItemSize"];
            skillEditorWindowSize = (Vector2)data["skillEditorWindowSize"];
            skillFunctionButtonArea = (Rect)data["skillFunctionButtonArea"];
            chainIntroduceArea = (Rect)data["chainIntroduceArea"];
            skillRangeArea = (Rect)data["skillRangeArea"];
            skillGiftArea = (Rect)data["skillGiftArea"];
        }
    }

    private static Dictionary<string, object> Deserialize(XmlNode node)
    {
        Dictionary<string, object> param = new Dictionary<string, object>();
        foreach(XmlElement it in node.ChildNodes)
        {
            switch(it.Name)
            {
                case "Rect":
                    {
                        if(!param.ContainsKey(it.GetAttribute("Name")))
                        {
                            param.Add(it.GetAttribute("Name"),StringToRect(it.GetAttribute("Value")));
                        }
                        break;
                    }
                case "Vector2":
                    {
                        if (!param.ContainsKey(it.GetAttribute("Name")))
                        {
                            param.Add(it.GetAttribute("Name"), StringToVector2(it.GetAttribute("Value")));
                        }
                        break;
                    }
                case "Vector3":
                    {
                        if (!param.ContainsKey(it.GetAttribute("Name")))
                        {
                            param.Add(it.GetAttribute("Name"), StringToVector3(it.GetAttribute("Value")));
                        }
                        break;
                    }
                case "Float":
                    {
                        if (!param.ContainsKey(it.GetAttribute("Name")))
                        {
                            param.Add(it.GetAttribute("Name"), StringToFloat(it.GetAttribute("Value")));
                        }
                        break;
                    }
                case "Int":
                    {
                        if (!param.ContainsKey(it.GetAttribute("Name")))
                        {
                            param.Add(it.GetAttribute("Name"), StringToInt(it.GetAttribute("Value")));
                        }
                        break;
                    }
            }
        }

        return param;
    }

    private static Vector2 StringToVector2(string str)
    {
        string[] val = str.Split(',');
        if(val.Length == 2)
        {
            return new Vector2(float.Parse(val[0]), float.Parse(val[1]));
        }
        else
        {
            Debug.LogError("错误字符串！！！");
            return Vector2.zero;
        }
    }
    private static Vector3 StringToVector3(string str)
    {
        string[] val = str.Split(',');
        if (val.Length == 3)
        {
            return new Vector3(float.Parse(val[0]), float.Parse(val[1]), float.Parse(val[2]));
        }
        else
        {
            Debug.LogError("错误字符串！！！");
            return Vector3.zero;
        }
    }
    private static Rect StringToRect(string str)
    {
        string[] val = str.Split(',');
        if (val.Length == 4)
        {
            return new Rect(float.Parse(val[0]), float.Parse(val[1]), float.Parse(val[2]), float.Parse(val[3]));
        }
        else
        {
            Debug.LogError("错误字符串！！！");
            return new Rect(0,0,100,100);
        }
    }
    private static float StringToFloat(string str)
    {
        return float.Parse(str);
    }
    private static int StringToInt(string str)
    {
        return int.Parse(str);
    }
}
