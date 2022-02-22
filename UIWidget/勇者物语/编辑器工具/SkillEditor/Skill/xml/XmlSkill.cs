using System.Collections.Generic;
using System;
using System.Xml;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
public class CXmlSkill
{
    public int ID { get; private set; }
    public int PhaseIndex { get; private set; }
    public CXmlSkillChain XmlSkillChain { get; private set; }
    public string Name { get; private set; }
    public string Text { get; private set; }
    public string Desc { get; private set; }

    private List<IChecker> checkers;
    private Dictionary<int, CXmlSkillEvent> events;

    public CXmlSkill(int nPhaseIndex, CXmlSkillChain xmlSkillChain)
    {
        PhaseIndex = nPhaseIndex;
        XmlSkillChain = xmlSkillChain;
        checkers = new List<IChecker>();
        events = new Dictionary<int, CXmlSkillEvent>();
    }

    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        ID = kXmlRead.Int("id");
        Name = kXmlRead.Str("name");
        Text = kXmlRead.Str("text");
        Desc = kXmlRead.Str("desc");

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "checkers":
                    {
                        foreach (XmlNode nodeChecker in node.ChildNodes)
                        {
                            if (nodeChecker.Name == "checker")
                            {
                                string szName = new CXmlRead(nodeChecker as XmlElement).Str("name");
                                IChecker checker = Assembly.GetExecutingAssembly().CreateInstance("C" + szName + "Checker") as IChecker;
                                if (checker != null)
                                {
                                    checker.Init(nodeChecker as XmlElement);
                                    checkers.Add(checker);
                                }
                                else
                                {
                                    Debug.Log(szName + "Checker not found!");
                                }
                            }
                        }
                    }
                    break;
                case "events":
                    {
                        foreach (XmlNode nodeEvent in node.ChildNodes)
                        {
							if(nodeEvent.Name == "event")
							{
	                            CXmlSkillEvent xmlSkillEvent = new CXmlSkillEvent();
	                            xmlSkillEvent.Init(nodeEvent as XmlElement);
	                            if (events.ContainsKey(xmlSkillEvent.Index))
	                            {
	                                Debug.LogError("event index " + xmlSkillEvent.Index + "duplicate!");
	                            }
	                            else
	                            {
	                                events.Add(xmlSkillEvent.Index, xmlSkillEvent);
	                            }
							}
                        }
                    }
                    break;
            }
        }
    }

    public CXmlSkillEvent GetEvent(int index)
    {
        CXmlSkillEvent ret = null;
        events.TryGetValue(index,out ret);
        if (ret == null )
        {
            Debug.LogError("event "+ index +"not exist!");
        }
        return ret;
   }

    public bool CanUse()
    {
        bool bRet = true;
        foreach (IChecker checker in checkers)
        {
            if (!checker.Check())
            {
                bRet = false;
                break;
            }
        }
        return bRet;
    }

#if UNITY_EDITOR
    private bool m_DrawChecker = true;
    private bool m_DrawEvent = true;
    private bool m_DrawSkill = true;
    private Vector2 eventScrollPos = Vector2.zero;
    private int m_RemoveID = -1;
    public string m_FileName = "";
    public bool m_Export;
    public void Draw()
    {
        GUILayout.BeginVertical("box");
        m_DrawSkill = EditorGUILayout.Foldout(m_DrawSkill, "Skill");
        if (m_DrawSkill)
        {
            EditorGUILayout.Separator();
            GUILayout.BeginVertical("box");
            if (GameApp.GetSkillManager().xmlSkills.ContainsKey(ID) && GameApp.GetSkillManager().xmlSkills[ID] != this)
            {
                GUI.color = Color.red;
            }
            ID = EditorGUILayout.IntField("技能ID", ID);

            GUI.color = Color.white;
            Name = EditorGUILayout.TextField("技能名字：", Name);
            Desc = EditorGUILayout.TextField("描述：", Desc);
            Text = EditorGUILayout.TextField("内容：", Text);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            m_DrawChecker = EditorGUILayout.Foldout(m_DrawChecker, " Checkers");
            if (m_DrawChecker)
            {
                foreach (IChecker check in checkers)
                {
                    GUILayout.BeginHorizontal("box");

                    GUILayout.Label("      " + check.GetType().Name);

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            EditorGUILayout.Separator();
            GUILayout.BeginVertical("box");
            m_DrawEvent = EditorGUILayout.Foldout(m_DrawEvent, " Events");
            eventScrollPos = GUILayout.BeginScrollView(eventScrollPos);

            if (m_DrawEvent)
            {
                foreach (KeyValuePair<int, CXmlSkillEvent> skillevent in events)
                {
                    GUILayout.BeginHorizontal("box");
                    GUILayout.BeginVertical();

                    //skillevent.Value.Index = EditorGUILayout.IntField("ID:", skillevent.Value.Index);
                    skillevent.Value.Draw();

                    GUILayout.EndVertical();
                    if (GUILayout.Button("Delete"))
                    {
                        if(EditorUtility.DisplayDialog("删除事件","确定要删除事件么!!!!","删除","取消"))
                        {
                            m_RemoveID = skillevent.Key;
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                if(GUILayout.Button("添加事件"))
                {
                    AddNewEvent();
                }
            }

            if(m_RemoveID != -1)
            {
                events.Remove(m_RemoveID);
                m_RemoveID = -1;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

        }
        GUILayout.EndVertical();
    }

    public void DrawTitle()
    {
        //GUILayout.Label("技能ID：" + ID.ToString());
        if (GameApp.GetSkillManager().xmlSkills.ContainsKey(ID) && GameApp.GetSkillManager().xmlSkills[ID] != this)
        {
            GUI.color = Color.red;
        }
        ID = EditorGUILayout.IntField("技能ID", ID);

        GUI.color = Color.white;
        Name = EditorGUILayout.TextField("技能名字：", Name);
        Desc = EditorGUILayout.TextField("描述：", Desc);
        Text = EditorGUILayout.TextField("内容：", Text);
    }

    private void AddNewEvent()
    {
        List<int> ids = new List<int>();

        ids = (from skillevent in events orderby skillevent.Key select skillevent.Key).ToList();

        Dictionary<int, CXmlSkillEvent> skillEvents = new Dictionary<int, CXmlSkillEvent>(events);
        events.Clear();

        int max = 0;
        foreach (var id in ids)
        {
            events.Add(skillEvents[id].Index, skillEvents[id]);
            if(max < id)
            {
                max = id;
            }
        }
        CXmlSkillEvent newEvent = new CXmlSkillEvent(++max);
        events.Add(newEvent.Index, newEvent);
    }

    public void CreateEditor(int id,string name)
    {
        ID = id;
        Name = name;
    }
    public Dictionary<int, CXmlSkillEvent> GetEvents()
    {
        if (events == null) { events = new Dictionary<int, CXmlSkillEvent>(); }
        return events;
    }
    public void Copy(CXmlSkill skill)
    {
        Text = skill.Text;
        Desc = skill.Desc;
        checkers = new List<IChecker>(skill.checkers);
        events = new Dictionary<int, CXmlSkillEvent>();
        foreach(var it in skill.GetEvents())
        {
            events.Add(it.Value.Index, it.Value);
        }
    }

    public void Export()
    {
        if (!m_Export) { return; }
        string path = "";
        if (string.IsNullOrEmpty(m_FileName))
        {
            path = Application.dataPath + "/Resources/DB/Skill/" + Name + ".xml";
        }
        else
        {
            path = Application.dataPath + "/Resources/DB/Skill/" + m_FileName + ".xml";
        }
        //string path = Application.dataPath + "/Resources/DB/Skill/" + Name + ".xml";

        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
        if (!System.IO.File.Exists(path))
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);

            XmlElement _root = doc.CreateElement("skills");
            doc.AppendChild(_root);

            if (!Application.isWebPlayer)
            {
                doc.Save(path);
            }
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        XmlNode root = xmlDoc.SelectSingleNode("skills") as XmlNode;

        //-------------------------------------------------------
        XmlElement skill = xmlDoc.CreateElement("skill");

        skill.SetAttribute("id",ID.ToString());
        skill.SetAttribute("name", Name);
        skill.SetAttribute("textID", Text);
        skill.SetAttribute("descID", Desc);

        XmlElement docEvents = xmlDoc.CreateElement("events");

        foreach (KeyValuePair<int, CXmlSkillEvent> item in events)
        {
            item.Value.Export(xmlDoc, docEvents);
        }

        skill.AppendChild(docEvents);

        root.AppendChild(skill);


        if (!Application.isWebPlayer)
        {
            xmlDoc.Save(path);
        }
    }
#endif
}