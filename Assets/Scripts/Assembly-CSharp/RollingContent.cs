using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RollingContent : MonoBehaviour
{
	private enum RollingState
	{
		Stop = 0,
		Rolling = 1,
		ChangeNext = 2,
		Spacing = 3,
		Continue = 4
	}

	[SerializeField]
	public Scrollbar.Direction m_scrollDir = Scrollbar.Direction.BottomToTop;

	[SerializeField]
	[Range(0f, 5f)]
	public float m_rollingSpeed = 1f;

	[SerializeField]
	[Range(0f, 10f)]
	public float m_spacingTime = 3f;

	[SerializeField]
	public bool m_loop = true;

	[SerializeField]
	public Transform content;

	[SerializeField]
	private ContentSizeFitter m_sizeFitter;

	[SerializeField]
	private GridLayoutGroup m_gridLayout;

	[SerializeField]
	public Transform m_sample;

	[HideInInspector]
	public int m_currentItem;

	private bool m_bRolling;

	private RollingState m_rollingState;

	private float m_velocity;

	private Vector2 m_size;

	private float fStartValue;

	private float fEndValue;

	private float fDeltaValue;

	private float m_totalDelay;

	private Dictionary<Scrollbar.Direction, float> dirVH = new Dictionary<Scrollbar.Direction, float>
	{
		{
			Scrollbar.Direction.BottomToTop,
			1f
		},
		{
			Scrollbar.Direction.TopToBottom,
			-1f
		},
		{
			Scrollbar.Direction.LeftToRight,
			1f
		},
		{
			Scrollbar.Direction.RightToLeft,
			-1f
		}
	};

	private void Start()
	{
	}

	public void Setup()
	{
		m_size = m_sample.GetChild(0).GetComponent<RectTransform>().sizeDelta;
		base.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(m_size.x, m_size.y);
		m_sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		m_sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
		m_gridLayout.cellSize = m_size;
		m_gridLayout.constraintCount = 3;
		content.localPosition = new Vector2(0f, 0f);
		m_sample.gameObject.SetActive(false);
		switch (m_scrollDir)
		{
		case Scrollbar.Direction.BottomToTop:
		case Scrollbar.Direction.TopToBottom:
			m_gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
			m_sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			break;
		case Scrollbar.Direction.LeftToRight:
		case Scrollbar.Direction.RightToLeft:
			m_gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			m_sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			break;
		}
		base.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(m_size.x, m_size.y);
		m_gridLayout.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(m_size.x, m_size.y);
	}

	public void SetRollingDir(Scrollbar.Direction dir)
	{
	}

	private void Update()
	{
		if (!m_bRolling)
		{
			return;
		}
		switch (m_rollingState)
		{
		case RollingState.Rolling:
			UpdatePositon();
			break;
		case RollingState.ChangeNext:
			if (m_loop)
			{
				m_currentItem++;
				if (m_currentItem >= m_sample.childCount)
				{
					m_currentItem = 0;
				}
				if (m_spacingTime > 0f)
				{
					m_rollingState = RollingState.Spacing;
				}
				else
				{
					StartRolling();
				}
			}
			else
			{
				StopRolling();
			}
			break;
		case RollingState.Spacing:
			UpdateDelay();
			break;
		case RollingState.Stop:
		case RollingState.Continue:
			break;
		}
	}

	private void UpdateDelay()
	{
		m_totalDelay += Time.smoothDeltaTime;
		if (m_totalDelay >= m_spacingTime)
		{
			m_totalDelay = 0f;
			StartRolling();
		}
	}

	private void UpdatePositon()
	{
		int childCount = m_sample.childCount;
		float smoothDeltaTime = Time.smoothDeltaTime;
		fDeltaValue += smoothDeltaTime * m_velocity;
		if (fDeltaValue >= fEndValue)
		{
			fDeltaValue = fEndValue;
			m_rollingState = RollingState.ChangeNext;
		}
		if (childCount <= 1)
		{
			return;
		}
		if (childCount == 2)
		{
			switch (m_scrollDir)
			{
			case Scrollbar.Direction.TopToBottom:
				content.localPosition = new Vector2(0f, fDeltaValue * dirVH[m_scrollDir]);
				break;
			case Scrollbar.Direction.BottomToTop:
				content.localPosition = new Vector2(0f, fDeltaValue * dirVH[m_scrollDir] - m_size.y);
				break;
			case Scrollbar.Direction.RightToLeft:
				content.localPosition = new Vector2(fDeltaValue * dirVH[m_scrollDir] + m_size.x, 0f);
				break;
			case Scrollbar.Direction.LeftToRight:
				content.localPosition = new Vector2(fDeltaValue * dirVH[m_scrollDir], 0f);
				break;
			}
		}
		else
		{
			switch (m_scrollDir)
			{
			case Scrollbar.Direction.BottomToTop:
			case Scrollbar.Direction.TopToBottom:
				content.localPosition = new Vector2(0f, fDeltaValue * dirVH[m_scrollDir]);
				break;
			case Scrollbar.Direction.LeftToRight:
			case Scrollbar.Direction.RightToLeft:
				content.localPosition = new Vector2(fDeltaValue * dirVH[m_scrollDir], 0f);
				break;
			}
		}
	}

	private void ResetPostion()
	{
		int childCount = m_sample.childCount;
		fDeltaValue = 0f;
		fStartValue = 0f;
		switch (childCount)
		{
		case 1:
			switch (m_scrollDir)
			{
			case Scrollbar.Direction.BottomToTop:
			case Scrollbar.Direction.TopToBottom:
				content.localPosition = new Vector2(0f, 0f - m_size.y);
				break;
			case Scrollbar.Direction.LeftToRight:
			case Scrollbar.Direction.RightToLeft:
				content.localPosition = new Vector2(m_size.x, 0f);
				break;
			}
			break;
		case 2:
			switch (m_scrollDir)
			{
			case Scrollbar.Direction.BottomToTop:
			case Scrollbar.Direction.TopToBottom:
				content.localPosition = new Vector2(0f, 0f - m_size.y);
				break;
			case Scrollbar.Direction.LeftToRight:
			case Scrollbar.Direction.RightToLeft:
				content.localPosition = new Vector2(m_size.x, 0f);
				break;
			}
			break;
		default:
			content.localPosition = new Vector2(0f, 0f);
			break;
		}
		switch (m_scrollDir)
		{
		case Scrollbar.Direction.BottomToTop:
		case Scrollbar.Direction.TopToBottom:
			fEndValue = m_size.y;
			break;
		case Scrollbar.Direction.LeftToRight:
		case Scrollbar.Direction.RightToLeft:
			fEndValue = m_size.x;
			break;
		}
		m_velocity = fEndValue / (1f * m_rollingSpeed);
	}

	public void ReStartRolling()
	{
		m_currentItem = 0;
		StartRolling();
	}

	public void StartRolling()
	{
		SetCurrentItem();
		ResetPostion();
		m_rollingState = RollingState.Rolling;
		m_bRolling = true;
	}

	private void SetCurrentItem()
	{
		if (m_rollingState == RollingState.Rolling || m_rollingState == RollingState.Continue)
		{
			return;
		}
		int num = m_currentItem;
		while (content.childCount > 0)
		{
			Object.DestroyImmediate(content.GetChild(0).gameObject);
		}
		int num2 = m_sample.childCount;
		if (num2 > 3)
		{
			num2 = 3;
		}
		for (int i = 0; i < num2; i++)
		{
			Object.Instantiate(m_sample.GetChild(num).gameObject, content, false).SetActive(true);
			if (++num >= m_sample.childCount)
			{
				num = 0;
			}
		}
	}

	public void StopRolling()
	{
		m_currentItem = 0;
		m_bRolling = false;
		m_rollingState = RollingState.Stop;
	}

	public void Clear()
	{
		StopRolling();
		while (content.childCount > 0)
		{
			Object.DestroyImmediate(content.GetChild(0).gameObject);
		}
		while (m_sample.childCount > 0)
		{
			Object.DestroyImmediate(m_sample.GetChild(0).gameObject);
		}
	}
}
