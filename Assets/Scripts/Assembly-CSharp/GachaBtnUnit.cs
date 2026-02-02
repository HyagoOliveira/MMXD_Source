#define RELEASE
using UnityEngine;
using UnityEngine.UI;

public class GachaBtnUnit : MonoBehaviour
{
	[SerializeField]
	private Image iconCost;

	[SerializeField]
	private OrangeText textCost;

	[SerializeField]
	private OrangeText textBtn;

	[SerializeField]
	private GameObject groupTip;

	[SerializeField]
	private OrangeText textTip;

	[SerializeField]
	private Image imgBtn;

	[SerializeField]
	private Sprite[] btnSps = new Sprite[2];

	[HideInInspector]
	public GACHALIST_TABLE gachaListTable;

	private ITEM_TABLE costItem;

	public Button Button { get; private set; }

	private void Awake()
	{
		Button = GetComponent<Button>();
		Button.interactable = false;
	}

	public void Setup(GACHALIST_TABLE p_gachaListTable)
	{
		gachaListTable = p_gachaListTable;
		SetCost();
		SetBtn();
	}

	private void SetCost()
	{
		costItem = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(gachaListTable.n_COIN_ID, out costItem))
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconItem(costItem.s_ICON), costItem.s_ICON, delegate(Sprite obj)
			{
				iconCost.sprite = obj;
				iconCost.color = Color.white;
			});
			textCost.text = "x" + gachaListTable.n_COIN_MOUNT;
		}
		else
		{
			Debug.LogWarning("Can't find n_COIN_ID:" + gachaListTable.n_COIN_ID);
			iconCost.color = Color.clear;
			textCost.text = "x" + 0;
		}
	}

	private void SetBtn()
	{
		textBtn.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(gachaListTable.w_BUTTON_TEXT);
		imgBtn.sprite = btnSps[gachaListTable.n_BUTTON_IMG - 1];
		Button.interactable = true;
		if (!(groupTip == null))
		{
			if (gachaListTable.w_BUTTON_TIP == "null")
			{
				groupTip.gameObject.SetActive(false);
				return;
			}
			groupTip.gameObject.SetActive(true);
			textTip.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(gachaListTable.w_BUTTON_TIP);
		}
	}

	public void OnClickGacha()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GachaConfirm", delegate(GachaConfirmUI ui)
		{
			ui.Setup(gachaListTable, costItem);
		});
	}
}
