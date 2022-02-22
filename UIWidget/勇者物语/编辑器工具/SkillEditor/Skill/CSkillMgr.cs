using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;

public class CSkillMgr:MonoBehaviour
{
    private ulong DefaultTargetID{get;set;}
    private double AutoSelTargetSeconds{get;set;}
    public Dictionary<string, CXmlSkillChain> xmlSkillChains;
    public Dictionary<int, CXmlSkill> xmlSkills;
    public Dictionary<int, List<CXmlSkillGiftGroup>> xmlSkillJobGiftGroups;
    public Dictionary<int, CXmlSkillGift> xmlSkillGifts;
    public List<CXmlSkillRange> xmlSkillRanges;
    private double s_dLastAutoSelSeconds = 0.0;

    private LineRenderer line;
    public bool IsLock = false;
    private ulong Lockid = 0;

    public void SetLock(bool islock)
    {  
        IsLock = islock;
        if (islock)
        {
          
            Lockid = DefaultTargetID;
        }
        else
        {
            Lockid = 0;
        }
    }

    public ulong GetDefaultTargetID(CharacterBase player)
    {
        if (player is Player || player is Pet)
        {
            return DefaultTargetID;
        }
        else if (player is AutoPlayer)
        {
            return ((AutoPlayer)player).m_AutofightCom.LockTargetID;
        }
        return 0;
    }

    public void SetDefaultTargetID(ulong tid)
    {
        DefaultTargetID=tid;
    }

    public CSkillMgr(){}

    public void Init()
    {
        LoadSkillGiftss();
        LoadSkillChains();
    }

    private void LoadSkillGiftss()
    {
        xmlSkillGifts = new Dictionary<int, CXmlSkillGift>();
        xmlSkillJobGiftGroups = new Dictionary<int, List<CXmlSkillGiftGroup>>();
        XmlDocument xmlDoc = new XmlDocument();
        TextAsset textAsset = GameApp.GetResourceManager().LoadDB("DB/Skill/SkillGift");
        if (textAsset == null)
        {
            Debug.LogError("Load SkillGift DB Failed!");
            return;
        }
        xmlDoc.Load(new StringReader(textAsset.text));
        XmlElement xmlRoot = xmlDoc.DocumentElement;
        foreach (XmlNode node in xmlRoot.ChildNodes)
        {
            switch (node.Name)
            {
                case "job":
                    {
                        CXmlRead xmlRead = new CXmlRead(node as XmlElement);
                        int id = xmlRead.Int("id");

                        List<CXmlSkillGiftGroup> xmlSkillGiftGroups = null;
                        if (!xmlSkillJobGiftGroups.TryGetValue(id,out xmlSkillGiftGroups))
                        {
                            xmlSkillGiftGroups = new List<CXmlSkillGiftGroup>();
                            xmlSkillJobGiftGroups.Add(id, xmlSkillGiftGroups);
                        }

                        foreach (XmlNode nodeGroup in node.ChildNodes)
                        {
                            switch (nodeGroup.Name)
                            {
                                case "skillGroup":
                                    {
                                        CXmlSkillGiftGroup xmlSkillGiftGroup = new CXmlSkillGiftGroup(id);
                                        xmlSkillGiftGroup.Init(nodeGroup as XmlElement);
                                        xmlSkillGiftGroups.Add(xmlSkillGiftGroup);
                                    }
                                    break;
                            }
                        }
                    }
                    break;
            }
        }
    }

    private void LoadSkillChains()
    {
        xmlSkillChains = new Dictionary<string, CXmlSkillChain>();
        xmlSkills = new Dictionary<int, CXmlSkill>();
        xmlSkillRanges = new List<CXmlSkillRange>();
        DefaultTargetID = 0;

        XmlDocument xmlDoc = new XmlDocument();
        TextAsset textAsset = GameApp.GetResourceManager().LoadDB("DB/Skill/Skills");
        if (textAsset == null)
        {
            Debug.LogError("Load Skills DB Failed!");
            return;
        }
      
        xmlDoc.Load(new StringReader(textAsset.text));
        XmlElement xmlRoot = xmlDoc.DocumentElement;
        foreach (XmlNode node in xmlRoot.ChildNodes)
        {
            switch (node.Name)
            {
                case "ranges":
                    {
                        foreach (XmlNode nodeRange in node.ChildNodes)
                        {
                            CXmlSkillRange xmlSkillRange = new CXmlSkillRange();
                            xmlSkillRange.Init(nodeRange as XmlElement);
                            xmlSkillRanges.Add(xmlSkillRange);
                        }
                    }
                    break;
                case "property":
                    {
                        CXmlRead xmlRead = new CXmlRead(node as XmlElement);
                        AutoSelTargetSeconds = xmlRead.Double("autoSelTargetSeconds");
                    }
                    break;
                case "chain":
                    {
                        CXmlSkillChain xmlSkillChain = new CXmlSkillChain();
                        xmlSkillChain.Init(node as XmlElement);
                        xmlSkillChains.Add(xmlSkillChain.Name, xmlSkillChain);
                    }
                    break;
            }
        }
    }

    public void Destroy()
    {
        xmlSkillChains.Clear();
        xmlSkillRanges.Clear();
        xmlSkills.Clear();
    }

    public CXmlSkillChain GetXmlSkillChainByName(string strSkillName)
    {
        CXmlSkillChain ret = null;
        if (!xmlSkillChains.TryGetValue(strSkillName,out ret))
        {
            Debug.LogError("chain not found!" + strSkillName);
        }
        return ret;
    }

    public CXmlSkillChain GetXmlSkillChainByGiftID(int GiftID)
    {
        CXmlSkillGift xmlGift = GetXmlSkillGift(GiftID);
        CXmlSkillChain c = null;
        xmlSkillChains.TryGetValue(xmlGift.XmlGroup.Chain, out c);
        if (c == null)
        {
            Debug.LogError(xmlGift.XmlGroup.Chain + " chain not found!");
        }
        return c;
    }


    public void AddXmlSkill(CXmlSkill xmlSkill)
    {
        if (!xmlSkills.ContainsKey(xmlSkill.ID))
        {
            xmlSkills.Add(xmlSkill.ID, xmlSkill);
            return;
        }
        Debug.LogError("skill id duplicate!" + xmlSkill.ID + "  name:" + xmlSkill.Name);
    }

    public void AddXmlSkillGift(CXmlSkillGift xmlSkillGift)
    {
        if (xmlSkillGifts.ContainsKey(xmlSkillGift.ID))
        {
            Debug.LogError("skill gift  id duplicate!" + xmlSkillGift.ID);
        }
        else
        {
            xmlSkillGifts.Add(xmlSkillGift.ID, xmlSkillGift);
        }
    }

    public CXmlSkill GetXmlSkill(int iID)
    {
        CXmlSkill ret = null;
        if (!xmlSkills.TryGetValue(iID, out ret))
        {
            Debug.Log("skill not found!id=" + iID);
        }
        return ret;
    }

    public CXmlSkillGift GetXmlSkillGift(int iID)
    {
        CXmlSkillGift ret = null;
        if (!xmlSkillGifts.TryGetValue(iID, out ret))
        {
            Debug.Log("skill gift not found!id=" + iID);
        }
        return ret;
    }

    public List<CXmlSkillGiftGroup> GetXmlSkillGiftGroup()
    {
        int job = GameApp.GetWorldManager().MainPlayer.GetProperty().Job;
        List<CXmlSkillGiftGroup> ret = null;
        xmlSkillJobGiftGroups.TryGetValue(job, out ret);
        return ret;
    }

    public List<CXmlSkillGiftGroup> GetXmlSkillGiftGroupByJobId(int job)
    {
        List<CXmlSkillGiftGroup> ret = null;
        xmlSkillJobGiftGroups.TryGetValue(job, out ret);
        return ret;
    }

    void Update()
    {
        //自动锁一个怪
        if (GameApp.GetWorldManager().MainPlayer == null)
            return;
        if (GameApp.GetWorldManager().MainPlayer != null && GameApp.GetWorldManager().MainPlayer.IsCreated() && (DefaultTargetID == 0 || Time.time - s_dLastAutoSelSeconds > AutoSelTargetSeconds))
        {
            s_dLastAutoSelSeconds = Time.time;
            if (DefaultTargetID > 0)
            {
                DefaultTargetID = 0;
            }

            List<CharacterBase> vecTargets = new List<CharacterBase>();
            foreach(CXmlSkillRange range in xmlSkillRanges)
            {
                range.GetTargets(null, null, vecTargets, 1, GameApp.GetWorldManager().MainPlayer);
                if (vecTargets.Count > 0)
                {
                    CharacterBase monster = vecTargets[0];
                    DefaultTargetID = monster.m_NetObjectID;
                    if (IsLock)
                    {
                        if (Lockid > 0)
                        {
                            CharacterBase cb = GameApp.GetWorldManager().GetObject(Lockid);
                            if (cb != null && cb.Dead!=true)
                            {
                                Vector3 v1 = cb.m_ObjInstance.transform.position;
                                v1.y = 0;
                                Vector3 v2 = GameApp.GetWorldManager().MainPlayer.m_ObjInstance.transform.position;
                                v2.y = 0;
                                if (Vector3.Distance(v1, v2) < 12)
                                {
                                    DefaultTargetID = Lockid;
                                }
                                else
                                {
                                    Lockid = DefaultTargetID;
                                }
                            }
                            else
                            {
                                Lockid = DefaultTargetID;
                            }
                        }
                        else
                        {
                            Lockid = DefaultTargetID;
                        }
                     }
                    Debug.Log("lock is=" + Lockid);
                    break;
                }
            }
        }
    }

    public ulong LockTarget(CharacterBase player)
    {
        List<CharacterBase> vecTargets = new List<CharacterBase>();
        foreach (CXmlSkillRange range in xmlSkillRanges)
        {
            range.GetTargets(null, null, vecTargets, 1, player);
            if (vecTargets.Count > 0)
            {
                CharacterBase monster = vecTargets[0];
                ulong uid= monster.m_NetObjectID;

                if (IsLock)
                {
                    if (Lockid > 0)
                    {
                        CharacterBase cb = GameApp.GetWorldManager().GetObject(Lockid);
                        if (cb != null)
                        {
                            Vector3 v1 = cb.m_ObjInstance.transform.position;
                            v1.y = 0;
                            Vector3 v2 = GameApp.GetWorldManager().MainPlayer.m_ObjInstance.transform.position;
                            v2.y = 0;
                            if (Vector3.Distance(v1, v2) < 12)
                            {
                                uid = Lockid;
                            }
                            else
                            {
                                Lockid = uid;
                            }
                        }
                        else
                        {
                            Lockid = uid;
                        }
                    }
                    else
                    {
                        Lockid = uid;
                    }
                }
                return uid;
            }
        }
        return 0;
    }

    public void UseMonsterSkill(ulong monsterID, int skillID, ulong playerID)
    {
        CharacterBase monster = GameApp.GetWorldManager().GetObject(monsterID);
        if (monster != null)
        {
            CharacterBase player = GameApp.GetWorldManager().GetObject(playerID);
            if (player != null)
            {
                if (player.CanBeAttack())
                {
                    monster.SkillComp.UseMonsterSkill(skillID, playerID);
                }
            }
            else
            {
                monster.SkillComp.UseMonsterSkill(skillID, playerID);
            }
            monster.MoveComp.VelDir.Stop();
        }
    }

    public void SkillInfluence(Vector3 pos, Vector3 dir, int skillID, int eventID, ulong castID, ulong targetID)
    {
        CharacterBase c = GameApp.GetWorldManager().GetObject(castID);
        CSkillChain chain = new CSkillChain(null, c.SkillComp);
        CSkill skill = chain.GetSkill(skillID, true);

        CXmlSkillEvent xmlEvent = skill.Xml.GetEvent(eventID);
        CSkillEvent ev = new CSkillEvent(xmlEvent, skill);

        ev.TargetID = targetID;
        if (xmlEvent.XmlPos != null)
        {
            ev.SkillPos = new CSkillPos(xmlEvent.XmlPos, ev);
            ev.SkillPos.Pos = pos;
            ev.SkillPos.Init();
        }

        if (xmlEvent.XmlDir != null)
        {
            ev.SkillDir = new CSkillDir(xmlEvent.XmlDir, ev);
            ev.SkillDir.Dir = dir;
            ev.SkillDir.Init();
        }

        if (xmlEvent.XmlInfluence != null)
        {
            ev.SkillInfluence = new CSkillInfluence(xmlEvent.XmlInfluence, ev);
            ev.SkillInfluence.Init();
        }
    }
}
