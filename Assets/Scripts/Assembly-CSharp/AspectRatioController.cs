using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;
using UnityEngine.Events;

public class AspectRatioController : MonoBehaviour
{
	private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	public UnityEvent resolutionChangedEvent;

	private static int tempCount = 1;

	private static int lastTempCount = 1;

	[SerializeField]
	private bool allowFullscreen = true;

	[SerializeField]
	private float aspectRatioWidth = 16f;

	[SerializeField]
	private float aspectRatioHeight = 9f;

	[SerializeField]
	private int minWidthPixel = 512;

	[SerializeField]
	private int minHeightPixel = 512;

	[SerializeField]
	private int maxWidthPixel = 2048;

	[SerializeField]
	private int maxHeightPixel = 2048;

	private static int MinWidthPixel = 512;

	private static int MinHeightPixel = 512;

	private static int MaxWidthPixel = 2048;

	private static int MaxHeightPixel = 2048;

	private static float aspect;

	private static int setWidth = -1;

	private static int setHeight = -1;

	private bool wasFullscreenLastFrame;

	private bool started;

	private int pixelHeightOfCurrentScreen;

	private int pixelWidthOfCurrentScreen;

	private bool quitStarted;

	private const int WM_SIZING = 532;

	private const int WMSZ_LEFT = 1;

	private const int WMSZ_RIGHT = 2;

	private const int WMSZ_TOP = 3;

	private const int WMSZ_BOTTOM = 6;

	private const int GWLP_WNDPROC = -4;

	private static WndProcDelegate wndProcDelegate = wndProc;

	private static EnumWindowsProc enumProcDelegate = enumProc;

	private const string UNITY_WND_CLASSNAME = "UnityWndClass";

	private static IntPtr unityHWnd;

	private static IntPtr oldWndProcPtr;

	private IntPtr newWndProcPtr;

	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();

	[DllImport("kernel32.dll")]
	private static extern uint GetCurrentThreadId();

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(IntPtr hwnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
	private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
	private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	private void Start()
	{
		setMinAndMaxPixel();
		Application.wantsToQuit += ApplicationWantsToQuit;
		EnumThreadWindows(GetCurrentThreadId(), enumProcDelegate, IntPtr.Zero);
		SetAspectRatio(aspectRatioWidth, aspectRatioHeight, true);
		wasFullscreenLastFrame = Screen.fullScreen;
		newWndProcPtr = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
		oldWndProcPtr = SetWindowLong(unityHWnd, -4, newWndProcPtr);
		started = true;
	}

	private void setMinAndMaxPixel()
	{
		MinWidthPixel = minWidthPixel;
		MaxWidthPixel = maxWidthPixel;
		MinHeightPixel = minHeightPixel;
		MaxHeightPixel = maxHeightPixel;
	}

	private static void resolutionChangedFunc()
	{
		tempCount++;
	}

	[MonoPInvokeCallback(typeof(EnumWindowsProc))]
	private static bool enumProc(IntPtr hWnd, IntPtr lParam)
	{
		StringBuilder stringBuilder = new StringBuilder("UnityWndClass".Length + 1);
		GetClassName(hWnd, stringBuilder, stringBuilder.Capacity);
		if (stringBuilder.ToString() == "UnityWndClass")
		{
			unityHWnd = hWnd;
			return false;
		}
		return true;
	}

	public void SetAspectRatio(float newAspectWidth, float newAspectHeight, bool apply)
	{
		aspectRatioWidth = newAspectWidth;
		aspectRatioHeight = newAspectHeight;
		aspect = aspectRatioWidth / aspectRatioHeight;
		if (apply)
		{
			Screen.SetResolution(Screen.width, Mathf.RoundToInt((float)Screen.width / aspect), Screen.fullScreen);
		}
	}

	[MonoPInvokeCallback(typeof(WndProcDelegate))]
	private static IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
	{
		if (msg == 532)
		{
			RECT structure = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
			RECT lpRect = default(RECT);
			GetWindowRect(unityHWnd, ref lpRect);
			RECT lpRect2 = default(RECT);
			GetClientRect(unityHWnd, ref lpRect2);
			int num = lpRect.Right - lpRect.Left - (lpRect2.Right - lpRect2.Left);
			int num2 = lpRect.Bottom - lpRect.Top - (lpRect2.Bottom - lpRect2.Top);
			structure.Right -= num;
			structure.Bottom -= num2;
			int num3 = Mathf.Clamp(structure.Right - structure.Left, MinWidthPixel, MaxWidthPixel);
			int num4 = Mathf.Clamp(structure.Bottom - structure.Top, MinHeightPixel, MaxHeightPixel);
			switch (wParam.ToInt32())
			{
			case 1:
				structure.Left = structure.Right - num3;
				structure.Bottom = structure.Top + Mathf.RoundToInt((float)num3 / aspect);
				break;
			case 2:
				structure.Right = structure.Left + num3;
				structure.Bottom = structure.Top + Mathf.RoundToInt((float)num3 / aspect);
				break;
			case 3:
				structure.Top = structure.Bottom - num4;
				structure.Right = structure.Left + Mathf.RoundToInt((float)num4 * aspect);
				break;
			case 6:
				structure.Bottom = structure.Top + num4;
				structure.Right = structure.Left + Mathf.RoundToInt((float)num4 * aspect);
				break;
			case 8:
				structure.Right = structure.Left + num3;
				structure.Bottom = structure.Top + Mathf.RoundToInt((float)num3 / aspect);
				break;
			case 5:
				structure.Right = structure.Left + num3;
				structure.Top = structure.Bottom - Mathf.RoundToInt((float)num3 / aspect);
				break;
			case 7:
				structure.Left = structure.Right - num3;
				structure.Bottom = structure.Top + Mathf.RoundToInt((float)num3 / aspect);
				break;
			case 4:
				structure.Left = structure.Right - num3;
				structure.Top = structure.Bottom - Mathf.RoundToInt((float)num3 / aspect);
				break;
			}
			setWidth = structure.Right - structure.Left;
			setHeight = structure.Bottom - structure.Top;
			structure.Right += num;
			structure.Bottom += num2;
			resolutionChangedFunc();
			Marshal.StructureToPtr(structure, lParam, true);
		}
		return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
	}

	private void Update()
	{
		if (!allowFullscreen && Screen.fullScreen)
		{
			Screen.fullScreen = false;
		}
		if (Screen.fullScreen && !wasFullscreenLastFrame)
		{
			int height;
			int width;
			if (aspect < (float)pixelWidthOfCurrentScreen / (float)pixelHeightOfCurrentScreen)
			{
				height = pixelHeightOfCurrentScreen;
				width = Mathf.RoundToInt((float)pixelHeightOfCurrentScreen * aspect);
			}
			else
			{
				width = pixelWidthOfCurrentScreen;
				height = Mathf.RoundToInt((float)pixelWidthOfCurrentScreen / aspect);
			}
			Screen.SetResolution(width, height, true);
			resolutionChangedFunc();
		}
		else if (!Screen.fullScreen && wasFullscreenLastFrame)
		{
			Screen.SetResolution(setWidth, setHeight, false);
			resolutionChangedFunc();
		}
		else if (!Screen.fullScreen && setWidth != -1 && setHeight != -1 && (Screen.width != setWidth || Screen.height != setHeight))
		{
			setHeight = Screen.height;
			setWidth = Mathf.RoundToInt((float)Screen.height * aspect);
			Screen.SetResolution(setWidth, setHeight, Screen.fullScreen);
			resolutionChangedFunc();
		}
		else if (!Screen.fullScreen)
		{
			pixelHeightOfCurrentScreen = Screen.currentResolution.height;
			pixelWidthOfCurrentScreen = Screen.currentResolution.width;
		}
		wasFullscreenLastFrame = Screen.fullScreen;
		if (lastTempCount != tempCount)
		{
			resolutionChangedEvent.Invoke();
			lastTempCount = tempCount;
		}
	}

	private static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
	{
		if (IntPtr.Size == 4)
		{
			return SetWindowLong32(hWnd, nIndex, dwNewLong);
		}
		return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
	}

	private bool ApplicationWantsToQuit()
	{
		if (!started)
		{
			return false;
		}
		if (!quitStarted)
		{
			StartCoroutine("DelayedQuit");
			return false;
		}
		return true;
	}

	private IEnumerator DelayedQuit()
	{
		SetWindowLong(unityHWnd, -4, oldWndProcPtr);
		yield return new WaitForEndOfFrame();
		quitStarted = true;
		Application.Quit();
	}
}
