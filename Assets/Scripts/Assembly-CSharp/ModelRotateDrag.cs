using UnityEngine;
using UnityEngine.EventSystems;

public class ModelRotateDrag : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	private Vector2 m_dragForce;

	private bool m_bDragging;

	private float m_deltaX;

	private float m_deltaY;

	private float m_scale = 0.5f;

	private float m_secToStop = 1f;

	private Transform m_modelTransform;

	[SerializeField]
	[Range(0f, 500f)]
	private float m_speedCap = 500f;

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			m_dragForce = new Vector2(m_deltaX, m_deltaY);
			m_bDragging = false;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!m_modelTransform)
		{
			return;
		}
		float num = Mathf.Clamp(m_deltaX, 0f - m_speedCap, m_speedCap) * m_scale * Time.deltaTime;
		m_modelTransform.Rotate(0f, 0f - num, 0f);
		if (!m_bDragging)
		{
			float num2 = (0f - m_dragForce.x) * (Time.deltaTime / m_secToStop);
			m_deltaX += num2;
			if (SameSign(m_deltaX, num2))
			{
				num2 = (m_deltaX = 0f);
			}
		}
	}

	public void SetModelTransform(Transform inputTransform)
	{
		m_modelTransform = inputTransform;
	}

	private bool SameSign(float value1, float value2)
	{
		return value1 < 0f == value2 < 0f;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_bDragging = true;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_deltaX = eventData.position.x - eventData.pressPosition.x;
			m_deltaY = eventData.position.y - eventData.pressPosition.y;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_dragForce = new Vector2(m_deltaX, m_deltaY);
			m_bDragging = false;
		}
	}

	public void onChange_Character_Pos()
	{
		if (!m_bDragging)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR07);
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_CHARACTER_POS);
		}
	}
}
