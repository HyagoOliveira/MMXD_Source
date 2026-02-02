#define RELEASE
using enums;

public class EquipMainUI : OrangeUIBase
{
	private void Start()
	{
	}

	public void OnOpenWeaponUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<WeaponMainUI>("UI_WEAPONMAIN", delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		});
	}

	public void OnOpenArmorUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemBox", delegate(ItemBoxUI ui)
		{
			ui.Setup(ItemType.Consumption);
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
	}

	public void OnOpenChipUI()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<ChipMainUI>("UI_CHIPMAIN", delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
		});
	}

	public override void OnClickCloseBtn()
	{
		base.OnClickCloseBtn();
	}

	public void OnClickPetBtn()
	{
		Debug.Log("OnClickPetBtn");
		MonoBehaviourSingleton<UIManager>.Instance.NotOpenMsgUI();
	}
}
