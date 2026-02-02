using System;
using UnityEngine;
using UnityEngine.UI;

public class CommonIconWanted : IconBase
{
	private string _rarityAssetNameFormat = "UI_iconsource_{0}_{1}";

	private string _bgName = "BG";

	private string _frameName = "frame";

	private string[] _rarityAssetNameProfix = new string[7] { "Dummy", "D", "C", "B", "A", "S", "SS" };

	private string _rarityImgName = "UI_Common_word_{0}";

	private string tempAssetName = string.Empty;

	[SerializeField]
	private Image imgRareBg;

	[SerializeField]
	private Transform imgStarRoot;

	[SerializeField]
	private ImageSpriteSwitcher[] imgStar;

	[SerializeField]
	private OrangeText textName;

	[SerializeField]
	private Image imgRare;

	[SerializeField]
	private Image imgRareFrame;

	[SerializeField]
	private Image imgLock;

	[SerializeField]
	private Transform tipUnlockable;

	[SerializeField]
	private Transform barUnlock;

	[SerializeField]
	private Image imgBarUnlock;

	[SerializeField]
	private OrangeText textUnlockCount;

	[SerializeField]
	private Image imgChip;

	[SerializeField]
	private Transform tipUsed;

	[SerializeField]
	private GameObject _disableMask;

	[SerializeField]
	private Toggle _toggleSelect;

	[SerializeField]
	private GameObject _selectIndex;

	[SerializeField]
	private Text _textSelectIndex;

	[SerializeField]
	private GameObject _dispatchedTag;

	private NetCharacterInfo _netCharacterInfo;

	private Action<int, bool> _onToggleValueChangedEvent;

	private void OnDestroy()
	{
		_onToggleValueChangedEvent = null;
	}

	public void Setup(NetCharacterInfo netCharacterInfo, bool isDisable, int selectedIndex, bool isDispatched, int idx = 0, Action<int, bool> onToggleValueChanded = null)
	{
		_netCharacterInfo = netCharacterInfo;
		CHARACTER_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(netCharacterInfo.CharacterID, out value))
		{
			bool flag = netCharacterInfo.State == 1;
			tempAssetName = value.s_ICON;
			SKIN_TABLE value2;
			if (netCharacterInfo.Skin > 0 && ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(netCharacterInfo.Skin, out value2))
			{
				tempAssetName = value2.s_ICON;
			}
			base.Setup(idx, AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + tempAssetName), "icon_" + tempAssetName, null, flag);
			string text = _rarityAssetNameProfix[value.n_RARITY];
			SetRareInfo(imgRareFrame, string.Format(_rarityAssetNameFormat, _frameName, text + "_L"));
			SetRareInfo(imgRareBg, string.Format(_rarityAssetNameFormat, _bgName, text), flag);
			tempAssetName = string.Format(_rarityImgName, text);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, tempAssetName, OnRaritySpriteLoaded);
			string l10nValue = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(value.w_NAME);
			string[] array = ManagedSingleton<OrangeMathf>.Instance.GetWarpString(l10nValue, textName).Split('\n');
			if (array.Length > 1)
			{
				string text2 = array[0];
				textName.text = text2.Substring(0, text2.Length - 2) + "...";
			}
			else
			{
				textName.text = l10nValue;
			}
			CheckUnlockable();
			imgStarRoot.gameObject.SetActive(flag);
			SetStar(netCharacterInfo.Star);
			_disableMask.SetActive(isDisable);
			_toggleSelect.interactable = !isDisable;
			bool flag2 = selectedIndex >= 0;
			_toggleSelect.isOn = flag2;
			_selectIndex.SetActive(flag2);
			_textSelectIndex.text = (selectedIndex + 1).ToString();
			_dispatchedTag.SetActive(isDispatched);
			_onToggleValueChangedEvent = onToggleValueChanded;
		}
	}

	private void OnRaritySpriteLoaded(Sprite sprite)
	{
		imgRare.sprite = sprite;
		imgRare.color = white;
	}

	private void CheckUnlockable()
	{
		CHARACTER_TABLE value;
		if (!(tipUnlockable == null) && ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(_netCharacterInfo.CharacterID, out value))
		{
			int itemValue = ManagedSingleton<PlayerHelper>.Instance.GetItemValue(value.n_UNLOCK_ID);
			int n_UNLOCK_COUNT = value.n_UNLOCK_COUNT;
			float x = Mathf.Clamp((float)itemValue / (float)n_UNLOCK_COUNT, 0f, 1f);
			if (_netCharacterInfo.State != 1)
			{
				tipUnlockable.gameObject.SetActive(itemValue >= n_UNLOCK_COUNT);
				barUnlock.gameObject.SetActive(true);
				imgBarUnlock.transform.localScale = new Vector3(x, 1f, 1f);
				textUnlockCount.text = string.Format("{0}/{1}", itemValue, n_UNLOCK_COUNT);
			}
			else
			{
				tipUnlockable.gameObject.SetActive(false);
				barUnlock.gameObject.SetActive(false);
			}
		}
	}

	private void SetStar(int star)
	{
		for (int i = 0; i < imgStar.Length; i++)
		{
			imgStar[i].ChangeImage((star > i) ? 1 : 0);
		}
	}

	public void OnToggleValueChanged(bool isOn)
	{
		Action<int, bool> onToggleValueChangedEvent = _onToggleValueChangedEvent;
		if (onToggleValueChangedEvent != null)
		{
			onToggleValueChangedEvent(_netCharacterInfo.CharacterID, isOn);
		}
	}
}
