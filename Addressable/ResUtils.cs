using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using AsyncHandleBundles =
    UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<System.Collections.Generic.IList<
        UnityEngine.ResourceManagement.ResourceProviders.IAssetBundleResource>>;
using Object = UnityEngine.Object;

public static class SpriteLoader {
    public static void LoadSprite(this UnityEngine.UI.Image img, string address) {
        if (address.Contains("[")) {
            // 带方括号的，认为是图集中加载的sprite
            var splitName = address.Split('[');
            if (splitName.Length == 2) {
                var spr = ResUtils.LoadSpriteFromAtlas(splitName[0], splitName[1].Replace("]", string.Empty));
                img.sprite = spr;
            }
        } else {
            var spr = ResUtils.Load<Sprite>(address);
            img.sprite = spr;
        }
    }
    public static void LoadSprite(this SpriteRenderer img, string address) {
        var spr = ResUtils.Load<Sprite>(address);
        img.sprite = spr;
    }
}

/// 加载资源工具
public class ResUtils : Singleton<ResUtils> {
    private Dictionary<string, AsyncHandleBundles> preloadKeyMap;

    private ResourceLocationMap locator;
    private IResourceLocator dynLocator;

    public static void Init() {
        Instance.preloadKeyMap = new Dictionary<string, AsyncHandleBundles>();
        ResourceManager.ExceptionHandler = null;
        var all = Addressables.ResourceLocators;
        foreach (var locator in all) {
#if UNITY_EDITOR
            if (locator.GetType().Name.Equals("AddressableAssetSettingsLocator")) {
                Instance.dynLocator = locator;
                break;
            }
#endif
            if (locator is ResourceLocationMap rlm) {
                Instance.locator = rlm;
                break;
            }
        }
    }

    public static void Dump() {
        Debug.Log("[SyncAddressables] preloaded");
        foreach (var kv in Instance.preloadKeyMap) {
            Debug.Log($"[SyncAddressables] p {kv.Key} {kv.Value.Status}");
        }
    }

    public static void Free(string address) {
        var km = Instance.preloadKeyMap;
        if (km.TryGetValue(address, out var handle)) {
            Debug.Log($"[SyncAddressables] Free {address}");
            Addressables.Release(handle);
            km.Remove(address);
        }
    }
    
    public static bool IsValidAddress<T>(string address) {
        if (string.IsNullOrEmpty(address)) {
            return false;
        }
        if (Instance.locator != null) {
            return Instance.locator.Locations.ContainsKey(address);
        } else {
            return Instance.dynLocator.Locate(address, typeof(T), out _);
        }
    }
    
    public static T LoadIntro<T>(string id) where T : UnityEngine.Object {
#if UNITY_EDITOR
        var config = UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"Assets/Artworks/ResDefault/Intros/{id}.asset");
#else
        var config = Load<T>($"Intros/{id}");
#endif
        
#if ENV_ALPHA || ENV_BETA || ENV_LOCAL
        if (config == null) {
            Debug.LogError($"[Intro] can not find {typeof(T)} {id}");
            ZEventManager.Instance.Send(GameEvents.PromptShowText.Create($"引导{id}加载失败"));
            return default;
        }
#endif
        config = Instantiate(config);
        return config;
    }

    public static TObject LoadPrefab<TObject>(string key, bool needInstantiate) where TObject : Object {
        var obj = Load<TObject>(key + ".prefab", out var err);
        if (!string.IsNullOrEmpty(err)) {
            // Debug.LogError($"[SyncAddressables] Load [{key}] failed. {err}.");
            return null;
        }
        return needInstantiate ? Object.Instantiate(obj) : obj;
    }

    public static TObject Load<TObject>(string key) {
        var obj = Load<TObject>(key, out var err);
        if (!string.IsNullOrEmpty(err)) {
            Debug.LogError($"[SyncAddressables] Load [{key}] failed. {err}.");
        }
        return obj;
    }

    public static void LoadScene(string address) {
        var handle = Addressables.LoadSceneAsync(address);
        handle.WaitForCompletion();
        Addressables.Release(handle);
    }
    
    private static TObject Load<TObject>(string key, out string err) {
        if (string.IsNullOrEmpty(key)) {
            err = "Key is null";
            return default;
        }
        var op = Addressables.LoadAssetAsync<TObject>(key);
        op.WaitForCompletion();
        TObject result = default;
        if (op.IsDone && op.Status == AsyncOperationStatus.Succeeded) {
            err = string.Empty;
            result = op.Result;
        } else {
            err = op.Status.ToString();
        }
        Addressables.Release(op);
        return result;
    }
    
    public static Sprite LoadSpriteFromAtlas(string atlasAddress, string spriteName) {
        var sa = Load<SpriteAtlas>(atlasAddress);
        if (sa) {
            return sa.GetSprite(spriteName);
        }
        return null;
    }
    
    public IEnumerator FindDependencies(string address, Action<HashSet<IResourceLocation>> onCompleted) {
        var ll = Addressables.LoadResourceLocationsAsync(address);
        yield return ll;

        if (ll.Status != AsyncOperationStatus.Succeeded) {
            Debug.LogError($"[SyncAddressables] FindDependance failed {address}");
            onCompleted(null);
            yield break;
        }

        var bundles = new HashSet<IResourceLocation>();

        foreach (var loc in ll.Result) {
            FindBundle(loc, bundles);
        }
        
        Addressables.Release(ll);
        
        onCompleted(bundles);
    }

    public static void GetResourceLocations(string address, Action<string> callback) {
        var ll = Addressables.LoadResourceLocationsAsync(address);
        ll.Completed += handle => {
            foreach (var loc in handle.Result) {
                callback(loc.PrimaryKey);
            }
            Addressables.Release(ll);
        };
    }
    
    public static IEnumerator Preload(string address, Action onCompleted = null) {
        if (Instance.preloadKeyMap.TryGetValue(address, out _)) {
            onCompleted?.Invoke();
            yield break;
        }

        var ll = Addressables.LoadResourceLocationsAsync(address);
        yield return ll;

        if (ll.Status != AsyncOperationStatus.Succeeded) {
            Debug.LogError($"[SyncAddressables] Preload failed {address}");
            onCompleted?.Invoke();
            yield break;
        }

        var handle = Instance.PreLoadImpl(ll.Result);
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (Instance.preloadKeyMap.ContainsKey(address)) {
                Addressables.Release(Instance.preloadKeyMap[address]);
            }

            Instance.preloadKeyMap[address] = handle;
        } else {
            Addressables.Release(handle);
            Debug.LogError($"[SyncAddressables] Preload failed {address}");
        }

        Addressables.Release(ll);

        onCompleted?.Invoke();
    }
    
    private AsyncHandleBundles PreLoadImpl(IList<IResourceLocation> locs) {
        var bundles = new HashSet<IResourceLocation>();

        foreach (var loc in locs) {
            //Debug.Log($"[SyncAddressables] Preload {loc.InternalId}");
            FindBundle(loc, bundles);
        }

        foreach (var bloc in bundles) {
            //Debug.Log($"[SyncAddressables] Preload bundle {bloc.InternalId}");
        }
        
        var la = Addressables.LoadAssetsAsync<IAssetBundleResource>(bundles.ToList(), obj => { });
        return la;
    }

    private void FindBundle(IResourceLocation loc, HashSet<IResourceLocation> result) {
        if (loc.ProviderId == typeof(AssetBundleProvider).FullName) {
            result.Add(loc);
        }

        if (loc.HasDependencies) {
            foreach (var dloc in loc.Dependencies) {
                FindBundle(dloc, result);
            }
        }
    }
    
    #region 对于addressable的internalid的转换处理
    [RuntimeInitializeOnLoadMethod]
    private static void SetInternalIdTransform() {
        Addressables.InternalIdTransformFunc = MyCustomTransform;
    }

    public static string MyCustomTransform(IResourceLocation location) {
        var realId = location.InternalId;

        if (location.InternalId.Contains("_zen_dev_")) {
            realId = location.InternalId.Replace("_zen_dev_", Application.streamingAssetsPath);
        }

        if (location.InternalId.Contains(OtaManager.REMOTE_PATH_TAG)) {
            if (location.InternalId == OtaManager.REMOTE_PATH_TAG + "/catalog_remote.hash") {
                realId = string.Empty;
            } else {
                realId = TranslateUrl(location.InternalId);
            }
        }
        
        return realId;
    }

    public static string GetPlatform() {
#if UNITY_STANDALONE_OSX
        return "StandaloneOSX";
#else
        return PlatformMappingService.GetPlatformPathSubFolder();
#endif
    }

    static string TranslateUrl(string url) {
#if UNITY_EDITOR
        //调试用
        string editorPath = Path.Combine(System.Environment.CurrentDirectory, "ServerData", GetPlatform());
        string realId = url.Replace(OtaManager.REMOTE_PATH_TAG, $"file://{editorPath}");
        
        // string realId = url.Replace(OtaManager.REMOTE_PATH_TAG,
        //     $"{ProjectConfig.SERVER_OTA}/1/StandaloneOSX");
#else
        string realId = url.Replace(OtaManager.REMOTE_PATH_TAG,
            $"{ProjectConfig.SERVER_OTA}/{ProjectConfig.BUILD_NUMBER}/{GetPlatform()}");
#endif
        return realId;
    }

    #endregion
}