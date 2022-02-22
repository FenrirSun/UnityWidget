using System;
using UnityEngine;

public class CSkillEffectBase
{
    public CSkillEvent Ev{get;private set;}
    public virtual Vector3 Pos
    {
        get
        {
            return Vector3.zero;
        }
    }
    public virtual Vector3 Dir
    {
        get
        {
            return Vector3.zero;
        }
    }
    public virtual String Name
    {
        get
        {
            return "";
        }
    }
    public virtual float DelaySeconds
    {
        get
        {
            return 0.0f;
        }
    }

    public virtual float Scale
    {
        get
        {
            return 1.0f;
        }
    }

    public bool IsActive
    {
        get
        {
            return effect != null || !began;
        }
    }

    private float beginTime;
    private bool began;
    public Effect effect;


    public CSkillEffectBase(CSkillEvent ev)
    {
        Ev = ev;
    }

    public virtual void Init()
    {
        began = false;
        if (DelaySeconds > 0.0f)
        {
            beginTime = Time.time + DelaySeconds;
        }
        else
        {
            BeginEffect();
        }
    }

    public virtual void Tick()
    {
        if (began)
        {

        }
        else
        {
            if (Time.time >= beginTime)
            {
                BeginEffect();
            }
        }
    }

    public void EndEffect()
    {
        OnEnd();
        if (effect != null)
        {
          //  Debug.Log("del skill effect----------------------------------------------");
            GameApp.GetEffectManager().DeleteEffect(effect);
            //GameObject.Destroy(effect);
            effect = null;
        }
        began = true;
    }

    public void Break()
    {
        EndEffect();
    }

    private void BeginEffect()
    {
        OnBegin();
        //string strEffectName = Name;
        //strEffectName = "Skill/" + strEffectName;
        //if (strEffectName == "Skill/LightingCage")
        {
            //effect = GameApp.GetEffectManager().InstEffect(strEffectName,Pos);
            //effect.transform.forward = Dir;

            //Debug.Log("@@@@@@@@@@@@@@@@@@@@@@@BeginEffect name = " + Name);

            effect = GameApp.GetEffectManager().CreateEffect(Name,Pos);
            effect.SetPosition(Pos);
            effect.SetDir(Dir);
            effect.SetScale(Scale);

            if (Ev.Owner is UIPlayer)
                effect.SetLayer(ML2Layer.LayerUI);
        }

        began = true;
    }

    protected virtual void OnBegin()
    {
    }

    protected virtual void OnEnd()
    {
    }
}