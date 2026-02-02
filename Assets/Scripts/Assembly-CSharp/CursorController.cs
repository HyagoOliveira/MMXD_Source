using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class CursorController : MonoBehaviourSingleton<CursorController>
{
	[Flags]
	public enum MouseEventFlags
	{
		LeftDown = 2,
		LeftUp = 4,
		MiddleDown = 0x20,
		MiddleUp = 0x40,
		Move = 1,
		Absolute = 0x8000,
		RightDown = 8,
		RightUp = 0x10
	}

	public struct MousePoint
	{
		public int x;

		public int y;

		public MousePoint(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public bool FEATURE_ENABLE = true;

	private const int SENSITIVITY_UNIT = 500;

	private const string CURSOR_SENSITIVITY = "CURSOR_SENSITIVITY";

	private int sensitivity = 1;

	private bool isMoving;

	private static MousePoint point;

	private int[] hashList;

	private float growSpeed = 2.75f;

	private float currentSpeed;

	private bool isEnable = true;

	public bool IsEnable
	{
		get
		{
			return isEnable;
		}
		set
		{
			isEnable = value;
		}
	}

	public int Sensitivity
	{
		get
		{
			return sensitivity;
		}
		set
		{
			sensitivity = value;
			PlayerPrefs.SetInt("CURSOR_SENSITIVITY", sensitivity);
		}
	}

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out MousePoint lpMousePoint);

	[DllImport("user32.dll")]
	private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

	private void Awake()
	{
		int @int = PlayerPrefs.GetInt("CURSOR_SENSITIVITY");
		if (@int < 1)
		{
			Sensitivity = 3;
		}
		else
		{
			Sensitivity = @int;
		}
	}

	private void Update()
	{
		if (!IsEnable || !FEATURE_ENABLE)
		{
			return;
		}
		if (hashList == null)
		{
			hashList = new int[3];
			hashList[0] = MonoBehaviourSingleton<InputManager>.Instance.ButtonHash[5];
			hashList[1] = MonoBehaviourSingleton<InputManager>.Instance.ButtonHash[6];
			hashList[2] = MonoBehaviourSingleton<InputManager>.Instance.ButtonHash[7];
		}
		for (int i = 0; i < hashList.Length; i++)
		{
			if (cInput.GetKeyDown(hashList[i]))
			{
				TriggerMouseEvent(MouseEventFlags.LeftDown);
				break;
			}
		}
		for (int j = 0; j < hashList.Length; j++)
		{
			if (cInput.GetKeyUp(hashList[j]))
			{
				TriggerMouseEvent(MouseEventFlags.LeftUp);
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!IsEnable || !FEATURE_ENABLE)
		{
			return;
		}
		float axis = MonoBehaviourSingleton<InputManager>.Instance.GetAxis(MonoBehaviourSingleton<InputManager>.Instance.HorizontalHash);
		float axis2 = MonoBehaviourSingleton<InputManager>.Instance.GetAxis(MonoBehaviourSingleton<InputManager>.Instance.VerticalHash);
		Vector2 vector = new Vector2(axis, axis2);
		if (axis != 0f || axis2 != 0f)
		{
			if (!isMoving)
			{
				isMoving = true;
				Cursor.lockState = CursorLockMode.Confined;
			}
		}
		else if (isMoving)
		{
			isMoving = false;
			Cursor.lockState = CursorLockMode.None;
			currentSpeed = 0f;
		}
		if (isMoving)
		{
			currentSpeed += growSpeed * Time.fixedDeltaTime;
			if (currentSpeed >= 1f)
			{
				currentSpeed = 1f;
			}
		}
		Vector2 position;
		if (GetCursorPosition(out position))
		{
			SetCursorPosition((int)(position.x + vector.x * currentSpeed * (float)sensitivity * 500f * Time.fixedDeltaTime), (int)(position.y + vector.y * currentSpeed * 500f * (float)sensitivity * Time.fixedDeltaTime));
		}
	}

	public static void MoveCursorPosition(float x, float y)
	{
		Vector2 position;
		if (GetCursorPosition(out position))
		{
			SetCursorPosition((int)(position.x + x), (int)(position.y + y));
		}
	}

	public static void SetCursorPosition(int x, int y)
	{
		SetCursorPos(x, y);
	}

	public static bool GetCursorPosition(out Vector2 position)
	{
		bool cursorPos = GetCursorPos(out point);
		position = new Vector2(point.x, point.y);
		return cursorPos;
	}

	public static void TriggerMouseEvent(MouseEventFlags value)
	{
		Vector2 position;
		GetCursorPosition(out position);
		mouse_event((int)value, (int)position.x, (int)position.y, 0, 0);
	}
}
