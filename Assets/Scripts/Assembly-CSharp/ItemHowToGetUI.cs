#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemHowToGetUI : OrangeUIBase
{
	public class Data
	{
		public HOWTOGET_TABLE HowToGetTable;

		public bool IsOpen;

		public string countLimit;

		public Data(HOWTOGET_TABLE HowToGetTable, bool IsOpen, string cond)
		{
			this.HowToGetTable = HowToGetTable;
			this.IsOpen = IsOpen;
			countLimit = cond;
		}
	}

	public struct TargetItem
	{
		public int ItemId;

		public int RequestCount;

		public bool IsExist
		{
			get
			{
				if (ItemId != -1)
				{
					return RequestCount > 0;
				}
				return false;
			}
		}

		public int GetOwnStack
		{
			get
			{
				return ManagedSingleton<PlayerHelper>.Instance.GetItemValue(ItemId);
			}
		}

		public string ToDisplayString
		{
			get
			{
				return string.Format("{0}/{1}", GetOwnStack, RequestCount);
			}
		}

		public TargetItem(int itemId, int requestCount)
		{
			ItemId = itemId;
			RequestCount = requestCount;
		}
	}

	public TargetItem Target = new TargetItem(-1, -1);

	[SerializeField]
	private OrangeText textTitle;

	[SerializeField]
	private ItemIconBase itemIcon;

	[SerializeField]
	private OrangeText textItemName;

	[SerializeField]
	private OrangeText textItemTip;

	[SerializeField]
	private Image imgEmptyMsgBg;

	[SerializeField]
	private OrangeText textMsgNothing;

	[SerializeField]
	private ItemHowToGetUIUnit unit;

	[SerializeField]
	private LoopVerticalScrollRect scrollRect;

	[SerializeField]
	private GameObject groupEventTime;

	[SerializeField]
	private OrangeText textEventTime;

	[SerializeField]
	private Image imgCardType;

	private int playerLV;

	private ITEM_TABLE itemTable;

	private int[] howToGetIds;

	private List<Data> listHow2GetData = new List<Data>();

	public void Setup(List<EVENT_TABLE> p_eventList)
	{
		ITEM_TABLE p_itemTable = ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT[p_eventList[0].n_DROP_ITEM];
		Setup(p_itemTable);
		textTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(p_eventList[0].w_NAME);
	}

	public void Setup(ITEM_TABLE p_itemTable, int p_requestCount = -1)
	{
		textTitle.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ITEM_INFO");
		playerLV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		itemTable = p_itemTable;
		itemIcon.SetRare(itemTable.n_RARE);
		if (itemTable.n_TYPE == 5 && itemTable.n_TYPE_X == 1 && (int)itemTable.f_VALUE_Y > 0)
		{
			CARD_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)itemTable.f_VALUE_Y, out value))
			{
				string s_ICON = value.s_ICON;
				string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
				itemIcon.Setup(value.n_ID, p_bundleName, s_ICON, OnClickCardInfo);
				string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(value.n_TYPE);
				imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
				imgCardType.gameObject.SetActive(true);
			}
		}
		else
		{
			itemIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(itemTable.s_ICON), itemTable.s_ICON);
			imgCardType.gameObject.SetActive(false);
		}
		textItemTip.alignByGeometry = false;
		textItemName.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemTable.w_NAME);
		textItemTip.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(itemTable.w_TIP);
		Target = new TargetItem(p_itemTable.n_ID, p_requestCount);
		Debug.Log(Target.ToDisplayString);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		StartCoroutine(OnStartSetUnit());
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private IEnumerator OnStartSetUnit()
	{
		long now = MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowUTC;
		List<EVENT_TABLE> elist = (from t in ManagedSingleton<ExtendDataHelper>.Instance.EVENT_TABLE_DICT
			where t.Value.n_TYPE == 2
			select t into i
			where i.Value.n_DROP_ITEM == itemTable.n_ID
			where ManagedSingleton<OrangeTableHelper>.Instance.IsOpeningDate(i.Value.s_BEGIN_TIME, i.Value.s_END_TIME, now)
			select i into o
			select o.Value).ToList();
		howToGetIds = ManagedSingleton<OrangeTableHelper>.Instance.ParseHowToGetRow(itemTable.s_HOWTOGET);
		List<EVENT_TABLE>.Enumerator enumerator = elist.GetEnumerator();
		enumerator.MoveNext();
		int[] array = howToGetIds;
		foreach (int key in array)
		{
			HOWTOGET_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.HOWTOGET_TABLE_DICT.TryGetValue(key, out value))
			{
				listHow2GetData.Add(new Data(value, IsSystemOpen(value), GetConditionStr(itemTable.n_ID, enumerator.Current)));
				enumerator.MoveNext();
			}
		}
		yield return CoroutineDefine._waitForEndOfFrame;
		if (listHow2GetData.Count > 0)
		{
			textMsgNothing.enabled = false;
			imgEmptyMsgBg.color = Color.clear;
			scrollRect.OrangeInit(unit, 3, listHow2GetData.Count);
			scrollRect.verticalScrollbarVisibility = LoopScrollRect.ScrollbarVisibility.Permanent;
		}
		else
		{
			textMsgNothing.enabled = true;
			imgEmptyMsgBg.color = new Color(0.7f, 0.7f, 0.7f, 1f);
			scrollRect.verticalScrollbarVisibility = LoopScrollRect.ScrollbarVisibility.AutoHide;
		}
		if (elist.Count != 0)
		{
			DateTime date = ManagedSingleton<OrangeTableHelper>.Instance.ParseDate(elist[0].s_END_TIME);
			bool remain = false;
			textEventTime.text = OrangeGameUtility.GetRemainTimeText(CapUtility.DateToUnixTime(date), MonoBehaviourSingleton<OrangeGameManager>.Instance.ServerUnixTimeNowLocale, out remain);
			groupEventTime.SetActive(true);
		}
		yield return null;
	}

	private bool IsSystemOpen(HOWTOGET_TABLE howToGetTable)
	{
		if (playerLV < howToGetTable.n_RANK)
		{
			return false;
		}
		bool result = true;
		switch ((UILinkHelper.LINK)howToGetTable.n_UILINK)
		{
		case UILinkHelper.LINK.STORY:
		{
			if (howToGetTable.n_VALUE_X == 0)
			{
				result = true;
				break;
			}
			STAGE_TABLE sTAGE_TABLE2 = null;
			sTAGE_TABLE2 = ((howToGetTable.n_VALUE_Y != 0) ? ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.FirstOrDefault((STAGE_TABLE x) => x.n_MAIN == howToGetTable.n_VALUE_X && x.n_SUB == howToGetTable.n_VALUE_Y && x.n_DIFFICULTY == howToGetTable.n_VALUE_Z) : ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.FirstOrDefault((STAGE_TABLE x) => x.n_MAIN == howToGetTable.n_VALUE_X && x.n_DIFFICULTY == howToGetTable.n_VALUE_Z));
			StageHelper.StageJoinCondition condition2 = StageHelper.StageJoinCondition.NONE;
			if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(sTAGE_TABLE2, ref condition2))
			{
				result = condition2 == StageHelper.StageJoinCondition.AP || condition2 == StageHelper.StageJoinCondition.COUNT;
			}
			break;
		}
		case UILinkHelper.LINK.EVENT:
			if (playerLV >= OrangeConst.OPENRANK_STAGE_EVENT)
			{
				if (howToGetTable.n_VALUE_X == 0)
				{
					break;
				}
				STAGE_TABLE sTAGE_TABLE = null;
				sTAGE_TABLE = ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.Values.FirstOrDefault((STAGE_TABLE x) => x.n_MAIN == howToGetTable.n_VALUE_X);
				if (sTAGE_TABLE != null)
				{
					StageHelper.StageJoinCondition condition = StageHelper.StageJoinCondition.NONE;
					if (!ManagedSingleton<StageHelper>.Instance.IsStageConditionOK(sTAGE_TABLE, ref condition))
					{
						result = condition == StageHelper.StageJoinCondition.AP || condition == StageHelper.StageJoinCondition.COUNT;
					}
				}
			}
			else
			{
				result = false;
			}
			break;
		case UILinkHelper.LINK.CHALLENGE:
			result = playerLV >= OrangeConst.OPENRANK_STAGE_CHALLENGE;
			break;
		case UILinkHelper.LINK.COOP:
			result = playerLV >= OrangeConst.OPENRANK_STAGE_CORP;
			break;
		default:
			result = true;
			break;
		}
		return result;
	}

	private string GetConditionStr(int itemID, EVENT_TABLE tbl)
	{
		if (tbl != null)
		{
			int missionCounter = ManagedSingleton<MissionHelper>.Instance.GetMissionCounter(tbl.n_COUNTER);
			int n_LIMIT = tbl.n_LIMIT;
			return string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("ITEM_EVENT_GET_CONDITION"), missionCounter, n_LIMIT);
		}
		return "";
	}

	public Data GetChildInfo(int idx)
	{
		return listHow2GetData[idx];
	}

	private void OnClickCardInfo(int p_idx)
	{
		CARD_TABLE card = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(p_idx, out card))
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CardInfo", delegate(CardInfoUI ui)
			{
				ui.bOnlyShowBasic = true;
				ui.bNeedInitList = true;
				ui.nTargetCardSeqID = 0;
				ui.nTargetCardID = card.n_ID;
			});
		}
	}
}
