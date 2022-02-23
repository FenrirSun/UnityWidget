using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 使用此脚本替代原Image，和使用 AlphaCheckManager 二选一
/// 适用于UI中的图片
/// </summary>
public class AlphaCheckImage : Image {
	private AlphaCheck _alphaCheck;
	private AlphaCheck AlphaCheck
	{
		get
		{
			if (!_alphaCheck)
			{
				_alphaCheck = gameObject.GetComponent<AlphaCheck>();
			}

			return _alphaCheck;
		}
	}

	public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		//坐标系转换 
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out var local);
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		local.x += rectTransform.pivot.x * pixelAdjustedRect.width;
		local.y += rectTransform.pivot.y * pixelAdjustedRect.height;
		local = MapCoordinate(local, pixelAdjustedRect);
		Rect textureRect = overrideSprite.textureRect;
		Vector2 vector = new Vector2(local.x / textureRect.width, local.y / textureRect.height);

		//计算屏幕坐标对应的UV坐标
		float u = Mathf.Lerp(textureRect.x, textureRect.xMax, vector.x) / overrideSprite.texture.width;
		float v = Mathf.Lerp(textureRect.y, textureRect.yMax, vector.y) / overrideSprite.texture.height;
		Vector2 pixelPos = new Vector2(u * AlphaCheck.textureWidth, v * AlphaCheck.textureHeight);
		var result = AlphaCheck.IsAlpha((int)pixelPos.x, (int)pixelPos.y);

		if (!result)
			Debug.Log($"点到图片了！{u} * {v}");
		return !result;
	}

	#region 搬运了基类的私有方法

	private Vector2 MapCoordinate(Vector2 local, Rect rect)
	{
		Rect spriteRect = sprite.rect;
		if (type == Type.Simple || type == Type.Filled)
			return new Vector2(local.x * spriteRect.width / rect.width, local.y * spriteRect.height / rect.height);

		Vector4 border = sprite.border;
		Vector4 adjustedBorder = GetAdjustedBorders(border / pixelsPerUnit, rect);

		for (int i = 0; i < 2; i++)
		{
			if (local[i] <= adjustedBorder[i])
				continue;

			if (rect.size[i] - local[i] <= adjustedBorder[i + 2])
			{
				local[i] -= (rect.size[i] - spriteRect.size[i]);
				continue;
			}

			if (type == Type.Sliced)
			{
				float lerp = Mathf.InverseLerp(adjustedBorder[i], rect.size[i] - adjustedBorder[i + 2], local[i]);
				local[i] = Mathf.Lerp(border[i], spriteRect.size[i] - border[i + 2], lerp);
			}
			else
			{
				local[i] -= adjustedBorder[i];
				local[i] = Mathf.Repeat(local[i], spriteRect.size[i] - border[i] - border[i + 2]);
				local[i] += border[i];
			}
		}

		return local;
	}

	private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
	{
		Rect originalRect = rectTransform.rect;

		for (int axis = 0; axis <= 1; axis++)
		{
			float borderScaleRatio;

			// The adjusted rect (adjusted for pixel correctness)
			// may be slightly larger than the original rect.
			// Adjust the border to match the adjustedRect to avoid
			// small gaps between borders (case 833201).
			if (originalRect.size[axis] != 0)
			{
				borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
				border[axis] *= borderScaleRatio;
				border[axis + 2] *= borderScaleRatio;
			}

			// If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
			// In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
			float combinedBorders = border[axis] + border[axis + 2];
			if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
			{
				borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
				border[axis] *= borderScaleRatio;
				border[axis + 2] *= borderScaleRatio;
			}
		}

		return border;
	}

	#endregion
}