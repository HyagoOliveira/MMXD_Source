using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class LoginBonusLoopUI : OrangeUIBase
{
	public const int BONUS_LOOP_SUB_TYPE = 0;

	public const int BONUS_LOOP_COUNT = 7;

	[SerializeField]
	private LoginBonusUnit[] unit = new LoginBonusUnit[7];

	[SerializeField]
	private UnityEngine.Transform stParent;

	[SerializeField]
	private UnityArmatureComponent getEffect;

	[SerializeField]
	private Canvas canvasTapScreen;

	private List<MISSION_TABLE> listMission;

	private int todayRewardIdx;

	private bool canClose;

	protected override void Awake()
	{
		base.Awake();
		canvasTapScreen.enabled = false;
		canClose = false;
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	public void Setup(int p_todayRewardIdx, List<MISSION_TABLE> p_listMission)
	{
		todayRewardIdx = p_todayRewardIdx % 7;
		listMission = p_listMission;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_0"), "ch_navi_0_db", delegate(GameObject obj)
		{
			StandNaviDb component = Object.Instantiate(obj, stParent, false).GetComponent<StandNaviDb>();
			if ((bool)component)
			{
				component.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL);
			}
			MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
			{
				StartCoroutine(SetUnit());
			});
		});
	}

	private IEnumerator SetUnit()
	{
		for (int i = 0; i < unit.Length; i++)
		{
			unit[i].Setup(i + 1, listMission[i].n_ITEMID_1, listMission[i].n_ITEMCOUNT_1, todayRewardIdx > i);
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.Play("NAVI_MENU", 42);
		yield return new WaitForSeconds(2f);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_STAMP01);
		float seconds = unit[todayRewardIdx].PlayStampEft(getEffect);
		yield return new WaitForSeconds(seconds);
		canClose = true;
		canvasTapScreen.enabled = true;
		yield return null;
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
