using System;
using MagicaCloth;
using UnityEngine;

public class CH107_Controller : CharacterControlBase, ILogicUpdate
{
	private int nowFrame;

	private int skillEventFrame;

	private int endFrame;

	private bool isSkillEventEnd;

	private int endBreakFrame;

	private bool isLightningStatus;

	private Vector3 skl1Offset = new Vector3(1.5f, 0f, 0f);

	private FxBase fxLightning;

	private FxBase fxUseSkl2;

	private CharacterMaterial hairNormal;

	private CharacterMaterial hairLightning;

	private CharacterMaterial body;

	private MagicaBoneCloth[] boneCloths;

	private bool spIsFull = true;

	private readonly string sCustomShootPoint = "CustomShootPoint";

	private readonly string fx_teleportIn = "fxdemo_zinogre_003";

	private readonly string fx_teleportOut = "fxdemo_zinogre_004";

	private readonly string fx_teleportOut2 = "fxdemo_zinogre_005";

	private readonly string fx_lightning = "fxduring_zinogre_000";

	private readonly string fx_lightning_stop = "fxduring_zinogre_001";

	private readonly string fxuse_skl1 = "fxuse_lightbowgun_001";

	private readonly int SKL0_TRIGGER = (int)(0.13f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END = (int)(0.667f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL0_END_BREAK = (int)(0.333f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_TRIGGER = (int)(0.25f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END = (int)(0.625f / GameLogicUpdateManager.m_fFrameLen);

	private readonly int SKL1_END_BREAK = (int)(0.3f / GameLogicUpdateManager.m_fFrameLen);

	private void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public override void Start()
	{
		base.Start();
		InitializeSkill();
	}

	public override void OverrideDelegateEvent()
	{
		base.OverrideDelegateEvent();
	}

	private void InitializeSkill()
	{
		_refEntity._transform.GetComponentsInChildren<Transform>(true);
		Transform transform = new GameObject(sCustomShootPoint + "0").transform;
		transform.SetParent(base.transform);
		transform.localPosition = new Vector3(0f, 2.1f, 0f);
		Transform transform2 = new GameObject(sCustomShootPoint + "1").transform;
		transform2.SetParent(base.transform);
		transform2.localPosition = skl1Offset;
		transform2.transform.localScale = new Vector3(1f, 1f, 1f);
		_refEntity.ExtraTransforms = new Transform[2] { transform, transform2 };
		CharacterMaterial[] components = _refEntity.CharacterMaterials.gameObject.GetComponents<CharacterMaterial>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].GetRenderer().Length == 1)
			{
				if (components[i].GetRenderer()[0].name == "HairMesh_01_c")
				{
					hairNormal = components[i];
				}
				else if (components[i].GetRenderer()[0].name == "HairMesh_02_c")
				{
					hairLightning = components[i];
				}
			}
			else
			{
				body = components[i];
			}
		}
		boneCloths = _refEntity.CharacterMaterials.gameObject.GetComponentsInChildren<MagicaBoneCloth>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fx_teleportIn);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fx_teleportOut);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fx_teleportOut2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fx_lightning);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fx_lightning_stop);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxuse_skl1);
	}

	public override void PlayerPressSkillCharacterCall(int id)
	{
		if (_refEntity.CurrentActiveSkill != -1)
		{
			return;
		}
		switch (id)
		{
		case 0:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ir3_skill01");
				_refEntity.CurrentActiveSkill = id;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				int reload_index = _refEntity.GetCurrentSkillObj().Reload_index;
				OrangeCharacter.SubStatus p_nextStatus2 = ((reload_index == 1) ? OrangeCharacter.SubStatus.SKILL0_1 : OrangeCharacter.SubStatus.SKILL0);
				fxUseSkl2 = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(fxuse_skl1, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL0_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL0_TRIGGER, SKL0_END, p_nextStatus2, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, HumanBase.AnimateId.ANI_SKILL_START, (HumanBase.AnimateId)66u, (HumanBase.AnimateId)67u);
			}
			break;
		case 1:
			if (_refEntity.CheckUseSkillKeyTrigger(id))
			{
				PlayVoiceSE("v_ir3_skill02");
				PlaySkillSE("ir3_raigeki");
				_refEntity.CurrentActiveSkill = id;
				ManagedSingleton<CharacterControlHelper>.Instance.TurnToAimTarget(_refEntity);
				_refEntity.ExtraTransforms[1].localPosition = skl1Offset * (float)_refEntity._characterDirection;
				int reload_index = _refEntity.GetCurrentSkillObj().Reload_index;
				OrangeCharacter.SubStatus p_nextStatus = ((reload_index == 1) ? OrangeCharacter.SubStatus.SKILL1_1 : OrangeCharacter.SubStatus.SKILL1);
				endBreakFrame = GameLogicUpdateManager.GameFrame + SKL1_END_BREAK;
				ManagedSingleton<CharacterControlHelper>.Instance.ChangeToSklStatus(_refEntity, id, SKL1_TRIGGER, SKL1_END, p_nextStatus, out skillEventFrame, out endFrame);
				ManagedSingleton<CharacterControlHelper>.Instance.SetAnimate(_refEntity, (HumanBase.AnimateId)68u, (HumanBase.AnimateId)69u, (HumanBase.AnimateId)70u);
				SetBoneClothWeight(0f);
			}
			break;
		}
	}

	public void LogicUpdate()
	{
		CheckLightninBuff();
		PerBuffManager refPBM = _refEntity.selfBuffManager.sBuffStatus.refPBM;
		if (!spIsFull)
		{
			if (refPBM.nMeasureNow == refPBM.nMeasureMax)
			{
				spIsFull = true;
			}
		}
		else if (refPBM.nMeasureNow != refPBM.nMeasureMax)
		{
			spIsFull = false;
		}
	}

	public override void PlayerReleaseSkillCharacterCall(int id)
	{
	}

	public override void CheckSkill()
	{
		if (_refEntity.CurMainStatus != OrangeCharacter.MainStatus.SKILL || _refEntity.IsAnimateIDChanged() || _refEntity.CurrentActiveSkill == -1)
		{
			return;
		}
		nowFrame = GameLogicUpdateManager.GameFrame;
		OrangeCharacter.MainStatus curMainStatus = _refEntity.CurMainStatus;
		if (curMainStatus != OrangeCharacter.MainStatus.SKILL)
		{
			return;
		}
		switch (_refEntity.CurSubStatus)
		{
		case OrangeCharacter.SubStatus.SKILL0:
		case OrangeCharacter.SubStatus.SKILL0_1:
			if (nowFrame >= endFrame)
			{
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				_refEntity.ShootDirection = Vector2.right * _refEntity.direction;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[0], MagazineType.ENERGY, _refEntity.GetCurrentSkillObj().Reload_index, 0);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				CheckBreakFrame();
			}
			break;
		case OrangeCharacter.SubStatus.SKILL1:
		case OrangeCharacter.SubStatus.SKILL1_1:
			if (nowFrame >= endFrame)
			{
				SetBoneClothWeight(1f);
				OnSkillEnd();
			}
			else if (!isSkillEventEnd && nowFrame >= skillEventFrame)
			{
				isSkillEventEnd = true;
				ManagedSingleton<CharacterControlHelper>.Instance.PushBulletSkl(_refEntity, _refEntity.ExtraTransforms[1], MagazineType.ENERGY, _refEntity.GetCurrentSkillObj().Reload_index, 0);
			}
			else if (isSkillEventEnd && nowFrame >= endBreakFrame)
			{
				CheckBreakFrame();
			}
			break;
		}
	}

	private void CheckLightninBuff()
	{
		if (_refEntity.PlayerSkills.Length == 0)
		{
			return;
		}
		if (_refEntity.PlayerSkills[0].Reload_index == 1)
		{
			if (!isLightningStatus)
			{
				PlayLightningFx();
			}
		}
		else if (isLightningStatus)
		{
			StopLightningFx();
		}
	}

	public override void ClearSkill()
	{
		_refEntity.EnableCurrentWeapon();
		if ((bool)fxUseSkl2)
		{
			fxUseSkl2.BackToPool();
		}
		SetBoneClothWeight(1f);
		isSkillEventEnd = false;
		_refEntity.IgnoreGravity = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
	}

	public override void SetStun(bool enable)
	{
		if (enable)
		{
			_refEntity.EnableCurrentWeapon();
		}
	}

	private void CheckBreakFrame()
	{
		if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.LEFT) || ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.RIGHT))
		{
			endFrame = nowFrame + 1;
		}
	}

	private void OnSkillEnd()
	{
		_refEntity.IgnoreGravity = false;
		isSkillEventEnd = false;
		_refEntity.SkillEnd = true;
		_refEntity.CurrentActiveSkill = -1;
		_refEntity.EnableCurrentWeapon();
		switch (_refEntity.AnimateID)
		{
		default:
			_refEntity.SetStatus(OrangeCharacter.MainStatus.FALL, OrangeCharacter.SubStatus.TELEPORT_POSE);
			break;
		case (HumanBase.AnimateId)66u:
		case (HumanBase.AnimateId)69u:
			_refEntity.Dashing = false;
			_refEntity.SetSpeed(0, 0);
			_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.IDLE);
			break;
		case HumanBase.AnimateId.ANI_SKILL_START:
		case (HumanBase.AnimateId)68u:
			_refEntity.Dashing = false;
			if (ManagedSingleton<InputStorage>.Instance.IsHeld(_refEntity.UserID, ButtonId.DOWN))
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.CROUCH, OrangeCharacter.SubStatus.WIN_POSE);
			}
			else
			{
				_refEntity.SetStatus(OrangeCharacter.MainStatus.IDLE, OrangeCharacter.SubStatus.CROUCH_UP);
			}
			break;
		}
	}

	private void PlayLightningFx()
	{
		isLightningStatus = true;
		_refEntity.CharacterMaterials.ClearSubCharacterMaterial();
		_refEntity.CharacterMaterials.SetSubCharacterMaterial(hairLightning);
		hairLightning.Appear(null, 0.3f);
		hairNormal.Disappear(null, 1f);
		body.UpdateEmission(2f);
		RefreashLightningFx(true);
	}

	private void StopLightningFx()
	{
		isLightningStatus = false;
		_refEntity.CharacterMaterials.ClearSubCharacterMaterial();
		_refEntity.CharacterMaterials.SetSubCharacterMaterial(hairNormal);
		hairNormal.Appear(null, 0.3f);
		hairLightning.Disappear(null, 1f);
		body.UpdateEmission(0f);
		RefreashLightningFx(false);
	}

	private void RefreashLightningFx(bool play)
	{
		if (fxLightning != null)
		{
			fxLightning.BackToPool();
			fxLightning = null;
			if (!play)
			{
				MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(fx_lightning_stop, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
			}
		}
		if (play)
		{
			fxLightning = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(fx_lightning, _refEntity.ModelTransform, Quaternion.identity, Array.Empty<object>());
		}
	}

	private void SetBoneClothWeight(float val)
	{
		MagicaBoneCloth[] array = boneCloths;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].BlendWeight = val;
		}
	}

	public override string[] GetCharacterDependAnimations()
	{
		return new string[6] { "ch107_skill_01_crouch", "ch107_skill_01_stand", "ch107_skill_01_jump", "ch107_skill_02_crouch", "ch107_skill_02_stand", "ch107_skill_02_jump" };
	}
}
