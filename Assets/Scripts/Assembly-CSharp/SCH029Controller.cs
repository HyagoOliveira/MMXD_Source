using UnityEngine;

public class SCH029Controller : SCH006Controller
{
	public int FollowSpeed = 10;

	public override string[] GetPetDependAnimations()
	{
		return null;
	}

	protected override void SkillLaunch()
	{
		_autoAim.SetEnable(false);
		_autoAim.targetMask = base.TargetMask;
		if ((bool)m_forceFieldEffect)
		{
			m_forceFieldEffect.gameObject.SetActive(true);
		}
		SetStatus(MainStatus.Ready);
	}

	public override void UpdateFunc()
	{
		if (Activate)
		{
			_transform.localPosition = Vector3.MoveTowards(_transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	protected override void AttackTarget()
	{
		Vector3 normalized = (_autoAim.GetTargetPoint() - base.transform.position).normalized;
		BulletBase bulletBase = BulletBase.TryShotBullet(m_bulletSkillTable, base.transform, normalized, m_weaponStatus, selfBuffManager.sBuffStatus, null, base.TargetMask);
		if ((bool)bulletBase && _follow_Player != null)
		{
			bulletBase.BulletLevel = _follow_Player.PlayerSkills[follow_skill_id].SkillLV;
			bulletBase.SetPetBullet();
			bulletBase.SetOwnerName(_follow_Player.sPlayerName);
		}
		m_bulletCountNow++;
	}

	public override void Set_follow_Player(OrangeCharacter mOC, bool linkBuffManager = true)
	{
		_follow_Player = mOC;
		isLocalPlayer = _follow_Player != null && _follow_Player.IsLocalPlayer;
		Initialize();
		if (tRefPassiveskill == null)
		{
			tRefPassiveskill = _follow_Player.tRefPassiveskill;
		}
		if (linkBuffManager)
		{
			selfBuffManager = _follow_Player.selfBuffManager;
		}
		else
		{
			selfBuffManager.Init(this);
		}
	}

	protected override void UpdateFollowPos()
	{
		if (!FollowEnabled)
		{
			return;
		}
		float x = FollowOffset.x;
		x = ((_autoAim.AutoAimTarget == null) ? ((float)_follow_Player.direction * FollowOffset.x) : (Mathf.Sign(_autoAim.GetTargetPoint().x - _follow_Player.transform.position.x) * FollowOffset.x));
		mFollow_Target = new VInt3(new Vector3(_follow_Player.transform.position.x + x, _follow_Player.transform.position.y + FollowOffset.y, _follow_Player.transform.position.z));
		if (Controller.LogicPosition.x != mFollow_Target.x)
		{
			int num = mFollow_Target.x - Controller.LogicPosition.x;
			_velocity.x = ((Mathf.Abs(num) > FollowSpeed) ? (num * FollowSpeed) : num);
			b_follow_pos_end = false;
		}
		else
		{
			_velocity.x = 0;
		}
		if (Controller.LogicPosition.y != mFollow_Target.y)
		{
			int num2 = mFollow_Target.y - Controller.LogicPosition.y;
			_velocity.y = ((Mathf.Abs(num2) > FollowSpeed) ? (num2 * FollowSpeed) : num2);
			b_follow_pos_end = false;
		}
		else
		{
			_velocity.y = 0;
		}
		if (OldLogicPosition == Controller.LogicPosition)
		{
			if (!b_follow_pos_end)
			{
				FollowEnd();
				b_follow_pos_end = true;
			}
		}
		else
		{
			OldLogicPosition = Controller.LogicPosition;
		}
	}
}
