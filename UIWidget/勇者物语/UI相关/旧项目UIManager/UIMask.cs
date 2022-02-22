using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class UIMask : MonoBehaviour {
    private UITexture m_image;

    public float Alpha
    {
        get
        {
            if (m_image == null) { Init(); }
            return m_image.alpha;
        }
        set
        {
            m_image.alpha = value;
        }
    }

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        m_image = transform.FindChild("Image").GetComponent<UITexture>();
        if (m_image.mainTexture == null)
        {
            Texture2D tex = new Texture2D(2, 2);
            for (int i = 0; i < tex.width; i++)
            {
                for (int j = 0; j < tex.height; j++)
                {
                    tex.SetPixel(i, j, Color.white);
                }
            }

            m_image.mainTexture = tex;
        }
    }

    void Update() { }
}
