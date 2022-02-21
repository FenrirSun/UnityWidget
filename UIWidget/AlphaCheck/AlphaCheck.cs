using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 需要把此脚本挂在对应的图片GameObject上,并关联对应的alpha图引用
/// </summary>
[DisallowMultipleComponent]
public class AlphaCheck : MonoBehaviour {
	public TextAsset json;
	public int textureWidth;
	public int textureHeight;
	private PixelData _pixelData;
	byte[] pBuffer;
	int pBufferIndex = 0;

	void Start()
	{
		if (json == null)
		{
			return;
		}

		pBuffer = json.bytes;
		textureWidth = ReadInt();
		textureHeight = ReadInt();
		if (textureWidth == 0)
		{
			Debug.Log(name);
		}

		_pixelData = new PixelData();
		_pixelData.pixcels = new List<PixelInfo>();

		while (pBufferIndex < pBuffer.Length - 3)
		{
			PixelInfo pixelInfo = new PixelInfo();
			pixelInfo.x = ReadInt();
			pixelInfo.s = ReadInt();
			pixelInfo.e = ReadInt();
			_pixelData.pixcels.Add(pixelInfo);
		}
	}

	public bool IsAlpha(int x, int y)
	{
		if (json == null)
		{
			return false;
		}

		foreach (var item in _pixelData.pixcels)
		{
			if (item.x == x && y >= item.s && y <= item.e)
			{
				return false;
			}
		}

		return true;
	}

	private byte GetByte()
	{
		return pBuffer[pBufferIndex++];
	}

	private int ReadInt()
	{
		int aT = GetByte() & 0xFF;
		int bT = GetByte() & 0xFF;
		int cT = GetByte() & 0xFF;
		int dT = GetByte() & 0xFF;
		return (dT << 24) | (cT << 16) | (bT << 8) | aT;
	}
}