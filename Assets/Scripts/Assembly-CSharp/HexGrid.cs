#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeApi;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using enums;

public class HexGrid : MonoBehaviour
{
	[SerializeField]
	private HexCell cellPrefab;

	[SerializeField]
	private HexPlayerCell cellPlayerPrefab;

	[SerializeField]
	private Transform cellParent;

	[SerializeField]
	private HexMesh HexMesh;

	[SerializeField]
	private Canvas gridCanvas;

	[SerializeField]
	private HexMapCamera mapCamera;

	[SerializeField]
	private Image imgSelect;

	private Dictionary<HexCoordinates, HexCell> dictHextable = new Dictionary<HexCoordinates, HexCell>();

	private List<HexPlayerCell> listPlayerCell = new List<HexPlayerCell>();

	private RenderTextureDescriptor tRenderTextureDescriptor;

	private RenderTexture tRenderTexture;

	private int width;

	private int height;

	private HexCell[] cells;

	private Camera eventCamera;

	private HexCoordinates nowSelect = new HexCoordinates(-1, -1);

	private HexCell lastSelectCell;

	private HexCell startCell;

	private RawImage renderTarget;

	private int TempAP;

	private HexCell cacheStartCell;

	public bool IsLock { get; set; } = true;


	public bool IsMovable
	{
		get
		{
			if (lastSelectCell != null && startCell != null)
			{
				return HexMetrics.Distance(lastSelectCell.Coordinates, startCell.Coordinates) == 1;
			}
			return false;
		}
	}

	public bool IsPlayerExist
	{
		get
		{
			if (listPlayerCell.Count > 0)
			{
				return listPlayerCell[0] != null;
			}
			return false;
		}
	}

	public Camera _Camera
	{
		get
		{
			return mapCamera.Camera;
		}
	}

	public HexMapCamera.Bound Bound
	{
		get
		{
			return mapCamera._Bound;
		}
	}

	public HexPlayerCell GetCurrentPlayerCell
	{
		get
		{
			return listPlayerCell[0];
		}
	}

	public bool IsContinuousMovement { get; set; }

	public bool IsContinuousMovementApEnough { get; private set; }

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_RESOLUTION, RefreashRenderTexture);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_RESOLUTION, RefreashRenderTexture);
	}

	private void RefreashRenderTexture()
	{
		if (renderTarget == null)
		{
			return;
		}
		LeanTween.cancel(renderTarget.gameObject);
		LeanTween.delayedCall(renderTarget.gameObject, 1f, (Action)delegate
		{
			if (!(tRenderTexture == null) && !(mapCamera == null) && !(mapCamera.Camera == null))
			{
				mapCamera.Camera.targetTexture = null;
				tRenderTexture.Release();
				RenderTexture.ReleaseTemporary(tRenderTexture);
				tRenderTexture = null;
				DrawRenderer(renderTarget);
				GC.Collect();
			}
		});
	}

	public void DrawRenderer(RawImage p_renderTarget)
	{
		int num = Mathf.CeilToInt(Screen.width);
		int num2 = Mathf.CeilToInt(Screen.height);
		Debug.Log(string.Format("[HexGrid] Width:{0} , Height:{1}", num, num2));
		tRenderTextureDescriptor = new RenderTextureDescriptor(num, num2, RenderTextureFormat.Default, 24);
		tRenderTextureDescriptor.dimension = TextureDimension.Tex2D;
		tRenderTextureDescriptor.useMipMap = false;
		tRenderTextureDescriptor.sRGB = false;
		tRenderTexture = RenderTexture.GetTemporary(tRenderTextureDescriptor);
		p_renderTarget.texture = tRenderTexture;
		p_renderTarget.color = Color.white;
		mapCamera.Camera.targetTexture = tRenderTexture;
		renderTarget = p_renderTarget;
	}

	public void Setup()
	{
		RECORD_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.RECORD_TABLE_DICT.TryGetValue(ManagedSingleton<DeepRecordHelper>.Instance.MapInfo.ID, out value))
		{
			width = value.n_MAP_MAX_X;
			height = value.n_MAP_MAX_Y;
		}
		else
		{
			width = (height = 40);
		}
		ManagedSingleton<DeepRecordHelper>.Instance.InitFinishPoint(width, height);
		ManagedSingleton<DeepRecordHelper>.Instance.ListMovePoint.Clear();
		StartCoroutine(OnSetup());
	}

	private IEnumerator OnSetup()
	{
		IsLock = true;
		cells = new HexCell[width * height];
		List<NetRecordGridCoordinateInfo> points = ManagedSingleton<DeepRecordHelper>.Instance.MapInfo.GridPositionList;
		for (int i = 0; i < points.Count; i++)
		{
			CreateCell(points[i], i);
			if (i % 30 == 0)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		HexMesh.Triangulate(cells);
		NetCommonCoordinateInfo currentPositionInfo = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.CurrentPositionInfo;
		HexCoordinates key = HexCoordinates.FromOffestCoordinates(currentPositionInfo.X, currentPositionInfo.Y);
		if (!dictHextable.TryGetValue(key, out startCell))
		{
			startCell = CreateDummyCell(currentPositionInfo.X, currentPositionInfo.Y);
		}
		startCell.SetFlag(HexCell.CellStatus.CurrentPoint, true);
		foreach (HexCell value in dictHextable.Values)
		{
			value.SetFlag(HexCell.CellStatus.Moveable, HexMetrics.Distance(value.Coordinates, startCell.Coordinates) == 1);
		}
		mapCamera.Init(startCell.transform.position, width);
		SetEventCell();
		CreateOtherPlayerCell();
		CreatePlayerCell();
		imgSelect.transform.SetAsLastSibling();
		ManagedSingleton<DeepRecordHelper>.Instance.MainUI.UpdateSelectInfo(startCell);
		UnlockInput();
	}

	private void CreateCell(NetRecordGridCoordinateInfo coordinateInfo, int i)
	{
		int x = coordinateInfo.X;
		int y = coordinateInfo.Y;
		Vector3 position = HexMetrics.GetPosition(x, y);
		HexCell hexCell = (cells[i] = UnityEngine.Object.Instantiate(cellPrefab));
		hexCell.transform.SetParent(cellParent, false);
		hexCell.transform.localPosition = new Vector3(position.x, 0f, position.z);
		HexCoordinates hexCoordinates = HexCoordinates.FromOffestCoordinates(x, y);
		hexCell.Setup(hexCoordinates, coordinateInfo, gridCanvas.transform);
		dictHextable.Add(hexCoordinates, hexCell);
		mapCamera.UpdateBound(hexCell.transform.position);
	}

	private HexCell CreateDummyCell(int x, int z)
	{
		Vector3 position = HexMetrics.GetPosition(x, z);
		HexCell hexCell = (cells[ManagedSingleton<DeepRecordHelper>.Instance.MapInfo.GridPositionList.Count] = UnityEngine.Object.Instantiate(cellPrefab));
		hexCell.transform.SetParent(cellParent, false);
		hexCell.transform.localPosition = new Vector3(position.x, 0f, position.z);
		HexCoordinates p_coordinates = HexCoordinates.FromOffestCoordinates(x, z);
		hexCell.Setup(p_coordinates, null, gridCanvas.transform);
		hexCell.SetFlag(HexCell.CellStatus.Available, false);
		mapCamera.UpdateBound(hexCell.transform.position);
		return hexCell;
	}

	private void CreatePlayerCell()
	{
		NetRecordGridPlayerInfo playerInfo = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo;
		if (playerInfo != null)
		{
			int x = playerInfo.CurrentPositionInfo.X;
			int y = playerInfo.CurrentPositionInfo.Y;
			Vector3 position = HexMetrics.GetPosition(x, y);
			HexPlayerCell hexPlayerCell = UnityEngine.Object.Instantiate(cellPlayerPrefab);
			hexPlayerCell.transform.SetParent(cellParent, false);
			hexPlayerCell.transform.localPosition = new Vector3(position.x, 0f, position.z);
			hexPlayerCell.Setup(true, "self", playerInfo.CharacterList[0], gridCanvas.transform);
			hexPlayerCell.SetGlow();
			listPlayerCell.Insert(0, hexPlayerCell);
		}
	}

	private void CreateOtherPlayerCell()
	{
		List<NetRecordGridOtherPlayerInfo> otherPlayerList = ManagedSingleton<DeepRecordHelper>.Instance.OtherPlayerList;
		if (otherPlayerList == null)
		{
			return;
		}
		for (int i = 0; i < otherPlayerList.Count; i++)
		{
			if (!(GetPlayerCellById(otherPlayerList[i].PlayerID) != null))
			{
				NetRecordGridOtherPlayerInfo netRecordGridOtherPlayerInfo = otherPlayerList[i];
				int x = netRecordGridOtherPlayerInfo.CurrentPositionInfo.X;
				int y = netRecordGridOtherPlayerInfo.CurrentPositionInfo.Y;
				Vector3 position = HexMetrics.GetPosition(x, y);
				HexPlayerCell hexPlayerCell = UnityEngine.Object.Instantiate(cellPlayerPrefab);
				hexPlayerCell.transform.SetParent(cellParent, false);
				hexPlayerCell.transform.localPosition = new Vector3(position.x, 0f, position.z);
				hexPlayerCell.Setup(false, netRecordGridOtherPlayerInfo.PlayerID, netRecordGridOtherPlayerInfo.CharacterID, gridCanvas.transform);
				listPlayerCell.Add(hexPlayerCell);
			}
		}
	}

	private void LateUpdate()
	{
		if (!IsLock && Input.GetMouseButtonDown(0))
		{
			HandleInput();
		}
	}

	private void HandleInput()
	{
		DeepRecordMainUI mainUI = ManagedSingleton<DeepRecordHelper>.Instance.MainUI;
		if (!(MonoBehaviourSingleton<UIManager>.Instance.LastUI != mainUI) && (!(mainUI != null) || mainUI.AllowTouch) && !TurtorialUI.IsTutorialing())
		{
			if (!eventCamera)
			{
				eventCamera = gridCanvas.worldCamera;
			}
			RaycastHit hitInfo;
			if (Physics.Raycast(eventCamera.ScreenPointToRay(Input.mousePosition), out hitInfo))
			{
				TouchCell(hitInfo.point);
			}
		}
	}

	private void TouchCell(Vector3 position)
	{
		position = base.transform.InverseTransformPoint(position);
		HexCoordinates key = HexCoordinates.FromPosition(position);
		if (nowSelect.X == key.X && nowSelect.Z == key.Z)
		{
			ChkSelectCell();
			return;
		}
		nowSelect = key;
		HexCell value;
		if (dictHextable.TryGetValue(key, out value) && value.IsAvailable)
		{
			IsLock = true;
			if (lastSelectCell != null)
			{
				lastSelectCell.SetFlag(HexCell.CellStatus.Select, false);
			}
			lastSelectCell = value;
			lastSelectCell.SetFlag(HexCell.CellStatus.Select, true);
			if (!IsContinuousMovement)
			{
				ManagedSingleton<DeepRecordHelper>.Instance.MainUI.UpdateSelectInfo(lastSelectCell);
			}
			ChkSelectCell(true);
			imgSelect.transform.localPosition = lastSelectCell.UI.transform.localPosition;
			imgSelect.color = Color.white;
		}
	}

	public void ChkSelectCell(bool bPlaySE = false)
	{
		if (lastSelectCell == null)
		{
			UnlockInput();
		}
		else if (lastSelectCell.IsMoveable)
		{
			if (IsContinuousMovement)
			{
				DoContinuousMovement();
			}
			else if (lastSelectCell.IsFinished)
			{
				ManagedSingleton<DeepRecordHelper>.Instance.AddMovementInfo(lastSelectCell.CoordinateInfo.X, lastSelectCell.CoordinateInfo.Y);
				PlayerMove();
				if (bPlaySE)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR04);
				}
			}
			else if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastRecordMoveChk)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordMoveChk", delegate(DeepRecordMoveChkUI ui)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(UnlockInput));
					ui.Setup(lastSelectCell, ChallengeRecordGridResCB);
				});
			}
			else if (lastSelectCell.CostEnough())
			{
				ManagedSingleton<DeepRecordHelper>.Instance.AddMovementInfo(lastSelectCell.CoordinateInfo.X, lastSelectCell.CoordinateInfo.Y);
				ManagedSingleton<DeepRecordHelper>.Instance.ChallengeRecordGridReq(ChallengeRecordGridResCB);
				if (bPlaySE)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR04);
				}
			}
			else
			{
				ShowApTip();
				UnlockInput();
			}
		}
		else
		{
			UnlockInput();
			if (bPlaySE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
			}
		}
	}

	public void ShowApTip()
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowTipDialogByKey("RECORD_MESSAGE_ACTIONLCK");
	}

	private void ChallengeRecordGridResCB(ChallengeRecordGridRes res)
	{
		switch ((Code)res.Code)
		{
		case Code.RECORDGRID_CHALLENGE_SUCCESS:
		case Code.RECORDGRID_CHALLENGE_FAILURE:
			ShowChallengeResultUI(res);
			break;
		case Code.RECORDGRID_CHALLENGE_FINISHED:
			PlayerMoveCB();
			break;
		}
	}

	private void SetEventCell()
	{
		foreach (NetRecordGridMapEventInfo eventInfo in ManagedSingleton<DeepRecordHelper>.Instance.MapInfo.EventList)
		{
			HexCell hexCell;
			if (GetHexCellByPosition(eventInfo.X, eventInfo.Y, out hexCell) && !hexCell.IsFinished)
			{
				NetRecordGridOtherPlayerInfo netRecordGridOtherPlayerInfo = ManagedSingleton<DeepRecordHelper>.Instance.OtherPlayerList.FirstOrDefault((NetRecordGridOtherPlayerInfo x) => x.PlayerID == eventInfo.PlayerID);
				if (netRecordGridOtherPlayerInfo != null && hexCell.AddEventInfo(eventInfo))
				{
					HexPlayerCell hexPlayerCell = UnityEngine.Object.Instantiate(cellPlayerPrefab);
					hexPlayerCell.transform.SetParent(cellParent, false);
					hexPlayerCell.Resize(new Vector3(0.35f, 0.35f, 1f));
					hexPlayerCell.Setup(false, netRecordGridOtherPlayerInfo.PlayerID, netRecordGridOtherPlayerInfo.CharacterID, gridCanvas.transform);
					hexCell.UI.AddPlayerInfo(hexPlayerCell.Rect);
				}
			}
		}
	}

	private void ShowChallengeResultUI(ChallengeRecordGridRes res)
	{
		switch (lastSelectCell.GetRecordGridLatticeType())
		{
		case RecordGridLatticeType.Battle:
		case RecordGridLatticeType.Explore:
		case RecordGridLatticeType.Hold:
			switch ((Code)res.Code)
			{
			case Code.RECORDGRID_CHALLENGE_SUCCESS:
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordWin", delegate(DeepRecordChallengeResultUI ui)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(PlayerMoveCB));
					ui.Setup();
				});
				break;
			case Code.RECORDGRID_CHALLENGE_FAILURE:
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_DeepRecordLose", delegate(DeepRecordChallengeResultUI ui)
				{
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(PlayerMoveCB));
					ui.Setup();
				});
				break;
			}
			ManagedSingleton<DeepRecordHelper>.Instance.AddLogFlag(DeepRecordHelper.GridRefreashEvent.Battle);
			break;
		case RecordGridLatticeType.Random:
			if (res.PlayerInfo.RandomEventList.Count > 0)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TipResult", delegate(TipResultUI ui)
				{
					NetRecordGridRandomEventInfo netRecordGridRandomEventInfo = res.PlayerInfo.RandomEventList[0];
					ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(PlayerMoveCB));
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.Setup(DeepRecordHelper.GetRandomLatticeMsg(netRecordGridRandomEventInfo.ID));
				});
			}
			else
			{
				Debug.LogWarning("[HexGrid] res.PlayerInfo.RandomEventList.Count = 0");
				PlayerMove();
			}
			ManagedSingleton<DeepRecordHelper>.Instance.AddLogFlag(DeepRecordHelper.GridRefreashEvent.Random);
			break;
		case RecordGridLatticeType.Ability:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_TipResult", delegate(TipResultUI ui)
			{
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, new Callback(PlayerMoveCB));
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui.Setup(lastSelectCell.GetTableTip());
			});
			ManagedSingleton<DeepRecordHelper>.Instance.AddLogFlag(DeepRecordHelper.GridRefreashEvent.Ability);
			break;
		}
	}

	private void PlayerMoveCB()
	{
		NetCommonCoordinateInfo currentPositionInfo = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.CurrentPositionInfo;
		HexCell hexCell;
		if (GetHexCellByPosition(currentPositionInfo.X, currentPositionInfo.Y, out hexCell) && (lastSelectCell.Coordinates.X != hexCell.Coordinates.X || lastSelectCell.Coordinates.Z != hexCell.Coordinates.Z))
		{
			IsLock = true;
			lastSelectCell.RefreashFinishedFlg();
			lastSelectCell = hexCell;
			mapCamera.MoveTo(hexCell.transform.position, PlayerMove);
			OtherPlayerMove();
			SetEventCell();
		}
		else
		{
			PlayerMove();
			OtherPlayerMove();
			SetEventCell();
		}
	}

	private void PlayerMove()
	{
		listPlayerCell[0].Move(lastSelectCell);
		UpdateMoveableCell(HexMetrics.NearCoordinates(startCell.Coordinates), false);
		UpdateMoveableCell(HexMetrics.NearCoordinates(lastSelectCell.Coordinates), true);
		startCell.SetFlag(HexCell.CellStatus.CurrentPoint, false);
		startCell = lastSelectCell;
		startCell.SetFlag(HexCell.CellStatus.CurrentPoint, true);
		startCell.RefreashFinishedFlg();
		if (!IsContinuousMovement)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK15);
		}
		DeepRecordMainUI mainUI = ManagedSingleton<DeepRecordHelper>.Instance.MainUI;
		if ((bool)mainUI)
		{
			mainUI.UpdateRecordVal();
			mainUI.UpdateProgress();
			mainUI.UpdatePlayerRanking();
			mainUI.UpdateSelectInfo(startCell);
			mainUI.ResetContinuousMovemntStatus();
			ResetContinuousMovemnt();
		}
		UnlockInput();
	}

	private void OtherPlayerMove()
	{
		List<NetRecordGridOtherPlayerInfo> otherPlayerList = ManagedSingleton<DeepRecordHelper>.Instance.OtherPlayerList;
		if (otherPlayerList.Count - 1 != listPlayerCell.Count)
		{
			CreateOtherPlayerCell();
		}
		foreach (NetRecordGridOtherPlayerInfo item in otherPlayerList)
		{
			HexPlayerCell playerCellById = GetPlayerCellById(item.PlayerID);
			HexCell hexCell;
			if ((bool)playerCellById && GetHexCellByPosition(item.CurrentPositionInfo.X, item.CurrentPositionInfo.Y, out hexCell))
			{
				playerCellById.Move(hexCell);
			}
		}
	}

	private HexPlayerCell GetPlayerCellById(string playerId)
	{
		return listPlayerCell.FirstOrDefault((HexPlayerCell x) => x.PlayerId == playerId);
	}

	private bool GetHexCellByPosition(int x, int y, out HexCell hexCell)
	{
		HexCoordinates key = HexCoordinates.FromPosition(HexMetrics.GetPosition(x, y));
		return dictHextable.TryGetValue(key, out hexCell);
	}

	private void UpdateMoveableCell(HexCoordinates[] cells, bool moveable)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			HexCell value;
			if (dictHextable.TryGetValue(cells[i], out value))
			{
				value.SetFlag(HexCell.CellStatus.Moveable, moveable);
			}
		}
	}

	private void UnlockInput()
	{
		IsLock = false;
	}

	public void SetContinuousMovemntActive(bool isActive)
	{
		if (isActive)
		{
			IsContinuousMovement = true;
			cacheStartCell = startCell;
			TempAP = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ActionPoint;
			ManagedSingleton<DeepRecordHelper>.Instance.SaveMovement();
			return;
		}
		IsContinuousMovement = false;
		UpdateMoveableCell(HexMetrics.NearCoordinates(startCell.Coordinates), false);
		startCell.SetFlag(HexCell.CellStatus.CurrentPoint, false);
		HexCell value;
		if (dictHextable.TryGetValue(nowSelect, out value))
		{
			value.SetFlag(HexCell.CellStatus.Select, false);
		}
		startCell = cacheStartCell;
		lastSelectCell = startCell;
		startCell.SetFlag(HexCell.CellStatus.CurrentPoint, true);
		startCell.SetFlag(HexCell.CellStatus.Select, true);
		nowSelect = startCell.Coordinates;
		RefreashContinuousMovementInfo(false);
		ManagedSingleton<DeepRecordHelper>.Instance.ListMovePoint.Clear();
		mapCamera.Move(startCell.transform.position);
		listPlayerCell[0].Move(startCell);
		UpdateMoveableCell(HexMetrics.NearCoordinates(startCell.Coordinates), true);
		startCell.RefreashFinishedFlg();
		ResetContinuousMovemnt();
		imgSelect.transform.localPosition = lastSelectCell.UI.transform.localPosition;
		imgSelect.color = Color.white;
		UnlockInput();
	}

	public void ResetContinuousMovemnt()
	{
		IsContinuousMovement = false;
		TempAP = 0;
		cacheStartCell = null;
	}

	private void DoContinuousMovement()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK15);
		if (lastSelectCell == cacheStartCell)
		{
			RefreashContinuousMovementInfo(false);
			ManagedSingleton<DeepRecordHelper>.Instance.ListMovePoint.Clear();
		}
		else
		{
			RefreashContinuousMovementInfo(false);
			ManagedSingleton<DeepRecordHelper>.Instance.AddMovementInfo(lastSelectCell.CoordinateInfo.X, lastSelectCell.CoordinateInfo.Y);
			RefreashContinuousMovementInfo(true);
		}
		listPlayerCell[0].Move(lastSelectCell);
		UpdateMoveableCell(HexMetrics.NearCoordinates(startCell.Coordinates), false);
		UpdateMoveableCell(HexMetrics.NearCoordinates(lastSelectCell.Coordinates), true);
		startCell.SetFlag(HexCell.CellStatus.CurrentPoint, false);
		startCell = lastSelectCell;
		startCell.SetFlag(HexCell.CellStatus.CurrentPoint, true);
		int actionPoint = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ActionPoint;
		IsContinuousMovementApEnough = TempAP >= 0;
		string cost = (IsContinuousMovementApEnough ? string.Format(DeepRecordHelper.RichTextGreen, actionPoint - TempAP) : string.Format(DeepRecordHelper.RichTextRed, actionPoint - TempAP));
		ManagedSingleton<DeepRecordHelper>.Instance.MainUI.UpdateCostInfo(cost);
		UnlockInput();
	}

	private void RefreashContinuousMovementInfo(bool move)
	{
		TempAP = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.ActionPoint;
		foreach (NetCommonCoordinateInfo item in ManagedSingleton<DeepRecordHelper>.Instance.ListMovePoint)
		{
			HexCell hexCell;
			if (GetHexCellByPosition(item.X, item.Y, out hexCell))
			{
				hexCell.SetTempMoveFlag(move);
				if (move && !hexCell.IsFinished)
				{
					TempAP -= hexCell.GetCost();
				}
			}
		}
	}

	public void PlayerContinuousMovementCB()
	{
		foreach (NetCommonCoordinateInfo item in ManagedSingleton<DeepRecordHelper>.Instance.ListMovePoint)
		{
			HexCell hexCell;
			if (GetHexCellByPosition(item.X, item.Y, out hexCell))
			{
				hexCell.SetTempMoveFlag(false);
				hexCell.RefreashFinishedFlg();
			}
		}
		ManagedSingleton<DeepRecordHelper>.Instance.ListMovePoint.Clear();
		PlayerMoveCB();
	}
}
