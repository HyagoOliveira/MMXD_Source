using System;
using System.Collections.Generic;
using UnityEngine;

public class ZeroController : CharacterControlBase
{
	protected VInt3 oriPos;

	protected int oriVY;

	protected float _nowHSTime;

	protected List<FxController> SkillHitStopFXList = new List<FxController>();

	protected int ryuenjinCount;

	protected bool bInkill;

	protected bool bStartJump;

	protected string ryuenjinEffect = "fxuse_flameblade_000_effect_f";

	protected string ryuenjinBladeEff = "fxuse_flameblade_000_blade_f";

	protected string ryuenjinEffectEX = "fxuse_flameblade_stronger_skill_000";

	protected string ryuenjinBladeEffEX = "fxuse_flameblade_stronger_blade_000";

	protected string fallingEffect = "fxuse_fallen_000_f";

	protected FxBase fx_ryuennjin;

	protected FxController fx_ryuennjin_blade;

	public override string[] GetCharacterDependAnimations()
	{
		return new string[2] { "ch002_skill_02", "ch002_skill_01_end" };
	}

	public override string[] GetCharacterDependBlendAnimations()
	{
		return new string[2] { "ch002_skill_01_start", "ch002_skill_01_Loop" };
	}

	public override void Start()
	{
		base.Start();
		_refEntity.ExtraTransforms = new Transform[1];
		Transform[] target = _refEntity._transform.GetComponentsInChildren<Transform>(true);
		_refEntity.ExtraTransforms[0] = OrangeBattleUtility.FindChildRecursive(ref target, "Bip", true);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(ryuenjinEffect, 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(ryuenjinBladeEff, 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(ryuenjinEffectEX, 5);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(ryuenjinBladeEffEX, 5);
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
		_refEntity.AnimationEndCharacterDependEvt = AnimationEndCharacterDepend;
	}

	public override void ClearSkill()
	{
		switch (_refEntity.CurrentActiveSkill)
		{
		case 0:
			_refEntity.ActivateMeleeAttack(_refEntity.PlayerSkills[0]);
			_refEntity.ReleaseJack = true;
			resetHitStopParm();
			_refEntity.EnableCurrentWeapon();
			bStartJump = false;
			break;
		case 1:
			_refEntity.EnableCurrentWeapon();
			break;
		}
	}

	public override void CheckSkill()
	{
		if (_refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.TELEPORT_POSE:
			if ((double)_refEntity.CurrentFrame > 0.38 && bInkill)
			{
				bInkill = false;
				_refEntity.PlaySE(_refEntity.SkillSEID, 7);
				OrangeBattleUtility.UpdateSkillCD(_refEntity.GetCurrentSkillObj());
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fallingEffect, OrangeBattleUtility.FindChildRecursive(_refEntity._transform, "HandMesh_R"), (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
				CreateSkillBullet(_refEntity.GetCurrentSkillObj());
				_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
			}
			break;
		case OrangeCharacter.SubStatus.SLASH_SKILL0_START:
			if (_refEntity.Controller.Collisions.below)
			{
				if (_refEntity.CurrentFrame > 0.7f && !bStartJump)
				{
					_refEntity.SetSpeed((int)_refEntity._characterDirection * OrangeCharacter.WalkSpeed, Mathf.RoundToInt((float)OrangeCharacter.JumpSpeed * 1.1f));
					if (_refEntity.PlayerSkills[0].ComboCheckDatas.Length != 0 && _refEntity.PlayerSkills[0].ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
					{
						_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
						_refEntity.RemoveComboSkillBuff(_refEntity.PlayerSkills[0].ComboCheckDatas[0].nComboSkillID);
					}
					else
					{
						_refEntity.CheckUsePassiveSkill(_refEntity.CurrentActiveSkill, _refEntity.GetCurrentSkillObj().weaponStatus, _refEntity.GetCurrentSkillObj().ShootTransform[0]);
					}
					bStartJump = true;
				}
				else
				{
					if (!bStartJump)
					{
						break;
					}
					if ((_refEntity.Velocity.y <= 0 && !_refEntity.IsHitStop) || ryuenjinCount > 4)
					{
						ryuennjinEnd();
					}
					else
					{
						if (!_refEntity.IsHitStop)
						{
							break;
						}
						if (_nowHSTime == 0f)
						{
							oriPos = _refEntity.Controller.LogicPosition;
							oriVY = _refEntity.Velocity.y;
						}
						if (_nowHSTime < OrangeBattleUtility.HitStopTime)
						{
							_refEntity.Controller.LogicPosition = oriPos;
							_nowHSTime += GameLogicUpdateManager.g_fixFrameLenFP.scalar;
							{
								foreach (FxController skillHitStopFX in SkillHitStopFXList)
								{
									skillHitStopFX.PauseAll();
									skillHitStopFX.SumHSTime += GameLogicUpdateManager.g_fixFrameLenFP.scalar;
								}
								break;
							}
						}
						_nowHSTime = 0f;
						ryuenjinCount++;
						_refEntity.SetSpeed(0, oriVY / 100 * 90);
						_refEntity.IsHitStop = false;
						{
							foreach (FxController skillHitStopFX2 in SkillHitStopFXList)
							{
								skillHitStopFX2.PlayAll();
							}
							break;
						}
					}
				}
				break;
			}
			if ((_refEntity.Velocity.y <= 0 && !_refEntity.IsHitStop) || ryuenjinCount > 4)
			{
				ryuennjinEnd();
				break;
			}
			if (_nowHSTime == 0f)
			{
				oriPos = _refEntity.Controller.LogicPosition;
				oriVY = _refEntity.Velocity.y;
			}
			if (!_refEntity.IsHitStop)
			{
				break;
			}
			if (_nowHSTime < OrangeBattleUtility.HitStopTime)
			{
				_refEntity.Controller.LogicPosition = oriPos;
				_nowHSTime += GameLogicUpdateManager.g_fixFrameLenFP.scalar;
				{
					foreach (FxController skillHitStopFX3 in SkillHitStopFXList)
					{
						skillHitStopFX3.PauseAll();
						skillHitStopFX3.SumHSTime += GameLogicUpdateManager.g_fixFrameLenFP.scalar;
					}
					break;
				}
			}
			_nowHSTime = 0f;
			ryuenjinCount++;
			_refEntity.SetSpeed(0, oriVY / 100 * 90);
			_refEntity.IsHitStop = false;
			{
				foreach (FxController skillHitStopFX4 in SkillHitStopFXList)
				{
					skillHitStopFX4.PlayAll();
				}
				break;
			}
		}
	}

	public virtual void AnimationEndCharacterDepend(OrangeCharacter.MainStatus mainStatus, OrangeCharacter.SubStatus subStatus)
	{
		if (mainStatus == OrangeCharacter.MainStatus.FALL && subStatus == OrangeCharacter.SubStatus.IDLE)
		{
			if (_refEntity.PreBelow)
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.SKILL_IDLE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.IDLE);
			}
			_refEntity.UpdateWeaponMesh(_refEntity.GetCurrentWeaponObj(), _refEntity.PlayerSkills[1]);
		}
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		switch (id)
		{
		case 0:
			_refEntity.ReleaseJack = false;
			if (((_refEntity.CurrentActiveSkill != id && _refEntity.Controller.Collisions.below) || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.SkillEnd = false;
				_refEntity.SetSpeed(0, 0);
				if (_refEntity.PlayerSkills[0].ComboCheckDatas.Length != 0 && _refEntity.PlayerSkills[0].ComboCheckDatas[0].CheckHasAllBuff(_refEntity.selfBuffManager))
				{
					fx_ryuennjin = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(ryuenjinEffectEX, _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					fx_ryuennjin_blade = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxController>(ryuenjinBladeEffEX, _refEntity.PlayerSkills[id].WeaponMesh[0].transform, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					SkillHitStopFXList.Add(fx_ryuennjin_blade);
				}
				else
				{
					fx_ryuennjin = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(ryuenjinEffect, _refEntity._transform, (_refEntity._characterDirection == CharacterDirection.LEFT) ? OrangeCharacter.NormalQuaternion : OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					fx_ryuennjin_blade = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxController>(ryuenjinBladeEff, _refEntity.PlayerSkills[id].WeaponMesh[0].transform, OrangeCharacter.ReversedQuaternion, Array.Empty<object>());
					SkillHitStopFXList.Add(fx_ryuennjin_blade);
				}
				_refEntity.PlaySE(_refEntity.VoiceID, 8);
				ryuennjinStart(ref _refEntity.PlayerSkills[id], id, OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.SLASH_SKILL0_START);
				_refEntity.CurrentActiveSkill = id;
				_refEntity.StopShootTimer();
				_refEntity.StartJumpThroughCorutine();
			}
			break;
		case 1:
			if (_refEntity.CurrentActiveSkill != id && (_refEntity.Controller.Collisions.below || _refEntity.Controller.Collisions.JSB_below) && _refEntity.CheckUseSkillKeyTrigger(id))
			{
				_refEntity.PlaySE(_refEntity.VoiceID, 10);
				_refEntity.CurrentActiveSkill = id;
				bInkill = true;
				_refEntity.SkillEnd = false;
				_refEntity.DisableCurrentWeapon();
				_refEntity.SetSpeed(0, 0);
				_refEntity.SetStatus(OrangeCharacter.MainStatus.SKILL, OrangeCharacter.SubStatus.TELEPORT_POSE);
				_refEntity.StopShootTimer();
			}
			break;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public override void CreateSkillBullet(WeaponStruct weaponStruct)
	{
		Vector3 position = _refEntity.ExtraTransforms[0].position;
		position.x += (float)_refEntity.direction * -0.25f;
		_refEntity.PushBulletDetail(weaponStruct.BulletData, weaponStruct.weaponStatus, position, weaponStruct.SkillLV, Vector3.up);
	}

	private void resetHitStopParm()
	{
		_refEntity.IsHitStop = false;
		_nowHSTime = 0f;
		SkillHitStopFXList.Clear();
		ryuenjinCount = 0;
	}

	private void ryuennjinEnd()
	{
		_refEntity.SkillEnd = true;
		_refEntity.SetHorizontalSpeed(0);
		_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.IDLE);
		_refEntity.ActivateMeleeAttack(_refEntity.PlayerSkills[0]);
		foreach (FxController skillHitStopFX in SkillHitStopFXList)
		{
			skillHitStopFX.StopAll();
		}
		if (_refEntity.CurrentFrame > 1f)
		{
			_refEntity.ReleaseJack = true;
		}
		bStartJump = false;
		resetHitStopParm();
	}

	private void ryuennjinStart(ref WeaponStruct weaponStruct, int id, OrangeCharacter.MainStatus targetMainStatus, OrangeCharacter.SubStatus targetSubStatus)
	{
		if (targetMainStatus == OrangeCharacter.MainStatus.NONE || targetSubStatus == OrangeCharacter.SubStatus.NONE || weaponStruct.MagazineRemain <= 0f || weaponStruct.ForceLock)
		{
			return;
		}
		if (_refEntity.CurrentActiveSkill != id)
		{
			_refEntity.CurrentActiveSkill = id;
			_refEntity.UpdateWeaponMesh(weaponStruct, _refEntity.GetCurrentWeaponObj());
		}
		OrangeBattleUtility.UpdateSkillCD(weaponStruct);
		_refEntity.SetStatus(targetMainStatus, targetSubStatus);
		_refEntity.SetMeleeStatus(weaponStruct);
		SKILL_TABLE sKILL_TABLE = weaponStruct.FastBulletDatas[0];
		if (sKILL_TABLE.n_COMBO_SKILL != 0)
		{
			bool flag = false;
			if (sKILL_TABLE.s_COMBO != "null")
			{
				string[] command = sKILL_TABLE.s_COMBO.Split(',');
				flag = _refEntity.CheckCanCombo(command);
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			for (int i = 1; i < weaponStruct.FastBulletDatas.Length; i++)
			{
				if (sKILL_TABLE.n_COMBO_SKILL == weaponStruct.FastBulletDatas[i].n_ID)
				{
					weaponStruct.FastBulletDatas[0] = weaponStruct.FastBulletDatas[i];
					_refEntity.ForceChangeSkillIcon(id + 1, weaponStruct.FastBulletDatas[0].s_ICON);
				}
			}
		}
		else
		{
			weaponStruct.FastBulletDatas[0] = weaponStruct.BulletData;
			_refEntity.ForceChangeSkillIcon(id + 1, weaponStruct.Icon);
		}
	}

	public override void SetStun(bool enable)
	{
		if (fx_ryuennjin != null)
		{
			fx_ryuennjin.BackToPool();
			fx_ryuennjin = null;
		}
		if (fx_ryuennjin_blade != null)
		{
			fx_ryuennjin_blade.StopAll();
			fx_ryuennjin_blade.BackToPool();
			fx_ryuennjin_blade = null;
		}
	}
}
