using UnityEngine;
using UnityEngine.UI;

public class OrangeScrollingImg : MonoBehaviour, IManagedUpdateBehavior
{
	public enum ScrollingType
	{
		OFFSET = 0,
		SCALE = 1
	}

	[SerializeField]
	private ScrollingType scrollingType;

	[SerializeField]
	protected Image imgScroll;

	[SerializeField]
	protected Vector2 scrollSpd = new Vector2(0f, 0f);

	private Vector2 offset = new Vector2(0f, 0f);

	private Material copyMaterial;

	private Vector3 one = Vector3.one;

	private void Awake()
	{
		if (scrollingType != 0)
		{
			int num = 1;
			return;
		}
		copyMaterial = Object.Instantiate(imgScroll.material);
		copyMaterial.mainTexture = Object.Instantiate(imgScroll.mainTexture);
		imgScroll.material = copyMaterial;
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected virtual void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	public void UpdateFunc()
	{
		switch (scrollingType)
		{
		case ScrollingType.OFFSET:
			OnOffset(Time.time);
			break;
		case ScrollingType.SCALE:
			OnScale();
			break;
		}
	}

	protected virtual void OnOffset(float time)
	{
		offset = scrollSpd * time;
		copyMaterial.SetTextureOffset(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_MainTex, offset);
	}

	protected virtual void OnScale()
	{
		imgScroll.rectTransform.localScale += (Vector3)scrollSpd * (Time.deltaTime * 60f);
	}
}
