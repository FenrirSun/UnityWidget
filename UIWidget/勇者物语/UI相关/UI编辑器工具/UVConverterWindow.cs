using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class UVConverterWindow : EditorWindow
{
    static EditorWindow window;
    static Vector2[] s_uv;

    public static Dictionary<string, List<List<Vector2>>> SceneUVs = new Dictionary<string, List<List<Vector2>>>();
    public static List<string> RegisterAssets = new List<string>();
    public static string ReplaceName = "";

    [MenuItem("ML2Editor/UVConverterWindow")]
    static void Execute()
    {
        if (window == null)
            window = (UVConverterWindow)GetWindow(typeof(UVConverterWindow));
        window.Show();
    }

    void OnGUI()
    {
        /*
        if (GUILayout.Button("Read And Save UV2", GUILayout.Width(200), GUILayout.Height(50)))
        {
            ReadAndSave("totempiller_02");
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Write UV2", GUILayout.Width(200), GUILayout.Height(50)))
        {
            Write("totempiller_02");
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Print", GUILayout.Width(200), GUILayout.Height(50)))
        {
            Print("totempiller_02");
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("PrintGlobal", GUILayout.Width(200), GUILayout.Height(50)))
        {
            PrintGlobal();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("SetTempUVS", GUILayout.Width(200), GUILayout.Height(50)))
        {
            SetAllTemp("totempiller_02", 0.5f);
        }
         * */
        EditorGUILayout.Space();
        if (GUILayout.Button("GetCurrentSceneUVs", GUILayout.Width(200), GUILayout.Height(50)))
        {
            GetAndPrintAllGameobject();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("SaveData", GUILayout.Width(500), GUILayout.Height(50)))
        {
            SaveData("SceneUVs.xml");
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("LoadData", GUILayout.Width(200), GUILayout.Height(50)))
        {
            LoadData("SceneUVs");
        }
        /*
        EditorGUILayout.Space();
        if (GUILayout.Button("Write", GUILayout.Width(200), GUILayout.Height(50)))
        {
            WriteSceneUVS();
        }
         * */
        EditorGUILayout.Space();
        if (GUILayout.Button("SceneUVs.count", GUILayout.Width(200), GUILayout.Height(50)))
        {
            Debug.Log("SceneUVs.count = " + SceneUVs.Count);
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("SceneUVs.Clear", GUILayout.Width(200), GUILayout.Height(50)))
        {
            SceneUVs.Clear();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("GetRegisterObjects", GUILayout.Width(200), GUILayout.Height(50)))
        {
            GetRegisterNames();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("SaveRegisterObjects", GUILayout.Width(200), GUILayout.Height(50)))
        {
            SaveRegisterNames("SceneUVSRegister.xml");
        }
        EditorGUILayout.Space();
        ReplaceName = EditorGUILayout.TextField("ReplaceFbx", ReplaceName);
        if (GUILayout.Button("Do Replace", GUILayout.Width(200), GUILayout.Height(50)))
        {
            Debug.Log("Replace All Fbx in scene, fbx.name = " + ReplaceName);
            TestDeleteSceneGameObjects(ReplaceName);
        }

    }

    void ReadAndSave(string name)
    {
        GameObject asset = GameObject.Find(name) as GameObject;
        foreach (MeshFilter meshFilter in asset.GetComponentsInChildren(typeof(MeshFilter)))
        {
            s_uv = meshFilter.sharedMesh.uv2;
        }
    }

    void Write(string name)
    {
        GameObject asset = GameObject.Find(name) as GameObject;
        foreach (MeshFilter meshFilter in asset.GetComponentsInChildren(typeof(MeshFilter)))
        {
            //meshFilter.sharedMesh.uv2 = new Vector2[s_uv.Length];
            //meshFilter.sharedMesh.uv2 = s_uv;
            MeshUtility.SetPerTriangleUV2(meshFilter.sharedMesh, s_uv);
        }
    }

    void Print(string name)
    {
        GameObject asset = GameObject.Find(name) as GameObject;
        int index = 0;
        foreach (MeshFilter meshFilter in asset.GetComponentsInChildren(typeof(MeshFilter)))
        {
            Vector2[] uvs = meshFilter.sharedMesh.uv2;
            for (int i = 0; i < uvs.Length; i++)
            {
                index++;
                Debug.Log("index = " + index + " uv2.<u, v> = " + uvs[i].x + " " + uvs[i].y);
            }
        }
    }

    void PrintGlobal()
    {
        int index = 0;
        if (s_uv == null)
            return;
        for (int i = 0; i < s_uv.Length; i++)
        {
            index++;
            Debug.Log("index = " + index + " s_uv.<u, v> = " + s_uv[i].x + " " + s_uv[i].y);
        }
    }

    void SetAllTemp(string name, float value)
    {
        GameObject asset = GameObject.Find(name) as GameObject;
        foreach (MeshFilter meshFilter in asset.GetComponentsInChildren(typeof(MeshFilter)))
        {
            Vector2[] uvs = meshFilter.sharedMesh.uv2;
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i].x = value;
                uvs[i].y = value;
            }
            meshFilter.sharedMesh.uv2 = uvs;
        }
    }

    void GetAndPrintAllGameobject2()
    {
        Object[] objects = GameObject.FindObjectsOfTypeAll(typeof(GameObject));
        List<GameObject> LstObjs = new List<GameObject>();
        foreach (Object obj in objects)
        {
            if ((obj as GameObject).GetComponentsInChildren(typeof(MeshFilter)).Length != 0)
                LstObjs.Add((obj as GameObject));
        }
        List<string> LstObjNames = new List<string>();
        foreach (GameObject go in LstObjs)
        {
            if (LstObjNames.Contains(go.name))
                continue;
            else
                LstObjNames.Add(go.name);
        }
        Debug.Log("GameObject.count = " + LstObjNames.Count);
        foreach (string name in LstObjNames)
        {
            Debug.Log(name);
        }
    }

    void GetAndPrintAllGameobject()
    {
        Object[] objects = GameObject.FindObjectsOfTypeAll(typeof(GameObject));
        List<GameObject> LstObjs = new List<GameObject>();
        foreach (Object obj in objects)
        {
            if ((obj as GameObject).GetComponentsInChildren(typeof(MeshFilter)).Length != 0)
                LstObjs.Add((obj as GameObject));
        }
        List<string> LstObjNames = new List<string>();
        foreach (GameObject go in LstObjs)
        {
            if (go.name.Contains("Rain") || go.name.Equals("flower_red01") || go.name.Equals("Snowflakes") || go.name.Equals("Snowbox") || go.name.Equals("levelmeshes_1")
                || go.name.Equals("Envi") || go.name.Equals("shrub") || go.name.Equals("bush") || go.name.Equals("EnVironments"))
            {
                continue;
            }

            if (SceneUVs.ContainsKey(go.name) || LstObjNames.Contains(go.name) )
                continue;
            else
                LstObjNames.Add(go.name);
        }

        foreach (string name in LstObjNames)
        {
            Debug.Log("name = " + name);
            // new GameObject
            List<List<Vector2>> newGoLst = new List<List<Vector2>>();
            GameObject asset = GameObject.Find(name) as GameObject;
            foreach (MeshFilter meshFilter in asset.GetComponentsInChildren(typeof(MeshFilter)))
            {
                // new MeshFilter
                List<Vector2> newMeshFilter = new List<Vector2>();
                Vector2[] uvs = meshFilter.sharedMesh.uv2;
                for (int i = 0; i < uvs.Length; i++)
                {
                    newMeshFilter.Add(new Vector2(uvs[i].x, uvs[i].y));
                }
                newGoLst.Add(newMeshFilter);
            }
            SceneUVs.Add(name, newGoLst);
        }
    }

    public void SaveData(string strFilename)
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlElement XmlRoot = XmlDoc.CreateElement("SceneData");
        XmlDoc.AppendChild(XmlRoot);

        foreach (KeyValuePair<string, List<List<Vector2>>> pair in SceneUVs)
        {
            XmlElement xmlGameObject = XmlDoc.CreateElement("GameObject");
            XmlRoot.AppendChild(xmlGameObject);
            xmlGameObject.SetAttribute("name", pair.Key);

            foreach (List<Vector2> meshUVS in pair.Value)
            {
                XmlElement xmlMeshFilter = XmlDoc.CreateElement("Mesh");
                xmlGameObject.AppendChild(xmlMeshFilter);

                foreach (Vector2 uv in meshUVS)
                {
                    XmlElement xmlUVValues = XmlDoc.CreateElement("UV");
                    xmlMeshFilter.AppendChild(xmlUVValues);

                    xmlUVValues.SetAttribute("U", XmlConvert.ToString(uv.x));
                    xmlUVValues.SetAttribute("V", XmlConvert.ToString(uv.y));
                }
            }
        }

        string strPath = "Assets/Resources/DB/" + strFilename;
        if (strPath != "")
            XmlDoc.Save(strPath);

    }

    public void LoadData(string strFilename)
    {
        XmlDocument xmlDoc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("DB/" + strFilename, typeof(TextAsset));
        if (textAsset == null)
        {
            Debug.LogError("Load Monster DB Failed!");
            return;
        }
        xmlDoc.Load(new StringReader(textAsset.text));
        XmlElement xmlRoot = xmlDoc.DocumentElement;

        SceneUVs.Clear();

        XmlNodeList xmlGOList = xmlRoot.ChildNodes;
        foreach (XmlNode go in xmlGOList)
        {
            if ((go is XmlElement) == false)
                continue;
            
            // GameObject
            XmlElement xmlGO = go as XmlElement;
            List<List<Vector2>> lstGOData = new List<List<Vector2>>();

            foreach (XmlNode mesh in xmlGO.ChildNodes)
            {
                if ((mesh is XmlElement) == false)
                    continue;

                // MeshFilter
                XmlElement xmlMesh = mesh as XmlElement;
                List<Vector2> lstMeshUVs = new List<Vector2>();

                foreach (XmlNode uv in xmlMesh.ChildNodes)
                {
                    if ((uv is XmlElement) == false)
                        continue;

                    XmlElement xmlUV = uv as XmlElement;
                    
                    // UV
                    lstMeshUVs.Add(new Vector2(XmlConvert.ToSingle(xmlUV.GetAttribute("U")), XmlConvert.ToSingle(xmlUV.GetAttribute("V"))));
                }

                lstGOData.Add(lstMeshUVs);
            }

            SceneUVs.Add(xmlGO.GetAttribute("name"), lstGOData);
        }
    }

    private void WriteSceneUVS()
    {
        Object[] objects = GameObject.FindObjectsOfTypeAll(typeof(GameObject));
        List<GameObject> LstObjs = new List<GameObject>();
        foreach (Object obj in objects)
        {
            if ((obj as GameObject).GetComponentsInChildren(typeof(MeshFilter)).Length != 0)
                LstObjs.Add((obj as GameObject));
        }
        List<string> LstObjNames = new List<string>();
        foreach (GameObject go in LstObjs)
        {
            if (go.name.Contains("Rain"))
            {
                continue;
            }
            if (LstObjNames.Contains(go.name))
                continue;
            else
                LstObjNames.Add(go.name);
        }

        foreach (string name in LstObjNames)
        {
            GameObject asset = GameObject.Find(name) as GameObject;
            int index = 0;
            foreach (MeshFilter meshFilter in asset.GetComponentsInChildren(typeof(MeshFilter)))
            {
                Vector2[] uvs = meshFilter.sharedMesh.uv2;
                for (int i = 0; i < uvs.Length; i++)
                {
                    if (i < SceneUVs[name][index].Count)
                    {
                        uvs[i].x = SceneUVs[name][index][i].x;
                        uvs[i].y = SceneUVs[name][index][i].y;
                    }
                }
                meshFilter.sharedMesh.uv2 = uvs;
                index++;
            }
        }
    }

    private void GetRegisterNames()
    {
        foreach (Object o in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            Debug.Log(o.name);
            if (!(o is GameObject))
                continue;

            Debug.Log(AssetDatabase.GetAssetPath(o));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(o));
        }
    }

    public void SaveRegisterNames(string strFilename)
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlElement XmlRoot = XmlDoc.CreateElement("Objects");
        XmlDoc.AppendChild(XmlRoot);

        foreach (string name in RegisterAssets)
        {
            Debug.Log("register.name = " + name);
            XmlElement xmlGameObject = XmlDoc.CreateElement("Fbx");
            xmlGameObject.SetAttribute("Name", name);
            XmlRoot.AppendChild(xmlGameObject);
        }

        string strPath = "Assets/Resources/DB/" + strFilename;
        if (strPath != "")
            XmlDoc.Save(strPath);
    }

    void TestDeleteSceneGameObjects(string fbxName)
    {
        Object goReplace = Resources.Load(fbxName);

        Object[] objects = GameObject.FindObjectsOfTypeAll(typeof(GameObject));
        List<GameObject> LstObjs = new List<GameObject>();
        foreach (Object obj in objects)
        {
            if (obj.name == fbxName)
                LstObjs.Add(obj as GameObject);
        }
        foreach (GameObject go in LstObjs)
        {
            GameObject goOK = GameObject.Instantiate(goReplace) as GameObject;
            goOK.name = go.name;
            goOK.transform.parent = go.transform.parent;
            goOK.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z);
            goOK.transform.localRotation = new Quaternion(go.transform.localRotation.x, go.transform.localRotation.y, go.transform.localRotation.z, go.transform.localRotation.w);
            goOK.transform.localScale = new Vector3(go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z);
            GameObject.DestroyImmediate(go);
        }

    }

}
