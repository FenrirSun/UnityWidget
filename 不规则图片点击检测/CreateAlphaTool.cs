using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateAlphaTool {
	
	private static float alphaThreshold = 0.1f;

	[MenuItem("MyTool/为选中图片创建AlphaCheck", false, 0)]
	public static void CreatureAlphaForSelectTexture()
	{
		var selected = Selection.activeObject;
		var path = AssetDatabase.GetAssetPath(selected);
		var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
		if (texture)
		{
			CreateAlpha(texture);
		}
	}
	
	private static void CreateAlpha(Texture2D tex)
	{
		string path = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/" + AssetDatabase.GetAssetPath(tex);
		string filePath = Path.GetDirectoryName(path);
		string fileName = tex.name + ".bytes";
		path = filePath + "/" + fileName;
		if (!File.Exists(path))
		{
			TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
			ti.isReadable = true;
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex));
			PixelData jsonData = new PixelData();
			jsonData.pixcels = new List<PixelInfo>();

			for (int x = 0; x < tex.width; x++)
			{
				int startY = -1;
				for (int y = 0; y < tex.height; y++)
				{
					float a = tex.GetPixel(x, y).a;
					if (a > alphaThreshold && startY == -1)
					{
						startY = y;
					}
					else if (a <= alphaThreshold)
					{
						if (startY >= 0)
						{
							PixelInfo info = new PixelInfo();
							info.x = x;
							info.s = startY;
							info.e = y - 1;
							jsonData.pixcels.Add(info);
							startY = -1;
						}
					}
				}

				if (startY >= 0)
				{
					PixelInfo info = new PixelInfo();
					info.x = x;
					info.s = startY;
					info.e = tex.height - 1;
					jsonData.pixcels.Add(info);
				}
			}

			ti.isReadable = false;
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex));
			MakeData(path, jsonData, tex);

			// File.WriteAllText(path, str, Encoding.UTF8);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex).Replace(".png", ".bytes"));
			AssetDatabase.SaveAssets();
		}
	}

	private static void MakeData(string path, PixelData data, Texture2D tex)
	{
		if (File.Exists(path))
			File.Delete(path);
		FileStream fs = new FileStream(path, FileMode.Create);
		BinaryWriter bw = new BinaryWriter(fs);
		bw.Write(tex.width);
		bw.Write(tex.height);
		foreach (var item in data.pixcels)
		{
			bw.Write(item.x);
			bw.Write(item.s);
			bw.Write(item.e);
		}
	}
}