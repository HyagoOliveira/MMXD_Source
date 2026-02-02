using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;
using UnityEngine.UI;

public class LoginBonusSpecialUI : OrangeUIBase
{
	[SerializeField]
	private LoginBonusUnit unit;

	[SerializeField]
	private RectTransform gridParentRt;

	[SerializeField]
	private GridLayoutGroup gridLayoutGroup;

	[SerializeField]
	private L10nRawImage Fg;

	[SerializeField]
	private UnityArmatureComponent getEffect;

	[SerializeField]
	private OrangeText textTime;

	[SerializeField]
	private Image bgTime;

	[SerializeField]
	private Canvas canvasTapScreen;

	private OrangeL10nRawBg bg;

	private List<MISSION_TABLE> listMission;

	private EVENT_TABLE eventTable;

	private int todayRewardIdx;

	private float[] rectSizeY = new float[2] { 285f, 495f };

	private bool canClose;

	protected override void Awake()
	{
		base.Awake();
		canvasTapScreen.enabled = false;
		canClose = false;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void Setup(int p_todayRewardIdx, EVENT_TABLE p_evenTable, List<MISSION_TABLE> p_listMission)
	{
		todayRewardIdx = p_todayRewardIdx;
		listMission = p_listMission;
		eventTable = p_evenTable;
		Vector2 sizeDelta = gridParentRt.sizeDelta;
		gridParentRt.sizeDelta = new Vector2(sizeDelta.x, (p_listMission.Count <= 7) ? rectSizeY[0] : rectSizeY[1]);
		gridParentRt.gameObject.SetActive(false);
		Fg.Init(L10nRawImage.ImageType.Texture, eventTable.s_IMG, delegate
		{
			bg = (OrangeL10nRawBg)Background;
			bg.UpdateImg(eventTable.s_IMG + "_BG", L10nRawImage.ImageEffect.None, false);
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
			{
				StartCoroutine(SetUnit());
			});
		}, L10nRawImage.ImageEffect.None, false);
		SetTime();
	}

	private IEnumerator SetUnit()
	{
		gridParentRt.gameObject.SetActive(true);
		LoginBonusUnit target = null;
		LoginBonusUnit last = null;
		int length = listMission.Count;
		for (int i = 0; i < length; i++)
		{
			last = Object.Instantiate(unit, gridLayoutGroup.transform);
			last.Setup(i + 1, listMission[i].n_ITEMID_1, listMission[i].n_ITEMCOUNT_1, todayRewardIdx > i);
			if (i == todayRewardIdx)
			{
				target = last;
			}
			if (length > 7 && i == 6)
			{
				last.OverridBg("UI_SubCommon_ItemFrame02", "UI_SubCommon_dayBG02");
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		last.OverridBg("UI_SubCommon_ItemFrame03", "UI_SubCommon_dayBG03");
		yield return new WaitForSeconds(2f);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP01);
		float seconds = target.PlayStampEft(getEffect);
		yield return new WaitForSeconds(seconds);
		canClose = true;
		canvasTapScreen.enabled = true;
		yield return null;
	}

	private void SetTime()
	{
		if (!ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(eventTable.s_IMG2))
		{
			Sprite assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_sub_common, eventTable.s_IMG2);
			bgTime.overrideSprite = assstSync;
		}
		if (null != textTime && null != bgTime && eventTable != null)
		{
			if (OrangeGameUtility.IsResidentEvent(eventTable.s_BEGIN_TIME, eventTable.s_END_TIME))
			{
				bgTime.color = Color.clear;
				textTime.text = string.Empty;
			}
			else
			{
				bgTime.color = Color.white;
				textTime.text = OrangeGameUtility.DisplayDatePeriod(eventTable.s_BEGIN_TIME, eventTable.s_END_TIME);
			}
		}
	}

	public override void OnClickCloseBtn()
	{
		if (canClose)
		{
			canClose = false;
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
			{
				base.OnClickCloseBtn();
			});
		}
	}
}
