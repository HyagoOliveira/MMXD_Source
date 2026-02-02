using UnityEngine;

public class PixelSnap : MonoBehaviour
{
	private Sprite sprite;

	private Vector3 actualPosition;

	private bool shouldRestorePosition;

	private void Start()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null)
		{
			sprite = component.sprite;
		}
		else
		{
			sprite = null;
		}
	}

	private void OnWillRenderObject()
	{
		Camera current = Camera.current;
		if (!current)
		{
			return;
		}
		PixelPerfectCamera component = current.GetComponent<PixelPerfectCamera>();
		bool flag = !(component == null) && component.retroSnap;
		if (!flag)
		{
			return;
		}
		shouldRestorePosition = true;
		actualPosition = base.transform.position;
		float num = (float)current.pixelHeight / (2f * current.orthographicSize);
		float num2 = 1f / num;
		Vector2 vector = current.transform.position.xy();
		Vector2 vector2 = actualPosition.xy();
		Vector2 vector3 = vector2 - vector;
		Vector2 vector4 = new Vector2(0f, 0f);
		vector4.x = ((current.pixelWidth % 2 == 0) ? 0f : 0.5f);
		vector4.y = ((current.pixelHeight % 2 == 0) ? 0f : 0.5f);
		Vector2 vector5 = new Vector2(0f, 0f);
		if (sprite != null)
		{
			vector5 = sprite.pivot - new Vector2(Mathf.Floor(sprite.pivot.x), Mathf.Floor(sprite.pivot.y));
			if (!flag)
			{
				float num3 = num / sprite.pixelsPerUnit;
				vector5 *= num3;
			}
		}
		if (flag)
		{
			float assetsPixelsPerUnit = component.assetsPixelsPerUnit;
			float num4 = 1f / assetsPixelsPerUnit;
			float num5 = num / assetsPixelsPerUnit;
			vector4.x /= num5;
			vector4.y /= num5;
			vector3.x = (Mathf.Round(vector3.x / num4 - vector4.x) + vector4.x + vector5.x) * num4;
			vector3.y = (Mathf.Round(vector3.y / num4 - vector4.y) + vector4.y + vector5.y) * num4;
		}
		else
		{
			vector3.x = (Mathf.Round(vector3.x / num2 - vector4.x) + vector4.x + vector5.x) * num2;
			vector3.y = (Mathf.Round(vector3.y / num2 - vector4.y) + vector4.y + vector5.y) * num2;
		}
		vector2 = vector3 + vector;
		base.transform.position = new Vector3(vector2.x, vector2.y, actualPosition.z);
	}

	private void OnRenderObject()
	{
		if (shouldRestorePosition)
		{
			shouldRestorePosition = false;
			base.transform.position = actualPosition;
		}
	}
}
