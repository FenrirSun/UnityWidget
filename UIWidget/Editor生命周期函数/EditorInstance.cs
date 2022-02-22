using System;
using System.Collections.Generic;
using Edu100.Table;
using RSG.Promises;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
namespace NewEditor
{
    /// <summary>
    /// 编辑器维护的静态实例
    /// @author Ollydbg
    /// @date 2019-10-16
    /// </summary>

    [InitializeOnLoad]
    public class EditorInstance {
        private static EditorDiscover _discover;

        public static bool isDataReady = false;

        public static bool requestTableData = false;

        /// <summary>
        /// 重新初始化框架
        /// </summary>
        public static EditorDiscover discover {
            get {
                if (_discover == null) {
                    Debug.Log($"_discover is null");

                    _discover = new EditorDiscover();

                    _discover.Execute();

                    // LoadTables();

                    EditorApplication.playModeStateChanged += EditorPlaymodeStateChanged;

                    EditorApplication.update += EditorUpdate;
                    EditorApplication.update += EditorTimer.Update;

                    CompilationPipeline.assemblyCompilationStarted += AssemblyCompilationStarted;
                }
                LoadTables();
                return _discover;
            }
        }
        [UnityEditor.Callbacks.DidReloadScripts]  
        static void ReBindEvent() {
            if (null != discover) {
                discover.Execute(); 
            }
        }

        public static void LoadTables()
        {
            //if (!requestTableData)
            //{
                List<Table_User_Identity> list = Table_User_Identity.GetAllPrimaryList();
                if (list == null || list.Count == 0)
                {
                    Debug.Log($"EditorDiscover LoadTables");
                    isDataReady = false;
                    requestTableData = true;
                    //初始化数据
                    EditorTableLoad.LoadTables(null, () =>
                    {
                        Debug.Log($"EditorDiscover LoadTables ok");
                        isDataReady = true;
                        requestTableData = false;
                        Dictionary<Type, EditorWindow> currentLiveWindow = EditorHelper.GetAllRegeditWindow();
                        if (currentLiveWindow != null)
                        {
                            foreach (KeyValuePair<Type, EditorWindow> ele in currentLiveWindow)
                            {
                                //BaseEditorView<Data>
                                var editor = ele.Value as CustomEditorWindow;
                                editor.OnCustomEnable();
                            }
                        }
                    }, false);
                }
                else
                {
                    isDataReady = true;
                }
            //}
        }

        private static void EditorPlaymodeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.Log($"EditorPlaymodeStateChanged !EditorApplication.isPlaying LoadTables");
                LoadTables();
            }
        }

        private static void EditorUpdate()
        {
            WebRequestManager.instance.Update();
        }

        /// <summary>
        /// 准备好编译了
        /// </summary>
        private static void ResumeCompiler()
        {
            EditorApplication.UnlockReloadAssemblies();
            Debug.Log(EditorApplication.timeSinceStartup);
            Dispose();
        }

        private static void AssemblyCompilationStarted(string obj)
        {
            if (EditorApplication.isCompiling)
            {
                EditorApplication.LockReloadAssemblies();
                Debug.Log(EditorApplication.timeSinceStartup);
                var promiseList = new List<Promise>();
                EventCenter.SendEvent(new CompilerEditorEvent(promiseList));
                //在编译前保存状态 如果全部的内容都处理完了 可以继续 或者捕获到错误 也一定要继续 防止出现死锁
                //TODO 超时可能是需要考虑的
                Promise.All(promiseList)
                    .Then(ResumeCompiler)
                    .Catch(e => ResumeCompiler());
            }
            Debug.Log($"Compilation Started {obj} current discover hashcode: {_discover?.GetHashCode()}");
        }

        public static void Dispose()
        {
            _discover?.Dispose();

            WebRequestManager.instance.Release();

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update -= EditorTimer.Update;
        }
    }
}
