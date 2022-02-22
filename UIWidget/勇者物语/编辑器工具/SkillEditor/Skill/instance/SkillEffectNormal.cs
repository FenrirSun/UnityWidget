using System;
using UnityEngine;

public class CSkillEffectNormal : CSkillEffectBase
{
    public CXmlSkillEffectNormal Xml{get;set;}
    public CSkillPos SkillPos{get;private set;}
    public CSkillDir SkillDir{get;private set;}
    public override Vector3 Pos
    {
        get
        {
            return SkillPos.Pos;
        }
    }
    public override Vector3 Dir
    {
        get
        {
            return SkillDir.Dir;
        }
    }
    public override String Name
    {
        get
        {
            return Xml.Name.GetValue(Ev.Owner) ;
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


    private float endTime;

    public CSkillEffectNormal(CXmlSkillEffectNormal xml, CSkillEvent ev)
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
            SkillPos.Tick();
            SkillDir.Tick();
            if (Time.time >= endTime)
            {
                EndEffect();
            }
            else
            {
                if (SkillPos.Xml.IsAutoChange)
                {
					effect.SetPosition(SkillPos.Pos);
                }

                if (SkillDir.Xml.IsAutoChange)
                {
					effect.SetDir(SkillDir.Dir);
                }
            }
        }

        base.Tick();
    }

    protected override void OnBegin()
    {
        if (Xml.XmlPos != null)
        {
            SkillPos = new CSkillPos(Xml.XmlPos, Ev);
            SkillPos.Init();
        }

        if (Xml.XmlDir != null)
        {
            SkillDir = new CSkillDir(Xml.XmlDir, Ev);
            SkillDir.Init();
        }


        endTime = Time.time + Xml.LifeSeconds.GetValue(Ev.Owner);
    }
}