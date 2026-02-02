using UnityEngine;

public class SCH016TrapTrigger : MonoBehaviour
{
	protected SCH016Controller _sch016Controller;

	public SCH016Controller SCH016Controller
	{
		get
		{
			return _sch016Controller;
		}
		set
		{
			_sch016Controller = value;
		}
	}

	protected void OnTriggerEnter2D(Collider2D col)
	{
		if ((bool)_sch016Controller)
		{
			_sch016Controller.OnTriggerHit(col);
		}
	}

	protected void OnTriggerStay2D(Collider2D col)
	{
		if ((bool)_sch016Controller)
		{
			_sch016Controller.OnTriggerHit(col);
		}
	}
}
