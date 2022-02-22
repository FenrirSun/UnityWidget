using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class GS_INFO 
{
    public int ID ;
    public string Name  = "" ;
    public string HostName  = "" ;
    public int Port ;
    public int Type ;
}

public class RegisterRecord
{
    private XmlDocument XmlDoc = null;
    private XmlElement XmlRoot = null;
    public List<string> RegisterRecords = new List<string>();
    public string UserName = "";            // 加密的，使用时动态解密
    public string Passward = "";            // 加密的，使用时动态解密
    public List<GS_INFO> LstLatestServer = new List<GS_INFO>();
    public GS_INFO selectGsInfo = null;

    public void AddLatestServer(GS_INFO info)
    {
        // 如果已经在第一个，则直接返回
  //      if (LstLatestServer.Count > 0 && LstLatestServer[0].HostName == info.HostName && LstLatestServer[0].Port == info.Port && LstLatestServer[0].Name == info.Name)
        if (LstLatestServer.Count > 0  && LstLatestServer[0].Name == info.Name)
            return;

        LstLatestServer.Insert(0, info);

        if (LstLatestServer.Count > 2)
        {
            LstLatestServer.RemoveRange(2, LstLatestServer.Count - 2);
        }

        SaveRegisterFiles();
    }

    public void LoadRegisterFiles()
    {
        RegisterRecords.Clear();

        XmlDoc = new XmlDocument();
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
                ReadForIPhone();
              // ReadForDefault();
                break;
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
                {
                    ReadForIPhone();
                }break;
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
               ReadForIPhone();
                break;
            default:
                Debug.LogError("<Platform Error>:undeal plateform: load GameRecord.RegisterOL.xml on " + Application.platform);
                break;
        }
        XmlRoot = XmlDoc.DocumentElement;
        if (XmlRoot != null)
        {
            CXmlRead reader = new CXmlRead(XmlRoot);
            UserName = reader.Str("UserName", "");
            Passward = reader.Str("Passward", "");

            XmlNode serverNode = XmlRoot.SelectSingleNode("Servers");
            if (serverNode != null)
            {
                foreach (XmlNode node in serverNode.ChildNodes)
                {
                    if ((node is XmlElement) == false)
                        continue;

                    GS_INFO info = new GS_INFO();
                    CXmlRead xmlReader = new CXmlRead(node as XmlElement);

                    info.Name = xmlReader.Str("Name");
                    info.HostName = xmlReader.Str("HostName");
                    info.Port = xmlReader.Int("Port");
                    info.Type = xmlReader.Int("Type");
                    LstLatestServer.Add(info);
                    Debug.LogWarning("name=" + info.Name);
                    Debug.LogWarning("HostName=" + info.HostName);
                    Debug.LogWarning("Port=" + info.Port);
                    Debug.LogWarning("Type=" + info.Type);
                }
            }
        }
        else
        {
            UserName = CommonFun.Encrypt("");
            Passward = CommonFun.Encrypt("");
        }
    }

    public void SaveRegisterFiles()
    {
        Debug.Log("SaveRegisterFiles-------------------------------------------" + UserName);
        XmlDoc = new XmlDocument();
        XmlRoot = XmlDoc.CreateElement("RegisterRecords");
        XmlDoc.AppendChild(XmlRoot);

        XmlElement recordNode = XmlDoc.CreateElement("Records");
        XmlRoot.AppendChild(recordNode);
        XmlRoot.SetAttribute("UserName", UserName.ToString());
        XmlRoot.SetAttribute("Passward", Passward.ToString());

        XmlElement serverNode = XmlDoc.CreateElement("Servers");
        XmlRoot.AppendChild(serverNode);
        foreach (GS_INFO info in LstLatestServer)
        {
            XmlElement newElem = XmlDoc.CreateElement("Server");
            newElem.SetAttribute("Name", info.Name);
            newElem.SetAttribute("HostName", info.HostName);
            newElem.SetAttribute("Port", info.Port.ToString());
            newElem.SetAttribute("Type", info.Type.ToString());
            serverNode.AppendChild(newElem);
        }

        string strPath = "";
        string strMLand2Path = "";
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXPlayer:
                {
                    Debug.Log("Application.persistentDataPath =" + Application.persistentDataPath);
                    strPath = Application.persistentDataPath + "/MLand2/RegisterOL.xml";
                    strMLand2Path = Application.persistentDataPath + "/MLand2";
                    if (!Directory.Exists(strMLand2Path))
                        Directory.CreateDirectory(strMLand2Path);
                    XmlDoc.Save(strPath);
                }break;

            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
               strPath = Application.persistentDataPath + "/MLand2/RegisterOL.xml";
                strMLand2Path = Application.persistentDataPath + "/MLand2";
                if (!Directory.Exists(strMLand2Path))
                    Directory.CreateDirectory(strMLand2Path);
                CommonFun.XmlSaveEncrypt(XmlDoc, strPath);
                break;
            default:
                Debug.LogError("<Platform Error>:undeal plateform: load Record.Register on " + Application.platform);
                break;
        }
    }

    public void NewRegisterFile(string fileName)
    {
        if (RegisterRecords.Contains(fileName))
        {
            Debug.LogWarning("<ReigsterFile Failed> NewRegisterFile. " + fileName + " already exist!");
            return;
        }
        RegisterRecords.Insert(0, fileName);
        SaveRegisterFiles();
    }

    public void UnregisterFile(string fileName)
    {
        if (RegisterRecords.Contains(fileName) == false)
        {
            Debug.LogWarning("<UnregisterFile Failed> Filename = " + fileName + " not exist!");
            return;
        }
        RegisterRecords.Remove(fileName);
        SaveRegisterFiles();
    }

    private void ReadForDefault()
    {
        TextAsset textAsset = (TextAsset)Resources.Load("DB/Register", typeof(TextAsset));
        if (textAsset == null)
        {
            Debug.LogError("Load PlayerConfig File Failed!");
            return;
        }
        XmlDoc.Load(new StringReader(textAsset.text));
    }

    private void ReadForIPhone()
    {
        Debug.Log("Application.persistentDataPath==" + Application.persistentDataPath);
        string strPath = Application.persistentDataPath + "/MLand2/RegisterOL.xml";
        if (File.Exists(strPath))
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CommonFun.XmlLoadDecrypt(XmlDoc, strPath);
            }
            else
            {
                Debug.Log("ReadForIPhone path=" + strPath);
                XmlDoc.Load(strPath);
            }
        }
        //else
        //{
        //    TextAsset textAsset = (TextAsset)Resources.Load("DB/Register", typeof(TextAsset));
        //    if (textAsset == null)
        //    {
        //        Debug.LogError("Load GameRecord.Register File Failed!");
        //        return;
        //    }
        //    XmlDoc.Load(new StringReader(textAsset.text));
        //}
    }

}
