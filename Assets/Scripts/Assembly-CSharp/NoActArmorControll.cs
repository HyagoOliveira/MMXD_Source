using System;
using System.Collections;
using System.Collections.Generic;
using StageLib;
using UnityEngine;

public class NoActArmorControll : RideBaseObj, IManagedUpdateBehavior, IManagedLateUpdateBehavior, IAimTarget, ILogicUpdate
{
	public const string NowStatusStr = "NowStatus";

	protected VEHICLE_TABLE tVEHICLE_TABLE;

	public float distanceDelta;

	protected VInt3 _velocity;

	protected VInt3 _velocityExtra;

	protected VInt3 _velocityShift;

	private float _moveSpeedMultiplier = 1f;

	private int nForwardSpeed = 5000;

	private int nBackSpeed = 5000;

	private int nUpSpeed = 5000;

	private int nDownSpeed = 5000;

	private Vector3 vMoveTmpDis;

	private float fTransfromDis = 2.5f;

	private ModelTransform tModelTransform;

	private Animator tAnimator;

	private int NowStatus;

	private float fSizeH = 0.3f;

	private List<Transform> listIgnoreTrans = new List<Transform>();

	private ParticleSystem efx_dash_root;

	private bool RUSH04_LP_play;

	private Renderer rushRender;

	private bool bCheck1 = true;

	private bool bCheck2 = true;

	private RaycastHit2D rObjHit;

	protected void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	protected void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	private void Start()
	{
		if (!ManagedSingleton<OrangeDataManager>.Instance.VEHICLE_TABLE_DICT.TryGetValue(nRideID, out tVEHICLE_TABLE))
		{
			tVEHICLE_TABLE = new VEHICLE_TABLE();
			tVEHICLE_TABLE.n_SPEED = 100;
			tVEHICLE_TABLE.n_HP = 100;
		}
		Hp = tVEHICLE_TABLE.n_HP;
		Controller = GetComponent<Controller2D>();
		Collider2D[] componentsInChildren = base.transform.GetComponentsInChildren<Collider2D>();
		foreach (Collider2D collider2D in componentsInChildren)
		{
			listIgnoreTrans.Add(collider2D.transform);
		}
		tModelTransform = base.transform.GetComponentInChildren<ModelTransform>();
		tAnimator = base.transform.GetComponentInChildren<Animator>();
		selfBuffManager.Init(this);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_transform_vh003", 2);
		if (tModelTransform != null)
		{
			nForwardSpeed = (int)((float)(tModelTransform.nForwardSpeed * tVEHICLE_TABLE.n_SPEED) / 100f);
			nBackSpeed = (int)((float)(tModelTransform.nBackSpeed * tVEHICLE_TABLE.n_SPEED) / 100f);
			nUpSpeed = (int)((float)(tModelTransform.nUpSpeed * tVEHICLE_TABLE.n_SPEED) / 100f);
			nDownSpeed = (int)((float)(tModelTransform.nDownSpeed * tVEHICLE_TABLE.n_SPEED) / 100f);
			fTransfromDis = tModelTransform.fTransfromDis;
		}
		SwitchModelByStatus(0);
		if (tVEHICLE_TABLE.n_ID == 4)
		{
			Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
			Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "efx_dash_root");
			if ((bool)transform)
			{
				efx_dash_root = transform.GetComponent<ParticleSystem>();
			}
		}
		StartCoroutine(CheckMainPlayerSize());
	}

	private IEnumerator CheckMainPlayerSize()
	{
		OrangeCharacter tOC = null;
		while (tOC == null)
		{
			tOC = StageUpdate.GetMainPlayerOC();
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		float num = 9999999f;
		float num2 = -9999999f;
		Collider2D[] componentsInChildren;
		for (int num3 = base.transform.childCount - 1; num3 >= 0; num3--)
		{
			componentsInChildren = base.transform.GetChild(num3).GetComponentsInChildren<Collider2D>();
			foreach (Collider2D collider2D in componentsInChildren)
			{
				if (collider2D.bounds.max.x > num2)
				{
					num2 = collider2D.bounds.max.x;
				}
				if (collider2D.bounds.min.x < num)
				{
					num = collider2D.bounds.min.x;
				}
			}
		}
		bool flag = tOC.Controller.Collider2D.enabled;
		tOC.Controller.Collider2D.enabled = true;
		float num4 = (tOC.Controller.Collider2D.bounds.max.x - tOC.Controller.Collider2D.bounds.min.x) * 2.5f / (num2 - num);
		tOC.Controller.Collider2D.enabled = flag;
		for (int num5 = base.transform.childCount - 1; num5 >= 0; num5--)
		{
			base.transform.GetChild(num5).localScale = new Vector3(num4, num4, num4);
		}
		Collider2D[] componentsInChildren2 = base.transform.GetComponentsInChildren<Collider2D>();
		float num6 = -9999999f;
		componentsInChildren = componentsInChildren2;
		foreach (Collider2D collider2D2 in componentsInChildren)
		{
			if (Controller.Collider2D != collider2D2 && collider2D2.bounds.max.y > num6)
			{
				num6 = collider2D2.bounds.max.y;
			}
		}
		fSizeH = num6 - base.transform.position.y;
	}

	public void SwitchModelByStatus(int tStatus)
	{
		if (tModelTransform == null)
		{
			return;
		}
		for (int i = 0; i < tModelTransform.listMTDs.Count; i++)
		{
			if (tModelTransform.listMTDs[i].nStatus != tStatus)
			{
				continue;
			}
			for (int num = tModelTransform.transform.childCount - 1; num >= 0; num--)
			{
				Transform child = tModelTransform.transform.GetChild(num);
				bool active = false;
				for (int j = 0; j < tModelTransform.listMTDs[i].listEnableModel.Count; j++)
				{
					if (child.name == tModelTransform.listMTDs[i].listEnableModel[j])
					{
						active = true;
						break;
					}
				}
				child.gameObject.SetActive(active);
			}
			NowStatus = tStatus;
			break;
		}
	}

	public void UpdateFunc()
	{
		vMoveTmpDis = base.transform.localPosition;
		base.transform.localPosition = Vector3.MoveTowards(vMoveTmpDis, Controller.LogicPosition.vec3, distanceDelta);
		vMoveTmpDis = base.transform.localPosition - vMoveTmpDis;
		if (MasterPilot != null)
		{
			MasterPilot.Controller.LogicPosition = MasterPilot.Controller.LogicPosition + new VInt3(vMoveTmpDis);
			MasterPilot.transform.localPosition = MasterPilot.transform.localPosition + vMoveTmpDis;
			MasterPilot.DistanceDelta = Vector3.Distance(MasterPilot.transform.localPosition, MasterPilot.Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
			vMoveTmpDis.x = MasterPilot.transform.localPosition.x - base.transform.localPosition.x;
			vMoveTmpDis.y = MasterPilot.transform.localPosition.y - base.transform.localPosition.y - fSizeH;
			vMoveTmpDis.z = 0f;
			if (vMoveTmpDis.x != 0f)
			{
				Controller.LogicPosition += new VInt3(vMoveTmpDis);
				base.transform.localPosition = base.transform.localPosition + vMoveTmpDis;
			}
		}
	}

	private void PlayRUSH04_LP(bool flag)
	{
		base.SoundSource.PlaySE("BattleSE02", flag ? 46 : 45);
		RUSH04_LP_play = flag;
	}

	public void LateUpdateFunc()
	{
		if (MasterPilot == null && bCanRide)
		{
			Bounds tAB = Controller.Collider2D.bounds;
			foreach (OrangeCharacter runPlayer in StageUpdate.runPlayers)
			{
				if ((int)runPlayer.Hp <= 0 || runPlayer.UsingVehicle || !(runPlayer.refRideBaseObj == null) || runPlayer.bIsNpcCpy)
				{
					continue;
				}
				Bounds tBB = runPlayer.Controller.Collider2D.bounds;
				if (StageResManager.CheckBoundsIntersectNoZEffect(ref tAB, ref tBB) && Mathf.Abs(tAB.center.x - tBB.center.x) < 0.1f)
				{
					MasterPilot = runPlayer;
					runPlayer.refRideBaseObj = this;
					runPlayer.StopPlayer();
					LinkControlToOC(runPlayer);
					if (MasterPilot.IsLocalPlayer)
					{
						MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>().Target = Controller;
					}
					tMasterPilotLockRangeObj = MasterPilot.GetComponentInChildren<LockRangeObj>();
					if (!base.gameObject.GetComponent<LockRangeObj>())
					{
						mLockRangeObj = base.gameObject.AddComponent<LockRangeObj>();
						mLockRangeObj.Init();
						LockRangeObj component = runPlayer.GetComponent<LockRangeObj>();
						if ((bool)component)
						{
							mLockRangeObj.vLockLR = component.vLockLR;
							mLockRangeObj.vLockTB = component.vLockTB;
							mLockRangeObj.nNoBack = component.nNoBack;
							mLockRangeObj.nReserveBack = component.nReserveBack;
							mLockRangeObj.vReserveBackLR = component.vReserveBackLR;
							mLockRangeObj.vReserveBackTB = component.vReserveBackTB;
						}
					}
					MasterPilot.PlaySE("BattleSE02", 33);
					break;
				}
				if (Mathf.Abs(tAB.center.x - tBB.center.x) < fTransfromDis && NowStatus == 0)
				{
					SetStatus(1);
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_transform_vh003", base.transform.position, Quaternion.identity, Array.Empty<object>());
					base.SoundSource.PlaySE("BattleSE02", 47, 1f);
				}
			}
		}
		if (MasterPilot != null && MasterPilot.IsDead())
		{
			SetHorizontalSpeed(0);
			SetVerticalSpeed(0);
		}
	}

	public override void UnRide(bool bDisadle)
	{
		if (MasterPilot != null)
		{
			MasterPilot.PlaySE("BattleSE02", 34);
			bCanRide = bDisadle;
			MasterPilot.ConnectStandardCtrlCB();
			MasterPilot.PlayerPressJumpCB();
			if (MasterPilot.IsLocalPlayer)
			{
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>().Target = MasterPilot.Controller;
			}
			MasterPilot.refRideBaseObj = null;
			MasterPilot = null;
			tMasterPilotLockRangeObj = null;
			if ((bool)mLockRangeObj)
			{
				UnityEngine.Object.Destroy(mLockRangeObj);
				mLockRangeObj = null;
			}
			Collider2D[] componentsInChildren = base.transform.GetComponentsInChildren<Collider2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			rushRender = base.transform.GetComponentInChildren<Renderer>();
			SetHorizontalSpeed(Mathf.RoundToInt(_moveSpeedMultiplier * (float)nBackSpeed));
		}
	}

	private void SetStatus(int tSetStatus)
	{
		NowStatus = tSetStatus;
		tAnimator.SetInteger("NowStatus", NowStatus);
	}

	private void CheckSelfToDriverPos()
	{
		if (MasterPilot != null)
		{
			vMoveTmpDis.x = base.transform.localPosition.x - MasterPilot.transform.localPosition.x;
			vMoveTmpDis.y = base.transform.localPosition.y + fSizeH - MasterPilot.transform.localPosition.y;
			vMoveTmpDis.z = 0f;
			if (vMoveTmpDis.x != 0f || vMoveTmpDis.y != 0f)
			{
				MasterPilot.Controller.LogicPosition = MasterPilot.Controller.LogicPosition + new VInt3(vMoveTmpDis);
				MasterPilot.transform.localPosition = MasterPilot.transform.localPosition + vMoveTmpDis;
			}
		}
	}

	private void CheckDriverTpSelfPos()
	{
		if (MasterPilot != null)
		{
			vMoveTmpDis.x = MasterPilot.transform.localPosition.x - base.transform.localPosition.x;
			vMoveTmpDis.y = MasterPilot.transform.localPosition.y - base.transform.localPosition.y - fSizeH;
			vMoveTmpDis.z = 0f;
			if (vMoveTmpDis.x != 0f || vMoveTmpDis.y != 0f)
			{
				Controller.LogicPosition += new VInt3(vMoveTmpDis);
				base.transform.localPosition = base.transform.localPosition + vMoveTmpDis;
			}
		}
	}

	private void CheckLockRange()
	{
		bCheck1 = true;
		bCheck2 = true;
		while (bCheck1 || bCheck2)
		{
			if (mLockRangeObj != null)
			{
				if (mLockRangeObj.CheckLogicPos(Controller, false, true))
				{
					CheckSelfToDriverPos();
					bCheck1 = true;
				}
				else
				{
					bCheck1 = false;
				}
			}
			else
			{
				bCheck1 = false;
			}
			if (MasterPilot != null && tMasterPilotLockRangeObj != null)
			{
				if (tMasterPilotLockRangeObj.CheckLogicPos(MasterPilot.Controller, true))
				{
					CheckDriverTpSelfPos();
					bCheck2 = true;
				}
				else
				{
					bCheck2 = false;
				}
			}
			else
			{
				bCheck2 = false;
			}
		}
	}

	public void LogicUpdate()
	{
		selfBuffManager.UpdateBuffTime();
		if (MasterPilot != null)
		{
			if (MasterPilot.IsDead())
			{
				if (RUSH04_LP_play && efx_dash_root.isPlaying)
				{
					PlayRUSH04_LP(false);
				}
				return;
			}
			if (!RUSH04_LP_play && efx_dash_root.isPlaying)
			{
				PlayRUSH04_LP(true);
			}
		}
		else if (rushRender != null && !rushRender.isVisible)
		{
			rushRender = null;
			base.SoundSource.PlaySE("BattleSE02", 45);
		}
		if (MasterPilot != null && MasterPilot.IsStun)
		{
			CheckLockRange();
			return;
		}
		if (tVEHICLE_TABLE.n_ID == 4 && NowStatus == 2 && (bool)efx_dash_root)
		{
			if (!efx_dash_root.isPlaying)
			{
				efx_dash_root.Play();
				PlayRUSH04_LP(true);
			}
			else
			{
				if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && RUSH04_LP_play)
				{
					PlayRUSH04_LP(false);
				}
				if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause && !RUSH04_LP_play)
				{
					PlayRUSH04_LP(true);
				}
			}
		}
		_velocityShift = (_velocity + _velocityExtra + OrangeBattleUtility.GlobalVelocityExtra) * GameLogicUpdateManager.m_fFrameLen + _velocityShift;
		vMoveTmpDis.x = _velocityShift.vec3.magnitude;
		if (vMoveTmpDis.x > 0f)
		{
			rObjHit = StageResManager.ObjMoveCollisionWithBoxCheck(Controller.Collider2D, _velocityShift.vec3 / vMoveTmpDis.x, vMoveTmpDis.x, Controller.collisionMask, listIgnoreTrans);
			if ((bool)rObjHit)
			{
				_velocityShift = new VInt3(_velocityShift.vec3 / vMoveTmpDis.x * rObjHit.distance);
				vMoveTmpDis.x = rObjHit.distance;
			}
			if (MasterPilot != null && vMoveTmpDis.x > 0f)
			{
				rObjHit = StageResManager.ObjMoveCollisionWithBoxCheck(MasterPilot.Controller.Collider2D, _velocityShift.vec3 / vMoveTmpDis.x, vMoveTmpDis.x, Controller.collisionMask, listIgnoreTrans);
				if ((bool)rObjHit)
				{
					_velocityShift = new VInt3(_velocityShift.vec3 / vMoveTmpDis.x * rObjHit.distance);
				}
			}
		}
		Controller.LogicPosition += _velocityShift;
		CheckLockRange();
		distanceDelta = Vector3.Distance(base.transform.localPosition, Controller.LogicPosition.vec3) * (Time.deltaTime / GameLogicUpdateManager.m_fFrameLen);
		if (MasterPilot != null)
		{
			_moveSpeedMultiplier = 1f + MasterPilot.selfBuffManager.sBuffStatus.fMoveSpeed * 0.01f;
		}
		else
		{
			_moveSpeedMultiplier = 1f;
		}
		_velocityExtra = VInt3.zero;
		_velocityShift = VInt3.zero;
		if (NowStatus != tAnimator.GetInteger("NowStatus"))
		{
			SwitchModelByStatus(tAnimator.GetInteger("NowStatus"));
		}
	}

	private void LinkControlToOC(OrangeCharacter tOC)
	{
		tOC.PlayerPressLeftCB = NoneCall;
		tOC.PlayerHeldLeftCB = HeldLeft;
		tOC.PlayerReleaseLeftCB = ReleaseLeftRight;
		tOC.PlayerPressRightCB = NoneCall;
		tOC.PlayerHeldRightCB = HeldRight;
		tOC.PlayerReleaseRightCB = ReleaseLeftRight;
		tOC.PlayerHeldUpCB = HeldUp;
		tOC.PlayerReleaseUpCB = ReleaseUpDown;
		tOC.PlayerPressDownCB = NoneCall;
		tOC.PlayerHeldDownCB = HeldDown;
		tOC.PlayerReleaseDownCB = ReleaseUpDown;
		tOC.PlayerPressJumpCB = NoneCall;
		tOC.PlayerReleaseJumpCB = NoneCall;
		tOC.PlayerPressDashCB = NoneCallObj;
		tOC.PlayerReleaseDashCB = NoneCall;
		tOC.CheckDashLockEvt = CheckDashLock;
	}

	protected void SetHorizontalSpeed(int speed)
	{
		_velocity.x = speed;
	}

	protected void SetVerticalSpeed(int speed)
	{
		_velocity.y = speed;
	}

	protected void NoneCallObj(object tParam)
	{
	}

	private void NoneCall()
	{
	}

	public void HeldLeft()
	{
		int num = Mathf.RoundToInt(_moveSpeedMultiplier * (float)nBackSpeed);
		SetHorizontalSpeed(-1 * num);
		if (MasterPilot != null)
		{
			MasterPilot.direction = -1;
		}
	}

	public void HeldRight()
	{
		int horizontalSpeed = Mathf.RoundToInt(_moveSpeedMultiplier * (float)nForwardSpeed);
		SetHorizontalSpeed(horizontalSpeed);
		if (MasterPilot != null)
		{
			MasterPilot.direction = 1;
		}
	}

	public void ReleaseLeftRight()
	{
		SetHorizontalSpeed(0);
	}

	public void HeldUp()
	{
		int verticalSpeed = Mathf.RoundToInt(_moveSpeedMultiplier * (float)nUpSpeed);
		SetVerticalSpeed(verticalSpeed);
	}

	public void HeldDown()
	{
		int num = Mathf.RoundToInt(_moveSpeedMultiplier * (float)nDownSpeed);
		SetVerticalSpeed(-1 * num);
	}

	public void ReleaseUpDown()
	{
		SetVerticalSpeed(0);
	}

	protected void PlayerPressDash(object tParam)
	{
	}

	public override void SetStun(bool enable, bool bCheckOtherObj = true)
	{
		if (enable)
		{
			SetHorizontalSpeed(0);
			SetVerticalSpeed(0);
		}
		if (MasterPilot != null)
		{
			MasterPilot.SetStun(enable, false);
		}
	}

	private bool CheckDashLock()
	{
		return true;
	}
}
