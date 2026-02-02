#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;
using UnityEngine.UI;
using cm;
using enums;

public class PvpRoomSelectUI : OrangeUIBase
{
	private static readonly int ScrollUnit = Animator.StringToHash("ScrollUnit");

	[SerializeField]
	private Button[] btnLR;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private PvpRoomSelectUnit unit;

	[SerializeField]
	private Button btnFriendBattle;

	[SerializeField]
	private Button btnFriendBattleNew;

	private List<PvpRoomSelectUnit> listUnit = new List<PvpRoomSelectUnit>();

	private RectTransform scrollRectTransform;

	private RectTransform content;

	private bool IsNeedEndSE = true;

	private int playerRank;

	private bool isTweening;

	private Vector2 offset = new Vector2(0f, 0f);

	private Vector3 sizeSmall = new Vector3(1f, 1f, 1f);

	private Vector3 sizeBig = new Vector3(1.25f, 1.25f, 1f);

	public int NowSelectIdx { get; set; }

	private void OnBtnTypeClick(int idx)
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		IsNeedEndSE = false;
		NowSelectIdx = idx;
		OnTween();
		UpdateFriendBattleBtn();
	}

	protected override void Awake()
	{
		base.Awake();
		content = scrollRect.content;
	}

	public void Setup()
	{
		playerRank = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		List<STAGE_TABLE> listStageByType = ManagedSingleton<OrangeTableHelper>.Instance.GetListStageByType(StageType.PVP);
		for (int i = 0; i < listStageByType.Count; i++)
		{
			PvpRoomSelectUnit pvpRoomSelectUnit = UnityEngine.Object.Instantiate(unit, content, false);
			STAGE_TABLE sTAGE_TABLE = listStageByType[i];
			bool flag = false;
			flag = ((playerRank >= sTAGE_TABLE.n_RANK) ? true : false);
			pvpRoomSelectUnit.IsOpen = flag;
			pvpRoomSelectUnit.Stage = sTAGE_TABLE;
			pvpRoomSelectUnit.Setup(i, OnBtnTypeClick);
			listUnit.Add(pvpRoomSelectUnit);
		}
		StartCoroutine(OnSetup());
		IsNeedEndSE = false;
	}

	private IEnumerator OnSetup()
	{
		MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
		NowSelectIdx = 0;
		UpdateFriendBattleBtn();
		btnLR[0].onClick.AddListener(delegate
		{
			OnOffset(-1);
		});
		btnLR[1].onClick.AddListener(delegate
		{
			OnOffset(1);
		});
		scrollRect.onValueChanged.AddListener(OnScrollChange);
		scrollRect.StopMovement();
		scrollRect.enabled = false;
		yield return CoroutineDefine._waitForEndOfFrame;
		scrollRect.enabled = true;
		OnOffset(-1);
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	public void OnClickBtnCreateRoom()
	{
		PvpRoomSelectUnit pvpRoomSelectUnit = listUnit[NowSelectIdx];
		if (!pvpRoomSelectUnit.IsOpen)
		{
			ShowNotOpenMsg();
		}
		else if (pvpRoomSelectUnit.SetMatchInfoOK())
		{
			MonoBehaviourSingleton<OrangeMatchManager>.Instance.CreatePVPPrepareRoom(MonoBehaviourSingleton<OrangeMatchManager>.Instance.PvpMatchType, MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID, 0, true, "", false, OnRSCreatePVPPrepareRoom);
		}
	}

	public void OnClickJoinRandom()
	{
		PvpRoomSelectUnit pvpRoomSelectUnit = listUnit[NowSelectIdx];
		if (!pvpRoomSelectUnit.IsOpen)
		{
			ShowNotOpenMsg();
		}
		else
		{
			if (!pvpRoomSelectUnit.SetMatchInfoOK())
			{
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
			{
				ui.Setup(MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID);
				ui.bJustReturnToLastUI = true;
				ui.bUseBlackOut = true;
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					CheckSelfData();
				});
			});
		}
	}

	public void OnClickRuleBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		PvpRoomSelectUnit select = listUnit[NowSelectIdx];
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CommonScrollMsg", delegate(CommonScrollMsgUI ui)
		{
			ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
			ui.Setup(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_RULE"), ManagedSingleton<OrangeTextDataManager>.Instance.STAGETEXT_TABLE_DICT.GetL10nValue(select.Stage.w_TIP));
		});
	}

	public void OnClickFriendBattleBtn()
	{
		PvpRoomSelectUnit pvpRoomSelectUnit = listUnit[NowSelectIdx];
		if (!pvpRoomSelectUnit.IsOpen)
		{
			ShowNotOpenMsg();
		}
		else
		{
			if (!pvpRoomSelectUnit.SetMatchInfoOK())
			{
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
			{
				ui.Setup(MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID);
				ui.FriendBattleMode(true);
				ui.bJustReturnToLastUI = true;
				ui.bUseBlackOut = true;
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					CheckSelfData();
				});
			});
		}
	}

	public void OnClickFriendBattleNewBtn()
	{
		PvpRoomSelectUnit pvpRoomSelectUnit = listUnit[NowSelectIdx];
		if (!pvpRoomSelectUnit.IsOpen)
		{
			ShowNotOpenMsg();
		}
		else
		{
			if (!pvpRoomSelectUnit.SetMatchInfoOK())
			{
				return;
			}
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GoCheck", delegate(GoCheckUI ui)
			{
				ui.Setup(MonoBehaviourSingleton<OrangeMatchManager>.Instance.StageID);
				ui.FriendBattleMode(true, true);
				ui.bJustReturnToLastUI = true;
				ui.bUseBlackOut = true;
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					CheckSelfData();
				});
			});
		}
	}

	private void CheckSelfData()
	{
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Host;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Match.Port;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogin(delegate
		{
			ManagedSingleton<PlayerNetManager>.Instance.SealBattleSettingReq(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.StandbyChara, delegate(string setting)
			{
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.SelfSealedBattleSetting = setting;
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.PVPRandomMatching();
				MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
			});
		}, false);
	}

	public void CloseRoomAndRoomlist()
	{
		base.OnClickCloseBtn();
	}

	public override void OnClickCloseBtn()
	{
		if (!isTweening)
		{
			base.OnClickCloseBtn();
		}
	}

	public void OnCickRewardBtn()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>(AssetBundleScriptableObject.Instance.m_uiPath + "CommonIconBaseSmall", "CommonIconBaseSmall", delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_PvpReward", delegate(PvpRewardUI ui)
			{
				ui.Setup();
			});
		});
	}

	private void OnRSCreatePVPPrepareRoom(object res)
	{
		if (!(res is RSCreatePVEPrepareRoom))
		{
			return;
		}
		RSCreatePVEPrepareRoom rs = (RSCreatePVEPrepareRoom)res;
		if (rs.Result != 61000)
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg((Code)rs.Result, false);
			return;
		}
		NetSealBattleSettingInfo setting = null;
		if (!ManagedSingleton<PlayerHelper>.Instance.ParserUnsealedBattleSetting(rs.Unsealedbattlesetting, out setting))
		{
			MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(Code.MATCH_CREATEROOM_FAIL, false);
			return;
		}
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Host = rs.Ip;
		MonoBehaviourSingleton<OrangeMatchManager>.Instance.Port = rs.Port;
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_CoopRoomMain", delegate(CoopRoomMainUI ui)
		{
			ui.IsRoomMaster = true;
			ui.RoomId = rs.Roomid;
			ui.Setup(setting);
		});
	}

	private void OnNTStopPVPMatching(object res)
	{
		if (res is NTStopPVPMatching)
		{
			Debug.Log("OnNTStopPVPMatching : " + ((NTStopPVPMatching)res).Result);
		}
	}

	public void OnScrollChange(Vector2 p_normalizedPosition)
	{
	}

	public void OnEndDrag()
	{
		if (isTweening)
		{
			return;
		}
		float num = float.MaxValue;
		int nowSelectIdx = 0;
		Vector2 anchoredPosition = content.anchoredPosition;
		for (int i = 0; i < listUnit.Count; i++)
		{
			float num2 = Mathf.Abs(scrollRect.transform.InverseTransformPoint(listUnit[i].transform.position).x);
			if (num2 < num)
			{
				nowSelectIdx = i;
				num = num2;
			}
		}
		NowSelectIdx = nowSelectIdx;
		OnTween();
		UpdateFriendBattleBtn();
	}

	private void UpdateFriendBattleBtn()
	{
		if (listUnit.Count > NowSelectIdx)
		{
			bool active = listUnit[NowSelectIdx].Stage.n_SECRET == 4;
			if ((bool)btnFriendBattle)
			{
				btnFriendBattle.gameObject.SetActive(active);
			}
			if ((bool)btnFriendBattleNew)
			{
				btnFriendBattleNew.gameObject.SetActive(active);
			}
			if ((bool)btnFriendBattle)
			{
				btnFriendBattle.gameObject.SetActive(false);
			}
		}
	}

	private void OnOffset(int add)
	{
		if (NowSelectIdx + add > listUnit.Count - 1)
		{
			add = 0;
		}
		else if (NowSelectIdx + add < 0)
		{
			add = 0;
		}
		NowSelectIdx += add;
		OnTween();
	}

	private void OnTween()
	{
		float x = content.anchoredPosition.x;
		float x2 = ((Vector2)scrollRect.transform.InverseTransformPoint(content.position) - (Vector2)scrollRect.transform.InverseTransformPoint(listUnit[NowSelectIdx].transform.position)).x;
		float time = 0.15f;
		for (int i = 0; i < listUnit.Count; i++)
		{
			listUnit[i].transform.localScale = sizeSmall;
		}
		LeanTween.scale(listUnit[NowSelectIdx].gameObject, sizeBig, time).setEaseInOutCubic();
		if (IsNeedEndSE)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		}
		else
		{
			IsNeedEndSE = true;
		}
		LeanTween.value(base.gameObject, x, x2, time).setOnUpdate(delegate(float f)
		{
			content.anchoredPosition = new Vector2(f, 0f);
		}).setOnComplete((Action)delegate
		{
			isTweening = false;
		});
	}

	public void OnClickSE()
	{
	}

	private void ShowNotOpenMsg()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Tip", delegate(TipUI ui)
		{
			string p_msg = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_PLAYER_RANK"), listUnit[NowSelectIdx].Stage.n_RANK.ToString());
			ui.Setup(p_msg);
		});
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
