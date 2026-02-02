using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrusadeRecordUI : OrangeUIBase
{
	[SerializeField]
	private CrusadeRecordGroup _battleRecordGroup;

	[SerializeField]
	private Transform _battleRecordGroupContent;

	[SerializeField]
	private Text _textTotalScore;

	public void Setup(List<NetCrusadeBattleRecord> battleRecordList, long totalScore)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		_textTotalScore.text = totalScore.ToString();
		for (int i = 0; i < battleRecordList.Count; i++)
		{
			NetCrusadeBattleRecord battleRecord = battleRecordList[i];
			AddPlayerRecord(battleRecord);
		}
	}

	private void AddPlayerRecord(NetCrusadeBattleRecord battleRecord)
	{
		CrusadeCharacterInfo useCharacter = battleRecord.UseCharacter;
		CrusadeWeaponInfo useMainWeapon = battleRecord.UseMainWeapon;
		CrusadeWeaponInfo useSubWeapon = battleRecord.UseSubWeapon;
		int battleTime = battleRecord.BattleTime;
		long score = battleRecord.Score;
		CrusadeRecordGroup crusadeRecordGroup = Object.Instantiate(_battleRecordGroup, _battleRecordGroupContent);
		crusadeRecordGroup.gameObject.SetActive(true);
		crusadeRecordGroup.Setup(useCharacter.ToNetCharacterInfo(), useMainWeapon.ToNetWeaponInfo(), useSubWeapon.ToNetWeaponInfo(), battleTime, score);
	}
}
