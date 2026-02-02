using UnityEngine;
using UnityEngine.UI;

internal class SeasonMember : ScrollIndexCallback
{
	[SerializeField]
	private Text textName;

	[SerializeField]
	private Transform stParent;

	private int idx;

	private int CharacterID;

	private GoCheckUI parentGoCheckUI;

	public override void ScrollCellIndex(int p_idx)
	{
	}

	public void Setup(int _idx, int _cid)
	{
		idx = _idx;
		CharacterID = _cid;
		parentGoCheckUI = GetComponentInParent<GoCheckUI>();
		CHARACTER_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(CharacterID, out value))
		{
			return;
		}
		string text = "st_" + value.s_ICON;
		int skin = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[CharacterID].netInfo.Skin;
		if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.ContainsKey(skin))
		{
			text = "st_" + ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[skin].s_ICON;
		}
		for (int num = stParent.transform.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(stParent.transform.GetChild(num).gameObject);
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, text), text, delegate(GameObject obj)
		{
			if (obj != null)
			{
				Object.Instantiate(obj).GetComponent<StandBase>().Setup(stParent);
			}
		});
	}

	public void OnRemoveCharacter()
	{
		if (CharacterID == 0)
		{
			return;
		}
		parentGoCheckUI.OnRemoveCharacter(idx);
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ModifySeasonIconFlag(false, CharacterID, idx + 1);
		for (int i = 0; i < MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList.Count; i++)
		{
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList[i] == CharacterID)
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.SeasonCharaterList[i] = 0;
			}
		}
		CharacterID = 0;
		for (int num = stParent.transform.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(stParent.transform.GetChild(num).gameObject);
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
	}
}
