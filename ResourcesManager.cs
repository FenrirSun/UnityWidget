using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using ZenSDK;

public class ResourcesManager
{
    // 这里没有继承 Singleton 是因为那个单例模式无法在非运行时执行，不能预览bytes图片
    private static ResourcesManager instance;
    public static ResourcesManager Instance
    {
        get
        {
            if(instance == null)
                instance = new ResourcesManager();
            return instance;
        }
    }
    
    public const string byteDataPath = "Assets/Resources/ByteImageData.asset";
    // 动态加载的图片缓存
    private Dictionary<string, Texture2D> loadTexRes = new Dictionary<string, Texture2D>();
    private Dictionary<string, int> loadTexCount = new Dictionary<string, int>();
    private ByteImageData byteData;
    // 默认的fb头像，再未联网下载图片之前显示
    public const string defaultFbAvatar = "Textures/profile/profile_fb_default";
    
    /// <summary>
    /// 动态加载sprite的通用方法
    /// </summary>
    public void SetSprite(Image img, string relativePath)
    {
        if (img == null)
            return;
        img.overrideSprite = null;
        if (IsByteImage(relativePath))
        {
            var bt = img.gameObject.GetComponent<ByteImage>();
            if (bt != null)
            {
                bt.SetSprite(relativePath);
            }
            else
            {
                bt = img.gameObject.AddComponent<ByteImage>();
                bt.bytesPath = relativePath;
            }
        }
        else
        {
            Sprite sp = Resources.Load<Sprite>(relativePath);
            if (sp != null)
            {
                img.sprite = sp;
            }
            else
            {
                int lastIndex = relativePath.LastIndexOf("/");
                string atlasPath = relativePath.Substring(0, lastIndex);
                string spriteName = relativePath.Substring(lastIndex + 1);
                var atlas = Resources.Load<SpriteAtlas>(atlasPath);
                if (atlas != null)
                {
                    var sprite = atlas.GetSprite(spriteName);
                    if (sprite != null)
                    {
                        img.sprite = sprite;
                    }
                }
            }
        }
    }
    
    public void SetAvatarIcon(Image img, string avatarId)
    {
        if (string.IsNullOrEmpty(avatarId))
            return;

        if (int.TryParse(avatarId, out int avatarIntId))
        {
            var avatarItem = ZTableManager.Instance.GetItem().entries
                .Find(t => t.Type.Contains("avatar") && t.TypeParam == avatarId);
            if(avatarItem != null)
                SetSprite(img, avatarItem.Icon);
        }
        else
        {
            // 如果id不是int类型的，认为是social id，当做url加载图片
            SetSprite(img, defaultFbAvatar);
            img.OverrideSpriteByURL(img, avatarId);
        }
    }
    
    /// <summary>
    /// 用于替代 Resource.Load
    /// 注意不要使用 Load<Sprite>,请使用SetSprite替代
    /// 手动调用 Load<Texture2D>() 时需要调用 ReleaseTexture 释放资源
    /// </summary>
    public T Load<T>(string relativePath) where T:Object
    {
        // texture 类型的资源，加载的时候先判断有没有Bytes化
        if (typeof(T) == typeof(Texture2D))
        {
            if (IsByteImage(relativePath))
            {
                var texture = LoadTextureByBytes(relativePath);
                if (texture != null)
                    return texture as T;
            }
        }
        
        T asset = Resources.Load<T>(relativePath);
//#if UNITY_EDITOR
//        if (asset == null)
//            asset = UnityEditor.AssetDatabase.LoadAssetAtPath(relativePath);
//#endif
        return asset;
    }

    public bool IsByteImage(string relativePath)
    {
        if(byteData == null)
            LoadByteData();

        return byteData.resTextureBytes.Contains(relativePath);
    }
    
    /// <summary>
    /// 通过加载bytes文件生成sprite
    /// </summary>
    public void CreateSpriteByBytes(Image img, string relativePath)
    {
        if (img == null)
            return;

        var texture = LoadTextureByBytes(relativePath);
        if (texture != null)
        {
            var sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            img.sprite = sp;
        }
    }

    /// <summary>
    /// 释放texture资源，调用 Load<Texture2D>() 时需要手动调用
    /// 调用 SetSprite() 时不需要调用此方法！
    /// </summary>
    public void ReleaseTexture(string relativePath)
    {
        if (loadTexCount.ContainsKey(relativePath))
        {
            loadTexCount[relativePath]--;
            if (loadTexCount[relativePath] == 0)
            {
                GameObject.Destroy(loadTexRes[relativePath]);
                loadTexRes.Remove(relativePath);
                loadTexCount.Remove(relativePath);
            }
        }
    }
    
    private Texture2D LoadTextureByBytes(string relativePath)
    {
        Texture2D texture = null;
        if (loadTexRes.ContainsKey(relativePath))
        {
            if (loadTexRes[relativePath] != null)
            {
                texture = loadTexRes[relativePath];
            }
            else
            {
                // 走到这里有可能是资源已经被卸载了，那就清空记录
                loadTexRes.Remove(relativePath);
                loadTexCount.Remove(relativePath);
            }
        }
        
        if(texture == null)
        {
            var ta = Resources.Load<TextAsset>(relativePath);
            if (ta != null)
            {
                texture = CreateTexture(ta.bytes);
                texture.name = ta.name;
            }
        }

        if (texture != null)
        {
            if(!loadTexRes.ContainsKey(relativePath))
                loadTexRes.Add(relativePath, texture);
            if (loadTexCount.ContainsKey(relativePath))
                loadTexCount[relativePath]++;
            else
                loadTexCount.Add(relativePath, 1);
        }
        
        return texture;
    }
    
    public static Texture2D CreateTexture(byte[] data)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.anisoLevel = 1;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.LoadImage(data);
        return texture;
    }
    
    private void LoadByteData()
    {
        byteData = Resources.Load<ByteImageData>("ByteImageData"); 
    }
}