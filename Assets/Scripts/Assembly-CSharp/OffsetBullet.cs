using UnityEngine;

public class OffsetBullet : BasicBullet
{
	private float separationTime;

	private bool isSeparating;

	private float separationTimer;

	private Vector3 moveDelta;

	public void SetOffset(Vector3 _offsetDelta, float time)
	{
		isSeparating = true;
		separationTimer = 0f;
		moveDelta = _offsetDelta;
		separationTime = time;
	}

	protected override void MoveBullet()
	{
		base.MoveBullet();
		if (isSeparating)
		{
			separationTimer += Time.deltaTime;
			_transform.position += moveDelta * Time.deltaTime;
			if (separationTimer >= separationTime)
			{
				isSeparating = false;
			}
		}
	}
}
