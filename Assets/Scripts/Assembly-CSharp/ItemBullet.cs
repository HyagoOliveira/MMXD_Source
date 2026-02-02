using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBullet : BulletBase, IManagedLateUpdateBehavior
{
	private bool bHasVec;

	private Vector3 vSpeed = Vector3.zero;

	public float fGFactor = -9f;

	private int AllBlockMask;

	public List<Transform> listignorec2dtrans = new List<Transform>();

	protected override void Awake()
	{
		base.Awake();
		AllBlockMask = LayerMask.GetMask("Block") | LayerMask.GetMask("SemiBlock");
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			return;
		}
		if (bHasVec && ((1 << col.gameObject.layer) & AllBlockMask) != 0 && !listignorec2dtrans.Contains(col.transform) && vSpeed.y < 0f)
		{
			bHasVec = false;
		}
		if (!col.isTrigger && ((1 << col.gameObject.layer) & (int)TargetMask) != 0)
		{
			StageObjParam component = col.GetComponent<StageObjParam>();
			if (component != null && component.tLinkSOB != null && (int)component.tLinkSOB.Hp > 0 && component.tLinkSOB.GetSOBType() != 4)
			{
				Hit(col);
			}
		}
	}

	private void OnTriggerStay2D(Collider2D col)
	{
		if (!ManagedSingleton<StageHelper>.Instance.bEnemyActive || MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			return;
		}
		if (bHasVec && ((1 << col.gameObject.layer) & AllBlockMask) != 0 && !listignorec2dtrans.Contains(col.transform) && vSpeed.y < 0f)
		{
			bHasVec = false;
		}
		if (!col.isTrigger && ((1 << col.gameObject.layer) & (int)TargetMask) != 0)
		{
			StageObjParam component = col.GetComponent<StageObjParam>();
			if (component != null && component.tLinkSOB != null && (int)component.tLinkSOB.Hp > 0 && component.tLinkSOB.GetSOBType() != 4)
			{
				Hit(col);
			}
		}
	}

	public override void Hit(Collider2D col)
	{
		if (!bIsEnd)
		{
			CaluDmg(BulletData, col.transform);
			PlaySE("BattleSE", "bt_getitem01", true);
			BackToPool();
		}
	}

	public override void OnStart()
	{
		base.OnStart();
		MonoBehaviourSingleton<UpdateManager>.Instance.AddLateUpdate(this);
		if (Velocity != Vector3.zero)
		{
			bHasVec = true;
			if (Direction.x > 0f)
			{
				vSpeed = new Vector3(-1f, 1f, 0f);
			}
			else
			{
				vSpeed = new Vector3(1f, 1f, 0f);
			}
			vSpeed = vSpeed.normalized;
			vSpeed *= Velocity.x;
		}
	}

	public virtual void OnDisable()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
	}

	public override void BackToPool()
	{
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveLateUpdate(this);
		isSubBullet = false;
		isBuffTrigger = false;
		base.BackToPool();
	}

	protected override IEnumerator OnStartMove()
	{
		yield return null;
	}

	public void LateUpdateFunc()
	{
		if (bHasVec)
		{
			_transform.position += vSpeed * Time.deltaTime;
			vSpeed.y += fGFactor * Time.deltaTime;
		}
	}
}
