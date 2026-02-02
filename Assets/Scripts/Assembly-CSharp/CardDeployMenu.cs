using UnityEngine;
using UnityEngine.UI;

public class CardDeployMenu : MonoBehaviour
{
	[SerializeField]
	private LoopVerticalScrollRect ScrollRect;

	[SerializeField]
	private CardDeploySetCell DeploySetCell;

	[SerializeField]
	private CardDeployCell DeployCell;

	[SerializeField]
	private CardDeployGoCheckCell DeployGoCheckCell;

	public void Setup(bool bCardMainOpen = false, bool bGoCheck = false)
	{
		int num = OrangeConst.CARD_DEPLOY_MAX;
		if (!bCardMainOpen)
		{
			num = OrangeConst.CARD_DEPLOY_OPEN + ManagedSingleton<PlayerHelper>.Instance.GetCardDeployExpansion();
		}
		num = ((num > OrangeConst.CARD_DEPLOY_MAX) ? OrangeConst.CARD_DEPLOY_MAX : num);
		if (ScrollRect != null)
		{
			ScrollRect.ClearCells();
		}
		if (bCardMainOpen)
		{
			ScrollRect.OrangeInit(DeploySetCell, num, num);
		}
		else if (bGoCheck)
		{
			ScrollRect.OrangeInit(DeployGoCheckCell, num, num);
		}
		else
		{
			ScrollRect.OrangeInit(DeployCell, num, num);
		}
	}
}
