using UnityEngine;

public class CH100_ShungokusatsuHitFx : FxBase
{
	[SerializeField]
	protected Transform _tfBlackBG;

	public void ActivePlayBlackBG(bool visible)
	{
		_tfBlackBG.gameObject.SetActive(visible);
	}
}
