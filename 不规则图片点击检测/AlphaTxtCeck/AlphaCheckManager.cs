using UnityEngine;

/// <summary>
/// 点击检测管理器，使用这里的检测和使用 AlphaCheckImage 二选一
/// 适用于场景中的 sprite renderer (非UI)
/// </summary>
public class AlphaCheckManager : MonoBehaviour {

	public Camera _camera;

	private Camera RenderCamera
	{
		get
		{
			if (!_camera)
			{
				_camera = Camera.main;
			}
			return _camera;
		}
	}
	
	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			GameObject item = GetTouchItem(Input.mousePosition.x, Input.mousePosition.y);
			if (item)
			{
				Debug.Log("点击到了图片");
			}
		}
	}

	private GameObject GetTouchItem(float focusX, float focusY)
	{
		var hits = Physics2D.RaycastAll(RenderCamera.ScreenToWorldPoint(new Vector2(focusX, focusY)), 
			RenderCamera.transform.forward);
		int maxOrder = -1;
		GameObject hitItem = null;
		foreach (var item in hits)
		{
			Renderer[] renderers = item.collider.gameObject.GetComponentsInChildren<Renderer>();
			foreach (var sp in renderers)
			{
				AlphaCheck alphaCheck = sp.gameObject.GetComponent<AlphaCheck>();
				if (alphaCheck != null)
				{
					Vector2 itemSize = sp.bounds.size;
					Vector2 itemStartPos = new Vector2(sp.transform.position.x - itemSize.x * 0.5f, sp.transform.position.y - itemSize.y * 0.5f);

					if (item.point.x >= itemStartPos.x && item.point.x <= itemStartPos.x + itemSize.x &&
					    item.point.y >= itemStartPos.y && item.point.y <= itemStartPos.y + itemSize.y)
					{
						float xPercent = (item.point.x - itemStartPos.x) / itemSize.x;
						float yPercent = (item.point.y - itemStartPos.y) / itemSize.y;
						Vector2 pixelPos = new Vector2(xPercent * alphaCheck.textureWidth, yPercent * alphaCheck.textureHeight);

						if (alphaCheck.IsAlpha((int)pixelPos.x, (int)pixelPos.y) == false)
						{
							if (sp.sortingOrder > maxOrder)
							{
								maxOrder = sp.sortingOrder;
								hitItem = item.collider.gameObject;
								continue;
							}
						}
					}
				}
			}
		}

		return hitItem;
	}
}