using UnityEngine;
using System.Collections;
using AFW;
using UnityEngine.SceneManagement;

public class CreateCharacterCtrl : MonoBehaviour 
{
    public GameObject stone;
    public GameObject hero;
    public GameObject chair;

    private static string DRSceneDir = "arcane/prefab/scene/DR2/DR2.unity";
    private static string ARSceneDir = "arcane/prefab/scene/AR/ArCharSelect.unity";

	void Start () 
    {
	}

    private float mouthPos = 0;

    void Update()
    {
        hero.SetActive(showHero);
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    GameObject gameObj = hitInfo.transform.gameObject;
                    if (gameObj == stone)
                    {
                        Debug.Log("进入AR扫描");
                        LoadARScene();
                    }
                    else if (gameObj == chair)
                    {
                        //Debug.Log("OpenSelectCharPanel");
                        //OpenSelectCharacterPanel();
                        showHero = !showHero    ;
                    }
                    else if(gameObj == hero)
                    {
                        Debug.Log("OpenSelectCharPanel");
                        OpenSelectCharacterPanel();
                    }
                }
            }
            //旋转模型
            float curPos = Input.GetTouch(0).position.x;
            if (mouthPos == 0)
            {
                mouthPos = curPos;
            }
            else
            {
                if (curPos != mouthPos)
                {
                    float changePos = (mouthPos - curPos) % 180f;
                    hero.transform.localRotation = new Quaternion(hero.transform.localRotation.x,
                        hero.transform.localRotation.y + changePos * 0.01f, hero.transform.localRotation.z, hero.transform.localRotation.w);
                    mouthPos = curPos;
                }
            }
        }
#endif

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                GameObject gameObj = hitInfo.transform.gameObject;
                if (gameObj == stone)
                {
                    Debug.Log("进入AR扫描");
                    LoadARScene();
                }
                else if (gameObj == chair)
                {
                    //Debug.Log("OpenSelectCharPanel");
                    //OpenSelectCharacterPanel();
                    showHero = !showHero;
                }
                else if (gameObj == hero)
                {
                    Debug.Log("OpenSelectCharPanel");
                    OpenSelectCharacterPanel();
                }
            }

        }

        float curPosPc = Input.mousePosition.x;
        if (mouthPos == 0)
        {
            mouthPos = curPosPc;
        }
        else
        {
            if (curPosPc != mouthPos)
            {
                float changePos = (mouthPos - curPosPc) % 180f;
                hero.transform.localRotation = new Quaternion(hero.transform.localRotation.x,
                    hero.transform.localRotation.y + changePos * 0.01f, hero.transform.localRotation.z, hero.transform.localRotation.w);
                mouthPos = curPosPc;
            }
        }
#endif
    }

    public static bool showHero = false;
    public static void LoadDRScene(bool showChar = false)
    {
        LoadScene("DR2", DRSceneDir, () =>
            {
                showHero = showChar;
            });

        //SceneManager.LoadScene(0);
        //showHero = true;
    }
    
    public void LoadARScene()
    {
        //LoadScene("ArCharSelect", ARSceneDir);
        SceneManager.LoadSceneAsync(1);
    }

    private void OpenSelectCharacterPanel()
    {
        string userID = Arcane.LocalConfig.data.userID;
        if (string.IsNullOrEmpty(userID))
        {
            var nameRandom = new System.Random();
            userID = nameRandom.Next(1000, 9999).ToString();
            Arcane.LocalConfig.data.userID = userID;
            Arcane.LocalConfig.SaveLocalConfig();
        }

        Util.CallMethod("LoginCtrl", "SendLogin", null, userID);
    }

    public static void LoadScene(string sceneName, string sceneRes, System.Action onFinish = null)
    {
        AssetBundle levelab = Util.ResMgr.LoadAssetBundle(sceneRes);
        //SceneManager.LoadScene(sceneName);
        Util.GameMgr.StartCoroutine(LoadScene(sceneName, onFinish));
    }

    static IEnumerator LoadScene(string sceneResName, System.Action onFinish)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneResName);

        yield return ao;

        if (onFinish != null && ao.isDone)
            onFinish();
    }
}
