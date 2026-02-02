using System;
using UnityEngine;
using UnityEngine.UI;

public class AimSystemItem : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Image image;

	private Vector2 defalutSize = new Vector2(1f, 1f);

	private Vector2 tweenSize = new Vector2(1.5f, 1.5f);

	private Transform _transform;

	private int tweenUid = -1;

	private bool enable;

	public void Awake()
	{
		_transform = base.transform;
	}

	public void SetEnable(bool p_enable)
	{
		if (enable != p_enable)
		{
			if (canvas.worldCamera == null)
			{
				canvas.worldCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.GetBattleGUICamera()._camera;
			}
			enable = p_enable;
			if (enable)
			{
				image.color = Color.white;
				UpdateTargetAnim();
			}
			else
			{
				image.transform.localScale = defalutSize;
				image.color = Color.clear;
			}
		}
	}

	public void UpdateTargetAnim()
	{
		if (tweenUid == -1)
		{
			tweenUid = LeanTween.scale(image.gameObject, tweenSize, 0.05f).setLoopPingPong(1).setEaseInOutCubic()
				.setOnComplete((Action)delegate
				{
					tweenUid = -1;
				})
				.uniqueId;
		}
	}

	private void LateUpdate()
	{
		_transform.eulerAngles = Vector3.zero;
	}

	private void OnDestroy()
	{
		LeanTween.cancel(ref tweenUid);
	}

	public void Show(bool bSet)
	{
		canvas.enabled = bSet;
	}
}
