using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;

public class EmptyBlockUI : OrangeUIBase
{
	[SerializeField]
	private NonDrawingGraphic NonDrawingGraphic;

	[SerializeField]
	private Button btn;

	private bool isClick;

	public void SetBlock(bool active)
	{
		NonDrawingGraphic.raycastTarget = active;
		btn.interactable = active;
	}

	public void SetClickCB(Callback p_cb)
	{
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(delegate
		{
			if (!isClick)
			{
				isClick = true;
				p_cb.CheckTargetToInvoke();
				OnClickCloseBtn();
			}
		});
	}

	public override void OnClickCloseBtn()
	{
		StartCoroutine(OnStartDelayClose());
	}

	private IEnumerator OnStartDelayClose()
	{
		yield return CoroutineDefine._0_3sec;
		_003C_003En__0();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.OnClickCloseBtn();
	}
}
