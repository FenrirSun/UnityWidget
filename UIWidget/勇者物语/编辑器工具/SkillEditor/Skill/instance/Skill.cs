using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSkill
{
    public CSkillChain Chain{get;private set;}
    public CXmlSkill Xml{get;private set;}
    public CharacterBase Owner
    {
        get
        {
            return Chain.Owner;
        }
    }
    public bool IsEnd
    {
        get
        {
            return events.Count == 0 && addEvents.Count == 0;
        }
    }
    public bool IsFromNet { get; set; }
    public ulong DefaultTargetID { get; set; }

    private List<CSkillEvent> events = new List<CSkillEvent>();
    private List<CSkillEvent> addEvents = new List<CSkillEvent>();
    public CSkill(CXmlSkill xml,CSkillChain chain,bool fromNet)
    {
        Xml = xml;
        Chain = chain;
        IsFromNet = fromNet;
    }

    public void Tick()
    {
        if (addEvents.Count > 0)
        {
            events.AddRange(addEvents);
            addEvents.Clear();
        }
        events.RemoveAll(r => r.IsEnd);
        int count = events.Count;
        for (int i = 0; i < count; ++i)
        {
            if(i < events.Count)
                events[i].Tick();
        }
    }

    private  void SendSkillEvent(int skillID, int eventIndex, List<ulong> targets, Vector3 pos, Vector3 dir)
    {
        if (!Owner.MoveComp.OwnerIsType(EOwnerType.MainPlayer) && 
            !Owner.MoveComp.OwnerIsType(EOwnerType.MasterMonster) && 
            !Owner.MoveComp.OwnerIsType(EOwnerType.AutoPlayer) &&
            !Owner.MoveComp.OwnerIsType(EOwnerType.Pet))
        {
            return;
        }

        PKT_IPAD_SKILL_EVENT_REQ req = new PKT_IPAD_SKILL_EVENT_REQ();
        req.CastID = (uint)Owner.m_NetObjectID;
        Vector3 playerPos = Owner.MoveComp.transform.position;
        req.CastPos = new DT_3DPOS() { PosX = playerPos.x, PosY = playerPos.y, PosZ = playerPos.z };
        req.Dir = new DT_3DPOS() { PosX = dir.x, PosY = dir.y, PosZ = dir.z };
        req.Pos = new DT_3DPOS() { PosX = pos.x, PosY = pos.y, PosZ = pos.z };
        req.SkillID = (uint)skillID;
        req.EventID = (ushort)eventIndex;
        req.TargetCount = (ushort)targets.Count;
        for (int i = 0; i < targets.Count; ++i)
        {
            req.TagetList[i] = (uint)targets[i];
        }
        GameApp.GetGameServerHandler().SkillEvent(req);

        if (Owner.MoveComp.OwnerIsType(EOwnerType.MainPlayer) || 
            Owner.MoveComp.OwnerIsType(EOwnerType.AutoPlayer) ||
            Owner.MoveComp.OwnerIsType(EOwnerType.Pet))
        {
            req.CastID = (uint)Owner.m_NetInstanceID;
            GameApp.GetNetHandler().SendSkillEvent(req);

            if (PlayerMovement.SimulateMove)
            {

                PKT_IPAD_SKILL_EVENT_NTF p = new PKT_IPAD_SKILL_EVENT_NTF();

                p.CastID = (uint)Owner.m_NetObjectID;
                playerPos = Owner.MoveComp.transform.position;
                p.CastPos = new DT_3DPOS() { PosX = playerPos.x, PosY = playerPos.y, PosZ = playerPos.z };
                p.Dir = new DT_3DPOS() { PosX = dir.x, PosY = dir.y, PosZ = dir.z };
                p.Pos = new DT_3DPOS() { PosX = pos.x, PosY = pos.y, PosZ = pos.z };
                p.SkillID = (uint)skillID;
                p.EventID = (ushort)eventIndex;
                p.TargetCount = (ushort)targets.Count;
                for (int i = 0; i < targets.Count; ++i)
                {
                    p.TagetList[i] = (uint)targets[i];
                }

                PlayerMovement.SimulatePlayer.SkillComp.TriggerSkillEvent(p);
            }
        }

         if (Owner.MoveComp.OwnerIsType(EOwnerType.MasterMonster))
         {
             if (MonsterMovement.SimulateMove)
             {
                 PKT_IPAD_SKILL_EVENT_NTF p = new PKT_IPAD_SKILL_EVENT_NTF();

                 p.CastID = (uint)Owner.m_NetObjectID;
                 playerPos = Owner.MoveComp.transform.position;
                 p.CastPos = new DT_3DPOS() { PosX = playerPos.x, PosY = playerPos.y, PosZ = playerPos.z };
                 p.Dir = new DT_3DPOS() { PosX = dir.x, PosY = dir.y, PosZ = dir.z };
                 p.Pos = new DT_3DPOS() { PosX = pos.x, PosY = pos.y, PosZ = pos.z };
                 p.SkillID = (uint)skillID;
                 p.EventID = (ushort)eventIndex;
                 p.TargetCount = (ushort)targets.Count;
                 for (int i = 0; i < targets.Count; ++i)
                 {
                     p.TagetList[i] = (uint)targets[i];
                 }
                 (Owner.MoveComp as MonsterMovement).SimulateMonster.SkillComp.TriggerSkillEvent(p);
             }
         }

    }

    // pkPreEvent:前一个Event数据
    // iNewEventIndex:EventID
    public void TriggerEvent(CSkillEvent pkPreEvent, int iNewEventIndex)//自己触发的
    {
        CXmlSkillEvent pkXmlSkillEvent = Xml.GetEvent(iNewEventIndex);
        CXmlSkillRange pkXmlSkillRange = pkXmlSkillEvent.XmlRange;
        if (IsFromNet && pkXmlSkillEvent.IsServerNeed && pkXmlSkillEvent.IsClientNeed)// 模拟到此结束
        {
            return;
        }
         if (IsFromNet && pkXmlSkillRange != null)// 模拟到此结束
        {
            return;
        }
        Vector3 ptPos = Vector3.zero;
        Vector3 ptDir = Vector3.zero;
        CXmlSkillPos pkXmlSkillPos = pkXmlSkillEvent.XmlPos;
        CXmlSkillDir pkXmlSkillDir = pkXmlSkillEvent.XmlDir;
        if (pkXmlSkillPos != null)
        {
            ptPos = pkXmlSkillPos.GetPosBeforeCreate(this,pkPreEvent,Owner);
        }
        if (pkXmlSkillDir != null)
        {
            ptDir = pkXmlSkillDir.GetDirBeforeCreate(this,pkPreEvent,Owner);
        }
        if (pkXmlSkillRange != null)
        {
            int iMaxNum = pkXmlSkillEvent.Num.GetValue(Owner);
            iMaxNum = Mathf.Min(iMaxNum,50);

            List<CharacterBase> vecTargets = new List<CharacterBase>();
            pkXmlSkillRange.GetTargets(this,pkPreEvent,vecTargets,iMaxNum,Owner);
            int iNum = Mathf.Min(vecTargets.Count, iMaxNum);
            if (iNum > 0)
            {
                List<ulong> vecTargetIDs = new List<ulong>();
                for (int i = 0; i < iNum; ++i)
                {
                    CharacterBase spTarget = vecTargets[i];
                    vecTargetIDs.Add(spTarget.m_NetObjectID);
                    CSkillEvent pkSkillEvent = new CSkillEvent(pkXmlSkillEvent,this);
                    pkSkillEvent.Init(ptPos, ptDir, spTarget.m_NetObjectID);
                    addEvents.Add(pkSkillEvent);
                }

                if (!IsFromNet && pkXmlSkillEvent.IsServerNeed)
                {
                    SendSkillEvent(Xml.ID, pkXmlSkillEvent.Index, vecTargetIDs, ptPos, ptDir);
                }
            }
            else
            {
                if (pkXmlSkillEvent.ZeroTargetEventIndex > 0)
                {
                    TriggerEvent(pkPreEvent,pkXmlSkillEvent.ZeroTargetEventIndex);
                }
            }
        }
        else
        {
            ulong nTargetID = 0;
            CXmlSkillTarget pkXmlSkillTarget = pkXmlSkillEvent.XmlTarget;
            if (pkXmlSkillTarget != null)
            {
                nTargetID = pkXmlSkillTarget.GetTargetBeforeCreate(pkPreEvent,this);
            }
            CSkillEvent pkSkillEvent = new CSkillEvent(pkXmlSkillEvent,this);
            pkSkillEvent.Init(ptPos,ptDir,nTargetID);
            addEvents.Add(pkSkillEvent);

            if (!IsFromNet && pkXmlSkillEvent.IsServerNeed)
            {
                List<ulong> vecTargetIDs = new List<ulong>();
                if (nTargetID > 0)
                {
                    vecTargetIDs.Add(nTargetID);
                }
                if (pkXmlSkillEvent.XmlPos != null)
                {
                    ptPos = pkSkillEvent.Pos;
                }
                if (pkXmlSkillEvent.XmlDir != null)
                {
                    ptDir = pkSkillEvent.Dir;
                }

                SendSkillEvent(Xml.ID, pkXmlSkillEvent.Index, vecTargetIDs, ptPos, ptDir);
            }

        }
    }

    public void TriggerEvent(PKT_IPAD_SKILL_EVENT_NTF ntf)//模拟时调的函数
    {
        CXmlSkillEvent pkXmlSkillEvent = Xml.GetEvent((int)ntf.EventID);

        CXmlSkillRange pkXmlSkillRange = pkXmlSkillEvent.XmlRange;
        if (pkXmlSkillRange != null)
        {
            if (ntf.TargetCount > 0)
            {
                if (ntf.TargetCount > 50)
                {
                    Debug.LogError("target count > 50");
                }
                for(ushort i = 0; i < ntf.TargetCount; ++i)
                {
                    Vector3 pos = new Vector3(ntf.Pos.PosX,ntf.Pos.PosY,ntf.Pos.PosZ);
                    Vector3 dir = new Vector3(ntf.Dir.PosX,ntf.Dir.PosY,ntf.Dir.PosZ);
                    CSkillEvent pkSkillEvent = new CSkillEvent(pkXmlSkillEvent,this);
                    pkSkillEvent.Init(pos,dir,ntf.TagetList[i]);
                    addEvents.Add(pkSkillEvent);
                }
            }
        }
        else
        {
            Vector3 pos = new Vector3(ntf.Pos.PosX, ntf.Pos.PosY, ntf.Pos.PosZ);
            Vector3 dir = new Vector3(ntf.Dir.PosX, ntf.Dir.PosY, ntf.Dir.PosZ);
            uint nTargetID = ntf.TargetCount > 0 ? ntf.TagetList[0] : 0;
            CSkillEvent pkSkillEvent = new CSkillEvent(pkXmlSkillEvent, this);
            pkSkillEvent.Init(pos, dir, nTargetID);
            addEvents.Add(pkSkillEvent);
        }
    }

    public void Break(bool force)
    {
        foreach (CSkillEvent ev in addEvents)
        {
            ev.Break(force);
        }
        foreach (CSkillEvent ev in events)
        {
            ev.Break(force);
        }
		if(force)
		{
			addEvents.Clear();
			events.Clear();
		}
		else
		{
 	       addEvents.RemoveAll(r => r.Xml.IsBreakable);
  	       events.RemoveAll(r => r.Xml.IsBreakable);
		}
    }

    public bool OwnerIsPlayer()
    {
        return Chain.OwnerIsPlayer();
    }

    public bool OwnerIsNetPlayer()
    {
        return Chain.OwnerIsNetPlayer();
    }

    public bool OwnerIsMonster()
    {
        return Chain.OwnerIsMonster();
    }

    public bool OwnerIsAutoPlayer()
    {
        return Chain.OwnerIsAutoPlayer();
    }
}