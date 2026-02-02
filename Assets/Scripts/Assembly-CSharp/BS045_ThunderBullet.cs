using UnityEngine;

public class BS045_ThunderBullet : BasicBullet
{
	private float XEnd;

	private int direction = -1;

	public void SetEndPos(float endpos)
	{
		XEnd = endpos;
		if (XEnd < _transform.position.x)
		{
			direction = -1;
		}
		else
		{
			direction = 1;
		}
	}

	protected override void MoveBullet()
	{
		base.MoveBullet();
		if ((float)direction * (_transform.position.x - XEnd) > 0f)
		{
			ChangeDirection(new Vector3(0f, 0f, 90f));
		}
	}
}
