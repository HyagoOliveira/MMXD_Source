using UnityEngine;

public class EquipIcon : ItemIconBase
{
	[SerializeField]
	private StarClearComponent EquipStar;

	[SerializeField]
	private OrangeText Level;

	[SerializeField]
	private GameObject rareEquipEffect;

	[SerializeField]
	private OrangeText enhanceLevel;

	private bool starInst;

	public void SetStarAndLv(int star, int lv, int enhanceLv = 0)
	{
		EquipStar.gameObject.SetActive(true);
		EquipStar.SetActiveStar(star);
		Level.text = lv.ToString();
		if (enhanceLevel != null)
		{
			if (enhanceLv != 0)
			{
				enhanceLevel.text = string.Format("+{0}", enhanceLv);
			}
			else
			{
				enhanceLevel.text = "";
			}
		}
	}

	public override void Clear()
	{
		base.Clear();
		EquipStar.gameObject.SetActive(false);
		SetRareEquipEffect(false);
	}

	public void SetRareEquipEffect(bool enable)
	{
		if (rareEquipEffect != null)
		{
			rareEquipEffect.SetActive(enable);
		}
	}
}
