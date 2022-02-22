using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using ZenSDK;

/// 打包设置工具
public class AddressableUtils : EditorWindow {
    public const string PROFILE_SETTING_NAME_DEFAULT = "Default";
    public const string PROFILE_SETTING_NAME_NO_OTA = "NoOTA";

    static string[] assetDatabaseFindArgsCache = new string[1];

    public static void BuildAddressable(BuildTarget target, string addressableProfileName) {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        Init(target.ToString());
        var backUp = settings.activeProfileId;
        try {
            var pid = settings.profileSettings.GetProfileId(addressableProfileName);
            settings.activeProfileId = pid;
            AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent();
        } finally {
            settings.activeProfileId = backUp;
        }
    }

    static void Init(string target) {
        var path = $"ServerData/{target}";
        FileUtil.DeleteFileOrDirectory(path);
        var setting = AddressableAssetSettingsDefaultObject.Settings;
        setting.ActivePlayModeDataBuilderIndex = setting.ActivePlayerDataBuilderIndex =
            setting.DataBuilders.FindIndex(a => a.GetType() == typeof(ZenBuildScriptPackedMode));
    }

    public static void BuildAndroidJenkins() {
        ZEditorUtils.CloseLogStackTrace();
        BuildAddressable(BuildTarget.Android, PROFILE_SETTING_NAME_DEFAULT);
    }

    public static void BuildIOSJenkins() {
        ZEditorUtils.CloseLogStackTrace();
        BuildAddressable(BuildTarget.iOS, PROFILE_SETTING_NAME_DEFAULT);
    }

    [MenuItem("Build/BuildOSXJenkins")]
    public static void BuildOSXJenkins() {
        ZEditorUtils.CloseLogStackTrace();
        BuildAddressable(BuildTarget.StandaloneOSX, PROFILE_SETTING_NAME_DEFAULT);
    }

    [MenuItem("Build/保存Addressable设置")]
    public static void SaveAddressableConfig() {
        assetDatabaseFindArgsCache[0] = "Assets/AddressableAssetsData";
        var guids = AssetDatabase.FindAssets("t:ScriptableObject", assetDatabaseFindArgsCache);
        foreach (var guid in guids) {
            var assetName = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetName);
            var sObj = new SerializedObject(obj);
            sObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
        }

        AssetDatabase.SaveAssets();
    }
    
#region 生成 ota group 配置

    [MenuItem("Build/生成Addressable OTA组")]
    public static void CreateAddressableGroupFromTable() {
        GenAddressableOtaGroup();
        SaveAddressableConfig();
    }
    
    class AssetInfo {
        public string address;
        public List<string> labels = new List<string>();
    }

    private static void OtaGroupSetting(BundledAssetGroupSchema bags) {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        bags.UseAssetBundleCrc = false;
        bags.UseAssetBundleCache = true;
        bags.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        bags.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.FileNameHash;
        bags.BuildPath.SetVariableByName(settings, "RemoteBuildPath");
        bags.LoadPath.SetVariableByName(settings, "RemoteLoadPath");
    }

    private static void BuiltinGroupSetting(BundledAssetGroupSchema bags) {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        bags.UseAssetBundleCrc = false;
        bags.UseAssetBundleCache = false;
        bags.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        bags.BuildPath.SetVariableByName(settings, "LocalBuildPath");
        bags.LoadPath.SetVariableByName(settings, "LocalLoadPath");
    }

    private static void GenAddressableOtaGroup() {
        var output = new StringBuilder();
        try {
            var otaAssetInfos = new Dictionary<string, AssetInfo>();
            var otaGroup = CreateOrGetGroup("OTA");
            foreach (var entry in otaGroup.entries) {
                if (AssetDatabase.IsValidFolder(entry.AssetPath)) {
                    var allAssets = AssetDatabase.FindAssets("", new[] { entry.AssetPath });
                    var prefixPath = entry.AssetPath.Substring(0, entry.AssetPath.LastIndexOf('/') + 1);
                    foreach (var guid in allAssets) {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        if (!AssetDatabase.IsValidFolder(path)) {
                            var address = path.Replace(prefixPath, "");
                            RecordAssetInfo(otaAssetInfos, address, guid);
                        }
                    }
                }
            }

            otaGroup = CleanGroup("OTA");
            
            var allLabels = new HashSet<string>() {
                OtaManager.LABEL_DEV,
                OtaManager.LABEL_FONTS,
                OtaManager.LABEL_DEFAULT,
                OtaManager.LABEL_TABLE,
                OtaManager.LABEL_REMOTE_TABLE,
            };
            
            foreach (var kv in otaAssetInfos) {
                var location = AssetDatabase.GUIDToAssetPath(kv.Key);
                location += "," + kv.Value.address + ",";
                for (int i = 0; i < kv.Value.labels.Count; i++) {
                    location += kv.Value.labels[i];
                    if (i != kv.Value.labels.Count - 1) {
                        location += ",";
                    }
                    allLabels.Add(kv.Value.labels[i]);
                }
                output.AppendLine(location);
            }
            
            EditorUtility.DisplayProgressBar("Processing", "Apply labels", 0.01f);
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var currentLabels = GetCurrentLabels();
            var excepts = currentLabels.Except(allLabels).ToArray();
            foreach (var e in excepts) {
                settings.RemoveLabel(e, true);
            }

            foreach (var e in allLabels) {
                settings.AddLabel(e, true);
            }

            EditorUtility.DisplayProgressBar("Processing", $"Gen group OTA", 0.02f);
            
            var otaSchema = otaGroup.GetSchema<BundledAssetGroupSchema>();
            OtaGroupSetting(otaSchema);

            foreach (var pair in otaAssetInfos) {
                var guid = pair.Key;
                var info = pair.Value;
                var entry = settings.CreateOrMoveEntry(guid, otaGroup, false, false);
                entry.address = info.address;
                foreach (var l in info.labels) {
                    entry.SetLabel(l, true, false, false);
                }
        
                CallInternalPropertyMainAssetType(entry);
            }
            
            ZEditorUtils.WriteString("Assets/Artworks/auto_create_addressables_group.log", output.ToString());
        } finally {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void RecordAssetInfo(Dictionary<string, AssetInfo> otaAssetInfos, string address, string guid) {
        var assetInfo = new AssetInfo();
        assetInfo.address = address;
        if (address.StartsWithOptimize("Rooms/room")) {
            var tempPath = address.Replace("Rooms/room", string.Empty);
            var roomIdStr = tempPath.Substring(0, tempPath.IndexOf('/'));
            if (int.TryParse(roomIdStr, out _)) {
                assetInfo.labels.Add(OtaManager.LABEL_PREFIX_ROOM + roomIdStr);
            }
        }
        otaAssetInfos[guid] = assetInfo;
    }
    
    private static void AddLabels(Dictionary<string, AssetInfo> infos, string guid, string[] labels, string address) {
        if (string.IsNullOrEmpty(guid)) {
            Debug.LogError($"{address} is null");
            return;
        }
        if (!infos.TryGetValue(guid, out var info)) {
            info = infos[guid] = new AssetInfo() {
                address = address,
            };
        }
        
        foreach (var label in labels) {
            if (!info.labels.Contains(label)) {
                info.labels.Add(label);
            }
        }
    }

    private static void ProcessDir(Dictionary<string, AssetInfo> infos, string[] label, string path,
        string addressRoot) {
        var paths = AssetDatabase.FindAssets("", new[] { path });
        foreach (var g in paths) {
            var address = AssetDatabase.GUIDToAssetPath(g);
            if (Directory.Exists(address))
                continue;
            address = Path.ChangeExtension(address, null);
            address = address.Substring(addressRoot.Length, address.Length - addressRoot.Length);
            AddLabels(infos, g, label, address);
        }
    }

    private static AddressableAssetGroup CreateOrGetGroup(string name) {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.groups.Find((assetGroup => assetGroup.Name == name));
        if (group == null) {
            group = settings.CreateGroup(name, false, false, true, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        }

        return group;
    }
    
    private static AddressableAssetGroup CleanGroup(string name) {
        var group = CreateOrGetGroup(name);
        if (group != null) {
            List<AddressableAssetEntry> del = new List<AddressableAssetEntry>(group.entries);
            foreach (var e in del) {
                group.RemoveAssetEntry(e, false);
            }

            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, del.ToArray(), true, true);
        }

        return group;
    }

    static List<string> GetCurrentLabels() {
        var type1 = AddressableAssetSettingsDefaultObject.Settings.GetType();
        var f1 = type1.GetField("m_LabelTable", BindingFlags.Instance | BindingFlags.NonPublic);
        var obj1 = f1.GetValue(AddressableAssetSettingsDefaultObject.Settings);
        var type2 = obj1.GetType();
        var f2 = type2.GetField("m_LabelNames", BindingFlags.Instance | BindingFlags.NonPublic);
        var list = f2.GetValue(obj1) as List<string>;
        return list;
    }
    
    private static PropertyInfo piMainAssetType =
        typeof(AddressableAssetEntry).GetProperty("MainAssetType", BindingFlags.Instance | BindingFlags.NonPublic);

    static void CallInternalPropertyMainAssetType(AddressableAssetEntry entry) {
        piMainAssetType.GetValue(entry);
    }
    
#endregion

}