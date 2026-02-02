using UnityEngine;
using UnityEngine.UI;

public class DeepRecordUseSetUIUnit : MonoBehaviour
{
	[SerializeField]
	private DeepRecordUseSetUI parent;

	[SerializeField]
	private Canvas commonIconCanvas;

	[SerializeField]
	private CommonIconBase commonIcon;

	[SerializeField]
	private Text textBattle;

	[SerializeField]
	private Text textExplore;

	[SerializeField]
	private Text textAction;

	[SerializeField]
	private FillSliceImg imgFillBattle;

	[SerializeField]
	private FillSliceImg imgFillExplore;

	[SerializeField]
	private FillSliceImg imgFillAction;

	private readonly string emptyMsg = "------";

	private int characterId = -1;

	private int weaponId = -1;

	public int BattleVal { get; private set; }

	public int ExploreVal { get; private set; }

	public int ActionVal { get; private set; }

	public int TotalVal
	{
		get
		{
			return BattleVal + ExploreVal + ActionVal;
		}
	}

	public void Init()
	{
		Clear();
	}

	private void Clear()
	{
		characterId = -1;
		weaponId = -1;
		commonIconCanvas.enabled = false;
		commonIcon.Clear();
		BattleVal = 0;
		ExploreVal = 0;
		ActionVal = 0;
		textBattle.text = emptyMsg;
		textExplore.text = emptyMsg;
		textAction.text = emptyMsg;
		imgFillBattle.SetFValue(0f);
		imgFillExplore.SetFValue(0f);
		imgFillAction.SetFValue(0f);
	}

	public void OnClickUnit()
	{
		WeaponInfo value2;
		if (characterId != -1)
		{
			CharacterInfo value;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(characterId, out value))
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
				parent.RemoveSelectCharacter(value, true);
			}
		}
		else if (weaponId != -1 && ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(weaponId, out value2))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			parent.RemoveSelectWeapon(value2, true);
		}
	}

	public void UpdateCharacterInfo(int p_characterId)
	{
		if (characterId != p_characterId)
		{
			CharacterInfo value;
			if (p_characterId == -1)
			{
				Clear();
			}
			else if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(p_characterId, out value))
			{
				characterId = p_characterId;
				UpdateVal(DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.BATTLE, value), DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.EXPLORE, value), DeepRecordHelper.GetCharacterRecordVal(CharacterHelper.SortType.ACTION, value));
				commonIcon.SetupCharacter(value.netInfo);
				commonIcon.EnableIsPlayingBadge(false);
				commonIcon.EnableRedDot(false);
				commonIconCanvas.enabled = true;
			}
		}
	}

	public void UpdateWeaponInfo(int p_weaponId)
	{
		if (weaponId != p_weaponId)
		{
			WeaponInfo value;
			if (p_weaponId == -1)
			{
				Clear();
			}
			else if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(p_weaponId, out value))
			{
				weaponId = p_weaponId;
				UpdateVal(DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.BATTLE, value), DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.EXPLORE, value), DeepRecordHelper.GetWeaponRecordVal(CharacterHelper.SortType.ACTION, value));
				commonIcon.SetDeepRecordWepaonInfo(value.netInfo);
				commonIconCanvas.enabled = true;
			}
		}
	}

	private void UpdateVal(int p_battleVal, int p_exploreVal, int p_actionVal)
	{
		BattleVal = p_battleVal;
		ExploreVal = p_exploreVal;
		ActionVal = p_actionVal;
		textBattle.text = BattleVal.ToString();
		textExplore.text = ExploreVal.ToString();
		textAction.text = ActionVal.ToString();
		imgFillBattle.SetFValue(Mathf.Clamp01((float)BattleVal / (float)OrangeConst.RECORD_BATTLE_MAX));
		imgFillExplore.SetFValue(Mathf.Clamp01((float)ExploreVal / (float)OrangeConst.RECORD_EXPLORE_MAX));
		imgFillAction.SetFValue(Mathf.Clamp01((float)ActionVal / (float)OrangeConst.RECORD_ACTION_MAX));
	}
}
