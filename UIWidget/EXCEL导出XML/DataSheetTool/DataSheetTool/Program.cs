using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

namespace DataSheetTools
{
    class Program
    {
        static string strConfigFileName = "./DataSheetToolConfig.ini";
        static string strType = "xml";
        static string strDataSheetSrc = "./";
        static string strClientDataSheetDir = "./";
        static string strClientCodeDir = "./";
        static string strServerDataSheetDir = "./";
        static string strServerCodeDir = "./";

        static void Main(string[] args)
        {
            //初始化
            Init();

            List<string> fileNames = new List<string>();
            if (args.Length == 0)
            {
                //直接运行 处理当前目录下所有xls文件
                string[] files = Directory.GetFiles(strDataSheetSrc/*Directory.GetCurrentDirectory()*/, "*.xls",SearchOption.AllDirectories);
                foreach (string fileName in files)
                    fileNames.Add(fileName);
            }
            else
            {
                if (Path.GetExtension(args[0]).Equals(".xls"))
                    fileNames.Add(args[0]);
            }

            // 固定规约 "TypeDef", "Data"
            foreach (string fileName in fileNames)
            {
                DataSet dsTypeDef = ExcelToDS(fileName, "TypeDef");
                DataSet dsData = ExcelToDS(fileName, "Data");

                string strDataName = Path.GetFileNameWithoutExtension(fileName);

                switch (strType)
                {
                    case "cs":
                        {
                            GenCSStaticCode(dsTypeDef, dsData, strDataName);
                        }
                        break;
                    case "xml":
                        {
                            GenXML(dsTypeDef, dsData, fileName);
                        }
                        break;
                    default:
                            //GenXML(dsTypeDef, dsData, strDataName);
                        break;
                }
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        static void Init()
        {
            strType = INIOperationClass.INIGetStringValue(strConfigFileName, "OutputConfig", "Type", "");
            strDataSheetSrc = INIOperationClass.INIGetStringValue(strConfigFileName, "OutputConfig", "DataSheetSrc", "");
            strClientDataSheetDir = INIOperationClass.INIGetStringValue(strConfigFileName, "OutputConfig", "ClientDataSheetDir", "");
            strClientCodeDir = INIOperationClass.INIGetStringValue(strConfigFileName, "OutputConfig", "ClientCodeDir", "");
            strServerDataSheetDir = INIOperationClass.INIGetStringValue(strConfigFileName, "OutputConfig", "ServerDataSheetDir", "");
            strServerCodeDir = INIOperationClass.INIGetStringValue(strConfigFileName, "OutputConfig", "ServerCodeDir", "");
        }


        // Excel处理
        static DataSet ExcelToDS(string path, string tableName)
        {
            try
            {
                //创建一个数据链接
                string strCon = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1;'";
                OleDbConnection myConn = new OleDbConnection(strCon);
                myConn.Open();

                //DataTable schemaTable = myConn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
                //string tableName = schemaTable.Rows[0][2].ToString().Trim();

                string strCom = " SELECT * FROM [" + tableName + "$] ";

                //打开数据链接，得到一个数据集
                OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, myConn);
                //创建一个 DataSet对象
                DataSet dataSet = new DataSet();
                //得到自己的DataSet对象
                myCommand.Fill(dataSet, "[" + tableName + "$]");
                //关闭此数据链接
                myConn.Close();
                return dataSet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }


        //静态cs文件
        static void GenCSStaticCode(DataSet dsTypeDef, DataSet dsData, string dataSheetName)
        {
            string outputFileName = dataSheetName + "DataSheet.cs";
            FileStream fs = new FileStream(outputFileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);
            StringBuilder sb = new StringBuilder();

            string className = dataSheetName + "Data";

            sb.Append("using System;" + Environment.NewLine);
            sb.Append("using System.IO;" + Environment.NewLine);
            sb.Append("using System.Collections.Generic;" + Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("[System.Serializable]" + Environment.NewLine);
            sb.Append("public class " + className + Environment.NewLine);
            sb.Append("{" + Environment.NewLine);

            sb.Append("\tpublic static Dictionary<int, " + className + "> data = new Dictionary<int," + className + ">();" + Environment.NewLine);
            sb.Append(Environment.NewLine);


            Dictionary<string, string> dicColumeType = new Dictionary<string, string>();

            // 表头
            foreach (DataRow dRow in dsTypeDef.Tables[0].Rows)
            {
                string strColume = null;
                string strType = null;
                string strComment = null;
                foreach (DataColumn dCol in dsTypeDef.Tables[0].Columns)
                {
                    if (!dRow[dCol].ToString().Equals(String.Empty))
                    {

                        // Console.Write(dCol.Caption+ ":"+dRow[dCol] + " ");
                        switch (dCol.Caption)
                        {
                            case "Column":
                                strColume = dRow[dCol].ToString();
                                break;
                            case "Type":
                                strType = dRow[dCol].ToString();
                                break;
                            case "Comment":
                                strComment = dRow[dCol].ToString();
                                break;
                        }
                    }
                }


                sb.Append("\tpublic " + GetCSType(strType) + " " + strColume + "{ get; private set; } //" + strComment + Environment.NewLine);

                dicColumeType.Add(strColume, strType);
            }


            //表体

            sb.Append(Environment.NewLine);
            sb.Append("\tstatic " + className + "()" + Environment.NewLine);
            sb.Append("\t{" + Environment.NewLine);
            sb.Append("\t\tdata = new Dictionary<int, " + className + ">()" + Environment.NewLine);
            sb.Append("\t\t{" + Environment.NewLine);

            int id = 0;
            int countId = 1;
            foreach (DataRow dRow in dsData.Tables[0].Rows)
            {
                foreach (DataColumn dCol in dsData.Tables[0].Columns)
                {
                    if (dCol.Caption == "id")
                    {
                        id = Convert.ToInt32(dRow[dCol].ToString());
                    }
                    else
                        id = countId;
                }
                sb.Append("\t\t\t{" + id + ",new " + className + "(){");
                foreach (DataColumn dCol in dsData.Tables[0].Columns)
                {
                    if (dicColumeType.ContainsKey(dCol.Caption))
                    {
                        bool bEmptyValue = false;
                        if (dRow[dCol].ToString().Equals(String.Empty))
                            bEmptyValue = true;

                        switch (dicColumeType[dCol.Caption])
                        {
                            case "int":
                            case "enum":
                                sb.Append(dCol.Caption + "=" + (bEmptyValue ? "0" : dRow[dCol].ToString()) + ", ");
                                break;
                            case "string":
                                sb.Append(dCol.Caption + "=\"" + (bEmptyValue ? "" : dRow[dCol].ToString()) + "\", ");
                                break;
                        }
                    }
                }
                sb.Append("}}," + Environment.NewLine);
                countId++;
            }

            sb.Append("\t\t};" + Environment.NewLine);
            sb.Append("\t}" + Environment.NewLine);
            sb.Append("}" + Environment.NewLine);

            sw.Write(sb.ToString());

            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
            Console.WriteLine("Created Static CS File:" + outputFileName);

        }

        //CS type
        static string GetCSType(string typeName)
        {
            switch (typeName)
            {
                case "int":
                    return "int";
                case "string":
                    return "string";
                case "enum":
                    return "int";
            }
            return "void";
        }

        //生成XML形式的表
        static void GenXML(DataSet dsTypeDef, DataSet dsData, string dataSheetName)
        {
            string outfileName = Path.GetFileNameWithoutExtension(dataSheetName);
            string dirName = Path.GetDirectoryName(dataSheetName);
            dirName = dirName.Replace('\\', '/');

            string outputFileName = strClientDataSheetDir + dirName +"/"+outfileName + ".xml";
            outputFileName = outputFileName.Remove(outputFileName.IndexOf(strDataSheetSrc), strDataSheetSrc.Length);

            string outputDir = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);


            // Create Xml Writer.
            XmlTextWriter xmlWriter = null;
            try
            {
                xmlWriter = new XmlTextWriter(outputFileName, Encoding.UTF8);
            }
            catch
            {
                Console.WriteLine("[ERROR] Fail to Create XML File:" + outputFileName);

                return;
            }
            xmlWriter.Formatting = Formatting.Indented;

            // This will output the XML declaration
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("root"); //根节点

            Dictionary<string, string> dicColumeType = new Dictionary<string, string>();

            // 表头
            foreach (DataRow dRow in dsTypeDef.Tables[0].Rows)
            {
                string strColume = null;
                string strType = null;
                string strComment = null;
                foreach (DataColumn dCol in dsTypeDef.Tables[0].Columns)
                {
                    if (!dRow[dCol].ToString().Equals(String.Empty))
                    {

                        // Console.Write(dCol.Caption+ ":"+dRow[dCol] + " ");
                        switch (dCol.Caption)
                        {
                            case "Column":
                                strColume = dRow[dCol].ToString();
                                break;
                            case "Type":
                                strType = dRow[dCol].ToString();
                                break;
                            case "Comment":
                                strComment = dRow[dCol].ToString();
                                break;
                        }
                    }
                }
                dicColumeType.Add(strColume, strType);
            }

            //表体
            foreach (DataRow dRow in dsData.Tables[0].Rows)
            {
                xmlWriter.WriteStartElement("row");

                foreach (DataColumn dCol in dsData.Tables[0].Columns)
                {
                    if (dicColumeType.ContainsKey(dCol.Caption))
                    {

                        bool bEmptyValue = false;
                        if (dRow[dCol].ToString().Equals(String.Empty))
                            bEmptyValue = true;


                        switch (dicColumeType[dCol.Caption])
                        {
                            case "int":
                            case "enum":
                                xmlWriter.WriteAttributeString(dCol.Caption, bEmptyValue ? "0" : dRow[dCol].ToString());
                                break;
                            case "double":
                                xmlWriter.WriteAttributeString(dCol.Caption, bEmptyValue ? "0.0" : dRow[dCol].ToString());
                                break;
                            case "string":
                                xmlWriter.WriteAttributeString(dCol.Caption, bEmptyValue ? "" : dRow[dCol].ToString());
                                break;
                        }
                    }
                }
                xmlWriter.WriteEndElement(); //结束一行
            }
            xmlWriter.WriteEndElement(); //结束root
            xmlWriter.WriteEndDocument();
            xmlWriter.Close(); 

            //复制客户端XML至服务器端
            Console.WriteLine("Created XML File:" + outputFileName);

            try
            {
                string ServerXmlFileName = strServerDataSheetDir + dirName + "/" + outfileName + ".xml";
                ServerXmlFileName = ServerXmlFileName.Remove(ServerXmlFileName.IndexOf(strDataSheetSrc), strDataSheetSrc.Length);

                string copyDir = Path.GetDirectoryName(ServerXmlFileName);
                if (!Directory.Exists(copyDir))
                    Directory.CreateDirectory(copyDir);

                File.Copy(outputFileName, ServerXmlFileName, true);
                Console.WriteLine("Copyed XML File to Server Path:" + ServerXmlFileName);
            }
            catch
            {
                Console.WriteLine("[ERROR] Fail to Copy Server XML File:" + outputFileName);
            }

        }
    }

}
