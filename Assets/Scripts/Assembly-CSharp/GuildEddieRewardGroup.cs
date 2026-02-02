#define RELEASE
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GuildEddieRewardGroup : MonoBehaviour
{
	[SerializeField]
	private GuildEddieRewardUnit _sourceObject;

	private GuildEddieRewardUnit[] _rewardUnits;

	public void Setup(int rank, int donateValue, List<NetEddieBoxGachaRecord> boxGachaRecordList)
	{
		int boxLevel = 0;
		int num = 0;
		GUILD_MAIN guildAttrData;
		if (ManagedSingleton<OrangeDataManager>.Instance.GUILD_MAIN_DICT.TryGetValue(rank, out guildAttrData))
		{
			List<BOXGACHA_TABLE> list = (from box in ManagedSingleton<OrangeDataManager>.Instance.BOXGACHA_TABLE_DICT.Values
				where box.n_GROUP == guildAttrData.n_GUILD_BOX
				orderby box.n_PRE descending
				select box).ToList();
			int num2 = list.FindIndex((BOXGACHA_TABLE box) => donateValue >= box.n_PRE);
			if (num2 < 0)
			{
				Debug.LogWarning(string.Format("No Box : {0} / Threshold : {1} of {2} Data", guildAttrData.n_GUILD_BOX, donateValue, "BOXGACHA_TABLE"));
				num2 = list.Count - 1;
			}
			BOXGACHA_TABLE boxAttrData = list[num2];
			boxLevel = list.Count - num2 - 1;
			num = ManagedSingleton<OrangeDataManager>.Instance.BOXGACHACONTENT_TABLE_DICT.Values.Where((BOXGACHACONTENT_TABLE content) => content.n_GROUP == boxAttrData.n_GACHA).Sum((BOXGACHACONTENT_TABLE content) => content.n_TOTAL);
		}
		else
		{
			Debug.LogError(string.Format("Invalid Rank : {0} of {1} Data", rank, "GUILD_MAIN"));
		}
		_rewardUnits = new GuildEddieRewardUnit[num];
		for (int i = 0; i < _rewardUnits.Length; i++)
		{
			_rewardUnits[i] = Object.Instantiate(_sourceObject, base.transform);
		}
		_sourceObject.gameObject.SetActive(false);
		GuildEddieRewardUnit[] rewardUnits = _rewardUnits;
		for (int j = 0; j < rewardUnits.Length; j++)
		{
			rewardUnits[j].Reset(boxLevel);
		}
		foreach (NetEddieBoxGachaRecord boxGachaRecord in boxGachaRecordList)
		{
			int boxIndex = boxGachaRecord.BoxIndex;
			if (boxIndex >= _rewardUnits.Length)
			{
				Debug.LogError(string.Format("Invalid BoxIndex : {0}, out of range ({1})", boxIndex, _rewardUnits.Length));
			}
			else
			{
				_rewardUnits[boxIndex].Setup(boxGachaRecord);
			}
		}
	}
}
