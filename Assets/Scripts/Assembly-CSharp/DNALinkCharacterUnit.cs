using UnityEngine;

public class DNALinkCharacterUnit : ScrollIndexCallback
{
	[SerializeField]
	private CommonIconBase portraitIcon;

	[SerializeField]
	private DNALink parentUI;

	[SerializeField]
	private GameObject selectionTick;

	[SerializeField]
	private GameObject linkedByTip;

	private int _idx;

	public override void ScrollCellIndex(int p_idx)
	{
		_idx = p_idx;
		if ((bool)parentUI)
		{
			CharacterInfo characterInfo = parentUI.GetCharacterInfo(_idx);
			base.gameObject.SetActive(true);
			portraitIcon.SetupCharacter(characterInfo.netInfo, _idx, parentUI.OnClickCharacterUnit);
			portraitIcon.EnableRedDot(false);
			selectionTick.gameObject.SetActive(false);
			portraitIcon.callback += UpdateSelectionTick;
			linkedByTip.gameObject.SetActive(parentUI.GetCharacterLinkerID(characterInfo.netInfo.CharacterID) != 0);
			CharacterInfo linkerCharacterInfo = parentUI.GetLinkerCharacterInfo();
			if (linkerCharacterInfo.netDNALinkInfo != null && linkerCharacterInfo.netDNALinkInfo.LinkedCharacterID == characterInfo.netInfo.CharacterID)
			{
				parentUI.OnClickCharacterUnit(p_idx);
				UpdateSelectionTick(p_idx);
			}
		}
	}

	private void SetSelectionTick(bool bEnable)
	{
		parentUI.PlaySEOK01();
		selectionTick.gameObject.SetActive(bEnable);
		if (bEnable)
		{
			CharacterInfo characterInfo = parentUI.GetCharacterInfo(_idx);
			if (parentUI.GetCharacterLinkerID(characterInfo.netInfo.CharacterID) != 0)
			{
				parentUI.DisplayLinkerPanel();
			}
		}
	}

	public int GetIndex()
	{
		return _idx;
	}

	private void UpdateSelectionTick(int index)
	{
		if (!parentUI.IsControlLocked())
		{
			DNALinkCharacterUnit currentCharacterSelection = parentUI.GetCurrentCharacterSelection();
			if (currentCharacterSelection == null)
			{
				parentUI.SetCurrentCharacterSelection(this);
				SetSelectionTick(true);
			}
			else if (currentCharacterSelection.GetIndex() == _idx)
			{
				parentUI.SetCurrentCharacterSelection();
				SetSelectionTick(false);
			}
			else
			{
				parentUI.SetCurrentCharacterSelection(this);
				currentCharacterSelection.SetSelectionTick(false);
				SetSelectionTick(true);
			}
		}
	}
}
