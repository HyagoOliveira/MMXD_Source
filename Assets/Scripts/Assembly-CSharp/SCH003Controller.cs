using System;
using CallbackDefs;
using Newtonsoft.Json;
using UnityEngine;

public class SCH003Controller : PetControllerBase
{
	private bool isATK;

	private int nFlyFrame;

	private int nFlyStatus;

	private ParticleSystem mskilleffectpts;

	private Transform mskilleffect;

	private Vector3 mEffect_Pos = new Vector3(0f, 0f, 0f);

	public RollController _master_Roll;

	public override string[] GetPetDependAnimations()
	{
		return new string[3] { "sch003_teleport_in", "sch003_put_item_out_jump", "sch003_fall_loop" };
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		Transform[] target = _transform.GetComponentsInChildren<Transform>(true);
		mskilleffect = OrangeBattleUtility.FindChildRecursive(ref target, "skilleffect", true);
		mskilleffectpts = mskilleffect.GetComponent<ParticleSystem>();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_rolls_pet_skill", 2);
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxhit_item_000");
	}

	protected override void AfterActive()
	{
		base.SoundSource.PlaySE("BattleSE02", "bt_edy01");
	}

	protected override void AfterDeactive()
	{
		base.SoundSource.PlaySE("BattleSE02", "bt_edy02");
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		UpdateMagazine();
		switch (nFlyStatus)
		{
		case 1:
			if (nFlyFrame > 0)
			{
				nFlyFrame--;
				break;
			}
			nFlyStatus = 2;
			nFlyFrame = 5;
			mskilleffectpts.Play();
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxuse_rolls_pet_skill", base.transform.position, Quaternion.identity, Array.Empty<object>());
			break;
		case 2:
		case 3:
			if (nFlyFrame > 0)
			{
				nFlyFrame--;
				mEffect_Pos.z += 0.05f;
				if (nFlyStatus == 2)
				{
					mEffect_Pos.y += 0.1f;
				}
				else
				{
					mEffect_Pos.y -= 0.1f;
				}
				mskilleffect.transform.localPosition = mEffect_Pos;
			}
			else
			{
				nFlyStatus = 3;
				nFlyFrame = 5;
			}
			break;
		}
	}

	protected override void AnimationEndDepend(PetHumanBase.MainStatus mainStatus, PetHumanBase.SubStatus subStatus)
	{
		int animateStatusId = AnimatorBase.GetAnimateStatusId(5);
		if (AnimatorBase.isCurrenAnimeId(animateStatusId) && _animateID == (PetHumanBase.PetAnimateId)5u)
		{
			SetActive(false);
			mskilleffectpts.Stop();
			MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>("fxhit_item_000", _follow_Player.transform, Quaternion.identity, Array.Empty<object>());
			if (_follow_Player != null)
			{
				_follow_Player.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_GETITEM01);
			}
		}
	}

	protected override void FollowEnd()
	{
		SetFollowEnabled(false);
		SetAnimateId((PetHumanBase.PetAnimateId)5u);
		nFlyFrame = 5;
		nFlyStatus = 1;
	}

	protected override void UpdateFollowPos()
	{
		if (!FollowEnabled)
		{
			return;
		}
		if (_follow_Player._characterDirection == CharacterDirection.RIGHT)
		{
			mFollow_Target = new VInt3(new Vector3(_follow_Player.transform.position.x - FollowOffset.x, _follow_Player.transform.position.y + FollowOffset.y, _follow_Player.transform.position.z));
		}
		else
		{
			mFollow_Target = new VInt3(new Vector3(_follow_Player.transform.position.x + FollowOffset.x, _follow_Player.transform.position.y + FollowOffset.y, _follow_Player.transform.position.z));
		}
		if (Controller.LogicPosition.x != mFollow_Target.x)
		{
			int num = mFollow_Target.x - Controller.LogicPosition.x;
			if (num > 5 || num < -5)
			{
				_velocity.x = (mFollow_Target.x - Controller.LogicPosition.x) * 10;
			}
			else
			{
				_velocity.x = mFollow_Target.x - Controller.LogicPosition.x;
			}
			b_follow_pos_end = false;
		}
		else
		{
			_velocity.x = 0;
		}
		if (Controller.LogicPosition.y != mFollow_Target.y)
		{
			int num2 = mFollow_Target.y - Controller.LogicPosition.y;
			if (num2 > 5 || num2 < -5)
			{
				_velocity.y = (mFollow_Target.y - Controller.LogicPosition.y) * 10;
			}
			else
			{
				_velocity.y = mFollow_Target.y - Controller.LogicPosition.y;
			}
			b_follow_pos_end = false;
		}
		else
		{
			_velocity.y = 0;
		}
		int num3 = OldLogicPosition.x - Controller.LogicPosition.x;
		int num4 = OldLogicPosition.y - Controller.LogicPosition.y;
		if (num3 < 50 && num3 > -50 && num4 < 50 && num4 > -50)
		{
			if (!b_follow_pos_end)
			{
				_velocity.y = (_velocity.x = 0);
				FollowEnd();
				b_follow_pos_end = true;
			}
		}
		else
		{
			OldLogicPosition = Controller.LogicPosition;
		}
	}

	public override void UpdateStatus(int nSet, string smsg, Callback tCB = null)
	{
		if (nSet == 1)
		{
			int n_rand_index = JsonConvert.DeserializeObject<int>(smsg);
			_master_Roll.n_rand_index = n_rand_index;
		}
	}

	public void Player_Effect(int mDirection)
	{
		nFlyStatus = 0;
		SetActive(true);
		SetFollowEnabled(true);
		mskilleffectpts.Stop();
		mEffect_Pos.x = (mEffect_Pos.y = (mEffect_Pos.z = 0f));
		mEffect_Pos.z = -0.05f;
		mEffect_Pos.y = 0.1f;
		mskilleffect.transform.localPosition = mEffect_Pos;
		SetFollowOffset(new Vector3(1.5f, 1f, 0f));
		SetAnimateId((PetHumanBase.PetAnimateId)6u);
		if (mDirection == 1)
		{
			SetPositionAndRotation(new Vector3(_follow_Player.transform.position.x - 5f, _follow_Player.transform.position.y + 10f, _follow_Player.transform.position.z), false);
			base.transform.localScale = new Vector3(2f, 2f, -1f);
		}
		else
		{
			SetPositionAndRotation(new Vector3(_follow_Player.transform.position.x + 5f, _follow_Player.transform.position.y + 10f, _follow_Player.transform.position.z), false);
			base.transform.localScale = new Vector3(2f, 2f, -1f);
		}
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[1] { "idle" };
		target = new string[1] { "sch003_stand_loop" };
	}
}
