using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class CommonSubMenu : MonoBehaviour
{
	[SerializeField]
	public GameObject content;

	[SerializeField]
	public Button[] subButtons;

	private void Awake()
	{
	}

	private void Start()
	{
		GetComponent<Button>().onClick.AddListener(OnClickBG);
	}

	private void OnDestroy()
	{
	}

	public void OnClickBG()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
		base.gameObject.SetActive(false);
	}

	public void OnShowSubMenu(Vector3 wposition)
	{
		base.gameObject.SetActive(true);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		RectTransform component = content.GetComponent<RectTransform>();
		base.transform.parent.GetComponent<RectTransform>();
		wposition.x = -310f;
		wposition.y = 0f;
		component.anchoredPosition = wposition;
	}
}
