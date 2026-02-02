using System;
using UnityEngine;

public class OrangeInputLocker : MonoBehaviour, ILogicUpdate
{
	[Flags]
	public enum CONTROL
	{
		NONE = 0,
		MOVE_POSITION = 1
	}

	private OrangeCharacter oc;

	private bool isFire;

	private int lockEndFrame;

	public Vector3 ControlPosition { get; set; }

	public void Lock(OrangeCharacter p_oc, int p_frame, CONTROL p_control)
	{
		oc = p_oc;
		oc.LockControl();
		lockEndFrame = GameLogicUpdateManager.GameFrame + p_frame;
		isFire = true;
		if (p_control.HasFlag(CONTROL.MOVE_POSITION))
		{
			oc.Controller.LogicPosition = new VInt3(ControlPosition);
			oc._transform.position = ControlPosition;
		}
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	public void LogicUpdate()
	{
		if (isFire && GameLogicUpdateManager.GameFrame >= lockEndFrame)
		{
			isFire = false;
			if (oc != null)
			{
				oc.LockInput = false;
				oc = null;
			}
			MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
		}
	}
}
