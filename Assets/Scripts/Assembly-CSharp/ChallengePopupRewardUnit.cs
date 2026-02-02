using UnityEngine;
using UnityEngine.UI;

public class ChallengePopupRewardUnit : ItemIconBase
{
	[SerializeField]
	protected Image fragment;

	public void SetPieceActive(bool active)
	{
		fragment.gameObject.SetActive(active);
	}
}
