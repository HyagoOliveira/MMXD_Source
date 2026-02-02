using UnityEngine;
using UnityEngine.UI;

public class MailSelectCell : ScrollIndexCallback
{
	[SerializeField]
	private Text textMailTitle;

	[SerializeField]
	private Text textMailDeliveryTime;

	[SerializeField]
	private Text textMailReservedTime;

	[SerializeField]
	private Text textMailRemainTime;

	[HideInInspector]
	public MailSelectUI Parent;

	private int listIndex;

	private int mailID;

	public override void ScrollCellIndex(int p_idx)
	{
		listIndex = p_idx;
		MailInfo mailInfo = ManagedSingleton<MailHelper>.Instance.GetMailInfo(listIndex);
		if (mailInfo != null)
		{
			mailID = mailInfo.netMailInfo.MailID;
			string p_key = string.Format("TITLE_{0}", mailInfo.netMailInfo.TitleID);
			string text = OrangeGameUtility.GetOperationText(mailInfo.netMailInfo.TitleID);
			if (string.IsNullOrEmpty(text))
			{
				text = ManagedSingleton<OrangeTextDataManager>.Instance.MAILTEXT_TABLE_DICT.GetL10nValue(p_key);
			}
			textMailTitle.text = text;
			textMailDeliveryTime.text = CapUtility.UnixTimeToDate(mailInfo.netMailInfo.DeliveryTime).ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss tt");
			textMailRemainTime.text = OrangeGameUtility.GetRemainTimeText(mailInfo.netMailInfo.RecycleTime);
			if (mailInfo.netMailInfo.ReservedTime == 0)
			{
				textMailReservedTime.text = "";
			}
			else
			{
				textMailReservedTime.text = CapUtility.UnixTimeToDate(mailInfo.netMailInfo.ReservedTime).ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss tt");
			}
		}
	}

	public void OnClickMailOpenBtn()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_MailDetail", delegate(MailDetailUI ui)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			MailInfo mailInfo = ManagedSingleton<MailHelper>.Instance.GetMailInfo(listIndex);
			if (mailInfo != null)
			{
				if (mailInfo.netMailInfo.ReservedTime == 0)
				{
					ui.Setup(mailID, true);
				}
				else
				{
					ui.Setup(mailID, false);
				}
			}
		});
	}
}
