using System;
using UnityEngine;

public class WantedMemberSelectUnitHelper : MonoBehaviour
{
	[SerializeField]
	private CommonIconBase _characterIcon;

	public event Action OnSelectButtonClicked;

	public void Setup(WantedMemberInfo memberInfo)
	{
		if (memberInfo != null)
		{
			_characterIcon.gameObject.SetActive(true);
			_characterIcon.SetupWanted(0, memberInfo.CharacterInfo);
		}
		else
		{
			_characterIcon.gameObject.SetActive(false);
		}
	}

	public void Reset()
	{
		_characterIcon.gameObject.SetActive(false);
	}

	public void OnClickSelectButton()
	{
		Action onSelectButtonClicked = this.OnSelectButtonClicked;
		if (onSelectButtonClicked != null)
		{
			onSelectButtonClicked();
		}
	}
}
