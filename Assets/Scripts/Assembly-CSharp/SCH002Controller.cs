using CallbackDefs;
using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class SCH002Controller : PetControllerBase
{
	private VInt3 mAtk_Target;

	private bool isATK;

	public override string[] GetPetDependAnimations()
	{
		return new string[1] { "sch002_move_loop" };
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void AfterActive()
	{
		base.SoundSource.PlaySE("BattleSE02", "bt_beat01");
	}

	protected override void AfterDeactive()
	{
		base.SoundSource.PlaySE("BattleSE02", "bt_beat02");
	}

	private void AtkUpdata()
	{
		if (Controller.LogicPosition.x != mAtk_Target.x)
		{
			int num = mAtk_Target.x - Controller.LogicPosition.x;
			if (num > 5 || num < -5)
			{
				_velocity.x = (mAtk_Target.x - Controller.LogicPosition.x) * 5;
			}
			else
			{
				_velocity.x = mAtk_Target.x - Controller.LogicPosition.x;
			}
		}
		else
		{
			_velocity.x = 0;
		}
		if (Controller.LogicPosition.y != mAtk_Target.y)
		{
			int num2 = mAtk_Target.y - Controller.LogicPosition.y;
			if (num2 > 5 || num2 < -5)
			{
				_velocity.y = (mAtk_Target.y - Controller.LogicPosition.y) * 5;
			}
			else
			{
				_velocity.y = mAtk_Target.y - Controller.LogicPosition.y;
			}
		}
		else
		{
			_velocity.y = 0;
		}
		int num3 = mAtk_Target.x - Controller.LogicPosition.x;
		if (num3 > 0 && direction != 1)
		{
			direction = 1;
		}
		else if (num3 < 0 && direction != -1)
		{
			direction = -1;
		}
		if (ModelTransform != null)
		{
			ModelTransform.localScale = new Vector3(1f, 1f, direction);
		}
		if (Controller.LogicPosition.y == mAtk_Target.y && Controller.LogicPosition.x == mAtk_Target.x)
		{
			StopATK();
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		switch (nSet)
		{
		case 1:
		{
			SetFollowEnabled(false);
			isATK = true;
			SetAnimateId(PetHumanBase.PetAnimateId.ANI_SKILL_START);
			NetSyncData netSyncData2 = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			mAtk_Target.x = netSyncData2.SelfPosX;
			mAtk_Target.y = netSyncData2.SelfPosY;
			mAtk_Target.z = netSyncData2.SelfPosZ;
			if (_follow_Player != null && _follow_Player.IsLocalPlayer)
			{
				PetWeapons[0].MagazineRemain -= PetWeapons[0].BulletData.n_USE_COST;
				PetWeapons[0].LastUseTimer.TimerStart();
				_collideBullet.UpdateBulletData(PetWeapons[0].BulletData, _follow_Player.sPlayerName, _follow_Player.GetNowRecordNO(), _follow_Player.nBulletRecordID++);
				_collideBullet.SetBulletAtk(_follow_Player.PlayerSkills[follow_skill_id].weaponStatus, selfBuffManager.sBuffStatus);
				_collideBullet.BulletLevel = _follow_Player.PlayerSkills[follow_skill_id].SkillLV;
				_collideBullet.SetPetBullet();
				_collideBullet.HitCallback = HitCB;
				_collideBullet.Active(base.TargetMask);
			}
			break;
		}
		case 2:
			isATK = false;
			SetAnimateId(PetHumanBase.PetAnimateId.ANI_STAND);
			SetFollowEnabled(true);
			break;
		case 3:
		{
			NetSyncData netSyncData = JsonConvert.DeserializeObject<NetSyncData>(smsg);
			mAtk_Target.x = netSyncData.SelfPosX;
			mAtk_Target.y = netSyncData.SelfPosY;
			mAtk_Target.z = netSyncData.SelfPosZ;
			break;
		}
		}
	}

	private void StopATK()
	{
		StageUpdate.RegisterPetSendAndRun(sNetSerialID, 2, "", true);
	}

	private void HitCB(object obj)
	{
		_collideBullet.BackToPool();
		StopATK();
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		UpdateMagazine();
		if (isATK)
		{
			if (_follow_Player.IsLocalPlayer)
			{
				if (_autoAim.AutoAimTarget != null)
				{
					VInt3 vInt = new VInt3(_autoAim.AutoAimTarget.AimTransform.transform.position);
					NetSyncData netSyncData = new NetSyncData();
					netSyncData.SelfPosX = vInt.x;
					netSyncData.SelfPosY = vInt.y;
					netSyncData.SelfPosZ = vInt.z;
					StageUpdate.RegisterPetSendAndRun(sNetSerialID, 3, JsonConvert.SerializeObject(netSyncData), true);
					AtkUpdata();
				}
				else
				{
					StopATK();
				}
			}
			else
			{
				AtkUpdata();
			}
		}
		if (_follow_Player != null && _follow_Player.IsLocalPlayer && !isATK && _autoAim.AutoAimTarget != null && isSkillAvailable(0))
		{
			VInt3 vInt2 = new VInt3(_autoAim.AutoAimTarget.AimTransform.transform.position);
			NetSyncData netSyncData2 = new NetSyncData();
			netSyncData2.SelfPosX = vInt2.x;
			netSyncData2.SelfPosY = vInt2.y;
			netSyncData2.SelfPosZ = vInt2.z;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, 1, JsonConvert.SerializeObject(netSyncData2), true);
			isATK = true;
		}
		_autoAim.SetUpdate(!isATK);
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[1] { "idle" };
		target = new string[1] { "sch002_stand_loop" };
	}

	public override void SetActive(bool isActive)
	{
		if (isActive)
		{
			isATK = false;
			if (_autoAim != null)
			{
				_autoAim.AutoAimTarget = null;
				_autoAim.ClearTargetList();
				_velocity = VInt3.zero;
				_velocityExtra = VInt3.zero;
				_velocityShift = VInt3.zero;
				SetFollowEnabled(true);
			}
		}
		base.SetActive(isActive);
	}
}
