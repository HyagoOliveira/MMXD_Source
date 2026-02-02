using UnityEngine;
using UnityEngine.UI;

public class Mini01Cell : ScrollIndexCallback
{
	[SerializeField]
	private GameObject SectionObj;

	[SerializeField]
	private Image SectionBG;

	[SerializeField]
	private Image SectionFG;

	[SerializeField]
	private OrangeText SectionText;

	[SerializeField]
	private Mini01Material[] Cells;

	private Minigame01UI m01UI;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void SetRewordCell(int cell, Minigame01UI.CListNode node)
	{
		Cells[cell].ItemAmout = "";
		if (node.itemTbl == null)
		{
			Cells[cell].ItemAmout = "";
			Cells[cell].ItemName = "";
		}
		else
		{
			Cells[cell].ItemAmout = node.itemTbl.n_ID.ToString();
			Cells[cell].ItemName = node.itemTbl.s_NAME;
			Cells[cell].p_convertItemID = node.itemTbl.n_ID;
		}
	}

	private void SetRankingCell(bool bCell1, Minigame01UI.CListNode cNode)
	{
		Sprite sct;
		Sprite fg;
		Sprite bg;
		Color color = ((!(cNode.section == m01UI.localStringKeys[0])) ? m01UI.GetSpriteType(Minigame01UI.RewardT.ResultReward, Minigame01UI.SectionT.BastResult, out sct, out fg, out bg) : m01UI.GetSpriteType(Minigame01UI.RewardT.ResultReward, Minigame01UI.SectionT.NormalResult, out sct, out fg, out bg));
		SectionBG.sprite = sct;
		SectionFG.color = color;
		SectionText.color = color;
		if (cNode.itemTbl == null)
		{
			return;
		}
		int num = ((!bCell1) ? 1 : 0);
		Cells[num].gameObject.SetActive(true);
		Cells[num].ItemAmout = "";
		Cells[num].SetBanner(fg, bg);
		Cells[num].Count = cNode.amount;
		if (cNode.itemTbl.n_TYPE == 5 && cNode.itemTbl.n_TYPE_X == 1 && (int)cNode.itemTbl.f_VALUE_Y > 0)
		{
			CARD_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)cNode.itemTbl.f_VALUE_Y, out value))
			{
				Cells[num].CardIcon = value;
			}
		}
		else
		{
			Cells[num].Icon = cNode.itemTbl.s_ICON;
		}
		Cells[num].ItemName = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(cNode.itemTbl.w_NAME);
		Cells[num].ItemNameColor = color;
		if (cNode.section == m01UI.localStringKeys[1])
		{
			Cells[num].ItemAmout = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("LABOEVENT_GET_INFO"), cNode.cnt, cNode.table.n_ITEM_LIMIT);
		}
		Cells[num].SetRera(cNode.itemTbl.n_RARE);
		Cells[num].p_convertItemID = cNode.itemTbl.n_ID;
	}

	private void SetRewardCell(bool bCell1, Minigame01UI.CListNode nd)
	{
		int num = 0;
		num = nd.sectionIdx;
		if (num > 3)
		{
			num = 3;
		}
		Sprite sct;
		Sprite fg;
		Sprite bg;
		Color spriteType = m01UI.GetSpriteType(Minigame01UI.RewardT.ResultReward, (Minigame01UI.SectionT)num, out sct, out fg, out bg);
		SectionBG.sprite = sct;
		SectionFG.color = spriteType;
		SectionText.color = spriteType;
		SectionText.text = nd.section;
		if (nd.itemTbl != null)
		{
			int num2 = ((!bCell1) ? 1 : 0);
			Cells[num2].transform.gameObject.SetActive(true);
			Cells[num2].Count = nd.amount;
			Cells[num2].Icon = nd.itemTbl.s_ICON;
			Cells[num2].ItemName = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(nd.itemTbl.w_NAME);
			Cells[num2].ItemNameColor = spriteType;
			Cells[num2].ItemAmout = "";
			Cells[num2].SetBanner(fg, bg);
			Cells[num2].SetRera(nd.itemTbl.n_RARE);
			Cells[num2].p_convertItemID = nd.itemTbl.n_ID;
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		m01UI = GetComponentInParent<Minigame01UI>();
		if (m01UI == null)
		{
			return;
		}
		Minigame01UI.CListNode cListNode = null;
		int num = p_idx * 2;
		if (m01UI.rewordType[0].isOn)
		{
			if (num < m01UI.m_nodeList.Count)
			{
				cListNode = m01UI.m_nodeList[num];
				if (num == 0)
				{
					SectionObj.SetActive(true);
				}
				else if (m01UI.m_nodeList[num - 1].section != cListNode.section)
				{
					SectionObj.SetActive(true);
				}
				SectionText.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr(cListNode.section);
				SetRankingCell(num % 2 == 0, cListNode);
			}
			num++;
			if (num < m01UI.m_nodeList.Count)
			{
				cListNode = m01UI.m_nodeList[num];
				SetRankingCell(num % 2 == 0, cListNode);
			}
		}
		else
		{
			if (num < m01UI.m_rankingNodeList.Count)
			{
				cListNode = m01UI.m_rankingNodeList[num];
				if (num == 0)
				{
					SectionObj.SetActive(true);
				}
				else if (m01UI.m_rankingNodeList[num - 1].sectionIdx != cListNode.sectionIdx)
				{
					SectionObj.SetActive(true);
				}
				SetRewardCell(num % 2 == 0, cListNode);
			}
			num++;
			if (num < m01UI.m_rankingNodeList.Count)
			{
				cListNode = m01UI.m_rankingNodeList[num];
				SetRewardCell(num % 2 == 0, cListNode);
			}
		}
		base.gameObject.SetActive(true);
	}

	private void ResetToPool()
	{
		SectionObj.SetActive(false);
		Cells[0].transform.gameObject.SetActive(false);
		Cells[1].transform.gameObject.SetActive(false);
	}

	public override void BackToPool()
	{
		ResetToPool();
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}
}
