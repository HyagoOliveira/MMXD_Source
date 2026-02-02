using Coffee.UISoftMask;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class StandBase : MonoBehaviour
{
	public enum Rotation
	{
		TOP = 0,
		LEFT = 1
	}

	private static int StandBaseCount;

	private readonly int TriggerGCCount = 5;

	[SerializeField]
	private Rotation rotation;

	private RectTransform rt;

	private Image img;

	private bool triggerUnloadUnusedAssets;

	[SerializeField]
	private float scale = 1f;

	public Rotation _Rotation
	{
		get
		{
			return rotation;
		}
	}

	public void Awake()
	{
		base.gameObject.AddOrGetComponent<SoftMaskable>();
		img = GetComponent<Image>();
		rt = GetComponent<RectTransform>();
		Vector2 sizeDelta = rt.sizeDelta;
		Vector2 pivot = img.sprite.pivot;
		GetComponent<RectTransform>().pivot = new Vector2(pivot.x / sizeDelta.x, pivot.y / sizeDelta.y);
		SetRotation();
		rt.localScale = new Vector3(scale, scale, scale);
		StandBaseCount++;
		triggerUnloadUnusedAssets = !MonoBehaviourSingleton<OrangeSceneManager>.Instance.IsBattleScene && StandBaseCount % TriggerGCCount == 0;
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_RESOLUTION, RefreashMask);
	}

	public void Setup(Transform parent)
	{
		base.gameObject.transform.SetParent(parent, false);
		img.color = Color.white;
	}

	private void SetRotation()
	{
		switch (rotation)
		{
		case Rotation.LEFT:
			rt.localRotation = Quaternion.Euler(0f, 0f, 270f);
			break;
		case Rotation.TOP:
			rt.localRotation = Quaternion.Euler(0f, 0f, 0f);
			break;
		}
	}

	public void SetColor(Color newColor)
	{
		img.color = newColor;
	}

	[Button(null)]
	private void UpdateScale()
	{
		GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_RESOLUTION, RefreashMask);
		if (triggerUnloadUnusedAssets)
		{
			Resources.UnloadUnusedAssets();
		}
	}

	private void RefreashMask()
	{
		SoftMaskable component = GetComponent<SoftMaskable>();
		if ((bool)component)
		{
			component.enabled = false;
			component.enabled = true;
		}
	}
}
