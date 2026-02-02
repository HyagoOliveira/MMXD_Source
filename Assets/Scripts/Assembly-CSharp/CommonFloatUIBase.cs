using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public abstract class CommonFloatUIBase : OrangeUIBase
{
	private readonly float BORDER_SPACE = 10f;

	[SerializeField]
	private SystemSE _soundOpen;

	[SerializeField]
	private SystemSE _soundClose;

	[SerializeField]
	private GameObject _content;

	[SerializeField]
	private float PosXShift;

	[SerializeField]
	private float PosYShift;

	private Canvas _canvasContent;

	private RectTransform _rtContent;

	private Coroutine _coroutine;

	protected override void Awake()
	{
		base.Awake();
		_canvasContent = _content.GetComponent<Canvas>();
		_rtContent = _content.GetComponent<RectTransform>();
		_canvasContent.enabled = false;
	}

	public virtual void Setup(Vector3 tarPos)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(_soundOpen);
		_coroutine = StartCoroutine(SetupPos(tarPos));
	}

	protected virtual void OnDestroy()
	{
		if (_coroutine != null)
		{
			StopCoroutine(_coroutine);
		}
	}

	public virtual void OnClickBackground()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(_soundClose);
		base.OnClickCloseBtn();
	}

	private IEnumerator SetupPos(Vector3 tarPos)
	{
		while (_rtContent.rect.size == Vector2.zero)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		_rtContent.position = tarPos;
		Vector3 localPosition = _rtContent.localPosition;
		localPosition.z = 0f;
		_rtContent.localPosition = localPosition;
		float num = rt.rect.size.x / 2f - _rtContent.rect.size.x / 2f - BORDER_SPACE;
		float num2 = rt.rect.size.y / 2f - _rtContent.rect.size.y / 2f - BORDER_SPACE;
		float a = 0f - num;
		float a2 = 0f - num2;
		Vector2 anchoredPosition = _rtContent.anchoredPosition;
		anchoredPosition.x = Mathf.Max(a, Mathf.Min(num, anchoredPosition.x)) + PosXShift;
		anchoredPosition.y = Mathf.Max(a2, Mathf.Min(num2, anchoredPosition.y)) + PosYShift;
		_rtContent.anchoredPosition = anchoredPosition;
		_canvasContent.enabled = true;
	}
}
