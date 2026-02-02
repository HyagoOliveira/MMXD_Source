using UnityEngine;
using UnityEngine.UI;

public class ServerRangeTab : ScrollIndexCallback
{
	[SerializeField]
	private Text textLabel;

	[SerializeField]
	private Image selectedImage;

	[SerializeField]
	private GameObject buttonObject;

	[SerializeField]
	private Image indicatorImage;

	private ServerSelectMainUI parentServerSelectMainUI;

	private int tabIndex;

	private void Start()
	{
	}

	private void Update()
	{
		if (!(parentServerSelectMainUI == null))
		{
			bool flag = parentServerSelectMainUI.GetSelectedGameTabIndex() == tabIndex;
			bool flag2 = selectedImage.IsActive();
			if (flag && !flag2)
			{
				ButtonSelectionAnimation(flag);
			}
			else if (!flag && flag2)
			{
				ButtonSelectionAnimation(flag);
			}
		}
	}

	private void ButtonSelectionAnimation(bool isSelected)
	{
		float from = 0f;
		float to = 1f;
		float time = 0.1f;
		float buttonOffset = 0f;
		if (!isSelected)
		{
			from = 1f;
			to = 0f;
		}
		Vector3 tempPosition = buttonObject.transform.localPosition;
		selectedImage.gameObject.SetActive(isSelected);
		indicatorImage.gameObject.SetActive(isSelected);
		LeanTween.value(from, to, time).setOnUpdate(delegate(float val)
		{
			selectedImage.color = new Color(1f, 1f, 1f, val);
			indicatorImage.color = new Color(1f, 1f, 1f, val);
			buttonObject.transform.localPosition = new Vector3(val * buttonOffset, tempPosition.y, tempPosition.z);
		});
	}

	public override void ScrollCellIndex(int p_idx)
	{
		tabIndex = p_idx;
		parentServerSelectMainUI = GetComponentInParent<ServerSelectMainUI>();
		if (p_idx == 0)
		{
			textLabel.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("AREA_RECOMMEND");
		}
		else
		{
			textLabel.text = parentServerSelectMainUI.GetServerGameName(p_idx - 1);
		}
		buttonObject.GetComponent<Button>().onClick.AddListener(delegate
		{
			parentServerSelectMainUI.OnClickServerGameButton(p_idx);
		});
	}
}
