using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
public class CreateNewSkillWindow : EditorWindow
{
    private CXmlSkill m_Skill;
    private CXmlSkillChain m_Chain;
    private int m_ID = -1;
    private string m_Name = "New Skill!!!";
    private int m_SourcesID = -1;
    void OnGUI() 
    {
        if(m_Skill != null)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("创建新技能！！！");
            if (GameApp.GetSkillManager().xmlSkills.ContainsKey(m_ID) || m_ID == -1)
            {
                GUI.color = Color.red;
            }
            m_ID = EditorGUILayout.IntField("技能ID:", m_ID);
            GUI.color = Color.white;
            if (m_Name == "新技能") { GUI.color = Color.red; }

            m_Name = EditorGUILayout.TextField("技能名字:", m_Name);
            GUI.color = Color.white;

            if (GUILayout.Button("创建"))
            {
                if (m_ID == -1 || m_Name == "New Skill!!!")
                {
                    EditorUtility.DisplayDialog("注意", "请填写ID和名字！！！", "知道");
                }
                else
                {
                    CreateNewSkill();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            GUILayout.Label("拷贝已有技能！！！");
            if (!GameApp.GetSkillManager().xmlSkills.ContainsKey(m_SourcesID)) { GUI.color = Color.red; }
            m_SourcesID = EditorGUILayout.IntField("拷贝源技能ID:", m_SourcesID);
            GUI.color = Color.white;

            if (GameApp.GetSkillManager().xmlSkills.ContainsKey(m_ID)) { GUI.color = Color.red; }
            m_ID = EditorGUILayout.IntField("新技能ID:", m_ID);
            GUI.color = Color.white;

            if (m_Name == "New Skill!!!") { GUI.color = Color.red; }
            m_Name = EditorGUILayout.TextField("技能名字:", m_Name);
            GUI.color = Color.white;

            if (GUILayout.Button("拷贝"))
            {
                if (m_ID == -1 || m_Name == "New Skill!!!")
                {
                    EditorUtility.DisplayDialog("注意", "请填写ID和名字！！！", "知道");
                }
                else
                {
                    CopySkill();
                }
            }
            GUILayout.EndVertical();
        }
        else
        {
            if (m_Chain != null)
            {
                if (GameApp.GetSkillManager().xmlSkillChains.ContainsKey(m_Name) || m_Name == "New Skill!!!")
                {
                    GUI.color = Color.red;
                }

                m_Name = EditorGUILayout.TextField("技能名字:", m_Name);
                GUI.color = Color.white;

                if (m_ID == -1) { GUI.color = Color.red; }
                m_ID = EditorGUILayout.IntField("技能链ID:", m_ID);
                GUI.color = Color.white;

                if (GUILayout.Button("创建"))
                {
                    if (m_ID == -1 || m_Name == "New Skill!!!")
                    {
                        EditorUtility.DisplayDialog("注意", "请填写ID和名字！！！", "知道");
                    }
                    else
                    {
                        CreateNewChain();
                    }
                }
            }
        }
    }

    public void Init(CXmlSkill skill)
    {
        m_Skill = skill;
    }
    public void Init(CXmlSkillChain chain)
    {
        m_Chain = chain;
    }
    public void CreateNewSkill()
    {
        m_Skill.CreateEditor(m_ID, m_Name);
        m_Skill.XmlSkillChain.AddSkill(m_Skill);
        Close();
    }
    public void CopySkill()
    {
        if (GameApp.GetSkillManager().xmlSkills.ContainsKey(m_SourcesID) && !GameApp.GetSkillManager().xmlSkills.ContainsKey(m_ID))
        {
            CXmlSkill skill = new CXmlSkill(GameApp.GetSkillManager().xmlSkills[m_SourcesID].XmlSkillChain.XmlSkillCount, GameApp.GetSkillManager().xmlSkills[m_SourcesID].XmlSkillChain);
            skill.CreateEditor(m_ID, m_Name);
            skill.Copy(GameApp.GetSkillManager().xmlSkills[m_SourcesID]);
            skill.XmlSkillChain.AddSkill(skill);
            Close();
        }   
    }
    public void CreateNewChain()
    {
        m_Chain.CreateEditor(m_ID,m_Name);
        GameApp.GetSkillManager().xmlSkillChains.Add(m_Name,m_Chain);
        Close();
    }
}
