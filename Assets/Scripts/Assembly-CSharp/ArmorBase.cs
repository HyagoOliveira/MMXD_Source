using System;
using System.Collections.Generic;
using System.Linq;
using Better;
using OrangeAudio;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ArmorBase : MonoBehaviour, ILogicUpdate
{
	protected class LinkSkillData
	{
		public ButtonId ButtonId;

		public WeaponStruct WeaponStruct;

		public bool IsNewWeaponStruct;
	}

	[SerializeField]
	protected int currentWeaponID = 13511;

	[SerializeField]
	protected string FX_LINK = "fxuse_summonunit_000";

	[SerializeField]
	protected Animator animator;

	[SerializeField]
	protected CharacterMaterial characterMaterial;

	[SerializeField]
	protected CharacterMaterial characterMaterialWeapon;

	[SerializeField]
	protected Transform[] shootTransforms = new Transform[4];

	protected readonly int hashVelocityY = Animator.StringToHash("fVelocityY");

	protected readonly int hashDirection = Animator.StringToHash("fDirection");

	protected readonly float hpBarExtraY = 1.2f;

	protected bool IsTeleportOut;

	protected int nowFrame;

	protected int skillEventFrame;

	protected int endFrame;

	protected OrangeCharacter _refEntity;

	protected Renderer[] entityRenderers;

	protected List<Transform[]> listEntityShootTransform = new List<Transform[]>();

	protected System.Collections.Generic.Dictionary<UpdateTimer, WeaponStruct> dictEntityTimer = new Better.Dictionary<UpdateTimer, WeaponStruct>();

	protected UpdateTimer[] timers = new UpdateTimer[0];

	protected Vector3 bulletDirection = Vector3.zero;

	protected System.Collections.Generic.Dictionary<HumanBase.AnimateId, int> dictAnimateHash = new Better.Dictionary<HumanBase.AnimateId, int>();

	protected System.Collections.Generic.Dictionary<ButtonId, LinkSkillData> dictSkill = new Better.Dictionary<ButtonId, LinkSkillData>();

	protected System.Collections.Generic.Dictionary<ButtonId, VirtualButton> dictDisplayButton = new Better.Dictionary<ButtonId, VirtualButton>();

	protected List<VirtualButton> listButtonInvisible = new List<VirtualButton>();

	protected ButtonId[] btnIds = new ButtonId[3]
	{
		ButtonId.SHOOT,
		ButtonId.SKILL0,
		ButtonId.SKILL1
	};

	protected ButtonId[] invisibleBtns = new ButtonId[1] { ButtonId.CHIP_SWITCH };

	protected BulletBase.ShotBullerParam CreateBulletUseParam = new BulletBase.ShotBullerParam();

	protected HumanBase.AnimateId lastID = HumanBase.AnimateId.MAX_ANI;

	public bool IsLink { get; protected set; }

	protected abstract void InitAnimateHash();

	protected abstract void SetLinkAnimationAndStatus();

	protected abstract void ResetLastStatus();

	public abstract void CheckSkill();

	public abstract void PlayerPressSkillCharacterCall(int id);

	public abstract void PlayerReleaseSkillCharacterCall(int id);

	public abstract void PlayerHeldShoot();

	protected virtual void Awake()
	{
		InitAnimateHash();
		characterMaterial.Disappear();
		IsLink = false;
		IsTeleportOut = false;
	}

	protected void OnDestroy()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	protected void InitDisplayBtn()
	{
		dictDisplayButton.Clear();
		listButtonInvisible.Clear();
		if (!_refEntity.IsLocalPlayer)
		{
			return;
		}
		ButtonId[] array = btnIds;
		foreach (ButtonId buttonId in array)
		{
			VirtualButton virtualButton = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(buttonId);
			dictDisplayButton.Add(buttonId, virtualButton);
			virtualButton.AllowUpdate = false;
		}
		array = invisibleBtns;
		foreach (ButtonId id in array)
		{
			VirtualButton virtualButton2 = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(id);
			if ((bool)virtualButton2)
			{
				virtualButton2.gameObject.SetActive(false);
				listButtonInvisible.Add(MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(id));
			}
		}
		foreach (KeyValuePair<ButtonId, LinkSkillData> item in dictSkill)
		{
			VirtualButton virtualButton3 = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(item.Key);
			if ((bool)virtualButton3)
			{
				virtualButton3.UpdateIconBySklTable(item.Value.WeaponStruct.BulletData);
				virtualButton3.ClearStick();
			}
		}
		VirtualButton virtualButton4 = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SELECT);
		if ((bool)virtualButton4)
		{
			virtualButton4.UpdateIconByBundlePath(AssetBundleScriptableObject.Instance.m_uiPath + "ui_battleinfo", "UI_battle_button_drive");
		}
	}

	protected void ResetDisplayBtn()
	{
		if (!_refEntity.IsLocalPlayer)
		{
			return;
		}
		foreach (VirtualButton value in dictDisplayButton.Values)
		{
			value.AllowUpdate = true;
		}
	}

	protected void UpdateDisplay()
	{
		if ((bool)_refEntity && _refEntity.IsLocalPlayer)
		{
			VirtualButton virtualButton = dictDisplayButton[ButtonId.SHOOT];
			virtualButton.AllowUpdate = true;
			virtualButton.UpdateValue(VirtualButtonId.SHOOT, dictSkill[ButtonId.SHOOT].WeaponStruct, _refEntity, _refEntity.tRefPassiveskill);
			virtualButton.AllowUpdate = false;
			VirtualButton virtualButton2 = dictDisplayButton[ButtonId.SKILL0];
			virtualButton2.AllowUpdate = true;
			virtualButton2.UpdateValue(VirtualButtonId.SKILL0, dictSkill[ButtonId.SKILL0].WeaponStruct, _refEntity, _refEntity.tRefPassiveskill);
			virtualButton2.AllowUpdate = false;
			VirtualButton virtualButton3 = dictDisplayButton[ButtonId.SKILL1];
			virtualButton3.AllowUpdate = true;
			virtualButton3.UpdateValue(VirtualButtonId.SKILL1, dictSkill[ButtonId.SKILL1].WeaponStruct, _refEntity, _refEntity.tRefPassiveskill);
			virtualButton3.AllowUpdate = false;
		}
	}

	protected void InitSkill(ButtonId[] buttonIds, WeaponStruct[] originalWeaponStruct)
	{
		if (dictSkill.Count > 0)
		{
			return;
		}
		dictSkill.Clear();
		for (int i = 0; i < originalWeaponStruct.Length; i++)
		{
			LinkSkillData linkSkillData = new LinkSkillData();
			linkSkillData.ButtonId = buttonIds[i];
			linkSkillData.IsNewWeaponStruct = false;
			int num = 0;
			if (originalWeaponStruct[i] == null)
			{
				linkSkillData.IsNewWeaponStruct = true;
				num = currentWeaponID;
			}
			else if (originalWeaponStruct[i].FastBulletDatas.Length > 1)
			{
				linkSkillData.IsNewWeaponStruct = true;
				num = originalWeaponStruct[i].FastBulletDatas[0].n_COMBO_SKILL;
			}
			else
			{
				linkSkillData.IsNewWeaponStruct = false;
				num = originalWeaponStruct[i].FastBulletDatas[0].n_ID;
			}
			if (linkSkillData.IsNewWeaponStruct)
			{
				WeaponStruct weaponStruct = new WeaponStruct();
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetNewValue(num, out weaponStruct.BulletData))
				{
					_refEntity.tRefPassiveskill.ReCalcuSkill(ref weaponStruct.BulletData);
					weaponStruct.ForceLock = false;
					weaponStruct.ChargeTimer = new UpdateTimer();
					weaponStruct.LastUseTimer = new UpdateTimer();
					weaponStruct.LastUseTimer.TimerStart();
					weaponStruct.MagazineRemain = weaponStruct.BulletData.n_MAGAZINE;
					weaponStruct.FastBulletDatas = new SKILL_TABLE[1] { weaponStruct.BulletData };
					if (!MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(weaponStruct.BulletData.s_MODEL) && weaponStruct.BulletData.s_MODEL != "null" && weaponStruct.BulletData.s_MODEL != "DUMMY")
					{
						BulletBase.PreloadBullet<BasicBullet>(weaponStruct.BulletData);
					}
					switch (buttonIds[i])
					{
					default:
					{
						WeaponStatus weaponStatus3 = new WeaponStatus();
						weaponStatus3.CopyWeaponStatus(_refEntity.PlayerWeapons[0].weaponStatus, 128);
						weaponStruct.weaponStatus = weaponStatus3;
						weaponStruct.ShootTransform = new Transform[1] { shootTransforms[3] };
						break;
					}
					case ButtonId.SKILL0:
					{
						WeaponStatus weaponStatus2 = new WeaponStatus();
						weaponStatus2.CopyWeaponStatus(_refEntity.PlayerWeapons[0].weaponStatus, 1);
						weaponStruct.weaponStatus = weaponStatus2;
						weaponStruct.ShootTransform = new Transform[1] { shootTransforms[1] };
						break;
					}
					case ButtonId.SKILL1:
					{
						WeaponStatus weaponStatus = new WeaponStatus();
						weaponStatus.CopyWeaponStatus(_refEntity.PlayerWeapons[1].weaponStatus, 2);
						weaponStruct.weaponStatus = weaponStatus;
						weaponStruct.ShootTransform = new Transform[1] { shootTransforms[2] };
						break;
					}
					}
				}
				linkSkillData.WeaponStruct = weaponStruct;
				dictSkill.Add(buttonIds[i], linkSkillData);
				continue;
			}
			for (int j = 0; j < _refEntity.PlayerSkills.Length; j++)
			{
				if (_refEntity.PlayerSkills[j].BulletData.n_ID == num)
				{
					linkSkillData.WeaponStruct = _refEntity.PlayerSkills[j];
					dictSkill.Add(buttonIds[i], linkSkillData);
					break;
				}
			}
		}
	}

	public void Link(OrangeCharacter p_entity)
	{
		if (IsLink)
		{
			return;
		}
		_refEntity = p_entity;
		_refEntity.DisableCurrentWeapon();
		Renderer[] renderer = _refEntity.CharacterMaterials.GetRenderer();
		entityRenderers = renderer;
		Renderer[] array = entityRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		_refEntity.CharacterMaterials.SetRenderer(new Renderer[0]);
		characterMaterial.Appear(null, 0.4f);
		InitSkill(originalWeaponStruct: new WeaponStruct[3]
		{
			null,
			_refEntity.PlayerSkills[0],
			_refEntity.PlayerSkills[1]
		}, buttonIds: new ButtonId[3]
		{
			ButtonId.SHOOT,
			ButtonId.SKILL0,
			ButtonId.SKILL1
		});
		InitDisplayBtn();
		dictEntityTimer.Clear();
		timers = new UpdateTimer[0];
		foreach (LinkSkillData value in dictSkill.Values)
		{
			if (!value.IsNewWeaponStruct || value.ButtonId == ButtonId.SHOOT)
			{
				continue;
			}
			for (int j = 0; j < _refEntity.PlayerSkills.Length; j++)
			{
				if (_refEntity.PlayerSkills[j].FastBulletDatas.Length > 1 && _refEntity.PlayerSkills[j].FastBulletDatas[0].n_COMBO_SKILL == value.WeaponStruct.BulletData.n_ID)
				{
					UpdateTimer updateTimer = new UpdateTimer();
					dictEntityTimer.Add(updateTimer, _refEntity.PlayerSkills[j]);
					SaveEntitySkillCD(updateTimer, _refEntity.PlayerSkills[j]);
					break;
				}
			}
		}
		timers = dictEntityTimer.Keys.ToArray();
		IsLink = true;
		_refEntity.SetHorizontalSpeed(0);
		_refEntity.PlayerStopDashing();
		_refEntity.UpdateAimRangeByWeapon(dictSkill[ButtonId.SHOOT].WeaponStruct);
		_refEntity.CurrentActiveSkill = 1;
		Vector3 localPosition = _refEntity.objInfoBar.transform.localPosition;
		_refEntity.objInfoBar.ForceSetNewPosition(new Vector3(localPosition.x, localPosition.y + hpBarExtraY, localPosition.z));
		listEntityShootTransform.Clear();
		Transform[] array2 = new Transform[10];
		Transform[] array3 = new Transform[10];
		for (int k = 0; k < 10; k++)
		{
			array2[k] = _refEntity.PlayerSkills[0].ShootTransform[k];
			array3[k] = _refEntity.PlayerSkills[1].ShootTransform[k];
		}
		listEntityShootTransform.Add(array2);
		listEntityShootTransform.Add(array3);
		_refEntity.PlayerSkills[0].ShootTransform = dictSkill[ButtonId.SKILL0].WeaponStruct.ShootTransform;
		_refEntity.PlayerSkills[1].ShootTransform = dictSkill[ButtonId.SKILL1].WeaponStruct.ShootTransform;
		SetLinkAnimationAndStatus();
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
		if (FX_LINK != string.Empty)
		{
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(FX_LINK, base.transform, Quaternion.identity, Array.Empty<object>());
		}
		if (_refEntity is OrangeConsoleCharacter)
		{
			OrangeConsoleCharacter obj = _refEntity as OrangeConsoleCharacter;
			PointerEventData pointer = new PointerEventData(EventSystem.current);
			obj.ExcuteUpHandler(ButtonId.SHOOT, pointer);
			obj.ExcuteUpHandler(ButtonId.SKILL0, pointer);
			obj.ExcuteUpHandler(ButtonId.SKILL1, pointer);
		}
	}

	public bool CancelLink()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		if (_refEntity.IsTeleporting)
		{
			return false;
		}
		if (IsTeleportOut)
		{
			return false;
		}
		_refEntity.CharacterMaterials.SetRenderer(entityRenderers);
		characterMaterial.Disappear(null, 0.4f);
		Renderer[] array = entityRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		if ((bool)_refEntity.objInfoBar)
		{
			Vector3 localPosition = _refEntity.objInfoBar.transform.localPosition;
			_refEntity.objInfoBar.ForceSetNewPosition(new Vector3(localPosition.x, localPosition.y - hpBarExtraY, localPosition.z));
		}
		_refEntity.CurrentActiveSkill = -1;
		ResetLastStatus();
		ResetDisplayBtn();
		foreach (KeyValuePair<UpdateTimer, WeaponStruct> item in dictEntityTimer)
		{
			RecoverEntitySkillCD(item.Key, item.Value);
		}
		_refEntity.PlayerSkills[0].ShootTransform = listEntityShootTransform[0];
		_refEntity.PlayerSkills[1].ShootTransform = listEntityShootTransform[1];
		IsLink = false;
		_refEntity.LeaveRider();
		_refEntity = null;
		entityRenderers = null;
		return true;
	}

	protected void UpdateEntityDirection()
	{
		if (!_refEntity.PlayerAutoAimSystem || !_refEntity.UseAutoAim)
		{
			return;
		}
		Vector3 shootDirection = _refEntity.ShootDirection;
		if (_refEntity.PlayerAutoAimSystem.AutoAimTarget != null)
		{
			shootDirection = (_refEntity.PlayerAutoAimSystem.GetTargetPoint().xy() - _refEntity._transform.position.xy()).normalized;
			int num = Math.Sign(shootDirection.x);
			if (_refEntity._characterDirection != (CharacterDirection)num && Mathf.Abs(shootDirection.x) > 0.05f)
			{
				_refEntity._characterDirection = (CharacterDirection)((int)_refEntity._characterDirection * -1);
			}
		}
	}

	public WeaponStruct GetCurrentWeaponStruct()
	{
		return dictSkill[ButtonId.SHOOT].WeaponStruct;
	}

	public void PlayerPressSelect()
	{
		if (_refEntity.CurrentActiveSkill == -1)
		{
			_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[0].ComboCheckDatas[0].nComboSkillID);
			_refEntity.PlayerSkills[0].Reload_index = 0;
		}
	}

	protected void CreateSkillBullet(ButtonId buttonId, Transform shootTransform, int bulletLevel)
	{
		CreateSkillBullet(dictSkill[buttonId].WeaponStruct.BulletData, dictSkill[buttonId].WeaponStruct.weaponStatus, shootTransform, bulletDirection, _refEntity, bulletLevel);
	}

	public void CreateSkillBullet(SKILL_TABLE tSkillTable, WeaponStatus weaponStatus, Transform shootTransform, Vector3 bulletDirection, OrangeCharacter _refEntity, int bulletLevel)
	{
		CreateBulletUseParam.ZeroParam();
		CreateBulletUseParam.tSkillTable = tSkillTable;
		CreateBulletUseParam.weaponStatus = weaponStatus;
		CreateBulletUseParam.tBuffStatus = _refEntity.selfBuffManager.sBuffStatus;
		CreateBulletUseParam.pTransform = shootTransform;
		CreateBulletUseParam.pDirection = bulletDirection;
		CreateBulletUseParam.nRecordID = _refEntity.GetNowRecordNO();
		CreateBulletUseParam.pTargetMask = _refEntity.TargetMask;
		CreateBulletUseParam.nNetID = _refEntity.nBulletRecordID++;
		CreateBulletUseParam.nDirection = _refEntity.direction;
		CreateBulletUseParam.owner = _refEntity.sPlayerName;
		CreateBulletUseParam.nBulletLV = bulletLevel;
		_refEntity.CheckUsePassiveSkill(0, CreateBulletUseParam.tSkillTable, CreateBulletUseParam.weaponStatus, CreateBulletUseParam.pTransform);
		BulletBase.TryShotBullet(CreateBulletUseParam);
	}

	public void LogicUpdate()
	{
		UpdateSkillUseTimer();
		for (int i = 0; i < timers.Length; i++)
		{
			if (timers[i].IsStarted())
			{
				UpdateTimer[] array = timers;
				int num = i;
				array[num] += GameLogicUpdateManager.m_fFrameLenMS;
			}
		}
		float value = Mathf.Abs(Vector2.SignedAngle(Vector2.up, _refEntity.ShootDirection)) / 180f;
		animator.SetFloat(hashDirection, value);
		UpdateDisplay();
	}

	public void UpdateSkillUseTimer()
	{
		foreach (LinkSkillData value in dictSkill.Values)
		{
			if (value.IsNewWeaponStruct)
			{
				value.WeaponStruct.UpdateMagazine();
				value.WeaponStruct.LastUseTimer += GameLogicUpdateManager.m_fFrameLenMS;
			}
		}
	}

	protected void SetLogicFrame(int p_triggerFrame, int p_endFrame)
	{
		nowFrame = GameLogicUpdateManager.GameFrame;
		skillEventFrame = nowFrame + p_triggerFrame;
		endFrame = nowFrame + p_endFrame;
	}

	protected bool CanUseSkl(ButtonId btnId, bool updateLockDirection = true, bool updateCD = true)
	{
		WeaponStruct weaponStruct = dictSkill[btnId].WeaponStruct;
		if (weaponStruct.LastUseTimer.GetMillisecond() < weaponStruct.BulletData.n_FIRE_SPEED || weaponStruct.MagazineRemain <= 0f || weaponStruct.ForceLock)
		{
			return false;
		}
		switch (btnId)
		{
		case ButtonId.SKILL0:
			if (!_refEntity.CheckUseSkillKeyTrigger(0, weaponStruct))
			{
				return false;
			}
			break;
		case ButtonId.SKILL1:
			if (!_refEntity.CheckUseSkillKeyTrigger(1, weaponStruct))
			{
				return false;
			}
			break;
		}
		if (updateLockDirection)
		{
			_refEntity.FreshBullet = true;
			_refEntity.IsShoot = 1;
			_refEntity.CheckLockDirection();
		}
		if (updateCD)
		{
			OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		}
		return true;
	}

	public void ClearSkill()
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			ResetLastStatus();
		}
	}

	protected void SaveEntitySkillCD(UpdateTimer p_timer, WeaponStruct p_recoverSkl)
	{
		if (p_recoverSkl.MagazineRemain <= 0f)
		{
			p_timer.SetStarted(true);
		}
		else
		{
			p_timer.TimerStop();
		}
		p_timer.SetTime(p_recoverSkl.LastUseTimer.GetTime());
	}

	protected void RecoverEntitySkillCD(UpdateTimer p_timer, WeaponStruct p_recoverSkl)
	{
		if (p_timer.IsStarted())
		{
			p_recoverSkl.MagazineRemain = 0f;
		}
		else
		{
			SKILL_TABLE bulletData = p_recoverSkl.BulletData;
			p_recoverSkl.MagazineRemain = bulletData.n_MAGAZINE;
		}
		p_recoverSkl.LastUseTimer.SetTime(p_timer.GetTime());
		p_recoverSkl.LastUseTimer.SetStarted(p_timer.IsStarted());
		p_timer.TimerStop();
	}

	public virtual void OverrideAnimator(HumanBase.AnimateId animateId)
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			animator.SetFloat(hashVelocityY, _refEntity.Velocity.y);
			if (lastID != animateId)
			{
				animator.Play(dictAnimateHash[animateId], 0);
			}
		}
	}

	protected bool PlayAnimation(HumanBase.AnimateId standAnimId, HumanBase.AnimateId jumpAnimId)
	{
		if (_refEntity.IsInGround)
		{
			_refEntity.SetAnimateId(standAnimId);
			animator.Play(dictAnimateHash[standAnimId], 0, 0f);
			return true;
		}
		_refEntity.IgnoreGravity = true;
		_refEntity.SetAnimateId(jumpAnimId);
		animator.Play(dictAnimateHash[jumpAnimId], 0, 0f);
		return false;
	}

	public void TeleportOutCharacterDepend()
	{
		IsTeleportOut = true;
		if (entityRenderers != null)
		{
			_refEntity._handMesh = new SkinnedMeshRenderer[0];
			Renderer[] array = entityRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
		_refEntity.CharacterMaterials.SetSubCharacterMaterial(base.gameObject);
	}

	public virtual void PlayVoice(Voice seId)
	{
		_refEntity.PlayVoice(seId);
	}

	public virtual void PlayCharaSE(CharaSE seId)
	{
		_refEntity.PlayCharaSE(seId);
	}
}
