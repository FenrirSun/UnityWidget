using System;
using UnityEngine;

public class CSkillEffectParabola : CSkillEffectBase
{
    public CXmlSkillEffectParabola Xml { get; set; }
    public CSkillPos SkillPos1 { get; private set; }
    public CSkillPos SkillPos2 { get; private set; }

    private float V0;
    private Vector3 EffectPos;
    private float BeginTime;
    private float EndTime;
    public override Vector3 Pos
    {
        get
        {
            return EffectPos;
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

    public CSkillEffectParabola(CXmlSkillEffectParabola xml, CSkillEvent ev)
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
            if (Time.time > EndTime)
            {
                EndEffect();
                if (Xml.EndEventIndex > 0)
                {
                    Ev.Skill.TriggerEvent(Ev, Xml.EndEventIndex);
                }
            }
            else
            {
                float t = Time.time - BeginTime;
                Vector3 p1 = SkillPos1.Pos;
                Vector3 p2 = SkillPos2.Pos;
                p1.y = 0f;
                p2.y = 0f;
              //  Debug.Log("CSkillEffectParabola p1="+p1+"    p2="+p2);
                Vector3 dir = p2 - p1;
                 
                    dir.Normalize();
                    p1 += dir * V0 * t;

                    float s = Xml.H0 * t - Xml.G * t * t / 2f;
                    p1.y = SkillPos1.Pos.y + s;
                    EffectPos = p1;

                  //  Transform tran = effect.Instances.transform;
                    effect.Instances.transform.position = EffectPos;
                

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

        float d = SkillPos1.Pos.y - SkillPos2.Pos.y;
        float t1 = Xml.H0 / Xml.G;
        float t2 = Mathf.Sqrt((Xml.H0 * Xml.H0 + 2 * Xml.G * d) / (Xml.G * Xml.G));

        Vector3 p1 = SkillPos1.Pos;
        Vector3 p2 = SkillPos2.Pos;
        p1.y = 0f;
        p2.y = 0f;

        V0 = (p2 - p1).magnitude / (t1 + t2);

        EffectPos = SkillPos1.Pos;
        BeginTime = Time.time;
        EndTime = Time.time + t1 + t2;
    }
}