using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameProtocol;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

/// bundle下载管理器
public class OtaManager : Singleton<OtaManager> {
    public const string LABEL_DEV = "b_dev";
    public const string LABEL_DEFAULT = "b_dft";
    public const string LABEL_DIALOG = "b_dlg";
    public const string LABEL_M3 = "b_m3";
    public const string LABEL_FONTS = "b_fonts";
    public const string LABEL_TABLE = "b_tables";

    public const string LABEL_REMOTE_TABLE = "b_remote_tables";
    public const string LABEL_PREFIX_ROOM = "r_";
    public const string REMOTE_PATH_TAG = "_zen_remote_";

    private const int local_room_num = 50;
    
    public enum ResState {
        PENDING = 0,
        DOWNING,
        FAILED,
        OK,
    }

    private class Bundle {
        public string name;          // Addressable中显示的可读的bundle名
        public string bundleName;    // 实际下载的bundle名字，即FileNameHash
        public Hash128 hash;

        public void CleanAll() {
            Caching.ClearAllCachedVersions(bundleName);
        }

        public void CleanOther() {
            Caching.ClearOtherCachedVersions(bundleName, hash);
        }

        public bool Cached {
            get => Caching.IsVersionCached(bundleName, hash);
        }
    }

    private class Bundles {
        public List<Bundle> bundles = new List<Bundle>();
        public long size;
        public ResState state = ResState.PENDING;

        public void CleanOld() {
            foreach (var bundle in bundles) {
                bundle.CleanOther();
            }
        }

        public void CleanAll() {
            foreach (var bundle in bundles) {
                bundle.CleanAll();
            }

            state = ResState.PENDING;
        }
    }

    private Dictionary<string, Bundles> bundlesMap = new Dictionary<string, Bundles>();
    private Dictionary<string, List<AsyncOperationHandle>> downloading = new Dictionary<string, List<AsyncOperationHandle>>();
    private Dictionary<string, List<float>> localProgress = new Dictionary<string, List<float>>(); // 本地已经存在的资源所占的初始进度
    private List<string> queue = new List<string>();
    private AssetManifest manifest;
    
    public static string RoomIdToLabel(int roomId) {
        return $"{LABEL_PREFIX_ROOM}{roomId}";
    }

    public void GetOtaStateByRoomId(int roomId, out ResState state, out float progress, out double size) {
        var label = RoomIdToLabel(roomId);
        if (bundlesMap.TryGetValue(label, out var bs)) {
            size = Mathf.Max(bs.size / 1024f / 1024f, 0.1f);
        } else {
            size = 0;
        }
        
        var entry = ZTableManager.Instance.GetRoomByID(roomId);
        if (entry == null || entry.Id <= local_room_num || IsLabelReady(label)) {
            state = ResState.OK;
            progress = 1f;
        } else if (downloading.TryGetValue(label, out var handles)) {
            var localProgressList = localProgress[label];
            state = ResState.DOWNING;
            progress = 0;
            float totalLocalProgress = 0;
            for (int i = 0; i < handles.Count; ++i) {
                var handle = handles[i];
                if(handle.Status != AsyncOperationStatus.Failed)
                    progress += handle.PercentComplete - localProgressList[i];
                totalLocalProgress += localProgressList[i];
            }
            progress /= handles.Count - totalLocalProgress;
        } else {
            state = bs?.state ?? ResState.PENDING;
            progress = 0;
        }
        progress = Mathf.Clamp01(progress);
    }

    public void CleanAllCache() {
        foreach (var kv in bundlesMap) {
            kv.Value.CleanAll();
        }
    }

    public IEnumerator Init() {
        manifest = ZProto.Parser.Parse<AssetManifest>(Resources.Load<TextAsset>("asset_manifest").text);
        
        var entries = ZTableManager.Instance.GetRoom().entries;
        foreach (var e in entries) {
            if (e.Id <= local_room_num) {
                continue;
            }
        
            var label = RoomIdToLabel(e.Id);
            yield return GenBundlesMap(label);
        }
        UpdateQueue();
    }

    private void UpdateQueue() {
        queue.Clear();
        var roomEntries = ZTableManager.Instance.GetRoom().entries;
        
        foreach (var entry in roomEntries) {
            if (entry.Id > local_room_num) {
                AddToQueue(entry);
            }
        }

        var curRoomId = ZGameRuntime.Instance.UserData.CurrentRoom.RoomId;
        if (curRoomId > local_room_num) {
            AddToQueue(ZTableManager.Instance.GetRoomByID(curRoomId), true);
        }
        Download();
    }

    // public void MoveToFront(int roomId) {
    //     AddToQueue(ZTableManager.Instance.GetRoomByID(roomId), true);
    //     Download();
    // }
    
    private void AddToQueue(ZTableRoom.Entry e, bool front = false) {
        var label = RoomIdToLabel(e.Id);
        if (e.Id > local_room_num && !IsLabelReady(label)) {
            if(queue.Contains(label))
                queue.Remove(label);
            if (front) {
                queue.Insert(0, label);
            } else {
                queue.Add(label);
            }
        }
    }

    private void Download() {
        Dump();
        while (downloading.Count < 1) {
            if (queue.Count > 0) {
                var label = queue[0];
                var bundles = bundlesMap[label];
                if (bundles != null) {
                    bundles.state = ResState.DOWNING;
                }
                queue.RemoveAt(0);
                var handle = Addressables.DownloadDependenciesAsync(label, true);
                List<AsyncOperationHandle> deps = new List<AsyncOperationHandle>();
                handle.GetDependencies(deps);
                foreach (var dep in deps) {
                    if (dep.Result is List<AsyncOperationHandle> _res) {
                        if (!downloading.ContainsKey(label)) {
                            downloading[label] = new List<AsyncOperationHandle>();
                            localProgress[label] = new List<float>();
                        }
                        foreach (var single in _res) {
                            // 只有未完成下载的才加入下载进度
                            if (single.Status == AsyncOperationStatus.None && single.PercentComplete < 1) {
                                downloading[label].Add(single);
                                localProgress[label].Add(single.PercentComplete);
                            }
                        }
                    }
                }
                handle.Completed += operationHandle => {
                    if (operationHandle.IsDone) {
                        downloading.Remove(label);
                        if (operationHandle.Status == AsyncOperationStatus.Succeeded) {
                            if (bundles != null) {
                                bundles.state = ResState.OK;
                                bundles.CleanOld();
                            }
                            StartCoroutine(Preload(label));
                            Download();
                        } else {
                            if (bundles != null) {
                                bundles.state = ResState.FAILED;
                            }
                        }
                    }

                    Debug.Log($"ota {label} : {operationHandle.IsDone} {operationHandle.Status}");
                };
                Debug.Log($"ota start download {label}");
            } else {
                break;
            }
        }
    }

    IEnumerator Preload(string label) {
        yield return ResUtils.Preload(label);
    }
    
    private IEnumerator GenBundlesMap(string label) {
        if (bundlesMap.ContainsKey(label)) {
            yield break;
        }

        yield return ResUtils.Instance.FindDependencies(label, set => {
            var bundles = bundlesMap[label] = new Bundles();
            var allCached = true;
            var size = 0L;
            foreach (var loc in set) {
                if (!loc.InternalId.Contains(REMOTE_PATH_TAG)) {
                    continue;
                }

                if (loc.Data is AssetBundleRequestOptions opt) {
                    var folderName = Path.GetFileNameWithoutExtension(loc.InternalId);
                    if (!manifest.BundleName.ContainsKey(folderName)) {
                        continue;
                    }
                    var bundle = new Bundle() {
                        name = folderName,
                        bundleName = manifest.BundleName[folderName]
                    };
                    if (manifest.Hash.TryGetValue(bundle.name, out var hash)) {
                        opt.Hash = hash;
                    }
                    bundle.hash = Hash128.Parse(opt.Hash);
                    size += opt.BundleSize;
                    bundles.bundles.Add(bundle);
                    if (!bundle.Cached) {
                        allCached = false;
                    }
                }
            }

            bundles.size = size;
            if (allCached) {
                bundles.state = ResState.OK;
            }
        });
    }

    private bool IsLabelReady(string label) {
        if (bundlesMap.TryGetValue(label, out var bs)) {
            if (bs.state == ResState.OK) {
                return true;
            }

            foreach (var b in bs.bundles) {
                if (!b.Cached) {
                    return false;
                }
            }

            bs.state = ResState.OK;
            return true;
        } else {
            Debug.LogError($"Unknown label {label}");
        }

        return false;
    }

    public void Dump() {
        var sb = new StringBuilder(100);
        sb.Append("ota queue: ");
        foreach (var s in queue) {
            sb.Append(s);
            sb.Append(" ");
        }

        sb.Append("ready: ");
        foreach (var s in bundlesMap) {
            if (s.Value.state == ResState.OK) {
                sb.Append(s.Key);
                sb.Append(" ");
            }
        }

        Debug.Log(sb.ToString());
    }
}