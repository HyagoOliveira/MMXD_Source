using UnityEngine;
using UnityEngine.UI;

public class HometopLockerChk : MonoBehaviour
{
	[SerializeField]
	private UIOpenChk.ChkUIEnum chk;

	[SerializeField]
	private OrangeText textParent;

	private Button btn;

	private UIOpenChk.OpenStateEnum openStateEnum;

	private int openRank;

	private Color colorKeep = Color.white;

	private void Awake()
	{
		btn = GetComponent<Button>();
		btn.onClick.AddListener(OnClickNotOpenBtn);
		colorKeep = textParent.color;
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.PLAYER_LEVEL_UP, ChkLv);
		ChkLv();
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.PLAYER_LEVEL_UP, ChkLv);
	}

	private bool ChkIsOpen()
	{
		openStateEnum = UIOpenChk.GetOpenState(chk, out openRank);
		switch (openStateEnum)
		{
		case UIOpenChk.OpenStateEnum.LOCK:
			base.gameObject.SetActive(true);
			textParent.color = Color.clear;
			return false;
		case UIOpenChk.OpenStateEnum.OPEN:
			base.gameObject.SetActive(false);
			textParent.color = colorKeep;
			return true;
		default:
			return true;
		}
	}

	private void ChkLv()
	{
		if (ChkIsOpen())
		{
			Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.PLAYER_LEVEL_UP, ChkLv);
		}
	}

	private void OnClickNotOpenBtn()
	{
		if (openStateEnum == UIOpenChk.OpenStateEnum.LOCK)
		{
			string msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), openRank);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
			{
				ui.Setup(msg);
			});
		}
	}
}
