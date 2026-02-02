using UnityEngine;

public class BS050_CollideBullet : CollideBullet
{
	public void GroupHitCheck(Collider2D col)
	{
		if (!CheckHitList(ref _ignoreList, col.transform))
		{
			int value = -1;
			_hitCount.TryGetValue(col.transform, out value);
			if (value == -1)
			{
				_hitCount.Add(col.transform, 1);
			}
			else
			{
				_hitCount[col.transform] = value + 1;
			}
			_ignoreList.Add(col.transform);
		}
	}
}
