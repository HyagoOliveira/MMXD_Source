#define RELEASE
using UnityEngine;

public class FastTest : MonoBehaviour
{
	[SerializeField]
	private CharacterIconBase[] characterIconBase;

	[SerializeField]
	private CharacterIconBase characterIconBaseSmall;

	[SerializeField]
	private WeaponIconBase[] weaponIconBase;

	[SerializeField]
	private WeaponIconBase weaponIconBaseSmall;

	[SerializeField]
	private OrangeRareText[] orangeRareText = new OrangeRareText[7];

	private void Start()
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[1] { AssetBundleScriptableObject.Instance.m_icon_rare }, delegate
		{
			characterIconBase[0].Setup(0, "texture/prototype/sampleicon", "character_sample01");
			characterIconBase[0].SetOtherInfo(GetFakeCharacter(1, 2), false);
			characterIconBase[1].Setup(0, "texture/prototype/sampleicon", "character_sample02");
			characterIconBase[1].SetOtherInfo(GetFakeCharacter(2, 5), true, true);
			characterIconBaseSmall.Setup(0, "texture/prototype/sampleicon", "character_sample03");
			characterIconBaseSmall.SetOtherInfo(GetFakeCharacter(3, 5), false);
			weaponIconBase[0].Setup(0, "texture/prototype/sampleicon", "weapon_sample01");
			weaponIconBase[0].SetOtherInfo(GetFakeWeapon(1, 2, 123456, 0), WeaponIconBase.WeaponEquipType.Main);
			weaponIconBase[1].Setup(0, "texture/prototype/sampleicon", "weapon_sample02");
			weaponIconBase[1].SetOtherInfo(GetFakeWeapon(2, 3, 999999, 300), WeaponIconBase.WeaponEquipType.UnEquip);
			weaponIconBaseSmall.Setup(0, "texture/prototype/sampleicon", "weapon_sample03");
			weaponIconBaseSmall.SetOtherInfo(GetFakeWeapon(1, 2, 1234, 0), WeaponIconBase.WeaponEquipType.Sub);
		});
		for (int i = 0; i < orangeRareText.Length; i++)
		{
			orangeRareText[i].UpdateaRare(i);
		}
	}

	private void LoadCommonUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
		{
			ui.SetupConfirm("測試", "測試文字", MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
			{
				Debug.Log("callbackkkkkk");
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui2)
				{
					ui2.SetupYesNO("測試2", "測試文字2", MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_YES"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_NO"), null);
				});
			});
		});
	}

	private NetCharacterInfo GetFakeCharacter(int characterId, sbyte star)
	{
		return new NetCharacterInfo
		{
			CharacterID = characterId,
			Star = star
		};
	}

	private NetWeaponInfo GetFakeWeapon(int weaponId, byte star, int exp, int prof)
	{
		return new NetWeaponInfo
		{
			WeaponID = weaponId,
			Star = star,
			Exp = exp,
			Prof = prof
		};
	}
}
