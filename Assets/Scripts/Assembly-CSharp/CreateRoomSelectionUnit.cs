using CallbackDefs;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomSelectionUnit : EnhanceItem
{
	private Color[] col = new Color[2]
	{
		new Color(0.24313726f, 0.18431373f, 0.29411766f),
		new Color(0.7607843f, 0.827451f, 74f / 85f)
	};

	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private Button uButton;

	[SerializeField]
	private UIShadow uiShadow;
    [System.Obsolete]
    private CallbackIdx m_cb;

	private int idx = -1;

    [System.Obsolete]
    public void Init(string p_str, int p_idx, CallbackIdx p_cb = null)
	{
		textTitle.text = p_str;
		idx = p_idx;
		m_cb = p_cb;
	}

	protected override void OnStart()
	{
		if (base.IsVaild)
		{
			uButton.onClick.AddListener(OnClickEnhanceItem);
		}
	}

	protected override void SetItemDepth(float depthCurveValue, int depthFactor, float itemCount)
	{
		int siblingIndex = (int)(depthCurveValue * itemCount);
		base.transform.SetSiblingIndex(siblingIndex);
	}

	public override void SetSelectState(bool isCenter)
	{
		uiShadow.enabled = !isCenter;
		if (isCenter)
		{
			textTitle.color = col[0];
			textTitle.fontSize = 34;
		}
		else
		{
			textTitle.color = col[1];
			textTitle.fontSize = 32;
		}
		if (isCenter && idx != -1)
		{
			m_cb.CheckTargetToInvoke(idx);
		}
	}
}
