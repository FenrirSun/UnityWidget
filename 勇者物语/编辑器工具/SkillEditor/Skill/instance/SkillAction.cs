using System;
using UnityEngine;
public class CSkillAction
{
    public CXmlSkillAction Xml{get;set;}
    public CSkillEvent Ev{get;private set;}

    public CSkillAction(CXmlSkillAction xml, CSkillEvent ev)
    {
        Xml = xml;
        Ev = ev;
    }

    public void Init()
    {
        if (Ev.Owner is UIPlayer ||
            Ev.Owner.MoveComp.OwnerIsType(EOwnerType.MainPlayer) ||
            Ev.Owner.MoveComp.OwnerIsType(EOwnerType.MasterMonster) ||
            Ev.Owner.MoveComp.OwnerIsType(EOwnerType.AutoPlayer) ||
            Ev.Owner.MoveComp.OwnerIsType(EOwnerType.Pet))
        {
            ML2Event tempevent = new ML2Event((int)GameEventID.FMS_EVENT_SKILL);
            Xml.m_Data.Movable = Xml.Movable.GetValue(Ev.Owner);
            tempevent.PushUserData<CXmlSkillActionData>(Xml.m_Data);
            tempevent.PushUserData<ulong>(Ev.TargetID);
            GameEvent.DispatchEvent(Ev.Owner, tempevent);
        }
    }
}