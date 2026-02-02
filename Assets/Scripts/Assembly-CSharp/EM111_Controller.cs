using UnityEngine;

public class EM111_Controller : EM062_Controller
{
	protected override void UpdateStatusLogic()
	{
		currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_velocity = VInt3.zero;
			RegisterStatus(MainStatus.Fly);
			break;
		case MainStatus.Fly:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (ModelTransform.eulerAngles.y != selfDirection)
				{
					_velocity = VInt3.zero;
					break;
				}
				Vector3.Distance(TargetPos.vec3, Controller.LogicPosition.vec3);
				if (Vector3.Distance(StartPos, _transform.position) >= MoveDis)
				{
					flyTimer += (int)AiTimer.GetMillisecond();
					SetStatus(MainStatus.Fly, SubStatus.Phase1);
				}
				else
				{
					Vector3 normalized = (TargetPos - Controller.LogicPosition).vec3.normalized;
					_velocity = new VInt3(normalized * flySpeed);
				}
				break;
			case SubStatus.Phase1:
				_velocity.x = 0;
				break;
			}
			break;
		case MainStatus.Shoot:
			_velocity = VInt3.zero;
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (currentFrame >= 1f)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
				if (AiTimer.GetMillisecond() >= EnemyWeapons[1].BulletData.n_FIRE_SPEED)
				{
					Target = _enemyAutoAimSystem.GetClosetPlayer();
					if (Target != null && Vector3.Dot(Target._transform.position - modelTransform.transform.position, modelTransform.forward) > 0f)
					{
						SetStatus(MainStatus.Shoot, SubStatus.Phase2);
					}
					else
					{
						SetStatus(MainStatus.Shoot, SubStatus.Phase3);
					}
				}
				break;
			case SubStatus.Phase2:
				if (EnemyWeapons[1].MagazineRemain > 0f)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase1);
				}
				else
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				if ((float)AiTimer.GetMillisecond() >= EnemyWeapons[1].MagazineRemain)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase4);
				}
				break;
			case SubStatus.Phase4:
				if (currentFrame >= 1f)
				{
					SetStatus(MainStatus.Shoot, SubStatus.Phase5);
				}
				break;
			case SubStatus.Phase5:
				RegisterStatus(MainStatus.Fly);
				break;
			}
			break;
		case MainStatus.Hurt:
			break;
		}
	}
}
