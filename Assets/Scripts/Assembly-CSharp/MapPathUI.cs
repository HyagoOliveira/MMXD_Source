using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class MapPathUI : MonoBehaviour
{
	[Serializable]
	public struct ControlPoint
	{
		public float rotation;

		public float length;
	}

	[SerializeField]
	private Sprite m_connectorSprite;

	[SerializeField]
	private Sprite m_pathSprite;

	[Slider(0f, 1f)]
	public float m_slider = 1f;

	private bool _bInspectorChanged = true;

	private GameObject _rootGameObject;

	[BoxGroup("Control Points")]
	public ControlPoint[] pointArray = new ControlPoint[1];

	private int previousPointArraySize;

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (_bInspectorChanged)
		{
			if (previousPointArraySize != pointArray.Length)
			{
				foreach (Transform item in base.transform)
				{
					UnityEngine.Object.DestroyImmediate(item.gameObject);
				}
				_rootGameObject = new GameObject("PathRoot");
				_rootGameObject.transform.SetParent(base.gameObject.transform, false);
				_rootGameObject.layer = base.gameObject.layer;
				if (pointArray != null && pointArray.Length != 0)
				{
					for (int i = 0; i < pointArray.Length; i++)
					{
						GameObject gameObject = new GameObject();
						gameObject.transform.SetParent(_rootGameObject.transform, false);
						gameObject.layer = _rootGameObject.layer;
						if ((bool)m_pathSprite)
						{
							Image image = gameObject.AddComponent<Image>();
							image.type = Image.Type.Simple;
							image.rectTransform.localPosition = new Vector3(0f, 0f, 0f);
							image.rectTransform.pivot = new Vector2(0.5f, 1f);
							image.sprite = m_pathSprite;
							image.SetNativeSize();
						}
						if (i == 0 || i == pointArray.Length - 1)
						{
							GameObject gameObject2 = new GameObject();
							gameObject2.transform.SetParent(gameObject.transform, false);
							gameObject2.layer = gameObject.layer;
							if ((bool)m_connectorSprite)
							{
								Image image2 = gameObject2.AddComponent<Image>();
								image2.type = Image.Type.Simple;
								RectTransform rectTransform = image2.rectTransform;
								Vector2 anchorMin = (image2.rectTransform.anchorMax = new Vector2(0.5f, 1f));
								rectTransform.anchorMin = anchorMin;
								image2.sprite = m_connectorSprite;
								image2.SetNativeSize();
							}
						}
					}
				}
				previousPointArraySize = pointArray.Length;
			}
			UpdatePath();
		}
		_bInspectorChanged = false;
	}

	private void UpdatePath()
	{
		float x = 49f;
		float num = 2.5f;
		Vector3 localPosition = Vector3.zero;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < pointArray.Length; i++)
		{
			ControlPoint controlPoint = pointArray[i];
			num2 += controlPoint.length;
		}
		int num4 = 0;
		foreach (Transform item in _rootGameObject.transform)
		{
			ControlPoint controlPoint2 = pointArray[num4];
			RectTransform component = item.GetComponent<RectTransform>();
			if (num4 != 0)
			{
				component.localPosition = localPosition;
			}
			float num5 = m_slider * num2 - num3;
			if (num5 < 0f)
			{
				num5 = 0f;
			}
			if (num5 > controlPoint2.length)
			{
				num5 = controlPoint2.length;
			}
			component.sizeDelta = new Vector2(x, num5);
			component.rotation = Quaternion.Euler(0f, 0f, controlPoint2.rotation);
			num3 += controlPoint2.length;
			localPosition = Quaternion.Euler(0f, 0f, controlPoint2.rotation) * new Vector3(0f, 0f - component.sizeDelta.y + num, 0f) + component.localPosition;
			if ((num4 == 0 || num4 == pointArray.Length - 1) && item.transform.childCount > 0)
			{
				Image component2 = item.transform.GetChild(0).GetComponent<Image>();
				if (num4 == pointArray.Length - 1)
				{
					component2.rectTransform.localPosition = new Vector3(0f, 0f - controlPoint2.length, 0f);
					component2.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -90f + controlPoint2.rotation));
					component2.gameObject.SetActive(m_slider == 1f);
				}
				else
				{
					component2.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f + controlPoint2.rotation));
					component2.gameObject.SetActive(m_slider > 0f);
				}
			}
			num4++;
		}
	}

	public void ForceValidate()
	{
		_bInspectorChanged = true;
	}

	private void OnValidate()
	{
		_bInspectorChanged = true;
	}
}
