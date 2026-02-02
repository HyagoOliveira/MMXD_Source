using Newtonsoft.Json;
using StageLib;
using UnityEngine;

public class TrackingBuffBullet : BasicBullet
{
	protected SyncBullet _pSyncBullet;

	protected bool _bIsLocalPlayer;

	protected override void Awake()
	{
		base.Awake();
		_pSyncBullet = base.gameObject.AddOrGetComponent<SyncBullet>();
	}

	public override void OnStart()
	{
		base.OnStart();
		if ((bool)_pSyncBullet && refPBMShoter.SOB as OrangeCharacter != null)
		{
			OrangeCharacter orangeCharacter = refPBMShoter.SOB as OrangeCharacter;
			_pSyncBullet.sNetSerialID = orangeCharacter.sNetSerialID + "SGB" + SyncBullet.GetSyncBulletCount();
			_bIsLocalPlayer = orangeCharacter.IsLocalPlayer;
		}
	}

	public void SendSyncBulletStatus()
	{
		if (!StageUpdate.gbIsNetGame || !_bIsLocalPlayer || _pSyncBullet == null)
		{
			return;
		}
		string sParam = string.Empty;
		VInt3 vInt = new VInt3(_transform.localPosition);
		VInt3 zero = VInt3.zero;
		if (Target != null)
		{
			zero = new VInt3(Target.AimPosition - _transform.position);
			if (Target as StageObjBase != null)
			{
				sParam = (Target as StageObjBase).sNetSerialID;
			}
		}
		else
		{
			zero = new VInt3(Direction);
		}
		SyncBullet.NetSyncData netSyncData = new SyncBullet.NetSyncData();
		netSyncData.SelfPosX = vInt.x;
		netSyncData.SelfPosY = vInt.y;
		netSyncData.SelfPosZ = vInt.z;
		netSyncData.TargetPosX = zero.x;
		netSyncData.TargetPosY = zero.y;
		netSyncData.TargetPosZ = zero.z;
		netSyncData.sParam0 = sParam;
		_pSyncBullet.SendSyncEvent(0, JsonConvert.SerializeObject(netSyncData));
	}

	public override void SyncBulletStatus(int nSet, string smsg)
	{
		if (StageUpdate.gbIsNetGame && !_bIsLocalPlayer)
		{
			SyncBullet.NetSyncData netSyncData = JsonConvert.DeserializeObject<SyncBullet.NetSyncData>(smsg);
			if (!string.IsNullOrEmpty(netSyncData.sParam0))
			{
				StageObjBase sOBByNetSerialID = StageResManager.GetStageUpdate().GetSOBByNetSerialID(netSyncData.sParam0);
				Target = (IAimTarget)sOBByNetSerialID;
			}
			else
			{
				Target = null;
			}
		}
	}

	protected override void TrackingTarget()
	{
		if (!activeTracking || TrackingData == null || ActivateTimer.GetMillisecond() < TrackingData.n_BEGINTIME_1 || ActivateTimer.GetMillisecond() >= TrackingData.n_ENDTIME_1)
		{
			return;
		}
		if (Target == null)
		{
			FindTarget(trackPriority);
			if (Target != null)
			{
				SendSyncBulletStatus();
			}
		}
		else if (!Target.BuffManager.CheckHasEffectByCONDITIONID(TrackingData.n_CONDITION))
		{
			Target = null;
			SendSyncBulletStatus();
		}
		if (Target != null)
		{
			DoAim(Target);
		}
	}

	protected override void FindTarget(TrackPriority trackPriority)
	{
		if (Target != null)
		{
			return;
		}
		int buffId = ((TrackingData != null) ? TrackingData.n_CONDITION : 0);
		switch (trackPriority)
		{
		case TrackPriority.EnemyFirst:
			if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
			{
				Target = NeutralAIS.GetClosetTarget(2, buffId);
			}
			if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
			{
				Target = NeutralAIS.GetClosetTarget(1, buffId);
			}
			if (Target == null && (refPBMShoter.SOB == null || refPBMShoter.SOB.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer))
			{
				Target = NeutralAIS.GetClosetTarget(3, buffId);
			}
			break;
		case TrackPriority.PlayerFirst:
			if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
			{
				Target = NeutralAIS.GetClosetTarget(1, buffId);
			}
			if (Target == null && (refPBMShoter.SOB == null || refPBMShoter.SOB.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer))
			{
				Target = NeutralAIS.GetClosetTarget(3, buffId);
			}
			if (Target == null && ((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
			{
				Target = NeutralAIS.GetClosetTarget(2, buffId);
			}
			break;
		case TrackPriority.NearFirst:
		{
			IAimTarget aimTarget = null;
			IAimTarget aimTarget2 = null;
			IAimTarget aimTarget3 = null;
			float num = float.MaxValue;
			if (((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.EnemyLayer)) != 0)
			{
				aimTarget = NeutralAIS.GetClosetTarget(2, buffId);
			}
			if (((int)TargetMask & (1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer)) != 0)
			{
				aimTarget2 = NeutralAIS.GetClosetTarget(1, buffId);
			}
			if (refPBMShoter.SOB == null || refPBMShoter.SOB.gameObject.layer != ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer)
			{
				aimTarget3 = NeutralAIS.GetClosetTarget(3, buffId);
			}
			if (aimTarget != null)
			{
				num = Vector2.Distance(_transform.position.xy(), aimTarget.AimPosition.xy());
				Target = aimTarget;
			}
			if (aimTarget2 != null)
			{
				float num2 = Vector2.Distance(_transform.position.xy(), aimTarget2.AimPosition.xy());
				if (num2 < num)
				{
					num = num2;
					Target = aimTarget2;
				}
			}
			if (aimTarget3 != null && aimTarget3 != aimTarget2)
			{
				float num3 = Vector2.Distance(_transform.position.xy(), aimTarget3.AimPosition.xy());
				if (num3 < num)
				{
					num = num3;
					Target = aimTarget3;
				}
			}
			break;
		}
		}
	}
}
