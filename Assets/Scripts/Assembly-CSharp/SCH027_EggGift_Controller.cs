using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class SCH027_EggGift_Controller : SCH020Controller
{
	private CH116_EggMeshController _meshController;

	protected override void Awake()
	{
		base.Awake();
		_meshController = GetComponentInChildren<CH116_EggMeshController>();
	}

	public void SetMeshIndex(int index)
	{
		_meshController.SetMeshIndex(index);
	}

	public override void UpdateFunc()
	{
		if (Activate && _mainStatus == MainStatus.Launch && _follow_Player != null)
		{
			_fAngle += Time.deltaTime * _fAngleSpeed;
			Vector3 position = _follow_Player.AimPosition + Quaternion.Euler(0f, _fAngle, 0f) * Vector3.right;
			_transform.SetPositionAndRotation(position, Quaternion.Euler(0f, _fAngle + 90f, 0f));
		}
	}

	public override void UpdateDirecion()
	{
		if (FollowEnabled && ModelTransform != null)
		{
			ModelTransform.localScale = Vector3.one;
		}
	}

	protected override void SetLaunchStatus()
	{
		if (_follow_Player.IsLocalPlayer)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.nParam0 = _meshController.MeshIndex;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)_mainStatus, JsonConvert.SerializeObject(netSyncData), true);
		}
	}

	protected override void SyncLaunchStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Launch;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
			SetMeshIndex(tSendb.nParam0);
		}
	}

	protected override void SetReadyStatus()
	{
		CreateSkillBullet(_aimTarget, _aimTarget.AimPosition, _meshController.MeshIndex);
		if (_follow_Player.IsLocalPlayer)
		{
			string empty = string.Empty;
			VInt3 vInt = new VInt3(_aimTarget.AimPosition - MonoBehaviourSingleton<PoolManager>.Instance.transform.position);
			if (_aimTarget as StageObjBase != null)
			{
				empty = (_aimTarget as StageObjBase).sNetSerialID;
			}
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.TargetPosX = vInt.x;
			netSyncData.TargetPosY = vInt.y;
			netSyncData.TargetPosZ = vInt.z;
			netSyncData.nParam0 = _meshController.MeshIndex;
			netSyncData.sParam0 = empty;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)_mainStatus, JsonConvert.SerializeObject(netSyncData), true);
		}
	}

	protected override void SyncReadyStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Ready;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
			VInt3 vInt = default(VInt3);
			vInt.x = tSendb.TargetPosX;
			vInt.y = tSendb.TargetPosY;
			vInt.z = tSendb.TargetPosZ;
			_autoAim.SetTargetByNetSerialID(tSendb.sParam0);
			CreateSkillBullet(_autoAim.AutoAimTarget, vInt.vec3, tSendb.nParam0);
			SetMeshIndex(tSendb.nParam0);
		}
	}

	protected override void SetDestructStatus()
	{
		if (_follow_Player.IsLocalPlayer)
		{
			NetSyncData netSyncData = new NetSyncData();
			netSyncData.SelfPosX = Controller.LogicPosition.x;
			netSyncData.SelfPosY = Controller.LogicPosition.y;
			netSyncData.SelfPosZ = Controller.LogicPosition.z;
			netSyncData.nParam0 = _meshController.MeshIndex;
			StageUpdate.RegisterPetSendAndRun(sNetSerialID, (int)_mainStatus, JsonConvert.SerializeObject(netSyncData), true);
		}
	}

	protected override void SyncDestructStatus(NetSyncData tSendb)
	{
		if (!_follow_Player.IsLocalPlayer)
		{
			_mainStatus = MainStatus.Destruct;
			Controller.LogicPosition.x = tSendb.SelfPosX;
			Controller.LogicPosition.y = tSendb.SelfPosY;
			Controller.LogicPosition.z = tSendb.SelfPosZ;
			SetMeshIndex(tSendb.nParam0);
		}
	}

	protected void CreateSkillBullet(IAimTarget target, Vector3 targetPos, int meshIndex)
	{
		SKILL_TABLE tSkillTable = ((_tSkill0_Table == null) ? PetWeapons[0].BulletData : _tSkill0_Table);
		Vector3 pDirection = ((target == null) ? (targetPos - _transform.position).normalized : (target.AimPosition - _transform.position).normalized);
		BulletBase bulletBase = ((!_follow_Player) ? BulletBase.TryShotBullet(tSkillTable, _transform, pDirection, _wsWeaponStatus, selfBuffManager.sBuffStatus, null, base.TargetMask, false, false, true) : BulletBase.TryShotBullet(tSkillTable, _transform, pDirection, _wsWeaponStatus, _follow_Player.selfBuffManager.sBuffStatus, null, base.TargetMask, false, false, true));
		if ((bool)bulletBase && _follow_Player != null)
		{
			bulletBase.BulletLevel = _nBulletLv;
			bulletBase.SetOwnerName(_follow_Player.sPlayerName);
		}
		CH116_EggGiftBullet cH116_EggGiftBullet = bulletBase as CH116_EggGiftBullet;
		if (cH116_EggGiftBullet != null)
		{
			cH116_EggGiftBullet.SetMeshIndex(meshIndex);
		}
	}
}
