using System;
using UnityEngine;

public class CSkillHitRoute
{
    public CXmlSkillHitRoute Xml { get; set; }
    public CSkillEvent Ev { get; private set; }
    public CSkillHitAction HitAction { get; private set; }



    public CSkillHitRoute(CXmlSkillHitRoute xml, CSkillEvent ev, CSkillHitAction hitAction)
    {
        Xml = xml;
        Ev = ev;
        HitAction = hitAction;
    }

    public void Init()
    {
        //if (Ev.Target != null)
        {
            float prob = UnityEngine.Random.Range(0.0f, 100.0f);
           //  Debug.Log("hurt=="+prob+"//" + Xml.Prob.GetValue(Ev.Owner));
            if (GameApp.GetSceneManager().CheckCombatMode(eCombatMode.CM_AutoFight) || GameApp.GetSceneManager().CheckCombatMode(eCombatMode.CM_Field) || GameApp.GetSceneManager().CheckCombatMode(eCombatMode.CM_DuoRenPK))
            {
                return;
            }
            //if (GameApp.GetSceneManager().CheckCombatMode(eCombatMode.CM_Field) )//“∞Õ‚
            //{
            //    return;
            //}

            if (prob <= Xml.Prob.GetValue(Ev.Owner))
            {
              //  Debug.Log("h1111111111111111111");
                Vector3 dir = HitAction.SkillDir != null ? HitAction.SkillDir.Dir : Vector3.zero;
                CharacterBase target = Ev.Target;
             
                float h0 = Xml.H0;
                float v0 = Xml.V0;
                if (!target.IsAttackFly())
                {
                    h0 = 0.0f;
                }
                if (!target.IsAttackMove())
                {
                    v0 = 0.0f;
                }
                if (h0 > 0.0f)
                {
                    if (h0 > 10)
                    {
                        h0 = 10.0f;
                    }
                    if (target is NetPlayer)
                    {
                        GameApp.GetNetHandler().SendNetPlayerHit((int)GameEventID.FMS_EVENT_HITFLY,(uint)target.m_NetObjectID, h0, v0, dir);
                    }
                    else
                    {
                      //  Debug.Log("222222222222222" + h0 + "vo==" + v0);
                        ML2Event tempevent = new ML2Event((int)GameEventID.FMS_EVENT_HITFLY);
                        tempevent.PushUserData<float>(v0);
                        tempevent.PushUserData<float>(h0);
                        tempevent.PushUserData<Vector3>(dir);
                        GameEvent.DispatchEvent(target, tempevent);
                    }
			    }
                else
                {
                    if (v0 > 6)
                    {
                        v0 = 6f;
                    }
                    if (target is NetPlayer)
                    {
                        GameApp.GetNetHandler().SendNetPlayerHit((int)GameEventID.FMS_EVENT_HURT, (uint)target.m_NetObjectID, Xml.T, v0, dir);
                    }
                    else
                    {
                       // Debug.Log("FMS_EVENT_HURT" + target.m_NetObjectID + "v0=" + v0);
                        ML2Event tempevent = new ML2Event((int)GameEventID.FMS_EVENT_HURT);
                        tempevent.PushUserData<float>(v0);
                        tempevent.PushUserData<float>(Xml.T);
                        tempevent.PushUserData<Vector3>(dir);
                        GameEvent.DispatchEvent(target, tempevent);
                    }
                }

                target.SkillComp.BreakAllSkill(false);
            }
        }
    }
}