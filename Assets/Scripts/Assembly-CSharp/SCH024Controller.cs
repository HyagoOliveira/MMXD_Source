using UnityEngine;

public class SCH024Controller : SCH016Controller
{
	public delegate void DeactiveCallBack(Vector3 pos);

	public DeactiveCallBack _cbDeactive;

	protected override void AfterDeactive()
	{
		if (_cbDeactive != null && _nDeactiveType == 1)
		{
			_cbDeactive(_transform.position);
		}
		base.AfterDeactive();
	}

	protected override void DestructToPool()
	{
		base.DestructToPool();
		_cbDeactive = null;
	}
}
