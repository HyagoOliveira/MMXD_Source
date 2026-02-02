using System;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class PvpRoomSelectUnit : MonoBehaviour
{
	[SerializeField]
	private Image[] imgPvpVisual;

	[SerializeField]
	private Image imgPvpAmount;

	[SerializeField]
	private Canvas canvasLock;

	[SerializeField]
	private OrangeText textStageName;

	private int idx;
    [Obsolete]
    private CallbackIdx clickCb;

	public STAGE_TABLE Stage { get; set; }

	public bool IsOpen { get; set; }

	private void Awake()
	{
		Stage = null;
		IsOpen = false;
	}

    [Obsolete]
    public void Setup(int p_idx, CallbackIdx p_cb)
	{
		idx = p_idx;
		canvasLock.enabled = !IsOpen;
		textStageName.text = ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(Stage.w_NAME);
		Image[] array = imgPvpVisual;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>("ui/ui_pvproomselect", Stage.s_BG);
		}
		imgPvpAmount.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>("ui/ui_pvproomselect", Stage.s_ICON);
		clickCb = p_cb;
	}

	public bool SetMatchInfoOK()
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = PVPMatchType.None;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType = PVPGameType.None;
		if (Stage == null)
		{
			return false;
		}
		string[] array = Stage.s_PATH.Split(',');
		if (array.Length < 2)
		{
			return false;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID = Stage.n_ID;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType = (PVPMatchType)Enum.Parse(typeof(PVPMatchType), array[0]);
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpGameType = (PVPGameType)Enum.Parse(typeof(PVPGameType), array[1]);
		return true;
	}

	public void OnClickUnit()
	{
		clickCb.CheckTargetToInvoke(idx);
	}
}
