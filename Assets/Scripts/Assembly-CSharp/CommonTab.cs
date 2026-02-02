using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CommonTab : MonoBehaviour
{
	[Serializable]
	public class MyEventType : UnityEvent
	{
	}

	public enum TabEffectType
	{
		NONE = 0,
		LEFT = 1,
		RIGHT = 2
	}

	[SerializeField]
	private MyEventType OnClick;

	[SerializeField]
	private Image selectedImage;

	[SerializeField]
	private GameObject buttonObject;

	[SerializeField]
	private Image lockedImage;

	[SerializeField]
	private TabEffectType moveEffectType;

	private Image btnDefaultImage;

	private CommonTabParent tabParent;

	private int tabIndex;

	private void Start()
	{
		tabParent = GetComponentInParent<CommonTabParent>();
		btnDefaultImage = buttonObject.GetComponent<Image>();
	}

	private void Update()
	{
		if ((bool)tabParent)
		{
			bool flag = tabParent.GetSelectedTabIndex() == tabIndex;
			bool flag2 = selectedImage.IsActive();
			if (flag && !flag2)
			{
				ButtonSelectionAnimation(flag);
				OnClick.Invoke();
			}
			else if (!flag && flag2)
			{
				ButtonSelectionAnimation(flag);
			}
		}
	}

	public void Setup(int index, Callback p_changeSceneCallback = null)
	{
		tabIndex = index;
	}

	public void SetButtonLock(bool bLock)
	{
		if ((bool)lockedImage)
		{
			lockedImage.gameObject.SetActive(bLock);
		}
		buttonObject.GetComponent<Button>().interactable = !bLock;
	}

	private void ButtonSelectionAnimation(bool isSelected)
	{
		Vector3 localPosition = buttonObject.transform.localPosition;
		if ((bool)selectedImage)
		{
			selectedImage.gameObject.SetActive(isSelected);
		}
		if ((bool)btnDefaultImage)
		{
			btnDefaultImage.enabled = !isSelected;
		}
	}
}
