using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 用于存储制作好的动态图预设上的sprite信息
/// </summary>
public class AtlasMap : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    List<Sprite> spriteList = new List<Sprite>();

    public Sprite GetSpriteByName(string name)
    {
        return spriteList.Find((Sprite sp) => { return sp.name == name; });
    }

    public void AddSprite(Sprite sp)
    {
        spriteList.Add(sp);
    }

}
