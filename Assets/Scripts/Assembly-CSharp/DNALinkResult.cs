#define RELEASE
using UnityEngine;

public class DNALinkResult : OrangeUIBase
{
	[SerializeField]
	private GameObject[] skillIconPos;

	[SerializeField]
	private GameObject skillBtnDecorator;

	private CharacterInfo _characterInfo;

	public void Setup(CharacterInfo characterInfo)
	{
		_characterInfo = characterInfo;
		int linkedCharacterID = _characterInfo.netDNALinkInfo.LinkedCharacterID;
		if (_characterInfo.netDNALinkInfo == null)
		{
			Debug.Log("_characterInfo.netDNALinkInfo is null.");
			return;
		}
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		CharacterInfo linkedCharacterInfo = null;
		ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(linkedCharacterID, out linkedCharacterInfo);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("ui/skillbutton", "SkillButton", delegate(Object asset)
		{
			GameObject original = asset as GameObject;
			for (int i = 0; i < _characterInfo.netDNALinkInfo.LinkedSlotID.Count; i++)
			{
				int key = _characterInfo.netDNALinkInfo.LinkedSlotID[i];
				int skillID = linkedCharacterInfo.netDNAInfoDic[key].SkillID;
				GameObject obj = Object.Instantiate(original, skillIconPos[i].transform, false);
				GameObject gameObject = Object.Instantiate(skillBtnDecorator, skillIconPos[i].transform, false);
				SkillButtonDecorator component = gameObject.GetComponent<SkillButtonDecorator>();
				SkillButton component2 = obj.GetComponent<SkillButton>();
				component2.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
				component2.Setup(skillID, SkillButton.StatusType.DEFAULT);
				gameObject.SetActive(true);
				component.Setup(SkillButtonDecorator.StyleType.UNLOCKED);
			}
		});
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void OnClickRelink()
	{
		base.CloseSE = SystemSE.NONE;
		OnClickCloseBtn();
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DNALink", delegate(DNALink ui)
		{
			ui.Setup(_characterInfo);
		});
	}
}
