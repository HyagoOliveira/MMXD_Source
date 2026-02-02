using System;
using System.Collections;
using System.Collections.Generic;
using Better;
using CallbackDefs;
using OrangeApi;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class DeepRecordMainUI : OrangeUIBase
{
	private readonly string bundleHexGridName = "prefab/hex/hexgrid";

	private readonly string prefabHexGridName = "HexGrid";

	[SerializeField]
	private OrangeText[] textJoinPlayerNames = new OrangeText[4];

	[SerializeField]
	private OrangeText textBattleVal;

	[SerializeField]
	private OrangeText textExploreVal;

	[SerializeField]
	private OrangeText textActionVal;

	[SerializeField]
	private OrangeText textProgress;

	[SerializeField]
	private OrangeText textSelectCellName;

	[SerializeField]
	private OrangeText textSelectCellCost;

	[SerializeField]
	private WrapRectComponent textSelectCellTip;

	[SerializeField]
	private Image imgSelectCellIcon;

	[SerializeField]
	private Image imgSelectCellIconClear;

	[SerializeField]
	private Canvas canvasArrow;

	[SerializeField]
	private RectTransform rectArrow;

	[SerializeField]
	private PlayerIconBase playerIcon;

	[SerializeField]
	private RectTransform gridParent;

	[SerializeField]
	private RawImage gridImg;

	[SerializeField]
	private Image[] imgMapArrow;

	private System.Collections.Generic.Dictionary<RecordGridLatticeType, Sprite> dictLatticeSprite = new Better.Dictionary<RecordGridLatticeType, Sprite>();

	private string[] privBGM = new string[2] { "", "" };

	private HexGrid hexGrid;

	private HexCoordinates cacheHexCoordinates = new HexCoordinates(int.MaxValue, int.MinValue);

	private int battleVal;

	private int exploreVal;

	private int actionVal;

	[SerializeField]
	private Canvas canvasContinuousMovement;

	[SerializeField]
	private Canvas canvasSelectInfo;

	[SerializeField]
	private Button btnContinuousMovementActive;

	[SerializeField]
	private Text textContinuousMovementCost;

	public bool AllowDrag { get; private set; }

	public bool AllowTouch { get; private set; } = true;


	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CHANGE_DAY, DayChange);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CHANGE_DAY, DayChange);
	}

	public void Setup()
	{
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, new Callback(Clear));
		backToHometopCB = (Callback)Delegate.Combine(backToHometopCB, (Callback)delegate
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SD_HOME_BGM);
		});
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[4]
		{
			AssetBundleScriptableObject.Instance.m_texture_ui_record,
			bundleHexGridName,
			AssetBundleScriptableObject.Instance.m_uiPath + "UI_DeepRecordWin",
			AssetBundleScriptableObject.Instance.m_uiPath + "UI_DeepRecordLose"
		}, delegate
		{
			foreach (RecordGridLatticeType value in Enum.GetValues(typeof(RecordGridLatticeType)))
			{
				Sprite assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_record, DeepRecordHelper.GetSpritesNameByLatticeType(value));
				dictLatticeSprite.Add(value, assstSync);
			}
			GameObject assstSync2 = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<GameObject>(bundleHexGridName, prefabHexGridName);
			if (assstSync2 != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(assstSync2);
				gridImg.rectTransform.sizeDelta = MonoBehaviourSingleton<UIManager>.Instance.CanvasSize;
				hexGrid = gameObject.GetComponent<HexGrid>();
				hexGrid.Setup();
				hexGrid.DrawRenderer(gridImg);
			}
		}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
		UpdateRecordVal();
		UpdateProgress();
		UpdatePlayerIcon();
		UpdateContinuousMovementBtn();
		if (!UpdatePlayerRanking() && ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			textJoinPlayerNames[ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.Rank - 1].text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
		}
		StartCoroutine(OnStartRefreashUI());
		privBGM[0] = MonoBehaviourSingleton<AudioManager>.Instance.bgmSheet;
		privBGM[1] = MonoBehaviourSingleton<AudioManager>.Instance.bgmCue;
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM02", "bgm_sys_dna");
	}

	public void Refreash()
	{
		UpdateRecordVal();
	}

	public Sprite GetHexIcon(RecordGridLatticeType latticeType)
	{
		Sprite value;
		dictLatticeSprite.TryGetValue(latticeType, out value);
		return value;
	}

	public bool UpdatePlayerRanking()
	{
		bool flag = false;
		foreach (NetRecordGridOtherPlayerInfo otherPlayer in ManagedSingleton<DeepRecordHelper>.Instance.OtherPlayerList)
		{
			string nickName = otherPlayer.NickName;
			if (!IsRankSame(textJoinPlayerNames[otherPlayer.Rank - 1].text, nickName))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			AnimationGroup[0].PlayAnimation();
			OrangeText[] array = textJoinPlayerNames;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].text = string.Empty;
			}
			foreach (NetRecordGridOtherPlayerInfo otherPlayer2 in ManagedSingleton<DeepRecordHelper>.Instance.OtherPlayerList)
			{
				textJoinPlayerNames[otherPlayer2.Rank - 1].text = otherPlayer2.NickName;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
			{
				textJoinPlayerNames[ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.Rank - 1].text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			}
		}
		return flag;
	}

	private bool IsRankSame(string oldName, string newName)
	{
		return oldName == newName;
	}

	private void UpdatePlayerIcon()
	{
		int key = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.CharacterList[0];
		CHARACTER_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(key, out value))
		{
			playerIcon.SetupByUnlockID(value.n_UNLOCK_ID);
		}
		else
		{
			playerIcon.SetupByUnlockID(-1);
		}
	}

	public void UpdateSelectInfo(HexCell hexCell)
	{
		textSelectCellName.text = hexCell.GetCellName();
		textSelectCellCost.text = hexCell.GetCostStr();
		imgSelectCellIcon.sprite = hexCell.GetIcon();
		imgSelectCellIconClear.color = (hexCell.IsFinished ? Color.white : Color.clear);
		textSelectCellTip.SetText(hexCell.GetCellTip());
		textSelectCellTip.VerticalNormalizedPosition(1f);
		textSelectCellTip.Text.alignByGeometry = false;
		textSelectCellTip.enabled = true;
		textSelectCellTip.MovementType(ScrollRect.MovementType.Elastic);
		if (cacheHexCoordinates.X != hexCell.Coordinates.X || cacheHexCoordinates.Z != hexCell.Coordinates.Z)
		{
			cacheHexCoordinates = hexCell.Coordinates;
			AnimationGroup[1].PlayAnimation();
		}
	}

	public void LateUpdate()
	{
		if (hexGrid == null || !hexGrid.IsPlayerExist)
		{
			return;
		}
		Vector3 position = hexGrid.GetCurrentPlayerCell.transform.position;
		Camera camera = hexGrid._Camera;
		Vector3 vector = camera.WorldToViewportPoint(position);
		if (vector.x > 1f || vector.y > 1f || vector.x < 0f || vector.y < 0f)
		{
			if (!canvasArrow.isActiveAndEnabled)
			{
				canvasArrow.enabled = true;
			}
			Vector2 vector2 = new Vector2(position.x, position.z) - new Vector2(camera.transform.position.x, camera.transform.position.z);
			float num = Vector3.Angle(Vector3.up, vector2);
			Vector3 lhs = Vector3.Cross(new Vector3(0f, 1f, 0f), vector2);
			num *= Mathf.Sign(Vector3.Dot(lhs, Vector3.forward));
			rectArrow.transform.eulerAngles = new Vector3(0f, 0f, num);
		}
		else if (canvasArrow.isActiveAndEnabled)
		{
			canvasArrow.enabled = false;
		}
		imgMapArrow[0].enabled = !hexGrid.Bound.IsHitLeft;
		imgMapArrow[2].enabled = !hexGrid.Bound.IsHitTop;
		imgMapArrow[3].enabled = !hexGrid.Bound.IsHitBottom;
	}

	public void UpdateRecordVal()
	{
		NetRecordGridPlayerInfo playerInfo = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo;
		textBattleVal.TweenValue(ref battleVal, playerInfo.BattlePoint);
		textExploreVal.TweenValue(ref exploreVal, playerInfo.ExplorePoint);
		textActionVal.TweenValue(ref actionVal, playerInfo.ActionPoint);
	}

	public void UpdateProgress()
	{
		textProgress.text = string.Format("{0}/{1}", ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.FinishPositionList.Count, ManagedSingleton<DeepRecordHelper>.Instance.MapInfo.GridPositionList.Count);
	}

	public void OnClickRuleBtn()
	{
		ManagedSingleton<DeepRecordHelper>.Instance.OpenRuleUI();
	}

	public void OnClickLogBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordLog", delegate(DeepRecordLogUI ui)
		{
			ui.Setup();
		});
	}

	public void OnClickRewardBtn()
	{
		ManagedSingleton<DeepRecordHelper>.Instance.OpenRewardUI();
	}

	public void OnClickTeamSetBtn()
	{
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK13);
		ManagedSingleton<DeepRecordHelper>.Instance.OpenDeepRecordTeamSetUI();
	}

	public override void OnClickCloseBtn()
	{
		if (privBGM[0] != "")
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM(privBGM[0], privBGM[1]);
		}
		Clear();
		base.OnClickCloseBtn();
	}

	private void Clear()
	{
		if (hexGrid != null && hexGrid.IsContinuousMovement)
		{
			ManagedSingleton<DeepRecordHelper>.Instance.ListMovePoint.Clear();
		}
		ManagedSingleton<DeepRecordHelper>.Instance.SaveMovement();
		ManagedSingleton<DeepRecordHelper>.Instance.MainUI = null;
		dictLatticeSprite.Clear();
		if (hexGrid != null)
		{
			UnityEngine.Object.Destroy(hexGrid.gameObject);
		}
		StopAllCoroutines();
	}

	public void OnPointerEnter()
	{
		AllowTouch = true;
	}

	public void OnPointerExit()
	{
		AllowTouch = false;
	}

	public void OnPointerDown()
	{
		AllowDrag = true;
	}

	public void OnPointerUp()
	{
		AllowDrag = false;
	}

	public override void SafeAreaChange(float x, float y)
	{
		gridParent.Left(0f - x);
		gridParent.Right(0f - x);
		gridParent.Top(0f);
		gridParent.Bottom(0f - y);
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			RefreashUI();
		}
	}

	private IEnumerator OnStartRefreashUI()
	{
		int num = 10;
		WaitForSeconds sec = new WaitForSeconds(num);
		while (true)
		{
			RefreashUI();
			yield return sec;
		}
	}

	private void RefreashUI()
	{
		if ((bool)hexGrid)
		{
			ManagedSingleton<DeepRecordHelper>.Instance.RetrieveRecordGridInfoReq(false);
		}
	}

	public void DayChange()
	{
		if ((bool)hexGrid && ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			textJoinPlayerNames[ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.Rank - 1].text = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
		}
	}

	public void UpdateContinuousMovementBtn()
	{
		if (!TurtorialUI.IsTutorialing())
		{
			btnContinuousMovementActive.gameObject.SetActive(true);
			btnContinuousMovementActive.gameObject.transform.localPosition = new Vector3(-773f, -234f);
		}
		else if (btnContinuousMovementActive.gameObject.activeSelf)
		{
			btnContinuousMovementActive.gameObject.SetActive(false);
		}
	}

	public void UpdateCostInfo(string cost)
	{
		textContinuousMovementCost.text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RECORD_SCREEN_ACTIONNEED", cost);
	}

	public void SetContinuousMovemntActive()
	{
		if (hexGrid != null && !hexGrid.IsLock)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
			hexGrid.SetContinuousMovemntActive(true);
			btnContinuousMovementActive.gameObject.SetActive(false);
			canvasContinuousMovement.enabled = true;
			canvasSelectInfo.enabled = false;
			UpdateCostInfo("0");
		}
	}

	public void SetContinuousMovemntInactive()
	{
		if (hexGrid != null && !hexGrid.IsLock)
		{
			PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_BACK01);
			hexGrid.SetContinuousMovemntActive(false);
			btnContinuousMovementActive.gameObject.SetActive(true);
			canvasContinuousMovement.enabled = false;
			canvasSelectInfo.enabled = true;
		}
	}

	public void ResetContinuousMovemntStatus()
	{
		UpdateContinuousMovementBtn();
		canvasContinuousMovement.enabled = false;
		canvasSelectInfo.enabled = true;
	}

	public void SetContinuousMovemntStart()
	{
		if (ManagedSingleton<DeepRecordHelper>.Instance.ListMovePoint.Count == 0 || !(hexGrid != null) || hexGrid.IsLock)
		{
			return;
		}
		if (!hexGrid.IsContinuousMovementApEnough)
		{
			hexGrid.ShowApTip();
			return;
		}
		PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
		hexGrid.IsLock = true;
		ManagedSingleton<DeepRecordHelper>.Instance.ChallengeMultiRecordGridReq(delegate(ChallengeMultiRecordGridRes res)
		{
			if (res.RecordGridLog.Count == 0)
			{
				if (hexGrid != null)
				{
					hexGrid.PlayerContinuousMovementCB();
				}
			}
			else
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordContinuousMovementLog", delegate(DeepRecordContinuousMovementLogUI ui)
				{
					PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
					if (hexGrid != null)
					{
						hexGrid.PlayerContinuousMovementCB();
					}
					ui.Setup();
				});
			}
		});
	}
}
