using StageLib;
using UnityEngine;

public class EM108_Controller : BS044_SubHeadController
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

	protected override void SetStatusOfSkill()
	{
		switch (subStatus)
		{
		case SubStatus.Phase0:
			_velocity = VInt3.zero;
			currentShootTimes = 0;
			break;
		case SubStatus.Phase1:
			if (StageUpdate.runPlayers.Count > 0)
			{
				OrangeCharacter orangeCharacter = StageUpdate.runPlayers[0];
				SplitBullet splitBullet = (SplitBullet)BulletBase.TryShotBullet(EnemyWeapons[1].BulletData, shootPointTransform.position, orangeCharacter._transform.position - shootPointTransform.position, null, selfBuffManager.sBuffStatus, EnemyData, targetMask);
				if (splitBullet != null)
				{
					splitBullet.AddIgnoreCollider(blockCollider);
					splitBullet.SetupData(selfBuffManager.sBuffStatus, EnemyData, targetMask);
				}
				currentShootTimes++;
			}
			break;
		}
	}

	protected override void UpdateStatusOfSkill()
	{
		switch (subStatus)
		{
		case SubStatus.Phase0:
			SetStatus(mainStatus, SubStatus.Phase1);
			break;
		case SubStatus.Phase1:
			if ((float)AiTimer.GetMillisecond() > shootIntervalTime * 1000f)
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
