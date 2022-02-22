using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabSaveModle : ScriptableObject
{
    [System.Serializable]
    public class PrefabPath
    {
        public string name;
        public string path;
    }

    public List<PrefabPath> windowPath;

}
