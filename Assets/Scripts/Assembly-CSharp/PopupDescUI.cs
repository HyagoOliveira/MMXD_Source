using UnityEngine;

public class PopupDescUI : OrangeUIBase
{
	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private OrangeText textDesc;

	public void Setup(Vector2 position, string p_titile, string p_desc)
	{
		base.transform.position = position;
		textTitle.text = p_titile;
		textDesc.text = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(p_desc, textDesc);
	}
}
