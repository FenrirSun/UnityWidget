using System;
using UnityEngine;
class SkillUtility
{
    public static float GetGroundHeight(Vector3 pos)
    {
        return GetGroundHeight(pos.x, pos.y, pos.z);
    }

    public static float GetGroundHeight(float x, float y, float z)
    {
        float ret = 0.0f;
        RaycastHit hit;

        int layerMask = 1<<25;/*(1 << (int)ML2Layer.LayerMonster) | 
            (1 << (int)ML2Layer.LayerPlayer) | 
            (1 << (int)ML2Layer.LayerNetPlayer) | 
            (1 << (int)ML2Layer.LayerNPC) | 
            (1 << (int)ML2Layer.LayerTrigger) |
            (1 << (int)ML2Layer.LayerEffect);
        layerMask = ~ layerMask;*/

        if (Physics.Raycast(new Vector3(x, y + 100.0f, z), Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            ret = hit.point.y;
        }
        return ret;
    }

    public static float GetNearestGroundHeight(Vector3 pos)
    {
        return GetNearestGroundHeight(pos.x, pos.y, pos.z);
    }

    public static float GetNearestGroundHeight(float x, float y, float z)
    {
        float ret = 0.0f;
        RaycastHit hit;
        int layerMask = 1 << 25;
        if (Physics.Raycast(new Vector3(x, y + 2.0f, z), Vector3.down, out hit, 5.0f, layerMask))
        {
            ret = hit.point.y;
        }
        else
        {
            if (Physics.Raycast(new Vector3(x, y + 100.0f, z), Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                ret = hit.point.y;
            }
        }
        return ret;
    }

}
