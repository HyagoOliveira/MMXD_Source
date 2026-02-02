#define RELEASE
using System;
using System.Collections.Generic;
using CallbackDefs;
using CodeStage.AntiCheat.ObscuredTypes;
using StageLib;
using UnityEngine;
using enums;

public class StageObjBase : PoolBaseObject
{
	public enum SOB_TYPE
	{
		SOB_TYPE_NONE = 0,
		SOB_TYPE_PLAYER = 1,
		SOB_TYPE_ENEMY = 2,
		SOB_TYPE_RIDEOBJ = 3,
		SOB_TYPE_PET = 4
	}

	[HideInInspector]
	public Transform _transform;

	[HideInInspector]
	public Transform ModelTransform;

	public string sNetSerialID = "";

	private bool _Activate;

	[HideInInspector]
	public ObscuredInt Hp;

	[HideInInspector]
	public ObscuredInt MaxHp;

	[HideInInspector]
	public ObscuredInt HealHp;

	[HideInInspector]
	public ObscuredInt DmgHp;

	private Transform aimTransform;

	private Vector3 aimPoint = Vector3.zero;

	private bool bVanish;

	protected internal List<int> GuardTransform;

	public bool bIsNpcCpy;

	private bool _isHidden;

	private HurtPassParam tHurtPassParam = new HurtPassParam();

	public RefPassiveskill tRefPassiveskill;

	public PerBuffManager selfBuffManager = new PerBuffManager();

	public List<BulletBase.DmgStack> listDmgStack = new List<BulletBase.DmgStack>();

	public Callback tAnimationCB;

	private List<AniSpeedData> listPauseASD = new List<AniSpeedData>();

	private List<ParticleSystem> listPausePS = new List<ParticleSystem>();

	public virtual bool Activate
	{
		get
		{
			if (_Activate)
			{
				return !MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause;
			}
			return false;
		}
		set
		{
			_Activate = value;
		}
	}

	public Transform AimTransform
	{
		get
		{
			return aimTransform;
		}
		set
		{
			aimTransform = value;
		}
	}

	public Vector3 AimPoint
	{
		get
		{
			return aimPoint;
		}
		set
		{
			aimPoint = value;
		}
	}

	public bool AllowAutoAim { get; set; }

	public bool VanishStatus
	{
		get
		{
			return bVanish;
		}
		set
		{
			bVanish = value;
		}
	}

	public virtual Vector3 AimPosition
	{
		get
		{
			if (AimTransform == null)
			{
				return base.transform.localRotation * AimPoint;
			}
			return AimTransform.position + base.transform.localRotation * AimPoint;
		}
		set
		{
			Debug.LogWarning("Not Used !");
		}
	}

	public AimTargetType AutoAimType { get; set; }

	public virtual bool IsInvincible { get; set; }

	public PerBuffManager BuffManager
	{
		get
		{
			return selfBuffManager;
		}
		set
		{
			Debug.LogWarning("Not Used !");
		}
	}

	public bool IsHidden
	{
		get
		{
			return _isHidden;
		}
		set
		{
			if (_isHidden != value)
			{
				_isHidden = value;
				selfBuffManager.SwitchBuffFX(!_isHidden);
			}
		}
	}

	public CharacterDirection _characterDirection { get; set; }

	public int direction
	{
		get
		{
			return (int)_characterDirection;
		}
		set
		{
			_characterDirection = (CharacterDirection)value;
		}
	}

	public event Action<StageObjBase> HurtActions;

	private void Awake()
	{
		_characterDirection = CharacterDirection.RIGHT;
		IsInvincible = false;
	}

	public virtual Transform GetFXShowTrans()
	{
		return aimTransform;
	}

	public virtual bool CheckIsLocalPlayer()
	{
		return false;
	}

	public virtual bool IsAlive()
	{
		return (int)Hp > 0;
	}

	public virtual bool IsCanTriggerEvent()
	{
		if (bIsNpcCpy || (int)Hp <= 0)
		{
			return false;
		}
		if (!base.gameObject.activeSelf)
		{
			return false;
		}
		return true;
	}

	public virtual int GetSOBType()
	{
		return 0;
	}

	public virtual bool CheckActStatus(int mainstatus, int substatus)
	{
		return false;
	}

	public void UpdateHurtAction()
	{
		if (this.HurtActions != null)
		{
			this.HurtActions(this);
		}
	}

	public void NullHurtAction()
	{
		this.HurtActions = null;
	}

	public bool CheckHurtAction()
	{
		if (this.HurtActions != null)
		{
			return true;
		}
		return false;
	}

	public virtual void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
	}

	public void CallAnimationCB()
	{
		if (tAnimationCB != null)
		{
			tAnimationCB();
			tAnimationCB = null;
		}
	}

	public virtual void BuffChangeCheck()
	{
	}

	public virtual ObscuredBool IsUnBreakX()
	{
		if (selfBuffManager.CheckHasEffect(9))
		{
			return true;
		}
		return false;
	}

	public virtual ObscuredBool IsUnBreak()
	{
		if (selfBuffManager.CheckHasEffect(7))
		{
			return true;
		}
		return false;
	}

	public virtual string GetSOBName()
	{
		return "";
	}

	public virtual ObscuredInt GetCurrentWeapon()
	{
		return 0;
	}

	public virtual ObscuredInt GetCurrentWeaponCheck()
	{
		return 16777215;
	}

	public virtual ObscuredInt GetDOD(int nCurrentWeapon)
	{
		return 0;
	}

	public virtual ObscuredInt GetDEF(int nCurrentWeapon)
	{
		return 0;
	}

	public virtual ObscuredInt GetReduceCriPercent(int nCurrentWeapon)
	{
		return 0;
	}

	public virtual ObscuredInt GetReduceCriDmgPercent(int nCurrentWeapon)
	{
		return 0;
	}

	public virtual ObscuredInt GetBlock()
	{
		return 0;
	}

	public virtual ObscuredInt GetBlockDmgPercent()
	{
		return 0;
	}

	public virtual ObscuredInt Hurt(HurtPassParam tHurtPassParam)
	{
		return 0;
	}

	public virtual int GetNowRecordNO()
	{
		return 0;
	}

	public virtual void CheckDmgStackByRecordNO()
	{
		int nowRecordNO = GetNowRecordNO();
		for (int num = listDmgStack.Count - 1; num >= 0; num--)
		{
			BulletBase.DmgStack dmgStack = listDmgStack[num];
			if ((int)dmgStack.nNetID == 0)
			{
				if (nowRecordNO == -1 || (int)dmgStack.nRecordID + 1 < nowRecordNO)
				{
					listDmgStack.RemoveAt(num);
					if ((bool)RunDmgStack(dmgStack))
					{
						listDmgStack.Clear();
						break;
					}
				}
			}
			else
			{
				OrangeCharacter playerByID = StageUpdate.GetPlayerByID(dmgStack.sPlayerID);
				if (!(playerByID != null) || (playerByID.GetNowRecordNO() >= (int)dmgStack.nRecordID + 1 && StageUpdate.NowHasBullet(dmgStack.nRecordID, dmgStack.nNetID) < 0))
				{
					listDmgStack.RemoveAt(num);
					if ((bool)RunDmgStack(dmgStack))
					{
						listDmgStack.Clear();
						break;
					}
				}
			}
		}
	}

	public virtual void CheckDmgStack(int nRecordID, int nNetID)
	{
		for (int num = listDmgStack.Count - 1; num >= 0; num--)
		{
			BulletBase.DmgStack dmgStack = listDmgStack[num];
			if ((int)dmgStack.nRecordID == nRecordID && (int)dmgStack.nNetID == nNetID)
			{
				listDmgStack.RemoveAt(num);
				if ((bool)RunDmgStack(dmgStack))
				{
					break;
				}
			}
		}
	}

	public void ClearDmgStack()
	{
		listDmgStack.Clear();
	}

	public virtual ObscuredBool RunDmgStack(BulletBase.DmgStack tDS)
	{
		bool flag = false;
		if ((int)tDS.nDmg > 0)
		{
			if ((int)Hp > 0)
			{
				tHurtPassParam.dmg = tDS.nDmg;
				tHurtPassParam.nSubPartID = tDS.nSubPartID;
				tHurtPassParam.wpnType = (WeaponType)tDS.nWeaponType;
				tHurtPassParam.owner = tDS.sOwner;
				HitBackV3(tDS.vHitBack);
				if ((int)tDS.nEndHP <= 0)
				{
					Hp = 0;
					DmgHp = 0;
					HealHp = 0;
					flag = true;
					tDS.nDmg = (int)tDS.nDmg - (int)tDS.nEnergyShield;
					selfBuffManager.ReduceDmgByEnergyShild(selfBuffManager.sBuffStatus.nEnergyShield);
					Hurt(tHurtPassParam);
				}
				else
				{
					tDS.nDmg = (int)tDS.nHP - (int)tDS.nEndHP;
					if ((int)tDS.nEnergyShield > (int)tHurtPassParam.dmg)
					{
						tDS.nEnergyShield = (int)tDS.nEnergyShield - (int)tHurtPassParam.dmg;
						selfBuffManager.ReduceDmgByEnergyShild(tHurtPassParam.dmg);
						tHurtPassParam.dmg = 0;
					}
					else
					{
						HurtPassParam hurtPassParam = tHurtPassParam;
						hurtPassParam.dmg = (int)hurtPassParam.dmg - (int)tDS.nEnergyShield;
						if (selfBuffManager.sBuffStatus.nEnergyShield == 0 && tDS.nBreakEnergyShieldBuffID > 0)
						{
							selfBuffManager.CheckDelBuffTrigger(tDS.nBreakEnergyShieldBuffID, 3, true);
						}
						selfBuffManager.ReduceDmgByEnergyShild(tDS.nEnergyShield);
						tHurtPassParam.dmg = 0;
					}
					if ((int)HealHp < (int)tDS.nHealHP)
					{
						tHurtPassParam.dmg = 0;
						HealHp = tDS.nHealHP;
						Hp = (int)MaxHp + (int)HealHp - (int)DmgHp;
					}
					if ((int)DmgHp < (int)tDS.nDmgHP)
					{
						tHurtPassParam.dmg = 0;
						DmgHp = tDS.nDmgHP;
						if ((int)Hp > 0)
						{
							Hp = (int)MaxHp + (int)HealHp - (int)DmgHp;
						}
					}
					Hurt(tHurtPassParam);
				}
				if ((int)tDS.nDmg > 0)
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, GetDamageTextPos(), tDS.nDmg.GetDecrypted(), GetSOBLayerMask(), (VisualDamage.DamageType)tDS.nDamageType);
				}
				else
				{
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, GetDamageTextPos(), 0, GetSOBLayerMask(), VisualDamage.DamageType.Reduce);
				}
			}
		}
		else if ((int)tDS.nDmg < 0)
		{
			if ((int)HealHp < (int)tDS.nHealHP)
			{
				HealHp = tDS.nHealHP;
				Hp = (int)MaxHp + (int)HealHp - (int)DmgHp;
				UpdateHurtAction();
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, GetDamageTextPos(), -tDS.nDmg.GetDecrypted(), GetSOBLayerMask(), VisualDamage.DamageType.Recover);
		}
		else if (tDS.nDamageType == 3)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, GetDamageTextPos(), 0, GetSOBLayerMask(), VisualDamage.DamageType.Reduce);
		}
		else
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SHOW_DAMAGE, GetDamageTextPos(), 0, GetSOBLayerMask(), VisualDamage.DamageType.Miss);
		}
		return flag;
	}

	public virtual ObscuredInt Heal(int nHeal)
	{
		if (nHeal <= 0)
		{
			return Hp;
		}
		Hp = (int)Hp + nHeal;
		if ((int)Hp > (int)MaxHp)
		{
			Hp = MaxHp;
		}
		UpdateHurtAction();
		return Hp;
	}

	public virtual Vector3 HitBack(int BackPos, Vector3 dir)
	{
		return Vector3.zero;
	}

	public virtual void HitBackV3(Vector3 vHitBackNet)
	{
	}

	public virtual Vector2 GetDamageTextPos()
	{
		return base.transform.position.xy();
	}

	public virtual Vector3 GetTargetPoint()
	{
		return base.transform.position;
	}

	public virtual LayerMask GetSOBLayerMask()
	{
		return 1 << base.gameObject.layer;
	}

	public virtual void LockAnimator(bool bLock)
	{
		if (bLock)
		{
			Animator[] componentsInChildren = GetComponentsInChildren<Animator>();
			foreach (Animator tAniPlayer in componentsInChildren)
			{
				AniSpeedData aniSpeedData = new AniSpeedData();
				aniSpeedData.tAniPlayer = tAniPlayer;
				aniSpeedData.fSpeed = aniSpeedData.tAniPlayer.speed;
				aniSpeedData.fSetSeed = 0f;
				listPauseASD.Add(aniSpeedData);
				aniSpeedData.tAniPlayer.speed = aniSpeedData.fSetSeed;
			}
			ParticleSystem[] componentsInChildren2 = base.transform.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren2)
			{
				if (!(particleSystem.GetComponentInParent<FxBase>() != null) && (!(particleSystem.transform.parent != null) || !(particleSystem.transform.parent.GetComponentInParent<ParticleSystem>() != null)) && particleSystem.isPlaying && !listPausePS.Contains(particleSystem))
				{
					listPausePS.Add(particleSystem);
					particleSystem.Pause(true);
				}
			}
			return;
		}
		foreach (AniSpeedData item in listPauseASD)
		{
			if (item.tAniPlayer.speed == item.fSetSeed)
			{
				item.tAniPlayer.speed = item.fSpeed;
			}
		}
		listPauseASD.Clear();
		for (int num = listPausePS.Count - 1; num >= 0; num--)
		{
			listPausePS[num].Play(true);
		}
		listPausePS.Clear();
	}

	public virtual void SetStun(bool enable, bool bCheckOtherObj = true)
	{
	}

	public virtual void SetBanWeapon(bool enable)
	{
	}

	public virtual void SetBanSkill(bool enable)
	{
	}

	public virtual void SetNoMove(bool enable, bool bCheckOtherObj = true)
	{
	}

	public virtual void SetReverseRightAndLeft(bool enable)
	{
	}

	public virtual void SetBanAutoAim(bool enable)
	{
	}

	public virtual void PlaySE(string s_acb, string cueName, float delay = 0f, bool ForceTrigger = false, bool UseDisCheck = true)
	{
	}

	public virtual void UpdatePassiveUseTime(int ID)
	{
		if (tRefPassiveskill != null)
		{
			tRefPassiveskill.NetUpdateUseTime(ID);
		}
	}
}
