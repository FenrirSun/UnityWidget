using UnityEngine;
using UnityEditor;

// <summary>
/// 导入资源的后处理管理机制
/// 可以检查出不合法的资源导入，可以进行更改
// </summary>
//-----------------------------------------------------------------------
 
public class AssetsPostManager : AssetPostprocessor
{
 
    static string basePath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
 
    static void OnPostprocessAllAssets(           // 这个函数必须为静态的，其他可以不是！
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (var path in importedAssets)
        {
            // 判断文件是不是配置文件 .csv, json的.txt (个人角色json的配置文件就是以.json为后缀名是最为合理的！)
            if (path.EndsWith(".csv") || path.EndsWith(".txt") || path.EndsWith(".json"))
            {
                string tempP = basePath + path;
                System.Text.Encoding encode;
                using (System.IO.FileStream fs = new System.IO.FileStream(tempP, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                     encode = GetFileEncodeType(fs);
                }
 
                if (System.Text.Encoding.UTF8 != encode)
                {
                    Debug.LogWarning("亲！配置文件" + tempP + "的编码格式不是UTF-8格式呦");
                    //string str = File.ReadAllText(path, Encoding.Default);   // 转换没有问题, UTF8读就是乱码！！！
                    //File.WriteAllText(tempP, str, Encoding.UTF8);           
                }
            }
        }
    }
 
    /// <summary>
    /// 判断配置文件的编码格式是不是utf-8
    /// </summary>
    /// <returns>The file encode type.</returns>
    /// <param name="filename">文件全路径.</param>
    /// 代码中没判断内容是不是空
    /// 检查时，csv文件不能用 office打开（因为独占）
    static public System.Text.Encoding GetFileEncodeType(System.IO.FileStream fs)
    {
        System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
        byte[] buffer = br.ReadBytes(2);
 
        if (buffer[0] >= 0xEF)
        {
            if (buffer[0] == 0xEF && buffer[1] == 0xBB)
            {
                return System.Text.Encoding.UTF8;
            }
            else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                return System.Text.Encoding.BigEndianUnicode;
            }
            else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                return System.Text.Encoding.Unicode;
            }
            else
            {
                return System.Text.Encoding.Default;
            }
        }
        else
        {
            return System.Text.Encoding.Default;
        }
        br.Close();
        fs.Close();
    }
}