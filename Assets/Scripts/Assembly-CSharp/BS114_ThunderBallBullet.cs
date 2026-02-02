using UnityEngine;

public class BS114_ThunderBallBullet : BasicBullet
{
	private bool hasTurn;

	public override void OnStart()
	{
		base.OnStart();
		hasTurn = false;
	}

	public override void ChangeDirection(Vector3 newDirection)
	{
		_transform.eulerAngles = newDirection;
		hasTurn = true;
	}

	protected override void MoveBullet()
	{
		base.MoveBullet();
		if (hasTurn)
		{
			return;
		}
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(_transform.position + Vector3.up * 20f, _colliderSize, 0f, Vector2.down, 40f, LayerMask.GetMask("Player"));
		if (!raycastHit2D)
		{
			return;
		}
		OrangeCharacter component = raycastHit2D.transform.GetComponent<OrangeCharacter>();
		if ((bool)component)
		{
			if ((component.GetTargetPoint() + Vector3.up).y > _transform.position.y)
			{
				ChangeDirection(new Vector3(0f, 0f, 90f));
			}
			else
			{
				ChangeDirection(new Vector3(0f, 0f, 270f));
			}
		}
	}
}
