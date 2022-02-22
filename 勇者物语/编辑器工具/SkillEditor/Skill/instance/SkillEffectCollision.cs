using System;
using UnityEngine;
using System.Collections.Generic;

public class CSkillEffectCollision : CSkillEffectBase
{
    public CXmlSkillEffectCollision Xml { get; set; }
    public CSkillPos SkillPos1 { get; private set; }
    public CSkillPos SkillPos2 { get; private set; }
    private float V0 { get; set; }
    private float LifeEndTime { get; set; }
    public override Vector3 Pos
    {
        get
        {
            return SkillPos1.Pos;
        }
    }
    public override Vector3 Dir
    {
        get
        {
            return SkillPos2.Pos - SkillPos1.Pos;
        }
    }
    public override String Name
    {
        get
        {
            return Xml.Name;
        }
    }
    public override float DelaySeconds
    {
        get
        {
            return Xml.DelaySeconds;
        }
    }
    public override float Scale
    {
        get
        {
            return Xml.Scale.GetValue(Ev.Owner);
        }
    }

    public CSkillEffectCollision(CXmlSkillEffectCollision xml, CSkillEvent ev)
        : base(ev)
    {
        Xml = xml;
    }
    public override void Init()
    {
        base.Init();
    }

    public override void Tick()
    {
        if (effect != null)
        {
            if (Time.time > LifeEndTime)
            {
                EndEffect();
                if (Xml.DeadEventIndex > 0)
                {
                    Ev.Skill.TriggerEvent(Ev, Xml.DeadEventIndex);
                }
            }
            else
            {
                if (SkillPos2.Xml.IsAutoChange)
                {
                    SkillPos2.Tick();
                }

                Vector3 ptDir = SkillPos2.Pos - SkillPos1.Pos;
                float fLen = ptDir.magnitude;
                float fInterval = Time.deltaTime;
                float fTravelLen = V0 * fInterval + 0.5f * Xml.A * fInterval * fInterval;
                if (fTravelLen > fLen)
                {
                    EndEffect();
                    if (Xml.EndEventIndex > 0)
                    {
                        Ev.Skill.TriggerEvent(Ev, Xml.EndEventIndex);
                    }
                }
                else
                {
                    List<CharacterBase> vecTargets = new List<CharacterBase>();
                    Xml.XmlRange.GetTargets(Ev.Skill, Ev, vecTargets, 1,Ev.Owner);
                    if (vecTargets.Count > 0)
                    {
                        EndEffect();
                        if (Xml.CollisionEventIndex > 0)
                        {
                            Ev.TargetID = vecTargets[0].m_NetObjectID;
                            Ev.Skill.TriggerEvent(Ev, Xml.CollisionEventIndex);
                        }
                    }
                    else
                    {
                        Transform tran = effect.Instances.transform;
                        ptDir.Normalize();
                        tran.forward = ptDir;

                        Vector3 ptNewPos = SkillPos1.Pos + fTravelLen * ptDir;
                        SkillPos1.Pos = ptNewPos;
                        tran.position = ptNewPos;
                        V0 += Xml.A * fInterval;
                    }
                }
            }
        }

        base.Tick();
    }

    protected override void OnBegin()
    {
        SkillPos1 = new CSkillPos(Xml.XmlPos1, Ev);
        SkillPos1.Init();
        SkillPos2 = new CSkillPos(Xml.XmlPos2, Ev);
        SkillPos2.Init();
        V0 = Xml.V0;
        LifeEndTime = Xml.LifeSeconds + Time.time;
    }
}