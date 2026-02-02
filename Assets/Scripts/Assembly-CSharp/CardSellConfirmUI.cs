using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class CardSellConfirmUI : OrangeUIBase
{
	[SerializeField]
	private LoopVerticalScrollRect m_scrollRect;

	[SerializeField]
	private CardSellConfirmCell m_cardSellConfirmCell;

	[SerializeField]
	private OrangeText m_textTitle;

	[SerializeField]
	private OrangeText m_textDescription;

	private List<NetCardInfo> m_listNetCardInfoFiltered = new List<NetCardInfo>();

	public CardSellUI tCardSellUI;

	public Callback tCallback;

	public void Setup(List<NetCardInfo> _list, string txtTitle, string txtDes, bool bOnlyProtected = false, Callback p_cb = null)
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		if (bOnlyProtected)
		{
			m_listNetCardInfoFiltered.Clear();
			for (int i = 0; i < _list.Count; i++)
			{
				if (_list[i].Protected == 1)
				{
					m_listNetCardInfoFiltered.Add(_list[i]);
				}
			}
		}
		else
		{
			m_listNetCardInfoFiltered = _list;
		}
		m_textTitle.text = txtTitle;
		m_textDescription.text = txtDes;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		tCallback = p_cb;
		m_scrollRect.OrangeInit(m_cardSellConfirmCell, m_listNetCardInfoFiltered.Count, m_listNetCardInfoFiltered.Count);
	}

	public void SetCardIcon(CardSellConfirmCell p_unit)
	{
		NetCardInfo netCardInfo = m_listNetCardInfoFiltered[p_unit.NowIdx];
		CARD_TABLE value = null;
		ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue(netCardInfo.CardID, out value);
		if (value != null)
		{
			int cardRank = ManagedSingleton<OrangeTableHelper>.Instance.GetCardRank(netCardInfo.Exp);
			p_unit.CardIcon.SetStarAndLv(netCardInfo.Star, cardRank);
			p_unit.CardIcon.SetRare(value.n_RARITY);
			p_unit.CardIcon.SetTypeImage(value.n_TYPE);
			p_unit.CardIcon.SetLockImage(netCardInfo.Protected == 1);
			string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
			string s_ICON = value.s_ICON;
			p_unit.CardIcon.Setup(p_unit.NowIdx, p_bundleName, s_ICON);
			p_unit.SetCardID(netCardInfo.CardSeqID, netCardInfo.CardID);
		}
		else
		{
			p_unit.CardIcon.Clear();
		}
	}

	public void OnClickConfirmBtn()
	{
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
		tCallback();
		OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
		if (m_scrollRect.prefabSource.pbo != null)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.ClearPoolItem(m_scrollRect.prefabSource.pbo.itemName);
		}
	}
}
