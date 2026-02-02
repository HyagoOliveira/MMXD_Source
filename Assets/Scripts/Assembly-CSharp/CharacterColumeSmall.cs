using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColumeSmall : ScrollIndexCallback
{
	public class PerCharacterSmallCell
	{
		public int nID = -1;

		public GameObject refRoot;

		public CommonIconBase tCharacterIconBase;

		public NetData tNetCharacterInfo;

		public Image selimage;

		public Image imgPlaying;

		public Text textPlaying;

		public void OnClick(int p_param)
		{
			if (nID != -1)
			{
				GoCheckUI goCheckUI = null;
				Transform transform = refRoot.transform;
				while (transform != null && goCheckUI == null)
				{
					goCheckUI = transform.GetComponent<GoCheckUI>();
					transform = transform.parent;
				}
				if (!(goCheckUI == null))
				{
					int nowRefSelectCharacter = goCheckUI.GetNowRefSelectCharacter();
					int nID2 = nID;
					goCheckUI.SetSelectCharacter(this);
				}
			}
		}

		public void OnNetCallSet()
		{
			GoCheckUI goCheckUI = null;
			Transform transform = refRoot.transform;
			while (transform != null && goCheckUI == null)
			{
				goCheckUI = transform.GetComponent<GoCheckUI>();
				transform = transform.parent;
			}
			if (!(goCheckUI == null))
			{
				goCheckUI.SetSelectCharacter(this);
			}
		}
	}

	private const int nCellCount = 2;

	public GameObject refCharacterBase;

	public PerCharacterSmallCell[] allPerPerCharacterSmallCells;

	public override void BackToPool()
	{
		GoCheckUI goCheckUI = null;
		Transform parent = base.transform;
		while (parent != null && goCheckUI == null)
		{
			goCheckUI = parent.GetComponent<GoCheckUI>();
			parent = parent.parent;
		}
		if (goCheckUI != null && goCheckUI.refSelectCharacter != null && allPerPerCharacterSmallCells != null)
		{
			for (int i = 0; i < allPerPerCharacterSmallCells.Length; i++)
			{
				if (goCheckUI.refSelectCharacter.nID == allPerPerCharacterSmallCells[i].nID)
				{
					goCheckUI.refSelectCharacter = null;
					break;
				}
			}
		}
		allPerPerCharacterSmallCells = null;
		MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(this, itemName);
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (allPerPerCharacterSmallCells == null)
		{
			allPerPerCharacterSmallCells = new PerCharacterSmallCell[2];
			for (int i = 0; i < 2; i++)
			{
				Transform transform = base.transform.Find("CharacterIconRoot" + i);
				allPerPerCharacterSmallCells[i] = new PerCharacterSmallCell();
				if (transform != null)
				{
					CommonIconBase commonIconBase = transform.GetComponentInChildren<CommonIconBase>();
					if (commonIconBase == null)
					{
						commonIconBase = Object.Instantiate(refCharacterBase, transform).GetComponent<CommonIconBase>();
					}
					allPerPerCharacterSmallCells[i].refRoot = transform.gameObject;
					allPerPerCharacterSmallCells[i].tCharacterIconBase = commonIconBase;
					allPerPerCharacterSmallCells[i].selimage = transform.Find("Image").GetComponent<Image>();
					allPerPerCharacterSmallCells[i].selimage.gameObject.SetActive(false);
					Transform transform2 = commonIconBase.transform;
					allPerPerCharacterSmallCells[i].imgPlaying = transform2.Find("imgPlaying").GetComponent<Image>();
					allPerPerCharacterSmallCells[i].textPlaying = transform2.Find("imgPlaying/TextPlaying").GetComponent<Text>();
				}
			}
		}
		GoCheckUI goCheckUI = null;
		Transform parent = base.transform;
		while (parent != null && goCheckUI == null)
		{
			goCheckUI = parent.GetComponent<GoCheckUI>();
			parent = parent.parent;
		}
		if (goCheckUI == null)
		{
			return;
		}
		List<CharacterInfo> sortedCharacterList = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList();
		int j;
		for (j = 0; j < 2; j++)
		{
			int num = p_idx * 2 + j;
			if (num >= sortedCharacterList.Count)
			{
				break;
			}
			allPerPerCharacterSmallCells[j].refRoot.SetActive(true);
			NetCharacterInfo netInfo = sortedCharacterList[num].netInfo;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.ContainsKey(netInfo.CharacterID))
			{
				continue;
			}
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[netInfo.CharacterID];
			if (netInfo.Skin > 0)
			{
				SKIN_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(netInfo.Skin, out value))
				{
					allPerPerCharacterSmallCells[j].tCharacterIconBase.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + value.s_ICON), "icon_" + value.s_ICON, allPerPerCharacterSmallCells[j].OnClick, !goCheckUI.listUsedPlayerID.Contains(netInfo.CharacterID));
				}
			}
			else
			{
				allPerPerCharacterSmallCells[j].tCharacterIconBase.Setup(0, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + cHARACTER_TABLE.s_ICON), "icon_" + cHARACTER_TABLE.s_ICON, allPerPerCharacterSmallCells[j].OnClick, !goCheckUI.listUsedPlayerID.Contains(netInfo.CharacterID));
			}
			allPerPerCharacterSmallCells[j].nID = netInfo.CharacterID;
			allPerPerCharacterSmallCells[j].tNetCharacterInfo = netInfo;
			if (netInfo.CharacterID == goCheckUI.nUseCharacter)
			{
				goCheckUI.SetSelectCharacter(allPerPerCharacterSmallCells[j]);
				allPerPerCharacterSmallCells[j].selimage.gameObject.SetActive(true);
			}
			else
			{
				allPerPerCharacterSmallCells[j].tCharacterIconBase.SetOtherInfo(netInfo, false, false, goCheckUI.listUsedPlayerID.Contains(netInfo.CharacterID));
			}
			allPerPerCharacterSmallCells[j].tCharacterIconBase.EnableLevel(false);
			GetComponentInParent<GoCheckUI>().BonusSub.SetCommonIcon(allPerPerCharacterSmallCells[j].tCharacterIconBase, netInfo.CharacterID);
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bSeasonBase)
			{
				allPerPerCharacterSmallCells[j].tCharacterIconBase.SetOtherInfo(netInfo, false, false, goCheckUI.listUsedPlayerID.Contains(netInfo.CharacterID));
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SetSeasonIconFlag(cHARACTER_TABLE.n_ID, allPerPerCharacterSmallCells[j]);
				int num2 = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CheckSeasonCharaterNum(cHARACTER_TABLE.n_ID);
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ModifySeasonIconFlag(true, cHARACTER_TABLE.n_ID, num2);
			}
			allPerPerCharacterSmallCells[j].tCharacterIconBase.UpdateFatiguedImage(netInfo.FatiguedValue);
		}
		for (; j < 2; j++)
		{
			allPerPerCharacterSmallCells[j].refRoot.SetActive(false);
		}
	}
}
