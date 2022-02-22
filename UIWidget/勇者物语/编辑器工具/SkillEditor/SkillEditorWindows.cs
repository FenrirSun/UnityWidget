using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using System.Linq;

public class SkillEditorWindows : EditorWindow
{
    [MenuItem("TNN/技能编辑器")]
    static void SkillEditor()
    {
        EditorWindow.GetWindow<SkillEditorWindows>();
    }
    [MenuItem("TNN/打开保存路径")]
    static void OpenSkillPath()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath + "/Skill");
    }
    public void OnEnable()
    {
        title = "技能编辑器";
        //maxSize = new Vector2(900,900);
        minSize = SkillEditorConfig.skillEditorWindowSize;
        Init();
    }
    private Vector2 skillListScrollPos = new Vector2(0, 0);
    private int selectedSkillID = -1;
    private int selectedSkillRangeIndex = -1;
    private CXmlSkillChain CurrectChain = null;
    private Vector2 m_giftValueScrollPos = new Vector2(0, 0);
    
    private void Init()
    {
        GameApp.Instance();
        GameApp.Instance().InitEditor();
        SkillEditorConfig.Init();

        Sort();
        OnGUI();
    }
    private bool m_Init = false;
    private void Sort()
    {
        List<int> ids = new List<int>();

        ids = (from skill in GameApp.GetSkillManager().xmlSkills orderby skill.Key select skill.Key).ToList();

        Dictionary<int, CXmlSkill> skills = new Dictionary<int,CXmlSkill>(GameApp.GetSkillManager().xmlSkills);
        GameApp.GetSkillManager().xmlSkills.Clear();

        foreach (var id in ids)
        {
            GameApp.GetSkillManager().AddXmlSkill(skills[id]);
        }
    }

    public void OnGUI()
    {
        if (m_Init == false)
        {
            foreach (KeyValuePair<int, CXmlSkill> item in GameApp.GetSkillManager().xmlSkills)
            {
                item.Value.Draw();
            }
            m_Init = true;
            return;
        }

        DrawChainList();
        if(m_DrawSkillChain)
        {
            DrawChain();
            DrawSkillInfo();
        }
        if(m_DrawSkillRange)
        {
            DrawRange();
        }
        if(m_DrawSkillGift)
        {
            DrawGift();
        }
        DrawFunctionButton();
    }

    private bool m_DrawSkillRange = false;
    private bool m_DrawSkillChain = false;
    private bool m_DrawSkillRangePre = false;
    private bool m_DrawSkillChainPre = false;

    private bool m_DrawSkillGift = false;
    private bool m_DrawSkillGiftPre = false;
    public void DrawTitle()
    {

    }

    private void DrawChainList() 
    {
        GUILayout.BeginArea(SkillEditorConfig.skillListArea);
        GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
        skillListScrollPos = EditorGUILayout.BeginScrollView(skillListScrollPos);

        m_DrawSkillRangePre = m_DrawSkillRange;
        m_DrawSkillChainPre = m_DrawSkillChain;
        m_DrawSkillGiftPre = m_DrawSkillGift;
        //------------------------------------------------------------------------
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();
        m_DrawSkillRange = EditorGUILayout.ToggleLeft("Range", m_DrawSkillRange);

        if (m_DrawSkillRange == true && m_DrawSkillRangePre == false)
        {
            m_DrawSkillChain = false;
            m_DrawSkillGift = false;
        }
        if (m_DrawSkillRange)
        {
            GUI.color = Color.green;
            if (GUILayout.Button("添加范围"))
            {
                GameApp.GetSkillManager().xmlSkillRanges.Add(new CXmlSkillRange());
            }
            GUI.color = Color.white;
        }

        GUILayout.EndHorizontal();
        if (m_DrawSkillRange) 
        {
            for (int i = 0; i < GameApp.GetSkillManager().xmlSkillRanges.Count; i++ )
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Range:" + GameApp.GetSkillManager().xmlSkillRanges[i].TargetType))
                {
                    selectedSkillRangeIndex = i;
                }
                if (GUILayout.Button("Delete")) 
                {
                    GameApp.GetSkillManager().xmlSkillRanges.RemoveAt(i);
                    selectedSkillRangeIndex = -1;
                }
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndVertical();

        //----------------------------------------------------------------------------
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();
        m_DrawSkillChain = EditorGUILayout.ToggleLeft("Chain", m_DrawSkillChain);
        if(m_DrawSkillChain == true && m_DrawSkillChainPre == false)
        {
            m_DrawSkillRange = false;
            m_DrawSkillGift = false;
        }

        if(m_DrawSkillChain)
        {
            if (GUILayout.Button("添加技能链", GUILayout.Height(SkillEditorConfig.skillItemSize.height)))
            {
                CreateNewSkillWindow window = (CreateNewSkillWindow)EditorWindow.GetWindowWithRect(typeof(CreateNewSkillWindow), new Rect(Screen.width * 0.5f - 150, position.y - 150, 300, 300), true);

                window.Init(new CXmlSkillChain());
            }
            if (GUILayout.Button("删除当前选中技能链", GUILayout.Height(SkillEditorConfig.skillItemSize.height)))
            {
                if (CurrectChain != null)
                {
                    if (EditorUtility.DisplayDialog("删除技能", "确定删除当前技能\n这可是你的劳动成果呀!!!", "确定", "取消"))
                    {
                        //GameApp.GetSkillManager().xmlSkills.Remove(selectedSkillID);
                        //selectedSkillID = -1;
                        GameApp.GetSkillManager().xmlSkillChains.Remove(CurrectChain.Name);
                        CurrectChain = null;
                        selectedSkillID = -1;
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("注意", "请选择要删除的技能链！", "我懂了");
                }
            }

            GUILayout.EndHorizontal();
            foreach (KeyValuePair<string, CXmlSkillChain> item in GameApp.GetSkillManager().xmlSkillChains)
            {
                //item.Value.Draw();

                GUILayout.BeginVertical("box");

                item.Value.m_PreDraw = item.Value.m_Draw;

                if (CurrectChain == item.Value)
                {
                    GUI.color = Color.green;
                }

                //m_Draw = EditorGUILayout.Foldout(m_Draw, Name);
                GUILayout.BeginHorizontal();


                item.Value.m_Draw = EditorGUILayout.ToggleLeft(item.Value.Index + ":  " + item.Value.Name, item.Value.m_Draw);
                if (CurrectChain == item.Value)
                {
                    if (GUILayout.Button("添加技能"))
                    {
                        CreateNewSkillWindow window = (CreateNewSkillWindow)EditorWindow.GetWindowWithRect(typeof(CreateNewSkillWindow), new Rect(Screen.width * 0.5f - 150, position.y - 150, 300, 300), true);

                        window.Init(new CXmlSkill(item.Value.XmlSkillCount, item.Value));
                    }
                }
                GUILayout.EndHorizontal();
                if (item.Value.m_PreDraw == false && item.Value.m_Draw == true)
                {
                    CurrectChain = item.Value;
                    if (CurrectChain.XmlSkillCount > 0)
                    {
                        selectedSkillID = CurrectChain.GetXmlSkill(0).ID;
                    }
                    else
                    {
                        selectedSkillID = -1;
                    }
                }
                else
                {
                    if (CurrectChain != item.Value)
                    {
                        item.Value.m_Draw = false;
                    }
                }
                //else if (item.Value.m_PreDraw == true && item.Value.m_Draw == false)
                //{
                //    Currect = null;
                //}
                GUI.color = Color.white;
                if (item.Value.m_Draw)
                {
                    GUILayout.BeginVertical("box");

                    for (int i = 0; i < item.Value.XmlSkillCount; i++)
                    {
                        if (selectedSkillID == item.Value.GetXmlSkill(i).ID)
                        {
                            GUI.color = Color.green;
                        }
                        GUILayout.BeginHorizontal();

                        item.Value.GetXmlSkill(i).m_Export = EditorGUILayout.Toggle(item.Value.GetXmlSkill(i).m_Export,GUILayout.Width(15));

                        if (GUILayout.Button("ID:" + item.Value.GetXmlSkill(i).ID + "  " + item.Value.GetXmlSkill(i).Name, GUILayout.Height(SkillEditorConfig.skillItemSize.height)))
                        {
                            selectedSkillID = item.Value.GetXmlSkill(i).ID;
                        }
                        
                        if(GUILayout.Button("Delete"))
                        {
                            item.Value.RemoveSkill(selectedSkillID);
                            selectedSkillID = -1;
                        } 
                        GUI.color = Color.white;

                        GUILayout.EndHorizontal();
                    } 

                    GUILayout.EndVertical();
                }


                GUILayout.EndVertical();
            }
        }
        GUILayout.EndVertical();
        //----------------------------------------------------------------------------------
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        m_DrawSkillGift = EditorGUILayout.ToggleLeft("SkillGift", m_DrawSkillGift);
        
        if (m_DrawSkillGift == true && m_DrawSkillGiftPre == false)
        {
            m_DrawSkillChain = false;
            m_DrawSkillRange = false;
        }

        if (m_DrawSkillGift)
        {
            if (GameApp.GetSkillManager().xmlSkillJobGiftGroups.ContainsKey(m_newGroupsID))
            {
                GUI.color = Color.red;
            }
            m_newGroupsID = EditorGUILayout.IntField(m_newGroupsID);

            GUI.color = Color.white;
            if (GUILayout.Button("添加"))
            {
                if (!GameApp.GetSkillManager().xmlSkillJobGiftGroups.ContainsKey(m_newGroupsID))
                {
                    if (GameApp.GetSkillManager().xmlSkillJobGiftGroups == null)
                    {
                        GameApp.GetSkillManager().xmlSkillJobGiftGroups = new Dictionary<int, List<CXmlSkillGiftGroup>>();
                    }
                    GameApp.GetSkillManager().xmlSkillJobGiftGroups.Add(m_newGroupsID, new List<CXmlSkillGiftGroup>());
                }

            }
            GUILayout.EndHorizontal();
            if (GameApp.GetSkillManager().xmlSkillJobGiftGroups != null)
            {
                foreach (KeyValuePair<int, List<CXmlSkillGiftGroup>> it in GameApp.GetSkillManager().xmlSkillJobGiftGroups)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Job:" + it.Key);

                    m_newGroupName = EditorGUILayout.TextField(m_newGroupName);
                    GUI.color = Color.white;
                    if (GUILayout.Button("添加"))
                    {
                        CXmlSkillGiftGroup newGroup = new CXmlSkillGiftGroup(it.Key);
                        newGroup.SetChainName(m_newGroupName);
                        it.Value.Add(newGroup);
                    }
                    GUILayout.EndHorizontal();
                    for (int i = 0 ;i < it.Value.Count; i++ )
                    {
                        GUILayout.BeginHorizontal();
                        if (it.Value[i] == m_GiftGroup)
                        {
                            GUI.color = Color.green;
                        }
                        if(GUILayout.Button(it.Value[i].Chain))
                        {
                            m_GiftGroup = it.Value[i];
                        }
                        if(GUILayout.Button("Delete"))
                        {
                            it.Value.RemoveAt(i);
                        }
                        GUI.color = Color.white;
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
            }
        }
        GUILayout.EndVertical();
        //foreach (KeyValuePair<int, CXmlSkill> item in GameApp.GetSkillManager().xmlSkills)
        //{
        //    if (selectedSkillID == item.Key)
        //    {
        //        GUI.color = Color.green;
        //    }
        //    if (GUILayout.Button(item.Value.Name + "\n(ID:" + item.Value.ID + ")", GUILayout.Height(SkillEditorConfig.skillItemSize.height)))
        //        selectedSkillID = item.Value.ID;

        //    GUI.color = Color.white;
        //}
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public void DrawRange()
    {
        GUILayout.BeginArea(SkillEditorConfig.skillRangeArea);

        if (selectedSkillRangeIndex != -1 && selectedSkillRangeIndex < GameApp.GetSkillManager().xmlSkillRanges.Count)
        {
            GameApp.GetSkillManager().xmlSkillRanges[selectedSkillRangeIndex].Draw();
        }

        GUILayout.EndArea();
    }

    public void DrawSkillInfo()
    {
        //GUILayout.BeginArea(SkillEditorConfig.skillIntroduceArea);

        //GUILayout.BeginVertical("box");

        //if (selectedSkillID != -1 && GameApp.GetSkillManager().xmlSkills.ContainsKey(selectedSkillID))
        //{
        //    //GUILayout.Label("技能ID：" + GameApp.GetSkillManager().xmlSkills[selectedSkillID].ID.ToString());
        //    //GameApp.GetSkillManager().xmlSkills[selectedSkillID].Name = EditorGUILayout.TextField("技能名字：", GameApp.GetSkillManager().xmlSkills[selectedSkillID].Name);
        //    //GameApp.GetSkillManager().xmlSkills[selectedSkillID].Desc = EditorGUILayout.TextField("描述：", GameApp.GetSkillManager().xmlSkills[selectedSkillID].Desc);
        //    //GameApp.GetSkillManager().xmlSkills[selectedSkillID].Text = EditorGUILayout.TextField("内容：", GameApp.GetSkillManager().xmlSkills[selectedSkillID].Text);
        //    GameApp.GetSkillManager().xmlSkills[selectedSkillID].DrawTitle();
        //}

        //GUILayout.EndVertical();

        //GUILayout.EndArea();


        GUILayout.BeginArea(SkillEditorConfig.skillDetailInfoArea);
        if (selectedSkillID != -1 && GameApp.GetSkillManager().xmlSkills.ContainsKey(selectedSkillID))
        {
            GameApp.GetSkillManager().xmlSkills[selectedSkillID].Draw();
        }
        GUILayout.EndArea();
    }

    public void DrawFunctionButton()
    {
        GUILayout.BeginArea(SkillEditorConfig.skillFunctionButtonArea);
        GUILayout.BeginHorizontal("box");
        //if (GUILayout.Button("添加技能链", GUILayout.Height(SkillEditorConfig.skillItemSize.height)))
        //{
        //    CreateNewSkillWindow window = (CreateNewSkillWindow)EditorWindow.GetWindowWithRect(typeof(CreateNewSkillWindow), new Rect(Screen.width * 0.5f - 150, position.y - 150, 300, 300), true);

        //    window.Init(new CXmlSkillChain());
        //}
        //if (GUILayout.Button("删除当前选中技能链", GUILayout.Height(SkillEditorConfig.skillItemSize.height)))
        //{
        //    if (CurrectChain != null)
        //    {
        //        if (EditorUtility.DisplayDialog("删除技能", "确定删除当前技能\n这可是你的劳动成果呀!!!", "确定", "取消"))
        //        {
        //            //GameApp.GetSkillManager().xmlSkills.Remove(selectedSkillID);
        //            //selectedSkillID = -1;
        //            GameApp.GetSkillManager().xmlSkillChains.Remove(CurrectChain.Name);
        //            CurrectChain = null;
        //            selectedSkillID = -1;
        //        }
        //    }
        //    else
        //    {
        //        EditorUtility.DisplayDialog("注意", "请选择要删除的技能链！","我懂了");
        //    }
        //}

        if (GUILayout.Button("导出", GUILayout.Height(SkillEditorConfig.skillItemSize.height)))
        {
            Export();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    public void DrawChain()
    {
        GUILayout.BeginArea(SkillEditorConfig.chainIntroduceArea);

        GUILayout.BeginVertical("box");

        if (CurrectChain != null)
        {
            CurrectChain.Draw();
        }

        GUILayout.EndVertical();

        GUILayout.EndArea();
    }

    private CXmlSkillGiftGroup m_GiftGroup = null;
    private int m_newGroupsID = -1;
    private string m_newGroupName = "";
    public void DrawGift()
    {
        GUILayout.BeginArea(SkillEditorConfig.skillGiftArea);
        m_giftValueScrollPos = GUILayout.BeginScrollView(m_giftValueScrollPos);
        if(m_GiftGroup != null)
        {
            m_GiftGroup.Draw();
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    public void Export()
    {
        ExportSkill();
        ExportGift();
        ExportGiftName();
        ExportGiftCurrect();
        ExportGiftNext();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("导出", "导出成功!!!", "OK");
    }
    public void ExportSkill()
    {
        //if (Directory.Exists(Application.persistentDataPath + "/Skill")) 
        //{
        //    Directory.Delete(Application.persistentDataPath + "/Skill",true);
        //}

        //Directory.CreateDirectory(Application.persistentDataPath + "/Skill");
        //string path = Application.persistentDataPath + "/Skill/Skills.xml";

        //if (Directory.Exists(Application.dataPath + "/Resources/DB/Skill/Skills.xml"))
        //{
        //    Directory.Delete(Application.dataPath + "/Resources/DB/Skill/Skills.xml", true);
        //}

        //Directory.CreateDirectory(Application.dataPath + "/Resources/DB/Skill/Skills.xml");
        string path = Application.dataPath + "/Resources/DB/Skill/Skills.xml";
        if(File.Exists(path))
        {
            File.Delete(path);
        }
        if (!File.Exists(path))
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
        //------------------Range----------------------------
        XmlElement ranges = xmlDoc.CreateElement("ranges");
        foreach (var it in GameApp.GetSkillManager().xmlSkillRanges)
        {
            it.Export(xmlDoc,ranges);
        }
        root.AppendChild(ranges);
        //---------------------------------
        foreach (KeyValuePair<string, CXmlSkillChain> item in GameApp.GetSkillManager().xmlSkillChains)
        {
            item.Value.Export(xmlDoc, root);

        }
        //foreach (KeyValuePair<int, CXmlSkill> skill in GameApp.GetSkillManager().xmlSkills)
        //{
        //    skill.Value.Export();
        //}


        if (!Application.isWebPlayer)
        {
            xmlDoc.Save(path);
        }
         
    }
    public void ExportGift()
    {
        //string path = Application.persistentDataPath + "/Skill/SkillGift.xml";
        string path = Application.dataPath + "/Resources/DB/Skill/SkillGift.xml";

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (!File.Exists(path))
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);

            XmlElement _root = doc.CreateElement("skillSet");
            doc.AppendChild(_root);

            if (!Application.isWebPlayer)
            {
                doc.Save(path);
            }
        }
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        XmlNode root = xmlDoc.SelectSingleNode("skillSet") as XmlNode;

        foreach (KeyValuePair<int, List<CXmlSkillGiftGroup>> it in GameApp.GetSkillManager().xmlSkillJobGiftGroups)
        {
            XmlElement job = xmlDoc.CreateElement("job");

            job.SetAttribute("id", it.Key.ToString());

            foreach(var gift in it.Value)
            {
                gift.Export(xmlDoc, job);
            }

            root.AppendChild(job);
        }

        if (!Application.isWebPlayer)
        {
            xmlDoc.Save(path);
        }
    }
    public void ExportGiftName()
    {
        //string path = Application.persistentDataPath + "/PlayerGiftNameLocal.xml";
        string path = Application.dataPath + "/Resources/DB/Localization/Chinese/PlayerGiftNameLocal.xml";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (!File.Exists(path))
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);

            XmlElement _root = doc.CreateElement("root");
            doc.AppendChild(_root);

            if (!Application.isWebPlayer)
            {
                doc.Save(path);
            }
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        XmlNode root = xmlDoc.SelectSingleNode("root") as XmlNode;

        foreach(var it in GameApp.GetSkillManager().xmlSkillGifts)
        {
            XmlElement row = xmlDoc.CreateElement("row");

            row.SetAttribute("id", it.Value.ID.ToString());
            row.SetAttribute("text",it.Value.m_Name);

            root.AppendChild(row);
        }

        if (!Application.isWebPlayer)
        {
            xmlDoc.Save(path);
        }
    }
    public void ExportGiftCurrect()
    {
        //string path = Application.persistentDataPath + "/PlayerGiftCurrentLevelNoteLocal.xml";
        string path = Application.dataPath + "/Resources/DB/Localization/Chinese/PlayerGiftCurrentLevelNoteLocal.xml";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (!File.Exists(path))
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);

            XmlElement _root = doc.CreateElement("root");
            doc.AppendChild(_root);

            if (!Application.isWebPlayer)
            {
                doc.Save(path);
            }
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        XmlNode root = xmlDoc.SelectSingleNode("root") as XmlNode;

        foreach (var it in GameApp.GetSkillManager().xmlSkillGifts)
        {
            XmlElement row = xmlDoc.CreateElement("row");

            row.SetAttribute("id", it.Value.ID.ToString());
            row.SetAttribute("text", it.Value.m_CurrectNote);

            root.AppendChild(row);
        }

        if (!Application.isWebPlayer)
        {
            xmlDoc.Save(path);
        }
    }
    public void ExportGiftNext() 
    {
        //string path = Application.persistentDataPath + "/PlayerGiftNextLevelNoteLocal.xml";
        string path = Application.dataPath + "/Resources/DB/Localization/Chinese/PlayerGiftNextLevelNoteLocal.xml";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (!File.Exists(path))
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(dec);

            XmlElement _root = doc.CreateElement("root");
            doc.AppendChild(_root);

            if (!Application.isWebPlayer)
            {
                doc.Save(path);
            }
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        XmlNode root = xmlDoc.SelectSingleNode("root") as XmlNode;

        foreach (var it in GameApp.GetSkillManager().xmlSkillGifts)
        {
            XmlElement row = xmlDoc.CreateElement("row");

            row.SetAttribute("id", it.Value.ID.ToString());
            row.SetAttribute("text", it.Value.m_NextNote);

            root.AppendChild(row);
        }

        if (!Application.isWebPlayer)
        {
            xmlDoc.Save(path);
        }
    }
    public void OnDestroy()
    {
        //EditorUtility.DisplayDialog("注意","确定要退出么，要不要先保存一下呢！！！","确定退出","取消退出");
    }
}
