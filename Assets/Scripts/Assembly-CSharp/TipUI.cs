using UnityEngine;

public class TipUI : OrangeUIBase
{
	[SerializeField]
	private OrangeText textMsg;

	[SerializeField]
	private NonDrawingGraphic graphicBlock;

	[SerializeField]
	public int alertSE = 31;

	public float Delay { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Delay = 0.5f;
		base.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
	}

	public void Setup(string p_msg, bool block = false)
	{
		textMsg.text = p_msg.Replace("\\n", "\n");
		graphicBlock.raycastTarget = block;
		AnimationGroup[0].PlayAnimation(delegate
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE((SystemSE)alertSE);
			AnimationGroup[0].listAnimation[0].Delay = Delay;
			AnimationGroup[0].PlayRevertAnimation(delegate
			{
				OnClickCloseBtn();
			});
		});
	}
}
