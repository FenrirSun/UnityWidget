using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GameProtocol;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEngine.Build.Pipeline;


/// 自定义打包方式
[CreateAssetMenu(fileName = "ZenBuildScriptPackedMode.asset",
    menuName = "Addressables/Content Builders/Zen Build Script")]
public class ZenBuildScriptPackedMode : BuildScriptPackedMode {
    public override string Name {
        get { return "Zen Build Script"; }
    }

    protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput,
        AddressableAssetsBuildContext aaContext) {
        TResult result = default;
        try {
            ClearDirtyFile();
            result = base.DoBuild<TResult>(builderInput, aaContext);
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var pid = settings.profileSettings.GetProfileId(AddressableUtils.PROFILE_SETTING_NAME_DEFAULT);
            if (settings.activeProfileId == pid) {
                string streamingAssetsPath = "Assets/StreamingAssets";
                if (!Directory.Exists(streamingAssetsPath))
                    Directory.CreateDirectory(streamingAssetsPath);

                ZEditorUtils.DelFiles(streamingAssetsPath, "*.bundle");

                var devPath =
                    settings.profileSettings.EvaluateString(pid,
                        settings.profileSettings.GetValueByName(pid, "DevBuildPath"));
                if (Directory.Exists(devPath)) {
#if ENV_LOCAL || ENV_ALPHA || ENV_BETA || UNITY_STANDALONE || UNITY_EDITOR
                    ZEditorUtils.CopyFiles(devPath + "/*.bundle", streamingAssetsPath);
#endif
                    Directory.Delete(devPath, true);
                }
            }

            var manifest = new AssetManifest() {
                Platform = builderInput.Target.ToString()
            };
            foreach (var ccde in aaContext.locations) {
                if (ccde.InternalId.Contains("_zen_remote_") && ccde.Data is AssetBundleRequestOptions abro) {
                    var path = ccde.InternalId.Replace("_zen_remote_", settings.profileSettings.EvaluateString(pid,
                        settings.profileSettings.GetValueByName(pid, "RemoteBuildPath")));
                    var key = Path.GetFileNameWithoutExtension(ccde.InternalId);
                    manifest.Hash[key] = GetBundleHash128(path);
                    manifest.BundleName[key] = abro.BundleName;
                }
            }

            ZEditorUtils.WriteString($"Assets/Resources/asset_manifest.json",
                ZProto.EnumAsIntFormatter.Format(manifest));
        } catch (Exception ex) {
            Debug.LogException(ex);
        } finally {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
        return result;
    }
    
    static string GetBundleHash128(string bundlePath) {
        var data = File.ReadAllBytes(bundlePath);
        var osha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
        var rhash = osha1.ComputeHash(data);
        var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        var rMd5 = md5.ComputeHash(rhash);
        var rMd5Str = System.BitConverter.ToString(rMd5).Replace("-", "").ToLower();
        Hash128 hashMd512 = Hash128.Parse(rMd5Str);
        osha1.Clear();
        md5.Clear();
        return hashMd512.ToString();
    }

    protected override string ConstructAssetBundleName(AddressableAssetGroup assetGroup, BundledAssetGroupSchema schema,
        BundleDetails info, string assetBundleName) {
        string groupName = assetGroup.Name.Replace(" ", "").Replace('\\', '/').Replace("//", "/").ToLower();
        assetBundleName = groupName + "_" + assetBundleName.Replace("/", "_");
        return assetBundleName;
    }

    void ClearDirtyFile() {
        ShellHelper.ProcessCommandSync("rm -rf *", $"{Application.dataPath}/../ServerData");
    }
}