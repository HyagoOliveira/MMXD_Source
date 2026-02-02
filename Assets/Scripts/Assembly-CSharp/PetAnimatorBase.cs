using UnityEngine;

public class PetAnimatorBase : MonoBehaviour
{
	public Vector3 _modelShift = Vector3.zero;

	public readonly int hashDirection = Animator.StringToHash("fDirection");

	public readonly int hashEquip = Animator.StringToHash("fEquip");

	public readonly int hashVelocityX = Animator.StringToHash("fVelocityX");

	public readonly int hashVelocityY = Animator.StringToHash("fVelocityY");

	public readonly int hashSpeedMultiplier = Animator.StringToHash("fSpeedMultiplier");

	public Animator _animator;

	public Transform AimTarget;

	private int[] _animateStatus;

	private int _currentUpperAnimeId;

	private void Start()
	{
		_animator = GetComponentInChildren<Animator>();
	}

	public void InitAnimator()
	{
		_animateStatus = new int[51];
		_animateStatus[0] = Animator.StringToHash("idle");
		_animateStatus[1] = Animator.StringToHash("login");
		_animateStatus[2] = Animator.StringToHash("win");
		_animateStatus[3] = Animator.StringToHash("logout");
		for (int i = 0; i < 30; i++)
		{
			_animateStatus[4 + i] = Animator.StringToHash("skillclip" + i);
		}
		for (int j = 0; j < 15; j++)
		{
			_animateStatus[35 + j] = Animator.StringToHash("btskillclip" + j);
		}
	}

	public int GetAnimateStatusId(short AUID)
	{
		return _animateStatus[AUID];
	}

	public bool isCurrenAnimeId(int AUID)
	{
		if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != AUID)
		{
			return false;
		}
		return true;
	}

	public void SetAnimatorParameters(AnimationParameters animationParams)
	{
		if (!(_animator == null))
		{
			_currentUpperAnimeId = _animateStatus[animationParams.AnimateUpperID];
			float normalizedTime = (animationParams.AnimateUpperKeepFlag ? _animator.GetCurrentAnimatorStateInfo(0).normalizedTime : 0f);
			if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _currentUpperAnimeId)
			{
				_animator.Play(_currentUpperAnimeId, 0, normalizedTime);
			}
		}
	}
}
