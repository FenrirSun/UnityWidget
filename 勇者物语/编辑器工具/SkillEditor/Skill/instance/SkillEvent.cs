using System;
using UnityEngine;
using System.Collections.Generic;

public class CSkillEvent
{
    public CXmlSkillEvent Xml { get; private set; }
    public CSkill Skill { get; private set; }
    public CSkillPos SkillPos { get; set; }
    public CSkillPosP2P SkillPosP2P { get; set; }
    public CSkillPosCircle SkillPosCircle { get; set; }
    public CSkillDir SkillDir { get; set; }
    public CSkillAction SkillAction { get; private set; }
    public CSkillEffectBase SkillEffect { get; private set; }
    public CSkillInfluence SkillInfluence { get; set; }
    public bool IsEnd { get; private set; }
    public ulong TargetID { get; set; }
    public bool IsFromNet { get { return Skill.IsFromNet; } }
    public CharacterBase Target
    {
        get
        {
            if (TargetID > 0)
            {
              //  Debug.Log("TargetID==" + TargetID);
                return GameApp.GetWorldManager().GetObject(TargetID);
            }
            else
            {
                return null;
            }
        }
    }
    public bool IsTargetValid
    {
        get
        {
            return Target != null;
        }
    }

    public Vector3 Pos
    {
        get { return SkillPos.Pos; }
    }

    public Vector3 PosP2P
    {
        get { return SkillPosP2P.Pos; }
    }

    public Vector3 PosCircle
    {
        get { return SkillPosCircle.Pos; }
    }

    public Vector3 Dir
    {
        get { return SkillDir.Dir; }
    }

    public Vector3 EffectPos
    {
        get
        {
            if (SkillEffect != null)
            {
                return SkillEffect.Pos;
            }
            //else if (SkillP2PEffect != null)
            //{
            //    return SkillP2PEffect.Pos;
            //}
            else
            {
                return Vector3.zero;
            }
        }
    }

    public Vector3 EffectDir
    {
        get
        {
            if (SkillEffect != null)
            {
                return SkillEffect.Dir;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public CharacterBase Owner
    {
        get
        {
            return Skill.Owner;
        }
    }

    private List<CSkillSubEvent> SubEvents;


    public CSkillEvent(CXmlSkillEvent xml, CSkill skill)
    {
        Xml = xml;
        Skill = skill;
        IsEnd = false;
        SubEvents = new List<CSkillSubEvent>();
    }
    public void Init(Vector3 pos, Vector3 dir, ulong nTargetID)
    {
        TargetID = nTargetID;

        //Debug.Log(Skill.Xml.Name + " event " + Xml.Index + "begin");
        if (Xml.XmlPos != null)
        {
            SkillPos = new CSkillPos(Xml.XmlPos, this);
            SkillPos.Pos = pos;
            SkillPos.Init();
        }

        if (Xml.XmlPosP2P != null)
        {
            SkillPosP2P = new CSkillPosP2P(Xml.XmlPosP2P, this);
            SkillPosP2P.Init();
        }

        if (Xml.XmlPosCircle != null)
        {
            SkillPosCircle = new CSkillPosCircle(Xml.XmlPosCircle, this);
            SkillPosCircle.Init();
        }

        if (Xml.XmlDir != null)
        {
            SkillDir = new CSkillDir(Xml.XmlDir, this);
            SkillDir.Dir = dir;
            SkillDir.Init();
        }

        if (Xml.XmlEffect != null)
        {
            //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@XMLEFFECT");
            SkillEffect = new CSkillEffectNormal(Xml.XmlEffect, this);
            SkillEffect.Init();
        }
        else if (Xml.XmlP2PEffect != null)
        {
            SkillEffect = new CSkillEffectP2P(Xml.XmlP2PEffect, this);
            SkillEffect.Init();
        }
        else if (Xml.XmlParabolaEffect != null)
        {
            SkillEffect = new CSkillEffectParabola(Xml.XmlParabolaEffect, this);
            SkillEffect.Init();
        }
        else if (Xml.XmlCollisionEffect != null)
        {
            SkillEffect = new CSkillEffectCollision(Xml.XmlCollisionEffect, this);
            SkillEffect.Init();
        }

        if (Xml.XmlAction != null)
        {
            SkillAction = new CSkillAction(Xml.XmlAction, this);
            SkillAction.Init();
        }

        if (Xml.XmlInfluence != null)
        {
            if (Xml.XmlInfluence.XmlDamage != null)
            {
                if (OwnerIsPlayer())
                {
                    (GameApp.GetUIManager().GetWnd("HUD") as HUD).IncCombo();
                }
            }
            //SkillInfluence = new CSkillInfluence(Xml.XmlInfluence, this);
            //SkillInfluence.Init();
        }

       // Debug.Log("Xml.DoAllTrifles");
        Xml.DoAllTrifles(this);

        //激活所有子事件
        foreach (CXmlSkillSubEvent xmlSubEvent in Xml.XmlSubEvents)
        {
            CSkillSubEvent subEvent = new CSkillSubEvent(xmlSubEvent, this);
            subEvent.Init();
            SubEvents.Add(subEvent);
        }
    }

    public void Tick()
    {
        if (!IsEnd)
        {
            if (SkillPos != null)
            {
                SkillPos.Tick();
            }
            if (SkillDir != null)
            {
                SkillDir.Tick();
            }

            bool bEnd = true;

            if (SkillPosP2P != null)
            {
                SkillPosP2P.Tick();
                if (!SkillPosP2P.IsEnd)
                {
                    bEnd = false;
                }
            }

            if (SkillPosCircle != null)
            {
                SkillPosCircle.Tick();
                if (!SkillPosCircle.IsEnd)
                {
                    bEnd = false;
                }
            }

            //先删除已经激发过的事件
            SubEvents.RemoveAll(r => r.IsEnd);
            //更新所有子事件
            if (SubEvents.Count > 0)
            {
                SubEvents.ForEach(r => r.Tick());
                bEnd = false;
            }

            if (SkillEffect != null)
            {
                SkillEffect.Tick();
                if (SkillEffect.IsActive)
                {
                    bEnd = false;
                }
            }

            IsEnd = bEnd;
        }
    }

    public void Break(bool force)
    {
        if (Xml.IsBreakable || force)
        {
            //Debug.Log("event + " + Xml.Index + " breaked");
            if (SkillEffect != null)
            {
                SkillEffect.Break();
            }
            // Event被打断时，停止所有的trifle
            Xml.EndAllTrifles(this);
        }
    }

    public bool OwnerIsPlayer()
    {
        return Skill.OwnerIsPlayer();
    }

    public bool OwnerIsNetPlayer()
    {
        return Skill.OwnerIsNetPlayer();
    }

    public bool OwnerIsMonster()
    {
        return Skill.OwnerIsMonster();
    }

    public bool OwnerIsAutoPlayer()
    {
        return Skill.OwnerIsAutoPlayer();
    }
}