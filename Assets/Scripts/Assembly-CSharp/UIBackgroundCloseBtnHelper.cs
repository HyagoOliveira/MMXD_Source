using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBackgroundCloseBtnHelper : MonoBehaviour
{
	private OrangeUIBase _ui;

	private Button _button;

	private void Awake()
	{
		_ui = GetComponentInParent<OrangeUIBase>();
		_button = GetComponent<Button>();
	}

	private void Start()
	{
		if (_ui != null)
		{
			_button.onClick.AddListener(OnClickButton);
		}
	}

	private void OnDestroy()
	{
		_button.onClick.RemoveAllListeners();
	}

	private void OnClickButton()
	{
		_ui.OnClickCloseBtn();
	}
}
