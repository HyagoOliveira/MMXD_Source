using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterInfoPageTab : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private Image imgSelectionIndicator;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OnSelect(BaseEventData eventData)
	{
		imgSelectionIndicator.enabled = true;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		imgSelectionIndicator.enabled = false;
	}
}
