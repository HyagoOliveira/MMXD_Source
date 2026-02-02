using CallbackDefs;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;

public class DeepRecordMoveChkUI : OrangeUIBase
{
	[SerializeField]
	private OrangeText textSelectCellName;

	[SerializeField]
	private OrangeText textSelectCellCost;

	[SerializeField]
	private WrapRectComponent textSelectCellTip;

	[SerializeField]
	private Image imgSelectCellIcon;

	[SerializeField]
	private Button btnOK;

	private int x;

	private int y;

	private Callback<ChallengeRecordGridRes> m_ChallengeCB;

	public void Setup(HexCell p_hexCell, Callback<ChallengeRecordGridRes> p_ChallengeCB)
	{
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		m_ChallengeCB = p_ChallengeCB;
		textSelectCellName.text = p_hexCell.GetCellName();
		textSelectCellCost.text = p_hexCell.GetCostStr();
		textSelectCellTip.SetText(p_hexCell.GetCellTip());
		imgSelectCellIcon.sprite = p_hexCell.GetIcon();
		x = p_hexCell.CoordinateInfo.X;
		y = p_hexCell.CoordinateInfo.Y;
		btnOK.interactable = p_hexCell.CostEnough();
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
	}

	public void OnClickCancel()
	{
		OnClickCloseBtn();
	}

	public void OnClickOK()
	{
		if (ManagedSingleton<DeepRecordHelper>.Instance.IsExpired)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowConfirmMsg("EVENT_OUTDATE", delegate
			{
				ManagedSingleton<DeepRecordHelper>.Instance.Reset();
				MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
			});
			return;
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		ManagedSingleton<DeepRecordHelper>.Instance.AddMovementInfo(x, y);
		ManagedSingleton<DeepRecordHelper>.Instance.ChallengeRecordGridReq(delegate(ChallengeRecordGridRes res)
		{
			m_ChallengeCB(res);
			base.CloseSE = SystemSE.NONE;
			OnClickCloseBtn();
		});
	}
}
