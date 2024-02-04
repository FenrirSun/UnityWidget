using UnityEngine;

public class TestTouch : MonoBehaviour {
	public int furnishIndex;
	private int _furnishIndexId;
	private int FurnishIndexIndexId
	{
		get
		{
			if(_furnishIndexId == 0)
			{
				_furnishIndexId = Shader.PropertyToID("_FurnishId");
			}

			return _furnishIndexId;
		}
	}
	public Material furnishMat;
	
	// 把id作为参数设置到材质球中
	public void SetMaterial()
	{
		if (!furnishMat)
		{
			furnishMat = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
			var furIndex = furnishIndex + 5;
			furnishMat.SetFloat(FurnishIndexIndexId, furIndex);
		}
	}

	public void Update()
	{
		if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
		{
			var touchId = FurnishTouchDetectHelp.Instance.GetTouchFurnishId(1);
			Debug.Log($"点到了第{touchId}个家具");
		}
	}
}