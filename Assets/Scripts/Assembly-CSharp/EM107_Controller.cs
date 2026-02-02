using StageLib;
using UnityEngine;

public class EM107_Controller : BS044_SubHeadController
{
	[SerializeField]
	private int totalShootTimes = 4;

	[SerializeField]
	private int trackTargetSpeed = 3;

	[SerializeField]
	private int bulletCount = 4;

	[SerializeField]
	private float separationTime = 0.2f;

	[SerializeField]
	private Vector3 bulletSpacing = new Vector3(0f, 1f, 0f);

	private int currentShootTimes;

	public float shootIntervalTime = 1f;

	[SerializeField]
	private Direction skillDirection;

	protected override void SetStatusOfSkill()
	{
		switch (subStatus)
		{
		case SubStatus.Phase0:
			_velocity = VInt3.zero;
			SetFacingDirection(skillDirection);
			currentShootTimes = 0;
			break;
		case SubStatus.Phase2:
		{
			Vector3 vector = bulletSpacing * (bulletCount - 1);
			for (int i = 0; i < bulletCount; i++)
			{
				BulletBase bulletBase = BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, shootPointTransform.position, shootPointTransform.rotation.eulerAngles, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				bulletBase.transform.SetPositionAndRotation(shootPointTransform.position, shootPointTransform.rotation);
				((OffsetBullet)bulletBase).SetOffset((shootPointTransform.position + vector * 0.5f - bulletSpacing * i - shootPointTransform.position) / separationTime, separationTime);
			}
			PlaySE("BossSE03", "bs026_gisig04");
			currentShootTimes++;
			break;
		}
		case SubStatus.Phase1:
			break;
		}
	}

	protected override void UpdateStatusOfSkill()
	{
		switch (subStatus)
		{
		case SubStatus.Phase0:
			if (modelTransform.eulerAngles.y == currentDirection)
			{
				SetStatus(mainStatus, SubStatus.Phase1);
			}
			break;
		case SubStatus.Phase1:
		{
			if (StageUpdate.runPlayers.Count <= 0)
			{
				break;
			}
			OrangeCharacter orangeCharacter = StageUpdate.runPlayers[0];
			float num = shootPointTransform.position.y - orangeCharacter._transform.position.y;
			if ((float)AiTimer.GetMillisecond() > 1000f * shootIntervalTime)
			{
				_velocity = VInt3.zero;
				SetStatus(mainStatus, SubStatus.Phase2);
			}
			else if (Mathf.Abs(num) > 1.2f)
			{
				if (num < 0f)
				{
					_velocity = new VInt3(Vector3.up * trackTargetSpeed);
				}
				else
				{
					_velocity = new VInt3(Vector3.down * trackTargetSpeed);
				}
			}
			else
			{
				_velocity = VInt3.zero;
			}
			break;
		}
		case SubStatus.Phase2:
			if (AiTimer.GetMillisecond() > 300)
			{
				if (currentShootTimes < totalShootTimes)
				{
					SetStatus(MainStatus.Skill, SubStatus.Phase1);
				}
				else
				{
					RegisterStatus(MainStatus.Idle);
				}
			}
			break;
		}
	}
}
