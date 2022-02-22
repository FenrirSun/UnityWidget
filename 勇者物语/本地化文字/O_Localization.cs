using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class O_Localization : MonoBehaviour
{
	static O_Localization mInst;

	static public O_Localization instance
	{
		get
		{
			if (mInst == null)
			{
				mInst = Object.FindObjectOfType(typeof(O_Localization)) as O_Localization;

				if (mInst == null)
				{
					GameObject go = new GameObject("_Localization");
					DontDestroyOnLoad(go);
					mInst = go.AddComponent<O_Localization>();
				}
			}
			return mInst;
		}
	}
    //public string startingLanguage;

    /// <summary>
    /// 所有语言名的集合,用英文标识的
    /// </summary>
    private List<string> listLanguageName = new List<string>();

    /// <summary>
    /// 语言名对应的本地化名
    /// </summary>
    Dictionary<string, string> dicLanguageName = new Dictionary<string, string>();

    Dictionary<string, string> dicLanguageFont = new Dictionary<string, string>();

    /// <summary>
    /// 选择某个语言后存储所有本地化文档
    /// </summary>
    Dictionary<string, Dictionary<string, string>> dicLocalLanguages = new Dictionary<string, Dictionary<string, string>>();

	string curLanguage= "Chinese";
    O_UIFont ReferenceFont;
    O_UIFont ReplacementFont;

	/// <summary>
	/// Name of the currently active language.
	/// </summary>
	public string currentLanguage
	{
		get
		{
            if (string.IsNullOrEmpty(curLanguage))
			{
                currentLanguage = "Chinese";
			}
            return curLanguage;
		}
        set { }
	}

	void Awake()
    {
        if (mInst == null)
        {
            mInst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        LoadFont(curLanguage);
    
        dicLocalLanguages.Clear();
    }

    public void PreInit()
    {
        string name = "GameStartLocal";
        string strPath = "DBLocal/" + curLanguage + "/" + name;
        Debug.Log(strPath);
        LoadFile(name, strPath);
    }

    public void init()
    {
        Debug.Log("LoadAllLocalizations----" + curLanguage);
        LoadAllLocalizations(curLanguage);
    }

	/// <summary>
	/// Oddly enough... sometimes if there is no OnEnable function in Localization, it can get the Awake call after UILocalize's OnEnable.
	/// </summary>
	void OnEnable () { if (mInst == null) mInst = this; }

	/// <summary>
	/// Remove the instance reference.
	/// </summary>
	void OnDestroy () { if (mInst == this) mInst = null; }

	/// <summary>
	/// 得到相应文件中key对应的值
	/// </summary>
	public string GetText(string filename,int key)
    {
        if (dicLocalLanguages.ContainsKey(filename))
        {
            string val = null;
            dicLocalLanguages[filename].TryGetValue(key.ToString(), out val);
            if (val == null)
            {
                 Debug.LogWarning("localization filename key not found!" + filename + " " + key);
				 return "no string";
            }
            return val;
        }
        else
        {
            Debug.LogWarning("localization filename not found!" + filename);
        }
        return "no string";
	}

    /// <summary>
    /// 得到相应文件中key对应的值
    /// </summary>
    public string GetText(string filename, string key)
    {
        if (dicLocalLanguages.ContainsKey(filename))
        {
            string val = null;
            dicLocalLanguages[filename].TryGetValue(key, out val);
            if (val == null)
            {
                Debug.LogWarning("localization filename key not found!" + filename + " " + key);
				 return "no string";
            }
            return val;
        }
        else
        {
            Debug.LogWarning("localization filename not found!" + filename);
        }
        return "no string";
    }

    /// <summary>
    /// 得到语言名的本地化名
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetLocalizeLangName(string name)
    {
        string val;
        return (dicLanguageName.TryGetValue(name, out val)) ? val : null;
    }

    /// <summary>
    /// 根据本地化名得到语言名
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetLangNameByLocalizeName(string name)
    {
        foreach (KeyValuePair<string, string> pair in dicLanguageName)
        {
            if (string.Equals(name, pair.Value))
            {
                return pair.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据语言名得到对应的字体名
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetFontNameByLanguage(string name)
    {
        string key = GetLangNameByLocalizeName(name);
        string val = "";
        if (!string.IsNullOrEmpty(key))
        {
            dicLanguageFont.TryGetValue(key, out val);
        }
        return val;
    }

    /// <summary>
    /// 必须放在LoadLanguageName后
    /// </summary>
    /// <param name="language"></param>
    void LoadFont(string language)
    {
        Debug.Log("LoadFont==" + language);
        ReplacementFont = Resources.Load("UI/font/ChineseFont26", typeof(O_UIFont)) as O_UIFont;
        if (ReferenceFont == null)
        {
            ReferenceFont = Resources.Load("UI/font/Font_3_7", typeof(O_UIFont)) as O_UIFont;
        }

        ReferenceFont.replacement = ReplacementFont;
    }

    /// <summary>
    /// 加载和某个语言相关的所有本地化文档
    /// </summary>
    /// <param name="language"></param>
    void LoadAllLocalizations(string language)
    {
        XmlDocument xmlDoc = new XmlDocument();
        TextAsset textAsset = GameApp.GetResourceManager().LoadDB("DB/ExcelExport/Localization/LocalizationFileName");
        if (textAsset == null)
        {
            Debug.LogError("Load LoadLanguageName DB Failed!");
            return;
        }
        xmlDoc.Load(new StringReader(textAsset.text));
        XmlElement xmlRoot = xmlDoc.DocumentElement;

        foreach (XmlNode node in xmlRoot.ChildNodes)
        {
            if (node.Name == "row")
            {
                CXmlRead xmlRead = new CXmlRead(node as XmlElement);
                string name = xmlRead.Str("name");
                string strPath = "DB/ExcelExport/Localization/" + curLanguage + "/" + name;
                if(GameApp.Instance().SiFu>=10&&name=="DaoJuLocal")
                    strPath = "DB/ExcelExport/Localization/" + curLanguage + "/sf" + name;
                Debug.Log(strPath);
                LoadFile(name, strPath);
            }
        }
        O_UIRoot.Broadcast("OnLocalize", this);
        OnChangeLanguage();
    }

    void LoadFile(string name,string path)
    {
        Dictionary<string, string> texts = new Dictionary<string, string>();

        XmlDocument xmlDoc = new XmlDocument();
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError("Load LoadLanguageName DB Failed! path=" + path);
            return;
        }
        xmlDoc.Load(new StringReader(textAsset.text));
        XmlElement xmlRoot = xmlDoc.DocumentElement;

        if (xmlRoot == null)
		{
			Debug.LogWarning("LoadFile fail:" + path);
            return;
		}
        foreach (XmlNode node in xmlRoot.ChildNodes)
        {
            if (node.Name == "row")
            {
                CXmlRead xmlRead = new CXmlRead(node as XmlElement);
                string id = xmlRead.Str("id");
                string text = xmlRead.Str("text");
                if (texts.ContainsKey(id))
                {
                    Debug.LogWarning("duplicate record id name:" + name + "id:" + id);
                    continue;
                }
                texts.Add(id, text);
            }
        }
        if (!dicLocalLanguages.ContainsKey(name))
        {
            dicLocalLanguages.Add(name, texts);
        }
    }

    /// <summary>
    /// OnLocalize不起作用时调用
    /// </summary>
    void OnChangeLanguage()
    {
        if (NPCManager.instance != null)
        {
            NPCManager.GetInstance().OnLocalize();
            NPCManager.GetInstance().OnLocalizeBubble();
        }
    }
}

