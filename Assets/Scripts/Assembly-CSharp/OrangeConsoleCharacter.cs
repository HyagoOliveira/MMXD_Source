using System.IO;
using Newtonsoft.Json;
using OrangeSocket;
using StageLib;
using UnityEngine;
using UnityEngine.EventSystems;
using enums;

public class OrangeConsoleCharacter : OrangeNetCharacter
{
	private readonly int _syncInfoReset = 20;

	private InputInfo _prevSendSyncInputInfo = new InputInfo();

	private bool _virtualButtonInitialized;

	private readonly VirtualButton[] _virtualButton = new VirtualButton[6];

	public override void SetLocalPlayer(bool isLocal)
	{
		_localPlayer = isLocal;
	}

	public OrangeConsoleCharacter()
	{
		SetLocalPlayer(true);
	}

	public new void Start()
	{
		base.Start();
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_SETTING, UpdateSetting);
	}

	protected override void Initialize()
	{
		base.Initialize();
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && ManagedSingleton<ServerStatusHelper>.Instance.EnableLockStep)
		{
			base.gameObject.AddComponent<LockStepController>();
		}
	}

	public override void SetActiveFalse()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_SETTING, UpdateSetting);
		base.SetActiveFalse();
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
	}

	public override void LogicNetworkUpdate()
	{
		if (base.IsLocalPlayer && StageUpdate.gbIsNetGame && !StageUpdate.bWaitReconnect && !MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect && !bIsNpcCpy)
		{
			tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_WEAPON1_MAGZ] = (int)PlayerWeapons[0].MagazineRemain;
			tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_WEAPON2_MAGZ] = (int)PlayerWeapons[1].MagazineRemain;
			tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_SKILL1_MAGZ] = (int)PlayerSkills[0].MagazineRemain;
			tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_SKILL2_MAGZ] = (int)PlayerSkills[1].MagazineRemain;
			tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_CURRENT_WEAPON] = base.WeaponCurrent;
			tOcSyncData.dicIntParams[ESyncData.INT_PARAMS_BUFF_MEASURE] = selfBuffManager.nMeasureNow;
			string value = ((base.IAimTargetLogicUpdate as StageObjBase != null) ? (base.IAimTargetLogicUpdate as StageObjBase).sNetSerialID : "");
			tOcSyncData.dicStringParams[ESyncData.STRING_PARAMS_SERIALID] = value;
			tOcSyncData.listPerBuff = selfBuffManager.listBuffs;
			SendNetworkRuntimeData();
		}
	}

	private void SendNetworkRuntimeData()
	{
		if (base.IsLocalPlayer && !bIsNpcCpy && !(base.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify))
		{
			InputInfo inputInfo = ManagedSingleton<InputStorage>.Instance.GetInputInfo(base.sPlayerID);
			inputInfo.ShootDir = base.ShootDirection;
			inputInfo.UpdatePos = base.Controller.LogicPosition.vec3;
			inputInfo.bLockInput = base.bLockInputCtrl;
			inputInfo.nRecordNO++;
			inputInfo.tOcSyncData = tOcSyncData;
			int inputRuntimeDiff;
			int ocRuntimeDiff;
			_prevSendSyncInputInfo.MakeRuntimeDiff(inputInfo, out inputRuntimeDiff, out ocRuntimeDiff);
			if (inputInfo.nRecordNO % _syncInfoReset == 0)
			{
				inputRuntimeDiff = int.MaxValue;
				ocRuntimeDiff = int.MaxValue;
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write((byte)1);
			binaryWriter.WriteExString(base.sPlayerID);
			inputInfo.RecordByRuntimeDiff(binaryWriter, inputRuntimeDiff, ocRuntimeDiff);
			MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastToRoomIncludeSelf(memoryStream.ToArray()));
			inputInfo.CopyTo(_prevSendSyncInputInfo);
		}
	}

	public override void PlayerDead(bool bJustRemove = false)
	{
		base.PlayerDead(bJustRemove);
		LockStepController component = base.gameObject.GetComponent<LockStepController>();
		if ((bool)component)
		{
			component.OncePauseSync();
		}
	}

	protected override void TriggerDeadAfterHurt(string killer)
	{
		BroadcastPlayerDead(killer);
		PlayerDead();
	}

	private void BroadcastPlayerDead(string killerName)
	{
		if (base.IsLocalPlayer && StageUpdate.gbIsNetGame)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ShowPvpReport(killerName, sPlayerName, base.sPlayerID);
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write((byte)2);
			binaryWriter.WriteExString(base.sPlayerID);
			binaryWriter.WriteExString(killerName);
			binaryWriter.WriteExString(sPlayerName);
			MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastBinaryToRoom(memoryStream.ToArray()));
		}
	}

	private bool InitializeVirtualButton()
	{
		if (_virtualButtonInitialized)
		{
			return true;
		}
		for (int i = 0; i < _virtualButton.Length; i++)
		{
			switch (i)
			{
			default:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SHOOT);
				break;
			case 1:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SKILL0);
				break;
			case 2:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SKILL1);
				break;
			case 3:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.SELECT);
				break;
			case 4:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.FS_SKILL);
				break;
			case 5:
				_virtualButton[i] = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(ButtonId.CHIP_SWITCH);
				break;
			}
			if (_virtualButton[i] == null)
			{
				return false;
			}
			_virtualButton[i].SetMagazineRemainDirty();
		}
		_virtualButtonInitialized = true;
		UpdateWeaponIcon();
		UpdateSkillIcon(PlayerSkills);
		return true;
	}

	public override void UpdateValue()
	{
		base.UpdateValue();
		if (!base.UsingVehicle && !base.IsJacking)
		{
			UpdateVirtualButtonDisplay();
		}
	}

	public void UpdateVirtualButtonDisplay()
	{
		if (!base.IsLocalPlayer || !InitializeVirtualButton())
		{
			return;
		}
		for (int i = 0; i < _virtualButton.Length; i++)
		{
			WeaponStruct currentWeaponStruct;
			switch (i)
			{
			default:
				currentWeaponStruct = PlayerWeapons[base.WeaponCurrent];
				break;
			case 1:
				currentWeaponStruct = PlayerSkills[0];
				break;
			case 2:
				currentWeaponStruct = PlayerSkills[1];
				break;
			case 3:
				currentWeaponStruct = PlayerWeapons[(base.WeaponCurrent + 1 < PlayerWeapons.Length) ? (base.WeaponCurrent + 1) : 0];
				break;
			case 4:
				currentWeaponStruct = PlayerFSkills[base.WeaponCurrent];
				break;
			case 5:
				currentWeaponStruct = PlayerWeapons[base.WeaponCurrent];
				break;
			}
			_virtualButton[i].UpdateValue((VirtualButtonId)i, currentWeaponStruct, this, tRefPassiveskill);
		}
	}

	public void SetVirtualButtonAnalog(VirtualButtonId vBtnId, bool allowAnalog)
	{
		if (!IgnoreUpdatePad())
		{
			_virtualButton[(int)vBtnId].AllowAnalog = allowAnalog;
		}
	}

	public void ClearVirtualButtonStick(VirtualButtonId vBtnId)
	{
		if (!IgnoreUpdatePad())
		{
			_virtualButton[(int)vBtnId].ClearStick();
		}
	}

	protected override bool IsVirtualButtonReloading(VirtualButtonId id, WeaponStruct currentWeaponStruct)
	{
		if ((int)id < _virtualButton.Length)
		{
			_virtualButton[(int)id].GetReloadProgressFillAmount();
			switch (currentWeaponStruct.BulletData.n_MAGAZINE_TYPE)
			{
			case 0:
				if (currentWeaponStruct.MagazineRemain == 0f)
				{
					return true;
				}
				break;
			case 1:
				if (currentWeaponStruct.ForceLock)
				{
					return true;
				}
				break;
			case 2:
				if (selfBuffManager.nMeasureNow < currentWeaponStruct.BulletData.n_USE_COST)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	protected override void UpdateComboSkill(bool hasCombo, int nSkillID, int reloadIdx)
	{
		UpdateSkillIconByBuff(nSkillID, hasCombo ? reloadIdx : (-1));
		base.UpdateComboSkill(hasCombo, nSkillID, reloadIdx);
	}

	private void UpdateWeaponIcon()
	{
		if (IgnoreUpdatePad())
		{
			return;
		}
		int i;
		for (i = 0; i < _virtualButton.Length; i++)
		{
			WeaponStruct weaponStruct;
			switch (i)
			{
			case 0:
			{
				weaponStruct = PlayerWeapons[base.WeaponCurrent];
				WeaponType weaponType = (WeaponType)weaponStruct.WeaponData.n_TYPE;
				if (weaponType != WeaponType.Melee)
				{
					_virtualButton[i].AllowAnalog = true;
					break;
				}
				_virtualButton[i].AllowAnalog = false;
				_virtualButton[i].ClearStick();
				break;
			}
			case 3:
				weaponStruct = PlayerWeapons[(base.WeaponCurrent + 1 < PlayerWeapons.Length) ? (base.WeaponCurrent + 1) : 0];
				break;
			case 4:
				_virtualButton[i].gameObject.SetActive(false);
				_virtualButton[i].AllowAnalog = false;
				if (base.WeaponCurrent == 0)
				{
					if (PlayerFSkills[0] != null && PlayerFSkills[0].BulletData.n_ID != 0)
					{
						_virtualButton[i].SetIcon(PlayerFSkills[0].Icon);
						_virtualButton[i].gameObject.SetActive(true);
						if (PlayerFSkills[0].MagazineRemain > 0f)
						{
							_virtualButton[i].SetMaskProgress(1f);
						}
						else
						{
							_virtualButton[i].SetMaskProgress(0f);
						}
					}
				}
				else if (base.WeaponCurrent == 1 && PlayerFSkills[1] != null && PlayerFSkills[1].BulletData.n_ID != 0)
				{
					_virtualButton[i].SetIcon(PlayerFSkills[1].Icon);
					_virtualButton[i].gameObject.SetActive(true);
					if (PlayerFSkills[1].MagazineRemain > 0f)
					{
						_virtualButton[i].SetMaskProgress(1f);
					}
					else
					{
						_virtualButton[i].SetMaskProgress(0f);
					}
				}
				continue;
			case 5:
			{
				_virtualButton[i].gameObject.SetActive(false);
				if (BattleInfoUI.Instance != null)
				{
					BattleInfoUI.Instance.playersp.transform.parent.gameObject.SetActive(false);
				}
				DISC_TABLE value = null;
				if (SetPBP.WeaponChipList[base.WeaponCurrent] == 0 || !ManagedSingleton<OrangeDataManager>.Instance.DISC_TABLE_DICT.TryGetValue(SetPBP.WeaponChipList[base.WeaponCurrent], out value) || !(value.s_ICON != "") || !(value.s_ICON != "null"))
				{
					continue;
				}
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_iconChip, value.s_ICON, delegate(Sprite obj)
				{
					_virtualButton[i].SetIcon(obj);
					_virtualButton[i].gameObject.SetActive(true);
					if (BattleInfoUI.Instance != null)
					{
						BattleInfoUI.Instance.playersp.transform.parent.gameObject.SetActive(true);
					}
				});
				continue;
			}
			default:
				continue;
			}
			if (weaponStruct.Icon == null)
			{
				_virtualButton[i].gameObject.SetActive(false);
				continue;
			}
			_virtualButton[i].gameObject.SetActive(true);
			_virtualButton[i].SetIcon(weaponStruct.Icon);
		}
	}

	public override void UpdateSkillIcon(WeaponStruct[] PlayerSkills)
	{
		if (IgnoreUpdatePad())
		{
			return;
		}
		for (int i = 0; i < _virtualButton.Length; i++)
		{
			WeaponStruct weaponStruct;
			switch (i)
			{
			case 1:
				weaponStruct = PlayerSkills[0];
				break;
			case 2:
				weaponStruct = PlayerSkills[1];
				break;
			default:
				continue;
			}
			if (weaponStruct.WeaponData != null)
			{
				WeaponType weaponType = (WeaponType)weaponStruct.WeaponData.n_TYPE;
				if (weaponType != WeaponType.Melee)
				{
					_virtualButton[i].AllowAnalog = weaponStruct.BulletData.n_USE_TYPE == 1;
				}
				else
				{
					_virtualButton[i].AllowAnalog = false;
				}
			}
			else
			{
				_virtualButton[i].AllowAnalog = weaponStruct.BulletData.n_USE_TYPE == 1;
			}
			if (!(weaponStruct.Icon == null))
			{
				_virtualButton[i].SetIcon(weaponStruct.Icon);
			}
		}
	}

	public void UpdateSkillIconByBuff(int idx, int reloadIdx)
	{
		if (IgnoreUpdatePad() || base.UsingVehicle)
		{
			return;
		}
		WeaponStruct weaponStruct = PlayerSkills[idx];
		VirtualButtonId vBtnId = ((idx == 0) ? VirtualButtonId.SKILL0 : VirtualButtonId.SKILL1);
		if (reloadIdx != -1)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(weaponStruct.FastBulletDatas[reloadIdx].s_ICON), weaponStruct.FastBulletDatas[reloadIdx].s_ICON, delegate(Sprite obj)
			{
				if (obj != null)
				{
					_virtualButton[(int)vBtnId].SetIcon(obj);
				}
			});
		}
		else
		{
			_virtualButton[(int)vBtnId].SetIcon(weaponStruct.Icon);
		}
	}

	public override void ForceChangeSkillIcon(int id, string sIcon)
	{
		if (!base.IsLocalPlayer)
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(sIcon), sIcon, delegate(Sprite obj)
		{
			if (obj != null)
			{
				ForceChangeSkillIcon(id, obj);
			}
		});
	}

	public override void ForceChangeSkillIcon(int id, Sprite icon)
	{
		if (base.IsLocalPlayer && icon != null && (id == 1 || id == 2) && !(_virtualButton[id] == null))
		{
			_virtualButton[id].SetIcon(icon);
		}
	}

	public override void UpdateManualShoot(bool isManual)
	{
		if (base.IsLocalPlayer)
		{
			MonoBehaviourSingleton<InputManager>.Instance.ForceDisplayAnalog = isManual;
		}
	}

	public override void DerivedContinueCall()
	{
		if (base.sPlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
		{
			EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
			stageCameraFocus.nMode = 4;
			stageCameraFocus.bRightNow = true;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
			MonoBehaviourSingleton<InputManager>.Instance.ClearManualInput();
		}
		else
		{
			base.DerivedContinueCall();
		}
		ClearCommandQueue();
	}

	public override void UpdateCharacterInput()
	{
		if (base.IsLocalPlayer)
		{
			MonoBehaviourSingleton<InputManager>.Instance.ManualUpdate();
		}
	}

	public override void ClosePadAndPlayerUI()
	{
		base.ClosePadAndPlayerUI();
		if (base.IsLocalPlayer)
		{
			_virtualButtonInitialized = false;
			MonoBehaviourSingleton<InputManager>.Instance.DestroyVirtualPad();
		}
	}

	public override void PlayerPressSelect()
	{
		base.PlayerPressSelect();
		if (base.IsLocalPlayer && !CheckActStatusEvt(15, -1))
		{
			UpdateWeaponIcon();
		}
	}

	private void UpdateSetting()
	{
		if (base.IsLocalPlayer)
		{
			_setting = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting;
			_playerAutoAimSystem.SetEnable(base.PlayerSetting.AutoAim != 0);
			if (StageUpdate.gbIsNetGame)
			{
				string content = JsonConvert.SerializeObject(_setting);
				MemoryStream memoryStream = new MemoryStream();
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write((byte)3);
				binaryWriter.WriteExString(base.sPlayerID);
				binaryWriter.WriteExString(content);
				MonoBehaviourSingleton<CBSocketClient>.Instance.SendProtocol(FlatBufferCBHelper.CreateRQBroadcastBinaryToRoom(memoryStream.ToArray()));
			}
		}
	}

	public override bool EnterRideArmor(RideBaseObj targetRideArmor)
	{
		bool result = base.EnterRideArmor(targetRideArmor);
		if (!base.IsLocalPlayer)
		{
			return result;
		}
		ClearVirtualButtonStick(VirtualButtonId.SHOOT);
		ClearVirtualButtonStick(VirtualButtonId.SKILL0);
		ClearVirtualButtonStick(VirtualButtonId.SKILL1);
		PlayerSkills[0].BackupIcon = _virtualButton[1].GetIcon();
		PlayerSkills[1].BackupIcon = _virtualButton[2].GetIcon();
		return result;
	}

	public override void LeaveRideArmor(RideBaseObj targetRideArmor)
	{
		base.LeaveRideArmor(targetRideArmor);
		if (!IgnoreUpdatePad())
		{
			MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>().Target = base.Controller;
			VirtualButton[] virtualButton = _virtualButton;
			for (int i = 0; i < virtualButton.Length; i++)
			{
				virtualButton[i].SetMagazineRemainDirty();
			}
			if (PlayerSkills[0].BackupIcon != null)
			{
				_virtualButton[1].SetIcon(PlayerSkills[0].BackupIcon);
			}
			if (PlayerSkills[1].BackupIcon != null)
			{
				_virtualButton[2].SetIcon(PlayerSkills[1].BackupIcon);
			}
			UpdateWeaponIcon();
		}
	}

	public override void LeaveRider()
	{
		base.LeaveRider();
		if (!IgnoreUpdatePad())
		{
			VirtualButton[] virtualButton = _virtualButton;
			for (int i = 0; i < virtualButton.Length; i++)
			{
				virtualButton[i].SetMagazineRemainDirty();
			}
			UpdateWeaponIcon();
			PointerEventData pointer = new PointerEventData(EventSystem.current);
			ExcuteUpHandler(ButtonId.SHOOT, pointer);
			ExcuteUpHandler(ButtonId.SKILL0, pointer);
			ExcuteUpHandler(ButtonId.SKILL1, pointer);
			ClearVirtualButtonStick(VirtualButtonId.SHOOT);
			ClearVirtualButtonStick(VirtualButtonId.SKILL0);
			ClearVirtualButtonStick(VirtualButtonId.SKILL1);
			UpdateSkillIcon(PlayerSkills);
		}
	}

	public void ExcuteUpHandler(ButtonId btnId, PointerEventData pointer)
	{
		VirtualButton virtualButton = MonoBehaviourSingleton<InputManager>.Instance.GetVirtualButton(btnId);
		if ((bool)virtualButton)
		{
			ExecuteEvents.Execute(virtualButton.gameObject, pointer, ExecuteEvents.pointerUpHandler);
		}
	}

	private bool IgnoreUpdatePad()
	{
		if (!base.IsLocalPlayer)
		{
			return true;
		}
		if (!_virtualButtonInitialized)
		{
			return true;
		}
		return false;
	}
}
