using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System.Collections.Generic;
public class AddData
{
    public int Hp;
    public int Ae;
    public int De;
    public int CriticalStrikeRate;
    public int DamageAvoidRate;
    public int HitRate;
}

public class SkillWnd : UIBaseWnd
{
	//读取数据
    private void LoadSkill()
    {
        if (mdata.Count > 0)
            return;
        XmlDocument xmlDoc = new XmlDocument();
        TextAsset textAsset = GameApp.GetResourceManager().LoadDB("DB/TianFU/TianFu");// (GameApp.GetResourceManager().LoadDB就是(TextAsset)Resources.Load(name, typeof(TextAsset));
        if (textAsset == null)
        {
            Debug.LogError("Load UIWnd DB Failed!");
        }
        xmlDoc.Load(new StringReader(textAsset.text));
        XmlElement xmlRoot = xmlDoc.DocumentElement;
        foreach (XmlNode node in xmlRoot.ChildNodes)
        {
            if (!(node is XmlElement))
            {
                continue;
            }
            XmlElement xmle = (XmlElement)node;
            int nID = XmlConvert.ToInt32(xmle.GetAttribute("Id"));
            SkillData data = new SkillData();
            data.Ae = XmlConvert.ToInt32(xmle.GetAttribute("Ae"));
            data.De = XmlConvert.ToInt32(xmle.GetAttribute("De"));
            data.HP = XmlConvert.ToInt32(xmle.GetAttribute("HP"));
            data.JiNengJuanZhou = XmlConvert.ToInt32(xmle.GetAttribute("JiNengJuanZhou"));
            data.Level = XmlConvert.ToInt32(xmle.GetAttribute("Level"));
            data.Money = XmlConvert.ToInt32(xmle.GetAttribute("Money"));

            data.JiNeng = XmlConvert.ToInt32(xmle.GetAttribute("JiNeng" + GameApp.GetWorldManager().MainPlayer.GetProperty().Job));
            data.TianFu = XmlConvert.ToInt32(xmle.GetAttribute("TianFu" + GameApp.GetWorldManager().MainPlayer.GetProperty().Job));
            mdata.Add(nID, data);
        }

    }
	//写入数据0
	static public Dictionary<string, uint> ReadBundleCRCInfo(string fileURL)
    {
        Dictionary<string, uint> dicCRCInfo = new Dictionary<string,uint>();
        if (!File.Exists(fileURL))
            return dicCRCInfo;
        XmlTextReader xmlReader = new XmlTextReader(fileURL);
        try
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name == "bundle")
                    {
                        string bundleName = xmlReader.GetAttribute("bundleName");
                        uint CRC = 0;
                        uint.TryParse(xmlReader.GetAttribute("CRC"), out CRC);
                        dicCRCInfo.Add(bundleName, CRC);
                    }
                }
            }
            xmlReader.Close();
            return dicCRCInfo;

        }
        catch 
        {
            return dicCRCInfo; 
        }
    }

	
	//写入数据1
	static public void WriteBundleCRCInfo(Dictionary<string, uint> dicCRCInfo)
    {
        XmlTextWriter xmlWriter = new XmlTextWriter(ResManager.ResMakePath + CRCInfoFileName, System.Text.Encoding.UTF8);
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("root");//根节点
        foreach (KeyValuePair<string, uint> pair in dicCRCInfo)//表体
        {
            xmlWriter.WriteStartElement("bundle");
            xmlWriter.WriteAttributeString("bundleName", pair.Key);
            xmlWriter.WriteAttributeString("CRC", pair.Value.ToString());
            xmlWriter.WriteEndElement();//结束一行
        }

        xmlWriter.WriteEndElement();//结束root
        xmlWriter.WriteEndDocument();
        xmlWriter.Close();

        AssetDatabase.Refresh();
        
    }
	//写入数据2
    static public void AddOrUpdateBundleCRCInfo(Dictionary<string,uint> addCRCInfo)
    {
        Dictionary<string, uint> dicCRCInfo = ReadBundleCRCInfo(ResManager.ResMakePath + CRCInfoFileName);
        foreach (KeyValuePair<string,uint> pair in addCRCInfo)
        {
            if (dicCRCInfo.ContainsKey(pair.Key))
            {
                dicCRCInfo[pair.Key] = pair.Value;
            }
            else 
            {
                dicCRCInfo.Add(pair.Key, pair.Value);
            }
                
        }

        WriteBundleCRCInfo(dicCRCInfo);
    }
	
	//写入数据3,制作XML的bundle
    static void BuildXMLBundle(FileInfo[] fis)
    {
        DirectoryInfo di = new DirectoryInfo(MakeResourcePath(dataSheetSrcPath));
        BuildAssetBundleOptions option = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;

        Dictionary<string, uint> CRCInfo = new Dictionary<string, uint>();

        foreach (FileInfo fi in fis)
        {
            Debug.Log("生成XML：" + fi.FullName);
            string resFileName = fi.FullName.Remove(0, di.FullName.Length);
            resFileName = resFileName.Replace("\\", "/");
            string resRawName = resFileName.Remove(resFileName.Length - 4);
            resRawName = resRawName.Replace("\\", "/");
            string resOutDir = ResManager.ResMakePath + ResManager.DataSheetURL + resFileName.Remove(resFileName.Length - fi.Name.Length);
            resOutDir = resOutDir.Replace("\\", "/");

            if (!Directory.Exists(resOutDir))
            {
                Directory.CreateDirectory(resOutDir);
            }

            string resName = dataSheetSrcPath + resRawName;
            TextAsset textAsset = (TextAsset)Resources.Load(resName, typeof(TextAsset));

            string outputName = ResManager.ResMakePath + ResManager.DataSheetURL + resRawName + ".assetbundle";
            uint CRC;
            BuildPipeline.BuildAssetBundle(textAsset, null, outputName, out CRC, option, buildTar);
            string bundleName = resName;
            CRCInfo.Add(bundleName, CRC);
        }
        BundleCRCInfoMgr.AddOrUpdateBundleCRCInfo(CRCInfo);
    }

	//制作XML的bundleadle,写入数据4
	[MenuItem("ResMake/Build Selected/Build XML")]
    static void BuildSelectXML()
    {
        foreach (Object ob in Selection.objects)
        {
            TextAsset ta = ob as TextAsset;

            if (ta)
            {
                DirectoryInfo di = new DirectoryInfo(MakeResourcePath(dataSheetSrcPath));
                FileInfo[] fis = di.GetFiles(ob.name + ".xml", SearchOption.AllDirectories);

                BuildXMLBundle(fis);                    
            }
        }

        AssetDatabase.Refresh();
    }
	

}
