using OrangeCriRelay;
using UnityEngine;

[RequireComponent(typeof(OrangeCharacter))]
public abstract class CharacterControlBase : MonoBehaviour
{
	protected OrangeCharacter _refEntity;

	public virtual void Start()
	{
		LinkEntityReference();
	}

	public void LinkEntityReference(OrangeCharacter refEntity)
	{
		if (_refEntity != refEntity)
		{
			_refEntity = refEntity;
			Setup();
		}
	}

	public void LinkEntityReference()
	{
		if (_refEntity == null)
		{
			_refEntity = GetComponent<OrangeCharacter>();
			Setup();
		}
	}

	protected virtual void Setup()
	{
	}

	public void PlayVoiceSE(string cue)
	{
		_refEntity.PlaySE(_refEntity.VoiceID, cue);
	}

	public void PlayVoiceSE(int cue)
	{
		_refEntity.PlaySE(_refEntity.VoiceID, cue);
	}

	public void PlaySkillSE(string cue)
	{
		_refEntity.PlaySE(_refEntity.SkillSEID, cue);
	}

	public void PlaySkillSE(int cue)
	{
		_refEntity.PlaySE(_refEntity.SkillSEID, cue);
	}

	public void PlayCharaSE(string cue)
	{
		_refEntity.PlaySE(_refEntity.CharaSEID, cue);
	}

	public void PlayCharaSE(int cue)
	{
		_refEntity.PlaySE(_refEntity.CharaSEID, cue);
	}

	public void PlaySE(string acb, string cue)
	{
		_refEntity.PlaySE(acb, cue);
	}

	public void PlaySE(string acb, int cue)
	{
		_refEntity.PlaySE(acb, cue);
	}

	public void PlayPetSE(string acb = null, string cue = null)
	{
		if (acb == null || cue == null)
		{
			CharacterParam callPetParam = _refEntity.CallPetParam;
			if (callPetParam != null)
			{
				_refEntity.PlaySE(callPetParam.PetSE[0], callPetParam.PetSE[1], callPetParam.Delay);
			}
		}
		else
		{
			_refEntity.PlaySE(acb, cue);
		}
	}

	public virtual void OverrideDelegateEvent()
	{
		_refEntity.CheckSkillEvt = CheckSkill;
		_refEntity.ClearSkillEvt = ClearSkill;
		_refEntity.PlayerPressSkillCharacterCallCB = PlayerPressSkillCharacterCall;
		_refEntity.PlayerReleaseSkillCharacterCallCB = PlayerReleaseSkillCharacterCall;
		_refEntity.CanPlayerPressSkillFunc = CanPlayerPressSkill;
	}

	public abstract void CheckSkill();

	public abstract void ClearSkill();

	public abstract void PlayerPressSkillCharacterCall(int id);

	public abstract void PlayerReleaseSkillCharacterCall(int id);

	public virtual void CreateSkillBullet(WeaponStruct weaponStruct)
	{
	}

	public virtual void ControlCharacterDead()
	{
	}

	public virtual void ControlCharacterContinue()
	{
	}

	public virtual void ExtraVariableInit()
	{
	}

	public virtual void RemovePet()
	{
	}

	public virtual void SetStun(bool enable)
	{
	}

	public virtual void CallPet(int nPetID, bool isHurt, int nSetNumID, Vector3? vSetPos)
	{
	}

	public virtual bool CheckMyShield(Transform tfObject)
	{
		return false;
	}

	public virtual PlayerCollider GetMyShield(bool checkUsing)
	{
		return null;
	}

	public virtual int ShieldDmgReduce(HurtPassParam tHurtPassParam)
	{
		return 100;
	}

	public virtual float GetCurrentAimRange()
	{
		return -1f;
	}

	public virtual bool GuardCalculate(HurtPassParam tHurtPassParam)
	{
		return false;
	}

	public virtual string GetTeleportInExtraEffect()
	{
		return string.Empty;
	}

	public virtual void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[1] { "null" };
		target = new string[1] { "null" };
	}

	public virtual void GetUniqueWeaponMotion(out string[] source, out string[] target)
	{
		source = new string[1] { "null" };
		target = new string[1] { "null" };
	}

	public virtual string[] GetCharacterDependBlendAnimations()
	{
		return null;
	}

	public virtual string[] GetCharacterDependAnimations()
	{
		return null;
	}

	public virtual string[][] GetCharacterDependAnimationsBlendTree()
	{
		return null;
	}

	public virtual int GetUniqueWeaponType()
	{
		return 0;
	}

	public virtual int JumpSpeed()
	{
		return OrangeCharacter.JumpSpeed;
	}

	public virtual int DashSpeed()
	{
		return OrangeCharacter.DashSpeed;
	}

	public virtual int WallSlideGravity()
	{
		return OrangeCharacter.MaxWallSlideGravity.i;
	}

	protected virtual bool CanPlayerPressSkill(int skillID)
	{
		return _refEntity.CanPlayerPressSkill(skillID);
	}

	public virtual void SetRushBullet(RushCollideBullet rushCollideBullet)
	{
	}

	public virtual void SyncSkillDirection(Vector3 dir, IAimTarget target)
	{
	}
}
