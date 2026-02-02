using UnityEngine;
using UnityEngine.UI;

public class CommonFloatInfoUI : CommonFloatUIBase
{
	[SerializeField]
	private Text _textInfo;

	public void Setup(string info, Vector3 tarPos)
	{
		base.Setup(tarPos);
		_textInfo.text = info;
	}
}
