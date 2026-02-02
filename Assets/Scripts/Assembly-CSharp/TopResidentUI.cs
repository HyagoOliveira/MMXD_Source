using UnityEngine;

public class TopResidentUI : MonoBehaviour
{
	[SerializeField]
	protected RectTransform rect;

	[SerializeField]
	protected float activeY = -540f;

	[SerializeField]
	protected float disableY = -440f;

	protected float animTime = 0.3f;

	[SerializeField]
	protected OrangeText textEnergy;

	[SerializeField]
	protected OrangeText textGmoney;

	[SerializeField]
	protected OrangeText textMoney;

	public void SetUIActive(bool active)
	{
		if (active)
		{
			UpdateValue();
		}
		float from = (active ? disableY : activeY);
		float to = (active ? activeY : disableY);
		LeanTween.value(from, to, animTime).setEaseInCubic().setOnUpdate(delegate(float f)
		{
			rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, f);
		});
	}

	public virtual void UpdateValue()
	{
		textEnergy.text = string.Format("{0}/{1}", ManagedSingleton<PlayerHelper>.Instance.GetStamina(), ManagedSingleton<PlayerHelper>.Instance.GetStaminaLimit());
		textGmoney.text = ManagedSingleton<PlayerHelper>.Instance.GetTotalJewel().ToString();
		textMoney.text = ManagedSingleton<PlayerHelper>.Instance.GetZenny().ToString();
	}
}
