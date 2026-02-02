using UnityEngine;
using UnityEngine.UI;

public class QualifingScrollCell : ScrollIndexCallback
{
	private QualifingUI parentQualifingUI;

	[SerializeField]
	private Text KillText;

	[SerializeField]
	private Text BeKillText;

	[SerializeField]
	private Text TimeText;

	[SerializeField]
	private Image[] ResultImage;

	[SerializeField]
	private HorizontalLayoutGroup HLG;

	[SerializeField]
	private GameObject RecordCell;

	private int idx;

	private NetPVPRecord NetRecord;

	private void SetHLGSetupCharacter(int _id, bool bType, int _star, int _skin, int idx = 0)
	{
		GameObject gameObject = Object.Instantiate(RecordCell, HLG.transform.position, new Quaternion(0f, 0f, 0f, 0f));
		gameObject.transform.SetParent(HLG.transform);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject.SetActive(true);
		if (bType)
		{
			gameObject.GetComponent<RecordInfoCell>().SetupCharacter(_id, _star, _skin);
		}
		else
		{
			gameObject.GetComponent<RecordInfoCell>().SetupWeapon(_id, _star, _skin, idx);
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		parentQualifingUI = GetComponentInParent<QualifingUI>();
		idx = p_idx;
		base.name = "q_" + p_idx;
		NetRecord = parentQualifingUI.OnGetNetRecordList(idx);
		KillText.text = NetRecord.KillCount.ToString();
		BeKillText.text = NetRecord.BeKilledCount.ToString();
		TimeText.text = CapUtility.UnixTimeToDate(NetRecord.BattleStartTime).ToLocalTime().ToString("MM/dd HH:mm");
		bool flag = NetRecord.KillCount >= 3;
		ResultImage[0].gameObject.SetActive(flag);
		ResultImage[1].gameObject.SetActive(!flag);
		int childCount = HLG.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Object.Destroy(HLG.transform.GetChild(i).gameObject);
		}
		for (int j = 0; j < NetRecord.CharacterIDList.Count; j++)
		{
			if (NetRecord.CharacterIDList[j] > 0)
			{
				SetHLGSetupCharacter(NetRecord.CharacterIDList[j], true, NetRecord.CharacterStarList[j], NetRecord.CharacterSkinList[j]);
			}
		}
		for (int k = 0; k < NetRecord.WeaponIDList.Count; k++)
		{
			if (NetRecord.WeaponIDList[k] > 0)
			{
				SetHLGSetupCharacter(NetRecord.WeaponIDList[k], false, NetRecord.WeaponStarList[k], NetRecord.WeaponSkinList[k], k);
			}
		}
	}
}
