using UnityEngine;
using UnityEngine.UI;
using enums;

public class ShopBuyIAPUIUnit : ItemIconWithAmount
{
	[SerializeField]
	private OrangeText textExtraMsg;

	[SerializeField]
	private Image imgPiece;

	private void Awake()
	{
		textExtraMsg.text = string.Empty;
	}

	public void SetExtraMsg(string msg)
	{
		textExtraMsg.text = msg;
	}

	public void SetPiece(ItemType itemType)
	{
		imgPiece.color = ((itemType == ItemType.Shard) ? Color.white : Color.clear);
	}
}
