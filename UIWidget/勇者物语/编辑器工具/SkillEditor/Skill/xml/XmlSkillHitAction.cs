using System;
using System.Xml;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillHitAction
{
    public CXmlSkillDir XmlDir { get; private set; }
    //public CXmlSkillHitRoute XmlHitRoute { get; private set; }
    private Dictionary<string, CXmlSkillHitRoute> XmlHitRoutes;

    public CXmlSkillHitAction() { }

    public void Init(XmlElement ele)
    {
        XmlHitRoutes = new Dictionary<string, CXmlSkillHitRoute>();
        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "dir":
                    {
                        string[] szPermission = { "self", "twoPoint", "event" };
                        XmlDir = new CXmlSkillDir();
                        XmlDir.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
                case "hitRoute":
                    {
                        CXmlSkillHitRoute XmlHitRoute = new CXmlSkillHitRoute();
                        XmlHitRoute.Init(node as XmlElement);

                        if (XmlHitRoutes.ContainsKey(XmlHitRoute.Where))
                        {
                            Debug.LogError(XmlHitRoute.Where + " duplicate!");
                        }
                        XmlHitRoutes.Add(XmlHitRoute.Where, XmlHitRoute);
                    }
                    break;
            }
        }
    }

    public CXmlSkillHitRoute GetRoute(string where)
    {
        CXmlSkillHitRoute ret = null;
        XmlHitRoutes.TryGetValue(where, out ret);
        return ret;
    }

#if UNITY_EDITOR
    private string[] m_Where = new string[3] { "land", "air", "fall" };
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("受击行为:", GUILayout.Width(60));

        if (XmlHitRoutes == null)
        {
            XmlHitRoutes = new Dictionary<string, CXmlSkillHitRoute>();
        }
        if (XmlDir == null) 
        {
            XmlDir = new CXmlSkillDir();
        }
        string key = "";
        foreach(KeyValuePair<string,CXmlSkillHitRoute> it in XmlHitRoutes)
        {
            GUILayout.BeginHorizontal();
            it.Value.Draw();
            if(GUILayout.Button("Delete"))
            {
                key = it.Key;
            }
            GUILayout.EndHorizontal();
        }


        if (!string.IsNullOrEmpty(key) && XmlHitRoutes.ContainsKey(key))
        {
            XmlHitRoutes.Remove(key);
        }

        if (GUILayout.Button("添加HitRoute")) 
        {
            if (XmlHitRoutes.Count >= 3)
            {
                EditorUtility.DisplayDialog("注意","最大个数为三个","知不知道");
            }
            else
            {
                if (!XmlHitRoutes.ContainsKey(m_Where[0]))
                {
                    CXmlSkillHitRoute route = new CXmlSkillHitRoute();

                    route.SetWhere(m_Where[0]);

                    XmlHitRoutes.Add(route.Where, route);
                }
                else
                {
                    if (!XmlHitRoutes.ContainsKey(m_Where[1]))
                    {
                        CXmlSkillHitRoute route = new CXmlSkillHitRoute();

                        route.SetWhere(m_Where[1]);

                        XmlHitRoutes.Add(route.Where, route);
                    }
                    else
                    {
                        if (!XmlHitRoutes.ContainsKey(m_Where[2]))
                        {
                            CXmlSkillHitRoute route = new CXmlSkillHitRoute();

                            route.SetWhere(m_Where[2]);

                            XmlHitRoutes.Add(route.Where, route);
                        }
                        
                    }
                }
            }
        }
        XmlDir.Draw();
        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "hitAction")
    {
        XmlElement hitAction = doc.CreateElement(name);

        if (XmlDir != null)
        {
            XmlDir.Export(doc,hitAction);
        }
        if (XmlHitRoutes.Count > 0)
        {
            foreach(KeyValuePair<string, CXmlSkillHitRoute> item in XmlHitRoutes)
            {
                item.Value.Export(doc,hitAction);
            }
        }
        parent.AppendChild(hitAction);
    }
#endif
}