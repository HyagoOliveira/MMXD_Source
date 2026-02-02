using OrangeAudio;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoCellSmall : ScrollIndexCallback
{
	[SerializeField]
	private Image selectionFrame;

	[SerializeField]
	private CommonIconBase characterIcon;

	private int cellIndex;

	private CHARACTER_TABLE characterTable;

	private CharacterInfoUI parentCharacterInfoUI;

	private void Start()
	{
	}

	private void Update()
	{
		if ((bool)parentCharacterInfoUI)
		{
			selectionFrame.gameObject.SetActive(cellIndex == parentCharacterInfoUI.GetCurrentSelectionIndex());
		}
	}

	public override void ScrollCellIndex(int p_idx)
	{
		parentCharacterInfoUI = GetComponentInParent<CharacterInfoUI>();
		characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList()[p_idx].netInfo.CharacterID];
		cellIndex = p_idx;
		if (ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList().Count != 0)
		{
			characterIcon.SetupCharacter(ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList()[p_idx].netInfo, p_idx, ClickImgCB);
		}
	}

	private void ClickImgCB(int p_idx)
	{
		if (p_idx == -1)
		{
			return;
		}
		CharacterInfoSkill uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoSkill>("UI_CharacterInfo_Skill");
		CharacterInfoUpgrade uI2 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoUpgrade>("UI_CharacterInfo_Upgrade");
		CharacterInfoBasic uI3 = MonoBehaviourSingleton<UIManager>.Instance.GetUI<CharacterInfoBasic>("UI_CharacterInfo_Basic");
		bool flag = false;
		if (uI3 != null)
		{
			flag |= uI3.IsEffectPlaying();
		}
		if (uI != null)
		{
			flag |= uI.IsEffectPlaying();
		}
		if (uI2 != null)
		{
			flag |= uI2.bEffectLock;
		}
		if (!(flag | parentCharacterInfoUI.IsEffectPlaying()))
		{
			int currentSelectionIndex = parentCharacterInfoUI.GetCurrentSelectionIndex();
			if (p_idx != currentSelectionIndex)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
				MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK14);
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UI_CHARACTERINFO_CHARACTER_CHANGE, ManagedSingleton<CharacterHelper>.Instance.GetSortedCharacterList()[p_idx], p_idx);
			}
		}
	}
}
