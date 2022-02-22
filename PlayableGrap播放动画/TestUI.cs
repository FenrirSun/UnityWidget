using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUI : MonoBehaviour
{

    public AnimationClip[] Clips;
    public GameObject[] Categories;
    public float fps;
    public int fpssamples;
    public int numInstances;
    private List<GameObject> m_Instances;

    private SimpleAnimation _sanim;
    private SimpleAnimation Sanim
    {
        get
        {
            if(!_sanim)
                _sanim = FindObjectOfType<SimpleAnimation>();
            return _sanim;
        }
    }
    
    // Use this for initialization
    void Start ()
    {
        m_Instances = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        if (Categories == null)
            return;

        GUILayout.BeginHorizontal();
        foreach(var category in Categories)
        {
            if (GUILayout.Button(category.name))
            {
                fpssamples = 0;
                foreach (var instance in m_Instances)
                {
                    Destroy(instance);
                }

                for (int i = 0; i < numInstances; i++)
                {
                    m_Instances.Add(Instantiate(category) as GameObject);
                }
            }
        }
        foreach(var clip in Clips)
        {
            if (GUILayout.Button(clip.name)) {
                if (Sanim.GetState(clip.name) == null) {
                    Sanim.AddState(clip, clip.name);
                }
                Sanim.CrossFade(clip.name, 0.2f);
            }
        }
        GUILayout.EndHorizontal();

        var fpsCounter = new Rect(new Vector2(0.9F * Screen.width, 0.1f * Screen.height), new Vector2(0.1F * Screen.width, 0.1f * Screen.height));
        float framerate = 1 / Time.deltaTime;
        fps = (1 * framerate + fpssamples * fps) / (1 + fpssamples);
        GUI.Label(fpsCounter, fps.ToString());
        fpssamples = Mathf.Min(1000, fpssamples + 1);
    }
}
