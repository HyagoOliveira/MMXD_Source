using UnityEngine;

public class EM102_Controller : Em001Controller, IManagedUpdateBehavior
{
	[SerializeField]
	private Transform modelTransform;

	protected override void Awake()
	{
		base.Awake();
		HASH_IDLE_LOOP = Animator.StringToHash("EM102@idle_loop");
		HASH_HIDE_START = Animator.StringToHash("EM102@hide_start");
		HASH_HIDE_LOOP = Animator.StringToHash("EM102@hide_loop");
		HASH_HIDE_END = Animator.StringToHash("EM102@hide_end");
		HASH_HURT_LOOP = Animator.StringToHash("EM102@hurt_loop");
		HASH_RUN_LOOP = Animator.StringToHash("EM102@run_loop");
		HASH_SK001 = Animator.StringToHash("EM102@skl01");
	}

	protected override void ReverseDirection()
	{
		base.direction *= -1;
		bool bBack = ((base.direction < 0) ? true : false);
		SetPositionAndRotation(_transform.position, bBack);
	}

	public override void SetPositionAndRotation(Vector3 pos, bool bBack)
	{
		Vector3 localPosition = shootTransform.localPosition;
		if (bBack)
		{
			base.direction = -1;
		}
		else
		{
			base.direction = 1;
		}
		shootTransform.localPosition = new Vector3((float)base.direction * Mathf.Abs(localPosition.x), localPosition.y, localPosition.z);
		modelTransform.localScale = new Vector3(modelTransform.localScale.x, modelTransform.localScale.y, Mathf.Abs(modelTransform.localScale.z) * (float)base.direction);
		_transform.position = pos;
	}
}
