using System;
using UnityEngine;

public class CSkillSubEvent
{
    public CXmlSkillSubEvent Xml { get; private set; }
    public CSkillEvent Ev { get; private set; }
    public bool IsEnd { get { return LeftNum == 0; } }
    private float EndTime;
    private int LeftNum;
    private float IntervalTime;
    public CSkillSubEvent(CXmlSkillSubEvent xml, CSkillEvent ev)
    {
        Xml = xml;
        Ev = ev;
    }

    public void Init()
    {
        EndTime = Time.time + Xml.Seconds.GetValue(Ev.Owner);
        IntervalTime = Xml.IntervalSeconds;
        LeftNum = Xml.Num;
    }

    public void Tick()
    {
        if (!IsEnd)
        {
            if (Time.time > EndTime)
            {
                Trigger();
            }
        }
    }

    private void Trigger()
    {
        int iNewEventIndex = Ev.IsTargetValid ? Xml.EventIndexWhenHasTarget.GetValue(Ev.Owner) : Xml.EventIndexWhenHasNotTarget.GetValue(Ev.Owner);
        if (iNewEventIndex >= 0)
        {
            float prob = UnityEngine.Random.Range(0.0f, 100.0f);
            if (prob <= Xml.Prob.GetValue(Ev.Owner))
            {
                Ev.Skill.TriggerEvent(Ev, iNewEventIndex);
            }
        }
        LeftNum--;
        if (LeftNum != 0)
        {
            EndTime += IntervalTime;
        }
    }
}