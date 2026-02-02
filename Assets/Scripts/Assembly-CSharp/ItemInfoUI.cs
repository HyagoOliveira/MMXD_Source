using UnityEngine;
using UnityEngine.UI;

public class ItemInfoUI : ItemInfoBase
{
	[SerializeField]
	private OrangeText textTite;

	[SerializeField]
	private Button btnUse;

	[SerializeField]
	private Button btnHow2Get;

	[SerializeField]
	private Image imgPiece;

	private int requestCount;

	public bool CanShowHow2Get { get; set; }

	protected override void Awake()
	{
		base.Awake();
		CanShowHow2Get = true;
	}

	protected override void SetItemUiInfo(int p_requestCount = 0)
	{
		requestCount = p_requestCount;
		base.SetItemUiInfo(p_requestCount);
		textTite.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ITEM_INFO");
		bool interactable = false;
		if (netItem != null && item.n_TYPE == 5)
		{
			interactable = true;
			btnUse.onClick.AddListener(OnClickUse);
		}
		imgPiece.color = ((item.n_TYPE == 4) ? Color.white : Color.clear);
		btnUse.interactable = interactable;
		btnHow2Get.interactable = CanShowHow2Get;
	}

	public override void Setup(EQUIP_TABLE p_equip)
	{
		base.Setup(p_equip);
		textTite.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("GET_GUIDE_6");
		btnUse.interactable = false;
		btnHow2Get.interactable = CanShowHow2Get;
	}

	public override void Setup(CHARACTER_TABLE p_character)
	{
		base.Setup(p_character);
		textTite.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHARACTER_INFO");
		btnUse.interactable = false;
		btnHow2Get.interactable = CanShowHow2Get;
	}

	public override void Setup(WEAPON_TABLE p_weaepon)
	{
		base.Setup(p_weaepon);
		textTite.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("WEAPON_INFO");
		btnUse.interactable = false;
		btnHow2Get.interactable = CanShowHow2Get;
	}

	public override void Setup(CARD_TABLE p_card, ITEM_TABLE p_item)
	{
		base.Setup(p_card, p_item);
		textTite.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("CHARA_CARD_INFO");
		btnUse.interactable = false;
		btnHow2Get.interactable = CanShowHow2Get;
	}

	private void Start()
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	protected override void OnClickSold()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemSell", delegate(ItemSellUI ui)
		{
			base.CloseSE = SystemSE.NONE;
			OnClickCloseBtn();
			ui.Setup(item, netItem);
		});
	}

	public void OnClickHow2Get()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemHowToGet", delegate(ItemHowToGetUI ui)
		{
			ui.Setup(item, requestCount);
		});
	}

	public void OnClickUse()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemUse", delegate(ItemUseUI ui)
		{
			base.CloseSE = SystemSE.NONE;
			OnClickCloseBtn();
			ui.Setup(item, netItem);
		});
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_PLAYER_BOX, UpdateItemAmount);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_PLAYER_BOX, UpdateItemAmount);
	}

	private void UpdateItemAmount()
	{
		if (netItem != null)
		{
			itemIcon.SetAmount(ManagedSingleton<PlayerHelper>.Instance.GetItemValue(netItem.ItemID));
		}
	}
}
