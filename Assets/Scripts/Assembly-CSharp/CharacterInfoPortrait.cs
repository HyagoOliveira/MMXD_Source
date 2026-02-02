#define RELEASE
using UnityEngine;

public class CharacterInfoPortrait : OrangeUIBase
{
	[SerializeField]
	private RectTransform m_portraitPos;

	private bool m_bIsInitialized;

	private Vector3 m_touchStart;

	private Vector3 m_touchNow;

	private float m_buttonDownStartTime;

	private float m_initializedTime;

	private const float m_closeClickTime = 0.2f;

	private const float m_minScale = 0.5f;

	private const float m_maxScale = 3f;

	private float m_halfViewScreenWidth = (float)Screen.width / 2f * 0.8f;

	private float m_halfViewScreenHeight = (float)Screen.height / 2f * 0.8f;

	private int m_scaleTweenId;

	private int m_translateTweenId;

	private int m_lastTouchCount;

	private Bounds m_portraitBounds;

	private bool m_bTouchEngaged;

	private DeviceOrientation mDeviceOrientation;

	private void Update()
	{
		if (!m_bIsInitialized)
		{
			return;
		}
		m_touchNow = Input.mousePosition;
		if (Input.GetMouseButtonDown(0))
		{
			m_bTouchEngaged = true;
			m_buttonDownStartTime = Time.time;
			m_touchStart = m_touchNow;
			LeanTween.cancel(ref m_translateTweenId);
			LeanTween.cancel(ref m_scaleTweenId);
		}
		if (m_bTouchEngaged)
		{
			if (Input.touchCount >= 2)
			{
				Touch touch = Input.GetTouch(0);
				Touch touch2 = Input.GetTouch(1);
				Vector2 vector = touch.position - touch.deltaPosition;
				Vector2 vector2 = touch2.position - touch2.deltaPosition;
				float magnitude = (vector - vector2).magnitude;
				float num = ((touch.position - touch2.position).magnitude - magnitude) * 0.004f;
				float num2 = Mathf.Clamp(m_portraitPos.localScale.x + num, 0.1f, 100f);
				m_portraitPos.localScale = new Vector3(num2, num2, 1f);
			}
			bool flag = false;
			if (m_lastTouchCount == Input.touchCount)
			{
				Vector3 vector3 = m_touchStart - m_touchNow;
				m_portraitPos.localPosition -= new Vector3(vector3.x, vector3.y, vector3.z);
				flag = true;
			}
			if (Input.GetMouseButtonUp(0))
			{
				if (Time.time - m_buttonDownStartTime < 0.2f && (m_touchNow - m_touchStart).magnitude < 2f)
				{
					OnClickCloseBtn();
					return;
				}
				ScaleCheck();
				if (flag)
				{
					MoveMomentum(m_touchStart - m_touchNow);
				}
				m_bTouchEngaged = false;
			}
			m_touchStart = m_touchNow;
		}
		if (Input.mouseScrollDelta.y != 0f)
		{
			float num3 = Input.mouseScrollDelta.y * 0.2f;
			float num4 = Mathf.Clamp(m_portraitPos.localScale.x + num3, 0.1f, 100f);
			if (num4 != m_portraitPos.localScale.x)
			{
				m_portraitPos.localScale = new Vector3(num4, num4, 1f);
			}
			ScaleCheck();
			TranslationCheck();
		}
		m_lastTouchCount = Input.touchCount;
		if (mDeviceOrientation == Input.deviceOrientation)
		{
			return;
		}
		switch (Input.deviceOrientation)
		{
		case DeviceOrientation.LandscapeLeft:
		case DeviceOrientation.LandscapeRight:
			m_portraitPos.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
			break;
		case DeviceOrientation.Portrait:
			if (mDeviceOrientation == DeviceOrientation.LandscapeLeft)
			{
				m_portraitPos.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
			}
			else if (mDeviceOrientation == DeviceOrientation.LandscapeRight)
			{
				m_portraitPos.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
			}
			break;
		case DeviceOrientation.PortraitUpsideDown:
			if (mDeviceOrientation == DeviceOrientation.LandscapeLeft)
			{
				m_portraitPos.localRotation = Quaternion.Euler(new Vector3(0f, 0f, -90f));
			}
			else if (mDeviceOrientation == DeviceOrientation.LandscapeRight)
			{
				m_portraitPos.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
			}
			break;
		}
		if (Input.deviceOrientation != DeviceOrientation.FaceDown && Input.deviceOrientation != DeviceOrientation.FaceUp && Input.deviceOrientation != 0)
		{
			mDeviceOrientation = Input.deviceOrientation;
		}
	}

	public void Setup(CHARACTER_TABLE characterTable, CharacterInfo characterInfo)
	{
		SKIN_TABLE value;
		ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(characterInfo.netInfo.Skin, out value);
		string dragonbonesFile = characterTable.s_ICON;
		if (value != null)
		{
			dragonbonesFile = value.s_ICON;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, dragonbonesFile), dragonbonesFile + "_db", delegate(GameObject obj)
		{
			if (obj == null)
			{
				Debug.LogWarning("Portrait file load failed: " + dragonbonesFile);
			}
			else
			{
				Object.Instantiate(obj, m_portraitPos, false);
			}
			m_bIsInitialized = true;
			m_initializedTime = Time.time;
			m_portraitBounds = CalculateBounds(obj);
		});
		Input.multiTouchEnabled = true;
		closeCB = StopTween;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		m_portraitPos.localRotation = new Quaternion(0f, 0f, 0f, 1f);
		mDeviceOrientation = Input.deviceOrientation;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public override void OnClickCloseBtn()
	{
		if (!(Time.time - m_initializedTime < 1f))
		{
			base.OnClickCloseBtn();
		}
	}

	private Bounds CalculateBounds(GameObject obj)
	{
		Bounds result = default(Bounds);
		result.size = new Vector3(1000f, 1500f, 1f);
		result.center = new Vector3(0f, 500f, 0f);
		return result;
	}

	private void MoveMomentum(Vector3 speed)
	{
		float time = 0.5f;
		float num = 5f;
		Vector3 vector = speed * num;
		Vector3 to = m_portraitPos.localPosition - new Vector3(vector.x, vector.y, vector.z);
		LeanTween.cancel(ref m_translateTweenId);
		m_translateTweenId = LeanTween.value(m_portraitPos.gameObject, m_portraitPos.localPosition, to, time).setOnUpdate(delegate(Vector3 v)
		{
			m_portraitPos.localPosition = new Vector3(v.x, v.y, v.z);
			TranslationCheck();
		}).setEaseOutCubic()
			.uniqueId;
	}

	private void ScaleCheck()
	{
		float time = 0.3f;
		Vector3 localScale = m_portraitPos.localScale;
		if (m_portraitPos.localScale.x < 0.5f)
		{
			localScale = new Vector3(0.5f, 0.5f, 1f);
			LeanTween.cancel(ref m_scaleTweenId);
			m_scaleTweenId = LeanTween.value(m_portraitPos.gameObject, m_portraitPos.localScale, localScale, time).setOnUpdate(delegate(Vector3 v)
			{
				m_portraitPos.localScale = new Vector3(v.x, v.y, v.z);
			}).setEaseOutCubic()
				.uniqueId;
		}
		else if (m_portraitPos.localScale.x > 3f)
		{
			localScale = new Vector3(3f, 3f, 1f);
			LeanTween.cancel(ref m_scaleTweenId);
			m_scaleTweenId = LeanTween.value(m_portraitPos.gameObject, m_portraitPos.localScale, localScale, time).setOnUpdate(delegate(Vector3 v)
			{
				m_portraitPos.localScale = new Vector3(v.x, v.y, v.z);
			}).setEaseOutCubic()
				.uniqueId;
		}
	}

	private void TranslationCheck()
	{
		float time = 0.5f;
		float x = m_portraitPos.localScale.x;
		Vector2 vector = new Vector2(m_portraitBounds.center.x * x, m_portraitBounds.center.y * x);
		Vector2 vector2 = new Vector2(m_portraitBounds.extents.x * x, m_portraitBounds.extents.y * x);
		float num = m_portraitPos.localPosition.x - vector2.x - vector.x;
		float num2 = m_portraitPos.localPosition.x + vector2.x - vector.x;
		float num3 = m_portraitPos.localPosition.y + vector2.y - vector.y;
		float num4 = m_portraitPos.localPosition.y - vector2.y - vector.y;
		Vector3 to = m_portraitPos.localPosition;
		bool flag = false;
		if (num2 > m_halfViewScreenWidth && num > m_halfViewScreenWidth)
		{
			to = new Vector3(m_halfViewScreenWidth + vector2.x + vector.x, to.y, to.z);
			flag = true;
		}
		if (num2 < 0f - m_halfViewScreenWidth && num < 0f - m_halfViewScreenWidth)
		{
			to = new Vector3(0f - m_halfViewScreenWidth - vector2.x + vector.x, to.y, to.z);
			flag = true;
		}
		if (num3 > m_halfViewScreenHeight && num4 > m_halfViewScreenHeight)
		{
			to = new Vector3(to.x, m_halfViewScreenHeight + vector2.y + vector.y, to.z);
			flag = true;
		}
		if (num3 < 0f - m_halfViewScreenHeight && num4 < 0f - m_halfViewScreenHeight)
		{
			to = new Vector3(to.x, 0f - m_halfViewScreenHeight - vector2.y + vector.y, to.z);
			flag = true;
		}
		if (flag)
		{
			LeanTween.cancel(ref m_translateTweenId);
			m_translateTweenId = LeanTween.value(m_portraitPos.gameObject, m_portraitPos.localPosition, to, time).setOnUpdate(delegate(Vector3 v)
			{
				m_portraitPos.localPosition = new Vector3(v.x, v.y, v.z);
			}).setEaseOutCubic()
				.uniqueId;
		}
	}

	private void StopTween()
	{
		LeanTween.cancel(ref m_translateTweenId);
		LeanTween.cancel(ref m_scaleTweenId);
	}
}
