using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemBoxTab : MonoBehaviour
{
	[SerializeField]
	private Button targetButton;

	[SerializeField]
	private OrangeText targetText;

	[SerializeField]
	private Sprite btnEnableSprite;

	[SerializeField]
	private Sprite btnDisableSprite;

	[SerializeField]
	private Color fontEnableColor;

	[SerializeField]
	private Color fontDisableColor;

	[SerializeField]
	private Vector2 EnableSize = new Vector2(0.98f, 0.98f);

	[SerializeField]
	private Vector2 DisableSize = new Vector2(1f, 1f);

	private RectTransform rt;

	private void Awake()
	{
		rt = GetComponent<RectTransform>();
		if (targetButton != null)
		{
			ColorBlock colors = targetButton.colors;
			colors.disabledColor = Color.white;
			targetButton.colors = colors;
		}
	}

	public void UpdateState(bool isEnable)
	{
		targetButton.interactable = isEnable;
		if (isEnable)
		{
			targetButton.image.sprite = btnEnableSprite;
			targetText.color = fontEnableColor;
			rt.localScale = EnableSize;
		}
		else
		{
			targetButton.image.sprite = btnDisableSprite;
			targetText.color = fontDisableColor;
			rt.localScale = DisableSize;
		}
	}

	public void AddBtnCB(UnityAction action)
	{
		targetButton.onClick.RemoveAllListeners();
		targetButton.onClick.AddListener(action);
	}

	public void SetTextStr(string str)
	{
		targetText.text = str;
	}
}
