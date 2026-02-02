using UnityEngine;
using UnityEngine.UI;

public class DeepRecordUseSetUITopUnit : ScrollIndexCallback
{
	[SerializeField]
	private Image imgArrow;

	[SerializeField]
	private CommonIconBase commonIcon;

	[SerializeField]
	private DeepRecordUseSetUI parent;

	[HideInInspector]
	public int NowIdx = -1;

	public override void ScrollCellIndex(int p_idx)
	{
		NowIdx = p_idx;
		switch (parent.UseSetType)
		{
		case DeepRecordUseSetUI.SetType.Character:
			SetCharacterIcon();
			break;
		case DeepRecordUseSetUI.SetType.Weapon:
			SetWeaponIcon();
			break;
		}
	}

	private void SetCharacterIcon()
	{
		CharacterInfo characterInfo = parent.ListAllCharacter[NowIdx];
		commonIcon.SetupCharacter(characterInfo.netInfo, NowIdx, OnClickIcon);
		commonIcon.EnableRedDot(false);
		commonIcon.EnableIsPlayingBadge(false);
		imgArrow.color = (parent.ListSetCharacter.Contains(characterInfo) ? Color.white : Color.clear);
	}

	private void SetWeaponIcon()
	{
		WeaponInfo weaponInfo = parent.ListAllWeapon[NowIdx];
		commonIcon.SetDeepRecordWepaonInfo(weaponInfo.netInfo, NowIdx, OnClickIcon);
		imgArrow.color = (parent.ListSetWeapon.Contains(weaponInfo) ? Color.white : Color.clear);
	}

	public void OnClickIcon(int idx)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		imgArrow.color = (parent.AddSelect(idx) ? Color.white : Color.clear);
	}

	public override void RefreshCell()
	{
		switch (parent.UseSetType)
		{
		case DeepRecordUseSetUI.SetType.Character:
			SetCharacterIcon();
			break;
		case DeepRecordUseSetUI.SetType.Weapon:
			SetWeaponIcon();
			break;
		}
	}
}
