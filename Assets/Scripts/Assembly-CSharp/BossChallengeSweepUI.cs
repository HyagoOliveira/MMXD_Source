#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class BossChallengeSweepUI : OrangeUIBase
{
	private const int visualCount = 8;

	[SerializeField]
	private BossChallengeUnit[] arrayUnit = new BossChallengeUnit[8];

	[SerializeField]
	private Toggle[] selectionToggle = new Toggle[8];

	[SerializeField]
	private OrangeText[] sweepCount = new OrangeText[8];

	[SerializeField]
	private GameObject arrowNext;

	[SerializeField]
	private GameObject arrowPrevious;

	[SerializeField]
	private OrangeText textPhase;

	[SerializeField]
	private StarClearComponent phase;

	[SerializeField]
	private Button sweepBtn;

	[SerializeField]
	private OrangeText sweepBtnText;

	[SerializeField]
	private OrangeText requiredAP;

	[SerializeField]
	private OrangeText currentAP;

	private string phaseFormat = "Phase {0}";

	private List<STAGE_TABLE> listStage = new List<STAGE_TABLE>();

	private int nowPage;

	private int maxPage;

	private HashSet<int> sweepStageIDHash = new HashSet<int>();

	private bool bBlock;

	private bool bNotEnoughAP;

	private List<NetRewardInfo> rewardList = new List<NetRewardInfo>();

	private int sweepReqSent;

	public void Setup()
	{
		listStage = ManagedSingleton<OrangeTableHelper>.Instance.GetListStageByType(StageType.BossChallenge);
		maxPage = ((listStage.Count % 8 == 0) ? (listStage.Count / 8 - 1) : (listStage.Count / 8));
		sweepStageIDHash = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HashBossSweepSelection;
		Debug.Log("HashBossSweepSelection size = " + MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HashBossSweepSelection.Count + ", listStage size = " + listStage.Count);
		if (sweepStageIDHash.Count > listStage.Count)
		{
			sweepStageIDHash.Clear();
			Debug.Log("Excess data detected.");
		}
		CheckInvalidStageID();
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CHARGE_STAMINA, UpdateSweepInfo);
		StartCoroutine(OnsSetUnit());
	}

	private void CheckInvalidStageID()
	{
		foreach (int stageID in sweepStageIDHash.ToList())
		{
			if (!listStage.Exists((STAGE_TABLE x) => x.n_ID == stageID))
			{
				Debug.Log("Invalid or obsolete stage id detected.");
				sweepStageIDHash.Remove(stageID);
			}
		}
	}

	private int GetStageClearCount(int stageID)
	{
		StageInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(stageID, out value))
		{
			return value.netStageInfo.ClearCount;
		}
		return 0;
	}

	private int GetStageMaxPlayCount(int stageID)
	{
		STAGE_TABLE value = null;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(stageID, out value))
		{
			if (value.n_PLAY_COUNT >= 0)
			{
				return value.n_PLAY_COUNT;
			}
			return 0;
		}
		return 0;
	}

	private int GetStageSweepLeft(int stageID)
	{
		return GetStageMaxPlayCount(stageID) - GetStageClearCount(stageID);
	}

	public void AddToSweepList(int index)
	{
		int index2 = index + nowPage * 8;
		int n_ID = listStage[index2].n_ID;
		if (selectionToggle[index].isOn)
		{
			if (!sweepStageIDHash.Contains(n_ID))
			{
				sweepStageIDHash.Add(n_ID);
			}
		}
		else if (sweepStageIDHash.Contains(n_ID))
		{
			sweepStageIDHash.Remove(n_ID);
		}
		UpdateSweepInfo();
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.HashBossSweepSelection = sweepStageIDHash;
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CHARGE_STAMINA, UpdateSweepInfo);
	}

	public override void OnClickCloseBtn()
	{
		if (!bBlock)
		{
			base.OnClickCloseBtn();
		}
	}

	private bool HasNextPage()
	{
		if (nowPage + 1 <= maxPage)
		{
			return true;
		}
		return false;
	}

	private bool HasLastPage()
	{
		if (nowPage - 1 >= 0)
		{
			return true;
		}
		return false;
	}

	public void OnClickPage(int add)
	{
		if (nowPage + add <= maxPage && nowPage + add >= 0)
		{
			nowPage += add;
			StartCoroutine(OnsSetUnit());
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		}
	}

	private IEnumerator OnsSetUnit()
	{
		ManagedSingleton<PlayerHelper>.Instance.GetLV();
		int num = 0;
		StageInfo value = null;
		textPhase.text = string.Format(phaseFormat, nowPage + 1);
		arrowPrevious.SetActive(HasLastPage());
		arrowNext.SetActive(HasNextPage());
		phase.SetActiveStar(nowPage + 1);
		for (int i = nowPage * 8; i < nowPage * 8 + 8; i++)
		{
			bool flag = listStage.Count > i;
			selectionToggle[num].gameObject.SetActive(flag);
			arrayUnit[num].gameObject.SetActive(flag);
			if (flag)
			{
				int starCount = 0;
				bool canChallenge = false;
				int num2 = listStage[i].n_PLAY_COUNT;
				int n_ID = listStage[i].n_ID;
				selectionToggle[num].isOn = sweepStageIDHash.Contains(n_ID);
				if (ManagedSingleton<PlayerNetManager>.Instance.dicStage.TryGetValue(n_ID, out value))
				{
					num2 = listStage[i].n_PLAY_COUNT - value.netStageInfo.ClearCount;
					canChallenge = num2 > 0;
					starCount = ManagedSingleton<StageHelper>.Instance.GetStarAmount(value.netStageInfo.Star);
					canChallenge = canChallenge && starCount >= 3;
				}
				if (num2 == 0)
				{
					sweepCount[num].text = "<color=#FF0000>" + num2 + "</color>/" + listStage[i].n_PLAY_COUNT;
				}
				else
				{
					sweepCount[num].text = "<color=#B6E5FF>" + num2 + "</color>/" + listStage[i].n_PLAY_COUNT;
				}
				int tmpIdx = num;
				arrayUnit[num].Setup(i, starCount, canChallenge, delegate
				{
					if (starCount < 3)
					{
						MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI tipUI)
						{
							tipUI.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_SWEEP_STAR"), true);
						});
					}
					else if (!bBlock)
					{
						PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
						selectionToggle[tmpIdx].isOn = !selectionToggle[tmpIdx].isOn;
						AddToSweepList(tmpIdx);
					}
				});
			}
			else
			{
				selectionToggle[num].gameObject.SetActive(false);
				arrayUnit[num].Setup(i, 0, false, null);
				arrayUnit[num].gameObject.SetActive(false);
			}
			arrayUnit[num].IgonreTween();
			num++;
		}
		UpdateSweepInfo();
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
		yield return null;
	}

	public void PlayToggleSE()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
	}

	private void UpdateSweepInfo()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (int item in sweepStageIDHash)
		{
			STAGE_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(item, out value))
			{
				num3 = GetStageSweepLeft(item);
				num2 += num3;
				num += value.n_AP * num3;
			}
		}
		sweepBtnText.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNTION_MULTI_SWEEP"), num2);
		requiredAP.text = num.ToString();
		currentAP.text = ManagedSingleton<PlayerHelper>.Instance.GetStamina().ToString();
		bNotEnoughAP = num > ManagedSingleton<PlayerHelper>.Instance.GetStamina();
		requiredAP.color = (bNotEnoughAP ? Color.red : Color.white);
		sweepBtn.interactable = !bNotEnoughAP && num2 > 0;
	}

	private IEnumerator WaitForSweepResult()
	{
		while (sweepReqSent > 0)
		{
			yield return CoroutineDefine._0_3sec;
		}
		Debug.Log("All response received!");
		if (rewardList.Count > 0)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_RewardPopup", delegate(RewardPopopUI ui)
			{
				if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsLvUp)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
					{
						MonoBehaviourSingleton<OrangeGameManager>.Instance.DisplayLvPerform();
					});
				}
				ui.Setup(rewardList);
				StartCoroutine(OnsSetUnit());
			});
		}
		UpdateSweepInfo();
		bBlock = false;
	}

	public void OnClickSweepBtn()
	{
		if (sweepStageIDHash.Count == 0)
		{
			return;
		}
		if (bNotEnoughAP)
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.YesSE = SystemSE.NONE;
				ui.SetupYesNoByKey("COMMON_TIP", "STAMINA_OUT", "COMMON_OK", "COMMON_CANCEL", delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ChargeStamina", delegate(ChargeStaminaUI newUI)
					{
						newUI.Setup(ChargeType.ActionPoint);
					});
				});
			});
			return;
		}
		foreach (int item in sweepStageIDHash)
		{
			STAGE_TABLE stage;
			if (!ManagedSingleton<OrangeTableHelper>.Instance.GetStage(item, out stage) || stage.n_TYPE != 3)
			{
				return;
			}
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		bBlock = true;
		sweepBtn.interactable = false;
		int num = 0;
		rewardList.Clear();
		foreach (int item2 in sweepStageIDHash)
		{
			num = GetStageSweepLeft(item2);
			if (num == 0)
			{
				continue;
			}
			sweepReqSent++;
			Debug.Log("Send sweepReqSent = " + sweepReqSent);
			MonoBehaviourSingleton<OrangeGameManager>.Instance.StageSweepReq(item2, num, delegate(object res)
			{
				(res as NetRewardsEntity).RewardList.ForEach(delegate(NetRewardInfo p)
				{
					rewardList.Add(p);
				});
				sweepReqSent--;
				Debug.Log("Received sweepReqSent = " + sweepReqSent);
			});
		}
		StartCoroutine(WaitForSweepResult());
	}
}
