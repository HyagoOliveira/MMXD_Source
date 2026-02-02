#define RELEASE
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfoCell : ScrollIndexCallback
{
	private void Start()
	{
	}

	public override void ScrollCellIndex(int p_idx)
	{
		if (ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList().Count != 0)
		{
			CommonIconBase componentInChildren = GetComponentInChildren<CommonIconBase>();
			if ((bool)componentInChildren)
			{
				Object.DestroyImmediate(componentInChildren.gameObject);
			}
			CharacterInfoSelect componentInParent = base.transform.GetComponentInParent<CharacterInfoSelect>();
			bool bSetFavorite = false;
			if (componentInParent != null)
			{
				bSetFavorite = componentInParent.IsSettinfFavorite();
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseBig", "CommonIconBaseBig", delegate(GameObject asset)
			{
				Object.Instantiate(asset, base.transform).GetComponent<CommonIconBase>().SetupCharacter(ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList()[p_idx].netInfo, p_idx, ClickImgCB, bSetFavorite);
			});
		}
	}

	private void ClickImgCB(int p_idx)
	{
		List<CharacterInfo> sortedCharacterList = ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList();
		if (sortedCharacterList == null || p_idx > sortedCharacterList.Count)
		{
			Debug.Log("Cause exception!!");
			return;
		}
		CharacterInfoSelect characterInfoSelect = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoSelect>("UI_CharacterInfo_Select");
		if (characterInfoSelect.IsSettinfFavorite())
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.ContainsKey(ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList()[p_idx].netInfo.CharacterID))
			{
				ManagedSingleton<PlayerNetManager>.Instance.CharacterFavoriteChange(ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList()[p_idx].netInfo.CharacterID, delegate
				{
					characterInfoSelect.RefreshCells();
				});
			}
			else
			{
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialog(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NOT_HAVE"), 42);
			}
			return;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		if ((bool)characterInfoSelect)
		{
			characterInfoSelect.SetActive(false);
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CharacterInfo_Main", delegate(CharacterInfoUI ui)
		{
			ui.Setup(ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList()[p_idx], p_idx);
		});
	}
}
