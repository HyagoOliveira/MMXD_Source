using UnityEngine;
using UnityEngine.UI;

public class BossIntroUI : OrangeUIBase
{
	[SerializeField]
	private OrangeText textBossName;

	[SerializeField]
	private OrangeText textBossDesc;

	[SerializeField]
	private Transform stParent;

	[SerializeField]
	private Image imgSbg;

	[SerializeField]
	private GameObject goInfo;

	private bool isClosed;

	public void Setup(int stageId)
	{
		STAGE_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(stageId, out value))
		{
			base.IsLock = false;
			OnClickCloseBtn();
			return;
		}
		goInfo.SetActive(true);
		if (ManagedSingleton<OrangeTableHelper>.Instance.IsNullOrEmpty(value.w_BOSS_INTRO))
		{
			return;
		}
		string p_key = "BOSS_NAME_" + value.w_BOSS_INTRO;
		textBossName.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(p_key);
		p_key = "BOSS_INTRO_" + value.w_BOSS_INTRO;
		textBossDesc.text = ManagedSingleton<OrangeTextDataManager>.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(p_key);
		p_key = "St_Enemy_" + value.w_BOSS_INTRO;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, p_key), p_key, delegate(GameObject obj)
		{
			if (obj != null)
			{
				Object.Instantiate(obj).GetComponent<StandBase>().Setup(stParent);
			}
		});
		p_key += "_ori";
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_icon_boss_intro, p_key, delegate(Sprite spr)
		{
			if (spr != null)
			{
				imgSbg.sprite = spr;
				imgSbg.color = Color.white;
			}
		});
		if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.IsActiveScene("hometop"))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP02);
			base.CloseSE = SystemSE.NONE;
		}
		else
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_BOSS03);
			base.CloseSE = SystemSE.NONE;
		}
		MonoBehaviourSingleton<CursorController>.Instance.IsEnable = true;
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<CursorController>.Instance.IsEnable = false;
	}

	public override void DoJoystickEvent()
	{
		bool isEnable = MonoBehaviourSingleton<CursorController>.Instance.IsEnable;
	}

	public override void OnClickCloseBtn()
	{
		if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.IsActiveScene("hometop"))
		{
			if (!isClosed)
			{
				isClosed = true;
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL02);
			}
		}
		else if (!isClosed)
		{
			isClosed = true;
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_BOSS04);
		}
		base.OnClickCloseBtn();
	}
}
