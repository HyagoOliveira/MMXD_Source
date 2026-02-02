using UnityEngine;

public class DeepRecordTeamSetUIUnit : MonoBehaviour
{
	[SerializeField]
	private DeepRecordTeamSetUI parent;

	[SerializeField]
	private Canvas canvasIcon;

	[SerializeField]
	private CommonIconBase commonIcon;

	private DeepRecordUseSetUI.SetType setType;

	private int id = -1;

	public void Init(DeepRecordUseSetUI.SetType p_setType)
	{
		setType = p_setType;
		Clear();
	}

	public void SetCharacterIcon(CharacterInfo characterInfo)
	{
		if (id != characterInfo.netInfo.CharacterID)
		{
			id = characterInfo.netInfo.CharacterID;
			commonIcon.SetupCharacter(characterInfo.netInfo);
			commonIcon.EnableIsPlayingBadge(false);
			commonIcon.EnableRedDot(false);
			canvasIcon.enabled = true;
		}
	}

	public void SetWeaponIcon(WeaponInfo weaponInfo)
	{
		if (id != weaponInfo.netInfo.WeaponID)
		{
			id = weaponInfo.netInfo.WeaponID;
			commonIcon.SetDeepRecordWepaonInfo(weaponInfo.netInfo);
			canvasIcon.enabled = true;
		}
	}

	public void Clear()
	{
		id = -1;
		canvasIcon.enabled = false;
	}

	public void OnClickUnit()
	{
		switch (setType)
		{
		case DeepRecordUseSetUI.SetType.Character:
			parent.OnClickOpenUseSetUI_Character_Btn();
			break;
		case DeepRecordUseSetUI.SetType.Weapon:
			parent.OnClickOpenUseSetUI_Weapon_Btn();
			break;
		}
	}
}
