using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

internal class CharacterIconBase : IconBase
{
	private string rare_asset_name = "rare_{0}_{1}";

	private string bgName = "bg";

	private string frameName = "frame";

	private string small = "_small";

	[SerializeField]
	private bool SmallVer = true;

	[SerializeField]
	private Image imgRareBg;

	[SerializeField]
	private Image imgStarBg;

	[SerializeField]
	private Image[] imgStar;

	[SerializeField]
	private Image imgPlaying;

	[SerializeField]
	private Image imgTest;

	[HideIf("SmallVer")]
	[SerializeField]
	private OrangeRareText textRare;

	[HideIf("SmallVer")]
	[SerializeField]
	private Text textName;

	[ShowIf("SmallVer")]
	[SerializeField]
	private Image imgRareFrame;

	private NetCharacterInfo netCharacter;

	private CHARACTER_TABLE character;

	public void SetOtherInfo(NetCharacterInfo p_netCharacter, bool isPlaying = true, bool isTest = false)
	{
		netCharacter = p_netCharacter;
		character = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[netCharacter.CharacterID];
		if (SmallVer)
		{
			SetRareInfo(imgRareFrame, string.Format(rare_asset_name, character.n_RARITY, frameName + small));
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, character.n_RARITY, bgName + small));
		}
		else
		{
			textRare.UpdateaRare(character.n_RARITY);
			SetRareInfo(imgRareBg, string.Format(rare_asset_name, character.n_RARITY, bgName));
			textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(character.w_NAME);
		}
		imgPlaying.gameObject.SetActive(isPlaying);
		imgStarBg.gameObject.SetActive(!isTest);
		imgTest.gameObject.SetActive(isTest);
		SetStar(netCharacter.Star);
	}

	private void SetStar(int p_star)
	{
		for (int i = 0; i < imgStar.Length; i++)
		{
			if (p_star > i)
			{
				imgStar[i].color = white;
			}
		}
	}

	public void EnablePortrait(bool bEnable)
	{
		StartCoroutine(ChangePortraitColor(bEnable));
	}

	private IEnumerator ChangePortraitColor(bool bEnable)
	{
		while (imgIcon.sprite == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		if (bEnable)
		{
			imgIcon.color = Color.white;
			imgRareBg.color = Color.white;
		}
		else
		{
			imgIcon.color = Color.grey;
			imgRareBg.color = Color.grey;
		}
	}
}
