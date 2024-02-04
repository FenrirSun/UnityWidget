using UnityEngine;

public class FurnishTouchDetectHelp : Singleton<FurnishTouchDetectHelp> {
	public int curRoomId;
	public Texture2D renderTex;
	public float sizeScale = 0.5f;
	
	public void CaptureRoom()
	{
		var uiCam = ZGameRuntime.Instance.cameras.ui;
		var shaderToRender = Shader.Find("Zenjoy/FurnishDetect");
		RenderTexture rt = RenderTexture.GetTemporary(Mathf.RoundToInt(Screen.width * sizeScale), Mathf.RoundToInt(Screen.height * sizeScale));

		uiCam.targetTexture = rt;
		uiCam.RenderWithShader(shaderToRender, "RenderType");
		uiCam.targetTexture = null;

		Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.R8, false);
		RenderTexture.active = rt;
		texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		RenderTexture.active = null;
		texture.Apply();

		RenderTexture.ReleaseTemporary(rt);

		if(renderTex)
			Destroy(renderTex);
		renderTex = texture;
	}

	public int GetTouchFurnishId(int roomId)
	{
		if (!renderTex || curRoomId != roomId)
		{
			CaptureRoom();
			curRoomId = roomId;
		}

#if UNITY_EDITOR || UNITY_STANDALONE
		var screenPosX = Input.mousePosition.x;
		var screenPosY = Input.mousePosition.y;
#else
		var touch = Input.GetTouch(0);
		var screenPosX = touch.position.x;
		var screenPosY = touch.position.y;
#endif

		var touchColor = renderTex.GetPixel(Mathf.RoundToInt(screenPosX * sizeScale), Mathf.RoundToInt(screenPosY * sizeScale));
		if (!GameUtils.FloatEqual(touchColor.r, 0))
		{
			return Mathf.RoundToInt(touchColor.r * 255 / 5f - 5);
		}
		
		return -1;
	}
}
