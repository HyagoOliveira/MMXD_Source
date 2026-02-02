using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class VisualHpBar : PoolBaseObject
{
	private Vector3 scale = new Vector3(0.01f, 0.01f, 0.01f);

	private Color[] hpColors = new Color[3]
	{
		new Color(0f, 0.878f, 0.086f),
		new Color(0.878f, 0.784f, 0f),
		new Color(0.643f, 0.047f, 0f)
	};

	[SerializeField]
	private Image imgBar;

	private UIEffect eftColor;

	private float hpMax;

	private Canvas canvas;

	public void Awake()
	{
		eftColor = imgBar.GetComponent<UIEffect>();
		canvas = GetComponent<Canvas>();
	}

	public void Setup(float p_hpMax, Transform p_parent, Vector3 p_localPos)
	{
		if (canvas.worldCamera == null)
		{
			canvas.worldCamera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.GetBattleGUICamera()._camera;
		}
		hpMax = p_hpMax;
		eftColor.effectColor = hpColors[0];
		base.transform.SetParent(p_parent);
		base.transform.localPosition = p_localPos;
		base.transform.localScale = scale;
		imgBar.fillAmount = 1f;
	}

	public void UpdateHpFill(float p_hpNow)
	{
		if (p_hpNow <= 0f)
		{
			BackToPool();
			return;
		}
		float fillAmount = imgBar.fillAmount;
		float to = p_hpNow / hpMax;
		LeanTween.value(fillAmount, to, 0.1f).setOnUpdate(delegate(float val)
		{
			imgBar.fillAmount = val;
		}).setOnComplete(UpdateHpColor);
	}

	private void UpdateHpColor()
	{
		if (imgBar.fillAmount > 0.51f)
		{
			eftColor.effectColor = hpColors[0];
		}
		else if (imgBar.fillAmount > 0.25f)
		{
			eftColor.effectColor = hpColors[1];
		}
		else
		{
			eftColor.effectColor = hpColors[2];
		}
	}
}
