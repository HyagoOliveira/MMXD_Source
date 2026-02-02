using UnityEngine;
using UnityEngine.UI;

public class MsgUI : OrangeUIBase
{
	[SerializeField]
	private Text textMsg;

	public void Setup(string msg)
	{
		textMsg.text = msg;
	}
}
