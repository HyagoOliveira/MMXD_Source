using System;
using System.Collections.Generic;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class WorldViewSceneController : OrangeSceneController
{
	public Transform cut1;

	public Transform cut2;

	public Transform cut3;

	private void Start()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM00", 2);
	}

	protected override void SceneInit()
	{
		MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
		{
			OpenDialog(1, OnCut1End);
		});
	}

	private void OnCut1End()
	{
		cut1.gameObject.SetActive(false);
		cut2.gameObject.SetActive(true);
		OpenDialog(11, OnCut2End);
	}

	private void OnCut2End()
	{
		cut2.gameObject.SetActive(false);
		cut3.gameObject.SetActive(true);
		OpenDialog(21, OnCut3End);
	}

	private void OnCut3End()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_EmptyBlock", delegate(EmptyBlockUI blockUI)
		{
			blockUI.SetBlock(true);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_0"), "ch_navi_0_db", delegate(GameObject obj)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(obj, blockUI.transform, false);
				StandNaviDb component = gameObject.GetComponent<StandNaviDb>();
				if ((bool)component)
				{
					component.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL);
					gameObject.transform.localPosition = new Vector3(0f, -1000f, 0f);
					LeanTween.moveLocalY(gameObject, 175f, 1f).setEaseInOutBack().setOnComplete((Action)delegate
					{
						OpenDialog(31, OnCut4End);
					});
				}
				else
				{
					OpenDialog(31, OnCut4End);
				}
			});
		});
	}

	private void OnCut4End()
	{
		EmptyBlockUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<EmptyBlockUI>("UI_EmptyBlock");
		if ((bool)uI)
		{
			uI.OnClickCloseBtn();
		}
		GoToStage();
	}

	private void OpenDialog(int scenarioId, Callback p_dialogCloseCB)
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Dialog", delegate(DialogUI ui)
		{
			ui.Setup(scenarioId, delegate
			{
				if (ui.logState == DialogUI.LogState.SKIP)
				{
					GoToStage();
				}
				else
				{
					p_dialogCloseCB.CheckTargetToInvoke();
				}
			});
		});
	}

	private void GoToStage()
	{
		List<string> listExtraLoadingAssets = MonoBehaviourSingleton<OrangeSceneManager>.Instance.ListExtraLoadingAssets;
		listExtraLoadingAssets.Add("prefab/bullet/p_chargeshot_000_f");
		listExtraLoadingAssets.Add("prefab/bullet/p_chargeshot_001_f");
		listExtraLoadingAssets.Add("prefab/bullet/p_chargeshot_002_f");
		listExtraLoadingAssets.Add("prefab/bullet/p_missile_002_f");
		listExtraLoadingAssets.Add("model/animation/buster/m");
		listExtraLoadingAssets.Add("model/animation/dualgun/m");
		listExtraLoadingAssets.Add("model/animation/gatling/m");
		listExtraLoadingAssets.Add("model/animation/launcher/m");
		listExtraLoadingAssets.Add("model/animation/mgun/m");
		listExtraLoadingAssets.Add("model/animation/saber/m");
		listExtraLoadingAssets.Add("model/animation/sprayheavy/m");
		listExtraLoadingAssets.Add("prefab/shadowprojector");
		listExtraLoadingAssets.Add("prefab/aimicon2");
		listExtraLoadingAssets.Add("prefab/fx/distortionfx");
		listExtraLoadingAssets.Add("prefab/fx/obj_player_die");
		listExtraLoadingAssets.Add("prefab/virtualpadsystem1");
		listExtraLoadingAssets.Add("ui/ui_stagereward");
		ManagedSingleton<StageHelper>.Instance.nLastStageID = 111;
		string p_stageKey = "stage01_0101_e1";
		ManagedSingleton<PlayerNetManager>.Instance.StageStartReq(ManagedSingleton<StageHelper>.Instance.nLastStageID, p_stageKey, ManagedSingleton<StageHelper>.Instance.GetStageCrc(ManagedSingleton<StageHelper>.Instance.nLastStageID), delegate(string S_STAGE)
		{
			StageUpdate.SetStageName(S_STAGE);
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("StageTest", OrangeSceneManager.LoadingType.STAGE, null, false);
		});
	}
}
