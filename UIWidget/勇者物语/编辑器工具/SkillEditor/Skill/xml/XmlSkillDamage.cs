using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillDamage {
    public List<CXmlSkillGroup> group;
    public int SpiritSqueeze = 0;
    public CXmlSkillDamage() { }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        group = new List<CXmlSkillGroup>();
        SpiritSqueeze = kXmlRead.Int("SpiritSqueeze");
        foreach (XmlNode node in ele.ChildNodes)
        {
            
            if (node.Name == "group")
            {
                CXmlSkillGroup groupItem = new CXmlSkillGroup();
                groupItem.Init(node as XmlElement);
                group.Add(groupItem);
            }
        }
    }

    public void ProcessDamage(ServerCreature cast, ServerCreature target, bool beHit, uint skillId, ushort eventId)
    {
        foreach (CXmlSkillGroup groupItem in group)
        {
            int curHP = (int)target.GetHP();
            groupItem.ProcessDamage(cast, target, beHit);

            // 记录玩家角色的技能施放
            var damage = curHP - (int)target.GetHP();
            var mainPlayer = GameApp.GetWorldManager().MainPlayer;
            if (beHit && damage > 0 && 
                ((cast is ServerPlayer && cast.ulObjectID == mainPlayer.m_NetObjectID) ||
                cast is ServerPet ||
                (cast is ServerMonster && target is ServerMonster) && ((ServerMonster)target).monsterConfig.DamegeRate >= 0.01))
            {
                var req = new PKT_CLI_GS_WN_SKILL_DAMAGE_NTF
                {
                    CastID = cast.ServerId,
                    TargetID = target.ServerId,
                    SkillID = skillId,
                    EventID = eventId,
                    SkillDmg = (uint)damage
                };
                GameApp.GetNetHandler().connect.SendProtocol_Real(req, (ushort)WANGZHEMSGID.CLI_GS_WN_SKILL_DAMAGE_NTF);
            }
        }
    }


#if UNITY_EDITOR
    public void Draw()
    {
        //EditorGUILayout.LabelField("伤害计算方式:", GUILayout.Width(80));
        //SpiritSqueeze = EditorGUILayout.IntField(SpiritSqueeze);
        //EditorGUILayout.Space();
        GUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("伤害:", GUILayout.Width(30));

        if (group == null) { group = new List<CXmlSkillGroup>(); }
        for (int i = 0; i < group.Count; i++ )
        {
            group[i].Draw();
        }

        GUILayout.EndVertical();
    }
    public void Export(XmlDocument doc, XmlNode parent, string name = "damage")
    {
        XmlElement damage = doc.CreateElement(name);

        if(group.Count > 0)
        {
            CXmlSkillGroup gr = null;
            foreach (var it in group) 
            {
                GUILayout.BeginHorizontal();
                it.Export(doc,damage);
                if(GUILayout.Button("Delete"))
                {
                    gr = it;
                }
                GUILayout.EndHorizontal();
            }

            if (gr != null)
            {
                group.Remove(gr);
            }
        }

        if(GUILayout.Button("添加group"))
        {
            group.Add(new CXmlSkillGroup());
        }

        parent.AppendChild(damage);
    }
#endif
}
