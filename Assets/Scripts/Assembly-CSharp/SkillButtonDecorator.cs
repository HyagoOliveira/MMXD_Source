using UnityEngine;
using UnityEngine.UI;

public class SkillButtonDecorator : MonoBehaviour
{
	public enum StyleType
	{
		UNLOCKED = 0,
		LOCKED_SKILL = 1,
		LOCKED_UNKNOW_SKILL = 2,
		UNLOCKABLE = 3,
		DEFRAGABLE = 4,
		UNKNOW_ONLY = 5,
		NOT_AVAILABLE = 6,
		GOLD = 7,
		MAX = 8
	}

	[SerializeField]
	private Image questionIcon;

	[SerializeField]
	private Image blackTransparentIcon;

	[SerializeField]
	private Image iconFrame;

	[SerializeField]
	private Image iconFrameGlow;

	[SerializeField]
	private Image iconFrameGold;

	[SerializeField]
	private OrangeText statusText;

	[SerializeField]
	private GameObject unlockInfoGroup;

	[SerializeField]
	private OrangeText unlockLevelText;

	public void Setup(StyleType style)
	{
		iconFrame.gameObject.SetActive(true);
		blackTransparentIcon.gameObject.SetActive(true);
		iconFrameGlow.gameObject.SetActive(false);
		iconFrameGold.gameObject.SetActive(false);
		questionIcon.gameObject.SetActive(false);
		statusText.gameObject.SetActive(false);
		unlockInfoGroup.SetActive(false);
		switch (style)
		{
		case StyleType.UNLOCKED:
			blackTransparentIcon.gameObject.SetActive(false);
			break;
		case StyleType.LOCKED_SKILL:
			unlockInfoGroup.SetActive(true);
			break;
		case StyleType.LOCKED_UNKNOW_SKILL:
			questionIcon.gameObject.SetActive(true);
			unlockInfoGroup.SetActive(true);
			break;
		case StyleType.UNLOCKABLE:
			statusText.gameObject.SetActive(true);
			iconFrame.gameObject.SetActive(false);
			iconFrameGlow.gameObject.SetActive(true);
			statusText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_RESEARCH_READY");
			break;
		case StyleType.DEFRAGABLE:
			statusText.gameObject.SetActive(true);
			iconFrame.gameObject.SetActive(false);
			iconFrameGlow.gameObject.SetActive(true);
			questionIcon.gameObject.SetActive(true);
			statusText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("DNA_RECOMPOSE_READY");
			break;
		case StyleType.NOT_AVAILABLE:
			statusText.gameObject.SetActive(true);
			iconFrame.gameObject.SetActive(true);
			iconFrameGlow.gameObject.SetActive(false);
			questionIcon.gameObject.SetActive(true);
			statusText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NOT_AVAILABLE");
			break;
		case StyleType.UNKNOW_ONLY:
			questionIcon.gameObject.SetActive(true);
			blackTransparentIcon.gameObject.SetActive(false);
			break;
		case StyleType.GOLD:
			iconFrame.gameObject.SetActive(false);
			blackTransparentIcon.gameObject.SetActive(false);
			iconFrameGold.gameObject.SetActive(true);
			break;
		}
	}

	public void SetUnlockStarCount(int starCount)
	{
		unlockLevelText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CARD_UNLOCK_RANK"), starCount);
	}
}
