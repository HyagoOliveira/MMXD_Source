using System.Collections.Generic;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class EM064_Controller : EnemyControllerBase, IManagedUpdateBehavior
{
	private enum MainStatus
	{
		Idle = 0,
		CloseGate = 1,
		IdleWaitNet = 2
	}

	private enum SubStatus
	{
		Phase0 = 0,
		Phase1 = 1,
		Phase2 = 2,
		Phase3 = 3,
		Phase4 = 4,
		Phase5 = 5,
		MAX_SUBSTATUS = 6
	}

	private enum AnimationID
	{
		ANI_IDLE = 0,
		ANI_CLOSE_START = 1,
		ANI_CLOSING = 2,
		ANI_CLOSE_END = 3,
		ANI_CLOSE_IDLE = 4,
		MAX_ANIMATION_ID = 5
	}

	private const int MAX_PILLAR_FRAGMENT = 20;

	[SerializeField]
	private Transform modelTransform;

	[SerializeField]
	private Transform bottomTransform;

	[SerializeField]
	private Transform pillarFragment;

	[SerializeField]
	private BoxCollider2D colliderOfBlock;

	[SerializeField]
	private BoxCollider2D colliderOfHit;

	[SerializeField]
	private float closingTime = 1f;

	[SerializeField]
	private float intervalDistance = 0.431f;

	private float closingSpeed;

	private float activeInterval;

	private int fragmentIndex;

	private Vector3 floorPoint;

	private float floorDistance;

	public Vector3 defalutBottomLocalPosition;

	private List<Transform> pillarFragmentList = new List<Transform>();

	private MainStatus mainStatus;

	private SubStatus subStatus;

	private float currentFrame;

	private AnimationID currentAnimationId;

	private int[] animationHash;

	private void OnEnable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override void Awake()
	{
		base.Awake();
		ModelTransform = modelTransform;
		_animator = GetComponentInChildren<Animator>();
		_collideBullet = base.gameObject.AddOrGetComponent<CollideBullet>();
		OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		animationHash = new int[5];
		animationHash[0] = Animator.StringToHash("EM064@standby_loop");
		animationHash[1] = Animator.StringToHash("EM064@active_start");
		animationHash[2] = Animator.StringToHash("EM064@active_loop");
		animationHash[3] = Animator.StringToHash("EM064@active_end");
		animationHash[4] = Animator.StringToHash("EM064@done_loop");
		mainStatus = MainStatus.Idle;
		subStatus = SubStatus.Phase0;
		if (null == _enemyAutoAimSystem)
		{
			OrangeBattleUtility.AddEnemyAutoAimSystem(_transform, out _enemyAutoAimSystem);
		}
		_enemyAutoAimSystem.UpdateAimRange(10f);
		IgnoreGravity = true;
		base.AllowAutoAim = false;
		AiTimer.TimerStart();
	}

	public override void LogicUpdate()
	{
		if (Activate && (bool)_enemyAutoAimSystem)
		{
			BaseLogicUpdate();
		}
	}

	public void UpdateFunc()
	{
		if (Activate)
		{
			UpdateStatusLogic();
			base.transform.localPosition = Vector3.MoveTowards(base.transform.localPosition, Controller.LogicPosition.vec3, distanceDelta);
		}
	}

	public override void SetActive(bool _isActive)
	{
		base.SetActive(_isActive);
		if (_isActive)
		{
			base.AllowAutoAim = false;
			BuildPillar();
			SetStatus(MainStatus.Idle);
		}
	}

	private void BuildPillar()
	{
		colliderOfBlock.enabled = false;
		colliderOfHit.enabled = false;
		bottomTransform.localPosition = defalutBottomLocalPosition;
		floorPoint = Physics2D.Raycast(bottomTransform.position, bottomTransform.up * -1f, 100f, Controller.collisionMask).point;
		if (!pillarFragmentList.Contains(pillarFragment))
		{
			pillarFragmentList.Insert(0, pillarFragment);
		}
		floorDistance = Vector2.Distance(bottomTransform.position, floorPoint);
		int num = (int)(floorDistance / ((intervalDistance == 0f) ? 0.431f : intervalDistance)) + 2;
		if (pillarFragmentList.Count < num)
		{
			Material sharedMaterial = pillarFragment.GetComponentInChildren<Renderer>().sharedMaterial;
			int num2 = num - pillarFragmentList.Count;
			for (int i = 0; i < num2; i++)
			{
				if (pillarFragmentList.Count < 20)
				{
					Transform transform = Object.Instantiate(pillarFragment);
					transform.parent = pillarFragment.parent;
					transform.GetComponentInChildren<Renderer>().sharedMaterial = sharedMaterial;
					pillarFragmentList.Add(transform);
				}
			}
		}
		CharacterMaterial componentInChildren = GetComponentInChildren<CharacterMaterial>();
		for (int j = 0; j < pillarFragmentList.Count; j++)
		{
			pillarFragmentList[j].gameObject.SetActive(true);
		}
		componentInChildren.RebuildRendererList();
		closingSpeed = ((closingTime == 0f) ? 1f : (floorDistance / closingTime));
		float num3 = Vector2.Distance(modelTransform.position, bottomTransform.position);
		Vector3 position = bottomTransform.position + new Vector3(0f, num3 * 0.2f, 0f);
		for (int k = 0; k < pillarFragmentList.Count; k++)
		{
			position.y += intervalDistance;
			pillarFragmentList[k].gameObject.SetActive(false);
			pillarFragmentList[k].SetPositionAndRotation(position, bottomTransform.rotation);
		}
		Vector2 offset = colliderOfBlock.offset;
		colliderOfBlock.offset = new Vector2(colliderOfBlock.offset.x, num3 * -0.5f);
		colliderOfBlock.size = new Vector2(colliderOfBlock.size.x, num3);
		colliderOfHit.offset = new Vector2(colliderOfHit.offset.x, num3 * -0.5f);
		colliderOfHit.size = new Vector2(colliderOfHit.size.x, num3);
		activeInterval = floorDistance / (float)pillarFragmentList.Count;
		fragmentIndex = 0;
		colliderOfBlock.enabled = true;
	}

	protected override void DeadBehavior(ref HurtPassParam tHurtPassParam)
	{
		base.DeadBehavior(ref tHurtPassParam);
		for (int i = 0; i < pillarFragmentList.Count; i++)
		{
			pillarFragmentList[i].gameObject.SetActive(false);
		}
	}

	private void RegisterStatus(MainStatus _mainStatus)
	{
		if (StageUpdate.gbIsNetGame)
		{
			if (StageUpdate.bIsHost)
			{
				StageUpdate.RegisterSendAndRun(sNetSerialID, (int)_mainStatus);
				mainStatus = MainStatus.IdleWaitNet;
			}
		}
		else
		{
			SetStatus(_mainStatus);
		}
	}

	public override void UpdateStatus(int _nSet, string _smsg, Callback _callback = null)
	{
		SetStatus((MainStatus)_nSet);
	}

	private void SetStatus(MainStatus _mainStatus, SubStatus _subStatus = SubStatus.Phase0)
	{
		mainStatus = _mainStatus;
		subStatus = _subStatus;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			_animator.Play(animationHash[0], 0, 0f);
			break;
		case MainStatus.CloseGate:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				_animator.Play(animationHash[1], 0, 0f);
				break;
			case SubStatus.Phase1:
				_animator.Play(animationHash[2], 0, 0f);
				break;
			case SubStatus.Phase2:
				_animator.Play(animationHash[3], 0, 0f);
				break;
			case SubStatus.Phase3:
				_animator.Play(animationHash[4], 0, 0f);
				break;
			}
			break;
		}
		AiTimer.TimerStart();
	}

	private void UpdateStatusLogic()
	{
		currentFrame = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		switch (mainStatus)
		{
		case MainStatus.Idle:
			Target = _enemyAutoAimSystem.GetClosetPlayer();
			if (Target != null)
			{
				RegisterStatus(MainStatus.CloseGate);
				PlaySE(EnemySE02.CRI_ENEMYSE02_EM028_BAR);
			}
			break;
		case MainStatus.CloseGate:
			switch (subStatus)
			{
			case SubStatus.Phase0:
				if (currentFrame >= 1f)
				{
					SetStatus(MainStatus.CloseGate, SubStatus.Phase1);
				}
				break;
			case SubStatus.Phase1:
			{
				bottomTransform.position += Vector3.down * closingSpeed * Time.deltaTime;
				if (Vector2.Distance(bottomTransform.position, modelTransform.position) - intervalDistance > (float)fragmentIndex * activeInterval)
				{
					if (fragmentIndex >= pillarFragmentList.Count)
					{
						fragmentIndex = pillarFragmentList.Count - 1;
					}
					pillarFragmentList[fragmentIndex].gameObject.SetActive(true);
					fragmentIndex++;
				}
				float num = Vector2.Distance(modelTransform.position, bottomTransform.position);
				colliderOfBlock.offset = new Vector2(colliderOfBlock.offset.x, num * -0.5f);
				colliderOfBlock.size = new Vector2(colliderOfBlock.size.x, num);
				colliderOfHit.offset = new Vector2(colliderOfHit.offset.x, num * -0.5f);
				colliderOfHit.size = new Vector2(colliderOfHit.size.x, num);
				Collider2D[] array = Physics2D.OverlapBoxAll(colliderOfBlock.transform.position + new Vector3(colliderOfBlock.offset.x, colliderOfBlock.offset.y, 0f), colliderOfBlock.size, 0f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.PlayerLayer);
				for (int i = 0; i < array.Length; i++)
				{
					for (int j = 0; j < StageUpdate.runPlayers.Count; j++)
					{
						OrangeCharacter orangeCharacter = StageUpdate.runPlayers[j];
						if (orangeCharacter.transform == array[i].transform)
						{
							float num2 = (colliderOfBlock.size.x + orangeCharacter.Controller.Collider2D.size.x) * 0.5f;
							if (modelTransform.position.x > orangeCharacter._transform.position.x)
							{
								num2 *= -1f;
							}
							Vector3 position = new Vector3(modelTransform.transform.position.x + num2, orangeCharacter._transform.position.y, orangeCharacter._transform.position.z);
							orangeCharacter.Controller.LogicPosition = new VInt3(position);
							orangeCharacter._transform.position = position;
						}
					}
				}
				if (bottomTransform.position.y < floorPoint.y)
				{
					bottomTransform.position = floorPoint;
					SetStatus(MainStatus.CloseGate, SubStatus.Phase2);
				}
				break;
			}
			case SubStatus.Phase2:
				if (currentFrame >= 1f)
				{
					colliderOfBlock.enabled = true;
					colliderOfHit.enabled = true;
					base.AllowAutoAim = true;
					base.AimPoint = (bottomTransform.position - modelTransform.position) * 0.5f;
					SetStatus(MainStatus.CloseGate, SubStatus.Phase3);
				}
				break;
			case SubStatus.Phase3:
				break;
			}
			break;
		case MainStatus.IdleWaitNet:
			break;
		}
	}
}
