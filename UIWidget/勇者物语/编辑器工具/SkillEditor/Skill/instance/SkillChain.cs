using System;
using System.Xml;
using UnityEngine;
using System.Collections.Generic;

public class CSkillChain
{
    public CXmlSkillChain Xml{get;private set;}
    public SkillComponent Comp{get;private set;}
    private Dictionary<int,CSkill> skills = new Dictionary<int,CSkill>();
    private Dictionary<int, Vector3> posSaves = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> dirSaves = new Dictionary<int, Vector3>();
    public CharacterBase Owner
    {
        get
        {
            return Comp.Owner;
        }
    }

    public CSkillChain(CXmlSkillChain xml,SkillComponent comp)
    {
        Xml = xml;
        Comp = comp;
        Init();
    }
    public void Init()
    {
        m_dLastCoolDownTime = 0f;
        m_iCurFhase = 0;
        m_bWaitingForPhaseAck = false;
    }

    // for mainplayer
    public void UseSkill(int index,int cooldown = -1)
    {
        float dCurTime = Time.time;
        bool bCanUse = true;
        if (m_bWaitingForPhaseAck)
        {
            bCanUse = false;
        }
        else
        {
            if (m_iCurFhase == 0)
            {
                if (cooldown == -1)
                {
                    //旧的CD判断
                    if (dCurTime < m_dLastCoolDownTime + Xml.CoolDown.GetValue(Owner))
                    {
                        bCanUse = false;
                    }
                }
                else
                {
                    //新的CD判断
                    if (dCurTime < m_dLastCoolDownTime + cooldown)
                    {
                        bCanUse = false;
                    }

                }
            }
        }

        if (bCanUse)
        {
            CXmlSkill pkXmlSkill = Xml.GetXmlSkill(m_iCurFhase);
         
            if (pkXmlSkill.CanUse())
            {
                m_bWaitingForPhaseAck = true;
                m_dLastCoolDownTime = dCurTime;
                CSkill pkSkill = GetSkill(pkXmlSkill.ID,false);
                pkSkill.DefaultTargetID = GameApp.GetSkillManager().GetDefaultTargetID(Owner);
                pkSkill.TriggerEvent(null,0);
                if (OwnerIsPlayer()  || OwnerIsAutoPlayer() || Comp.m_ML2GameObject is Pet)
                {
                    if (Owner.m_GroupType == eGroupType.EG_Enemy && OwnerIsAutoPlayer())
                        return;
                    ((HUD)UIBaseWnd.GetWndByName("HUD")).UseSkillSuccess(index, Xml.CoolDown.GetValue(Owner));
                }
            }
        }
    }

    // for mastermonster
    public void UseSkill(int skillID,ulong defaultTargetID)
    {
        CSkill pkSkill = GetSkill(skillID,false);
        pkSkill.DefaultTargetID = defaultTargetID;
        if (pkSkill != null)
        {
            pkSkill.TriggerEvent(null, 0);
            Debug.Log("useskill name="+ pkSkill.Xml.Name);
        }
        else
        {
            Debug.LogError("skill id = " + skillID + " not found");
        }
    }

    public void NetMonsterUseSkill(int skillID, ulong defaultTargetID)
    {
        CSkill pkSkill = GetSkill(skillID, true);
        pkSkill.DefaultTargetID = defaultTargetID;
        if (pkSkill != null)
        {
            Debug.Log("NetMonsterUseSkill=");
            pkSkill.TriggerEvent(null, 0);
        }
        else
        {
            Debug.LogError("skill id = " + skillID + " not found");
        }
    }

    public void PhaseAck(int iPhaseIndex,float dDuration)
    {
        float dCurTime = Time.time;
        m_bWaitingForPhaseAck = false;
        
        m_iCurFhase = iPhaseIndex + 1;
        if (m_iCurFhase >= Xml.XmlSkillCount)
        {
            m_iCurFhase = 0;
        }
        else
        {
            m_dPhaseInvalidTime = dCurTime + dDuration;
        }
    }

    public void SavePos(int index, Vector3 pos)
    {
        posSaves[index] = pos;
    }

    public void SaveDir(int index, Vector3 dir)
    {
        dirSaves[index] = dir;
    }

    public Vector3 GetDir(int index)
    {
        Vector3 ret = Vector3.zero;
        dirSaves.TryGetValue(index, out ret);
        return ret;

    }
    public Vector3 GetPos(int index)
    {
        Vector3 ret = Vector3.zero;
        posSaves.TryGetValue(index, out ret);
        return ret;
    }

    public void Tick()
    {
        float dTime = Time.time;
        if (!m_bWaitingForPhaseAck && m_iCurFhase != 0)
        {
            if (dTime > m_dPhaseInvalidTime)
            {
                m_iCurFhase = 0;
            }
        }

        //完全是为了纠错，怕配置忘了PhasePermission
        if (m_bWaitingForPhaseAck)
        {
            if (dTime > m_dLastCoolDownTime + 10.0f)
            {
                m_bWaitingForPhaseAck = false;
                m_iCurFhase = 0;
            }
        }

        foreach (CSkill skill in skills.Values)
        {
            if (!skill.IsEnd)
            {
                skill.Tick();
            }
        }
    }

    public void BreakAllSkill(bool force)
    {
        m_iCurFhase = 0;
        m_bWaitingForPhaseAck = false;
        foreach (CSkill skill in skills.Values)
        {
            skill.Break(force);
        }
    }

    public bool OwnerIsPlayer()
    {
        return Comp.OwnerIsPlayer();
    }

    public bool OwnerIsNetPlayer()
    {
        return Comp.OwnerIsNetPlayer();
    }

    public bool OwnerIsMonster()
    {
        return Comp.OwnerIsMonster();
    }

    public bool OwnerIsAutoPlayer()
    {
        return Comp.OwnerIsAutoPlayer();
    }

    public CSkill GetSkill(int iSkillID,bool fromNet)
    {
        CSkill ret = null;
        if (!skills.TryGetValue(iSkillID, out ret))
        {
            ret = new CSkill(GameApp.GetSkillManager().GetXmlSkill(iSkillID), this, fromNet);
            skills.Add(iSkillID,ret);
        }
        return ret;
    }

    public void ClearCoolDown()
    {
        m_dLastCoolDownTime = -100f;
    }

    public void ResetCoolDown(int index)
    {
        m_dLastCoolDownTime = Time.time;
    }

    public float AddCoolTime(float time)
    {
        m_dLastCoolDownTime = m_dLastCoolDownTime + time;
        float ct = Xml.CoolDown.GetValue(Owner);
        if (m_dLastCoolDownTime >= ct)
        {
            return m_dLastCoolDownTime - ct;
        }
        return -1;
    }

    public bool IsCoolTimeOK()
    {
        float ct = Xml.CoolDown.GetValue(Owner);
        return (m_dLastCoolDownTime >= ct);
    }

    public float GetCoolPre()
    {
        
          float cd = Xml.CoolDown.GetValue(Owner);
          Debug.LogWarning("------------------------>" + cd);
          float dd=0;
          if (cd <= 0)
          {
               dd = 1;
          }
          else
          {
               dd = m_dLastCoolDownTime / cd;
          }
          return dd;
    }

    float    m_dLastCoolDownTime;//上一次更新冷却的时刻
    float    m_dPhaseInvalidTime;//当前Phase失效的时刻
    int      m_iCurFhase;//当前phase
    bool     m_bWaitingForPhaseAck;//在等待phase确认，这段时间内不能进行下一段phase，也不能归0
}