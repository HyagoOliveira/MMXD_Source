using UnityEngine;

public class CH031_SKL00 : SCH006Controller
{
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
}
