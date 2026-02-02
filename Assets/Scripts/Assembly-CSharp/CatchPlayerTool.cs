using NaughtyAttributes;
using UnityEngine;

public class CatchPlayerTool : MonoBehaviour
{
	private enum TargetStatus
	{
		Null = 0,
		Dead = 1,
		Alive = 2
	}

	private bool _isCatching;

	[Header("目標狀態")]
	[SerializeField]
	[ReadOnly]
	private TargetStatus targetStatus;

	[Header("抓的位置")]
	[SerializeField]
	public Transform CatchTransform;

	[SerializeField]
	public Vector3 PosOffset;

	[Header("抓的時間")]
	[SerializeField]
	private float CatchTime = 3f;

	private int CatchFrame;

	public bool DieReleaseMastetPos;

	public OrangeCharacter TargetOC { get; private set; }

	public bool IsCatching
	{
		get
		{
			if (_isCatching)
			{
				return CheckTargetStatus == TargetStatus.Alive;
			}
			return false;
		}
	}

	private bool HaveTarget
	{
		get
		{
			return TargetOC != null;
		}
	}

	private TargetStatus CheckTargetStatus
	{
		get
		{
			if (!HaveTarget)
			{
				targetStatus = TargetStatus.Null;
			}
			else if ((int)TargetOC.Hp <= 0)
			{
				targetStatus = TargetStatus.Dead;
			}
			else
			{
				targetStatus = TargetStatus.Alive;
			}
			return targetStatus;
		}
	}

	public void CatchTarget(OrangeCharacter targetOC, bool CanCatchUnBreakPlayer = false, bool CanCatchStunnedPlayer = false)
	{
		if (!_isCatching)
		{
			CatchFrame = GameLogicUpdateManager.GameFrame + (int)(CatchTime * 20f);
			if ((!targetOC.IsUnBreakX() || CanCatchUnBreakPlayer) && (bool)targetOC && ((!CanCatchStunnedPlayer && !targetOC.IsStun) || CanCatchStunnedPlayer))
			{
				TargetOC = targetOC;
				TargetOC.SetStun(true);
				_isCatching = true;
			}
		}
	}

	public void MoveTarget()
	{
		if (_isCatching || CheckTargetStatus == TargetStatus.Alive)
		{
			TargetOC._transform.position = CatchTransform.position + PosOffset;
			TargetOC.Controller.LogicPosition = new VInt3(TargetOC._transform.position);
		}
	}

	public void MoveTargetWithForce(VInt3 force)
	{
		TargetOC.AddForce(force);
	}

	public void ReleaseTarget()
	{
		if (_isCatching && CheckTargetStatus != 0)
		{
			TargetOC._transform.position = new Vector3(TargetOC._transform.position.x, TargetOC._transform.position.y, 0f);
			TargetOC._transform.rotation = Quaternion.identity;
			TargetOC.SetStun(false);
			TargetOC = null;
			_isCatching = false;
		}
	}

	public void ReleaseTarget(Vector3 pos)
	{
		if (_isCatching && CheckTargetStatus != 0)
		{
			TargetOC._transform.position = pos;
			TargetOC._transform.rotation = Quaternion.identity;
			TargetOC.Controller.LogicPosition = new VInt3(pos);
			TargetOC.SetStun(false);
			TargetOC = null;
			_isCatching = false;
		}
	}

	private void Update()
	{
		if (CheckTargetStatus == TargetStatus.Dead)
		{
			if (!DieReleaseMastetPos)
			{
				ReleaseTarget();
			}
			else
			{
				ReleaseTarget(CatchTransform.position);
			}
		}
		if (GameLogicUpdateManager.GameFrame >= CatchFrame)
		{
			ReleaseTarget();
		}
	}
}
