using System;
using System.Collections;
using StageLib;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
	public enum CameraEvent
	{
		None = 0,
		Focus = 1,
		Shake = 2
	}

	[Serializable]
	public struct LockRange
	{
		public float MinX;

		public float MaxX;

		public float MinY;

		public float MaxY;

		public LockRange(float minx, float maxx, float miny, float maxy)
		{
			MinX = minx;
			MaxX = maxx;
			MinY = miny;
			MaxY = maxy;
		}

		public bool PositionInRange(Vector3 tPos)
		{
			if (tPos.x < MinX - ManagedSingleton<StageHelper>.Instance.fCameraWHalf || tPos.x > MaxX + ManagedSingleton<StageHelper>.Instance.fCameraWHalf || tPos.y < MinY - ManagedSingleton<StageHelper>.Instance.fCameraHHalf || tPos.y > MaxY + ManagedSingleton<StageHelper>.Instance.fCameraHHalf)
			{
				return false;
			}
			return true;
		}
	}

	private struct FocusArea
	{
		public Vector2 Center;

		public float _left;

		public float _right;

		public float _top;

		public float _bottom;

		public float _halfsizex;

		public float _halfsizey;

		private const float fBias = 0.001f;

		private float shiftX;

		private float shiftY;

		private float dis;

		public bool bNeedUnSlow;

		public FocusArea(Bounds targetBounds, Vector2 size, LockRange targetLockRange)
		{
			_left = targetBounds.center.x - size.x / 2f;
			_right = targetBounds.center.x + size.x / 2f;
			_bottom = targetBounds.min.y;
			_top = targetBounds.min.y + size.y;
			if (targetBounds.size.x >= size.x)
			{
				_halfsizex = size.x / 2f - 0.001f;
			}
			else
			{
				_halfsizex = targetBounds.size.x / 2f;
			}
			if (targetBounds.size.y >= size.y)
			{
				_halfsizey = size.y / 2f - 0.001f;
			}
			else
			{
				_halfsizey = targetBounds.size.y / 2f - 0.001f;
			}
			if (_left < targetLockRange.MinX && _right > targetLockRange.MaxX)
			{
				float num = (targetLockRange.MaxX + targetLockRange.MinX) / 2f - (_right + _left) / 2f;
				_left += num;
				_right += num;
			}
			else if (_left < targetLockRange.MinX)
			{
				float num2 = targetLockRange.MinX - _left;
				_left += num2;
				_right += num2;
			}
			else if (_right > targetLockRange.MaxX)
			{
				float num3 = _right - targetLockRange.MaxX;
				_left -= num3;
				_right -= num3;
			}
			if (_bottom < targetLockRange.MinY && _top > targetLockRange.MaxY)
			{
				float num4 = (targetLockRange.MaxY + targetLockRange.MinY) / 2f - (_top + _bottom) / 2f;
				_top += num4;
				_bottom += num4;
			}
			else if (_bottom < targetLockRange.MinY)
			{
				float num5 = targetLockRange.MinY - _bottom;
				_top += num5;
				_bottom += num5;
			}
			else if (_top > targetLockRange.MaxY)
			{
				float num6 = _top - targetLockRange.MaxY;
				_top -= num6;
				_bottom -= num6;
			}
			Center = new Vector2((_left + _right) / 2f, (_top + _bottom) / 2f);
			shiftX = 0f;
			shiftY = 0f;
			dis = 0f;
			bNeedUnSlow = false;
		}

		public void UpdateLockRange(LockRange targetLockRange)
		{
			if (_left < targetLockRange.MinX && _right > targetLockRange.MaxX)
			{
				float num = (targetLockRange.MaxX + targetLockRange.MinX) / 2f - (_right + _left) / 2f;
				_left += num;
				_right += num;
			}
			else if (_left < targetLockRange.MinX)
			{
				float num2 = targetLockRange.MinX - _left;
				_left += num2;
				_right += num2;
			}
			else if (_right > targetLockRange.MaxX)
			{
				float num3 = _right - targetLockRange.MaxX;
				_left -= num3;
				_right -= num3;
			}
			if (_bottom < targetLockRange.MinY && _top > targetLockRange.MaxY)
			{
				float num4 = (targetLockRange.MaxY + targetLockRange.MinY) / 2f - (_top + _bottom) / 2f;
				_top += num4;
				_bottom += num4;
			}
			else if (_bottom < targetLockRange.MinY)
			{
				float num5 = targetLockRange.MinY - _bottom;
				_top += num5;
				_bottom += num5;
			}
			else if (_top > targetLockRange.MaxY)
			{
				float num6 = _top - targetLockRange.MaxY;
				_top -= num6;
				_bottom -= num6;
			}
			Center.x = (_left + _right) / 2f;
			Center.y = (_top + _bottom) / 2f;
		}

		public void UpdateSmoothInRange(ref float maxa, ref float minb, float fdis, float minlock, float maxlock, float minidis)
		{
			if (Mathf.Abs(fdis) > minidis)
			{
				fdis = ((!(fdis > 0f)) ? (0f - minidis) : minidis);
			}
			if (minb + fdis >= minlock && maxa + fdis <= maxlock)
			{
				maxa += fdis;
				minb += fdis;
			}
			else if (fdis > 0f)
			{
				fdis = maxlock - maxa;
				if (fdis < 0f && 0f - fdis > minidis)
				{
					fdis = 0f - minidis;
				}
				maxa += fdis;
				minb += fdis;
			}
			else
			{
				fdis = minlock - minb;
				if (fdis > 0f && fdis > minidis)
				{
					fdis = minidis;
				}
				maxa += fdis;
				minb += fdis;
			}
		}

		public void UpdateSmooth(ref float a, ref float b, float fdis, bool bCheck, float minidis)
		{
			float num = fdis;
			if (Mathf.Abs(num) < minidis)
			{
				a += num;
				b += num;
			}
			else
			{
				num = ((!bCheck) ? minidis : ((!(num > 0f)) ? (0f - minidis) : minidis));
				a += num;
				b += num;
			}
		}

		private void MoveLR(float dis)
		{
			_left += dis;
			_right += dis;
		}

		private void MoveTB(float dis)
		{
			_top += dis;
			_bottom += dis;
		}

		private float GetFocusLeft(float focusCenterX)
		{
			return focusCenterX - _halfsizex;
		}

		private float GetFocusRight(float focusCenterX)
		{
			return focusCenterX + _halfsizex;
		}

		private float GetFocusTop(float focusCenterY)
		{
			return focusCenterY + _halfsizey;
		}

		private float GetFocusBottom(float focusCenterY)
		{
			return focusCenterY - _halfsizey;
		}

		public void Update(Vector3 focusCenter, LockRange targetLockRange)
		{
			shiftX = 0f;
			if (GetFocusLeft(focusCenter.x) < _left)
			{
				shiftX = GetFocusLeft(focusCenter.x) - _left;
			}
			else if (GetFocusRight(focusCenter.x) > _right)
			{
				shiftX = GetFocusRight(focusCenter.x) - _right;
			}
			float num = 0f;
			if (bNeedUnSlow)
			{
				num = fSpeed * 0.033f;
				if (Time.timeScale == 1f)
				{
					bNeedUnSlow = false;
				}
			}
			else
			{
				num = fSpeed * Time.deltaTime;
			}
			if (_left >= targetLockRange.MinX && _right <= targetLockRange.MaxX)
			{
				if (bNeedStickTarget)
				{
					Vector3 position = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position;
					if (position.x - ManagedSingleton<StageHelper>.Instance.fCameraWHalf - GetFocusLeft(focusCenter.x) > 0f - fMaxEdgeDis && shiftX < 0f)
					{
						num = Mathf.Max(num, position.x - ManagedSingleton<StageHelper>.Instance.fCameraWHalf + fMaxEdgeDis - GetFocusLeft(focusCenter.x));
					}
					if (position.x + ManagedSingleton<StageHelper>.Instance.fCameraWHalf - GetFocusRight(focusCenter.x) < fMaxEdgeDis && shiftX > 0f)
					{
						num = Mathf.Max(num, GetFocusRight(focusCenter.x) - (position.x + ManagedSingleton<StageHelper>.Instance.fCameraWHalf - fMaxEdgeDis));
					}
					bNeedStickTarget = false;
				}
				UpdateSmoothInRange(ref _right, ref _left, shiftX, targetLockRange.MinX, targetLockRange.MaxX, num);
			}
			else if (_right - _left > targetLockRange.MaxX - targetLockRange.MinX)
			{
				dis = (targetLockRange.MaxX + targetLockRange.MinX) / 2f - (_right + _left) / 2f;
				UpdateSmooth(ref _left, ref _right, dis, true, num);
			}
			else if (_left < targetLockRange.MinX)
			{
				dis = targetLockRange.MinX - _left;
				UpdateSmooth(ref _left, ref _right, dis, false, num);
			}
			else if (_right > targetLockRange.MaxX)
			{
				dis = _right - targetLockRange.MaxX;
				UpdateSmooth(ref _left, ref _right, 0f - dis, true, num);
			}
			shiftY = 0f;
			if (GetFocusBottom(focusCenter.y) < _bottom)
			{
				shiftY = GetFocusBottom(focusCenter.y) - _bottom;
			}
			else if (GetFocusTop(focusCenter.y) > _top)
			{
				shiftY = GetFocusTop(focusCenter.y) - _top;
			}
			if (_top <= targetLockRange.MaxY && _bottom >= targetLockRange.MinY)
			{
				UpdateSmoothInRange(ref _top, ref _bottom, shiftY, targetLockRange.MinY, targetLockRange.MaxY, num);
			}
			else if (_top - _bottom > targetLockRange.MaxY - targetLockRange.MinY)
			{
				dis = (targetLockRange.MaxY + targetLockRange.MinY) / 2f - (_top + _bottom) / 2f;
				UpdateSmooth(ref _top, ref _bottom, dis, true, num);
			}
			else if (_bottom < targetLockRange.MinY)
			{
				dis = targetLockRange.MinY - _bottom;
				UpdateSmooth(ref _top, ref _bottom, dis, false, num);
			}
			else if (_top > targetLockRange.MaxY)
			{
				dis = _top - targetLockRange.MaxY;
				UpdateSmooth(ref _top, ref _bottom, 0f - dis, true, num);
			}
			Center.x = (_left + _right) / 2f;
			Center.y = (_top + _bottom) / 2f;
		}
	}

	private int cameraFOV = Shader.PropertyToID("cameraFOV");

	[HideInInspector]
	public CameraEvent CurrentCameraEvent;

	[HideInInspector]
	public CameraEvent BackCameraEvent;

	[HideInInspector]
	public CameraEvent SaveCameraEvent;

	[HideInInspector]
	public bool bSavingCheck;

	[HideInInspector]
	public Vector3 BackPosition = Vector3.zero;

	public LockRange CurrentLockRange = new LockRange(0f, 1000f, 0f, 1000f);

	public Controller2D Target;

	public float VerticalOffset;

	public float fReserveVerticalOffset;

	public float VerticalSmoothTime;

	public Vector2 FocusAreaSize;

	private FocusArea _focusArea;

	private float _smoothVelocityY;

	private Vector3 _cameraMovementVal;

	private Camera[] _cameras;

	private Coroutine tRoomInPosCoroutine;

	private int cameraMode;

	private const float SpawnerRange = 10f;

	private int _spawnerIdx;

	private readonly float _defaultFov = 31.5f;

	private float _designFov = 31.5f;

	private const int DesignWidth = 1920;

	private const int DesignHeight = 1080;

	private float _hFovInRads;

	private float cameraHHalf;

	private float cameraWHalf;

	private int nNoBack;

	private LockRange lrReserve = new LockRange(0f, 1000f, 0f, 1000f);

	private int nReserveBack;

	public static float fSpeed = 30f;

	public static bool bNeedStickTarget = false;

	private float fReserveSpeed = 30f;

	private bool bInit;

	public const int nLockCameraSpeed = 1500;

	private RenderTexture screenRenderTexture;

	private GameObject renderTextureUI;

	public string[] SpecialStages = new string[7] { "stage01_0303_e1", "stage04_0201_e1", "stage04_0801_e1", "stage01_1006_e1", "stage09_0101_e1", "stage04_3601_e1", "stage04_3602_e1" };

	private Vector2 _focusPosition;

	private float _shakeLevel;

	private const float ShakeMovement = 0.1f;

	private float _shakeTime;

	private const float ShakeTimeMax = 0.2f;

	private const float Factor = 1f;

	private RenderTexture lowResRenderTexture;

	private float CameraMaxRangeX
	{
		get
		{
			return CurrentLockRange.MaxX + ManagedSingleton<StageHelper>.Instance.fCameraWHalf;
		}
	}

	private float CameraMinRangeX
	{
		get
		{
			return CurrentLockRange.MinX - ManagedSingleton<StageHelper>.Instance.fCameraWHalf;
		}
	}

	private float CameraMaxRangeY
	{
		get
		{
			return CurrentLockRange.MaxY + ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
		}
	}

	private float CameraMinRangeY
	{
		get
		{
			return CurrentLockRange.MinY - ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
		}
	}

	public int CameraMode
	{
		get
		{
			return cameraMode;
		}
	}

	public float DesignFov
	{
		get
		{
			return _designFov;
		}
		set
		{
			_designFov = value;
			if (_cameras != null)
			{
				Camera[] cameras = _cameras;
				for (int i = 0; i < cameras.Length; i++)
				{
					cameras[i].fieldOfView = _designFov;
				}
			}
		}
	}

	public static float fMaxEdgeDis
	{
		get
		{
			return Mathf.Min(5f, ManagedSingleton<StageHelper>.Instance.fCameraWHalf);
		}
	}

	public Vector2 FocusPosition
	{
		get
		{
			return _focusPosition;
		}
		set
		{
		}
	}

	private void Awake()
	{
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera = GetComponent<Camera>();
		Singleton<GenericEventManager>.Instance.AttachEvent<float, bool>(EventManager.ID.CAMERA_SHAKE, EventCameraShake);
		Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.LockRangeParam>(EventManager.ID.LOCK_RANGE, EventLockRange);
		Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageCameraFocus>(EventManager.ID.STAGE_CAMERA_FOCUS, EventFoucs);
		Singleton<GenericEventManager>.Instance.AttachEvent<GameObject>(EventManager.ID.STAGE_DELETE_CHECK, EventDeleteCheck);
		if ((bool)Target)
		{
			Init();
		}
		int width = Screen.width / 2;
		int height = Screen.height / 2;
		lowResRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default);
		InitSteamRenderToTexture();
	}

	private void InitSteamRenderToTexture()
	{
		float resolutionRate = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.ResolutionRate;
		if (resolutionRate == 1f)
		{
			return;
		}
		screenRenderTexture = new RenderTexture((int)(1920f * resolutionRate), (int)(1080f * resolutionRate), 0, RenderTextureFormat.Default);
		screenRenderTexture.filterMode = FilterMode.Point;
		Camera[] componentsInChildren = GetComponentsInChildren<Camera>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].targetTexture = screenRenderTexture;
		}
		renderTextureUI = new GameObject("RenderTextureUI");
		renderTextureUI.transform.SetParent(MonoBehaviourSingleton<UIManager>.Instance.CanvasUI);
		renderTextureUI.transform.SetAsFirstSibling();
		RawImage rawImage = renderTextureUI.AddComponent<RawImage>();
		rawImage.texture = screenRenderTexture;
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("shader/uieffect", "RenderTextureUnlit", delegate(Material obj)
		{
			if (obj != null)
			{
				rawImage.material = UnityEngine.Object.Instantiate(obj);
			}
		});
		RectTransform component = renderTextureUI.GetComponent<RectTransform>();
		component.anchorMax = new Vector2(1f, 1f);
		component.anchorMin = new Vector2(0f, 0f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.Left(0f);
		component.Top(0f);
		component.Right(0f);
		component.Bottom(0f);
		component.localPosition = Vector3.zero;
		component.localScale = Vector3.one;
	}

	public void Init()
	{
		if (!bInit)
		{
			bInit = true;
			_cameraMovementVal = new Vector3(0f, 0f, base.transform.position.z);
			_cameras = GetComponentsInChildren<Camera>();
			OrangeBattleUtility.AddBloom(_cameras[2].gameObject);
			UpdateCameraFov(_defaultFov);
			Camera component = GetComponent<Camera>();
			cameraHHalf = Mathf.Tan(0.5f * component.fieldOfView * ((float)Math.PI / 180f)) * Mathf.Abs(component.transform.position.z);
			cameraWHalf = cameraHHalf * component.aspect;
			ManagedSingleton<StageHelper>.Instance.fCameraHHalf = cameraHHalf;
			ManagedSingleton<StageHelper>.Instance.fCameraWHalf = cameraWHalf;
			Bounds targetBounds = new Bounds(base.transform.position, Target.Collider2D.bounds.size);
			_focusArea = new FocusArea(targetBounds, FocusAreaSize, CurrentLockRange);
			CurrentCameraEvent = CameraEvent.None;
		}
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<float, bool>(EventManager.ID.CAMERA_SHAKE, EventCameraShake);
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.LockRangeParam>(EventManager.ID.LOCK_RANGE, EventLockRange);
		Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageCameraFocus>(EventManager.ID.STAGE_CAMERA_FOCUS, EventFoucs);
		Singleton<GenericEventManager>.Instance.DetachEvent<GameObject>(EventManager.ID.STAGE_DELETE_CHECK, EventDeleteCheck);
		lowResRenderTexture.Release();
		if (screenRenderTexture != null)
		{
			screenRenderTexture.Release();
		}
		if (renderTextureUI != null)
		{
			UnityEngine.Object.DestroyImmediate(renderTextureUI);
		}
	}

	private void LateUpdate()
	{
		if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
		{
			return;
		}
		switch (CurrentCameraEvent)
		{
		case CameraEvent.Focus:
			CameraFocus();
			break;
		case CameraEvent.Shake:
			if (BackCameraEvent == CameraEvent.Focus)
			{
				CameraFocus();
			}
			else
			{
				base.transform.position = BackPosition;
			}
			CameraShake();
			break;
		}
		if (_cameras == null || !(BattleInfoUI.Instance != null))
		{
			return;
		}
		if (BattleInfoUI.Instance.IsBossAppear)
		{
			if (cameraMode != 2 && !Array.Exists(SpecialStages, (string stage) => stage == StageUpdate.gStageName))
			{
				Camera obj = _cameras[3];
				bool flag2 = (_cameras[4].enabled = true);
				obj.enabled = flag2;
				_cameras[0].cullingMask &= ~(1 << ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer);
				cameraMode = 2;
			}
		}
		else if (cameraMode != 1)
		{
			Camera obj2 = _cameras[3];
			bool flag2 = (_cameras[4].enabled = false);
			obj2.enabled = flag2;
			_cameras[0].cullingMask |= 1 << ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer;
			cameraMode = 1;
		}
	}

	private void EventLockRange(EventManager.LockRangeParam tLockRangeParam)
	{
		float fMinX = tLockRangeParam.fMinX;
		float fMaxX = tLockRangeParam.fMaxX;
		float fMinY = tLockRangeParam.fMinY;
		float fMaxY = tLockRangeParam.fMaxY;
		if (tLockRangeParam.nMode == 0)
		{
			LockRange currentLockRange = CurrentLockRange;
			CurrentLockRange.MinX = fMinX;
			CurrentLockRange.MaxX = fMaxX;
			CurrentLockRange.MaxY = fMaxY;
			CurrentLockRange.MinY = fMinY;
			if (CurrentLockRange.MaxX - CurrentLockRange.MinX > cameraWHalf * 2f)
			{
				CurrentLockRange.MaxX -= cameraWHalf;
				CurrentLockRange.MinX += cameraWHalf;
			}
			else
			{
				CurrentLockRange.MaxX = (CurrentLockRange.MaxX + CurrentLockRange.MinX) / 2f;
				CurrentLockRange.MinX = CurrentLockRange.MaxX;
			}
			if (CurrentLockRange.MaxY - CurrentLockRange.MinY > cameraHHalf * 2f)
			{
				CurrentLockRange.MaxY -= cameraHHalf;
				CurrentLockRange.MinY += cameraHHalf;
			}
			else
			{
				CurrentLockRange.MaxY = (CurrentLockRange.MaxY + CurrentLockRange.MinY) / 2f;
				CurrentLockRange.MinY = CurrentLockRange.MaxY;
			}
			int? num = tLockRangeParam.nNoBack;
			float? num2 = tLockRangeParam.fSpeed;
			float num3 = fSpeed;
			int num4 = nNoBack;
			nNoBack = num ?? 0;
			fSpeed = num2 ?? num3;
			float verticalOffset = VerticalOffset;
			if (tLockRangeParam.fOY.HasValue)
			{
				VerticalOffset = tLockRangeParam.fOY ?? 0f;
			}
			if (nNoBack == 5)
			{
				CurrentLockRange = lrReserve;
				nNoBack = nReserveBack;
				fSpeed = fReserveSpeed;
				VerticalOffset = fReserveVerticalOffset;
			}
			else
			{
				nReserveBack = num4;
				lrReserve = currentLockRange;
				fReserveSpeed = num3;
				fReserveVerticalOffset = verticalOffset;
			}
			if (CurrentCameraEvent == CameraEvent.None)
			{
				Bounds targetBounds = new Bounds(new Vector3(base.transform.position.x, base.transform.position.y + Target.Collider2D.bounds.size.y / 2f, base.transform.position.z), Target.Collider2D.bounds.size);
				_focusArea = new FocusArea(targetBounds, FocusAreaSize, new LockRange(-9999f, 9999f, -9999f, 9999f));
			}
			if (tLockRangeParam.bSetFocus ?? true)
			{
				if (Vector2.Distance(base.transform.position.xy(), _focusArea.Center + Vector2.up * VerticalOffset) > 0.05f)
				{
					Bounds targetBounds2 = new Bounds(new Vector3(base.transform.position.x, base.transform.position.y + Target.Collider2D.bounds.size.y / 2f - FocusAreaSize.y / 2f, base.transform.position.z), Target.Collider2D.bounds.size);
					_focusArea = new FocusArea(targetBounds2, FocusAreaSize, new LockRange(-9999f, 9999f, -9999f, 9999f));
				}
				CurrentCameraEvent = CameraEvent.Focus;
				if (bSavingCheck)
				{
					SaveCameraEvent = CurrentCameraEvent;
				}
			}
			if (tLockRangeParam.bSlowWhenMove)
			{
				StageUpdate.UnSlowStage();
				StageUpdate.SlowStage(0.1f, 0.2f);
				_focusArea.bNeedUnSlow = true;
			}
		}
		else if (tLockRangeParam.nMode == 1)
		{
			if (tLockRangeParam.fMinX < CurrentLockRange.MinX - cameraWHalf)
			{
				CurrentLockRange.MinX = tLockRangeParam.fMinX + cameraWHalf;
			}
			if (tLockRangeParam.fMaxX > CurrentLockRange.MaxX + cameraWHalf)
			{
				CurrentLockRange.MaxX = tLockRangeParam.fMaxX - cameraWHalf;
			}
			if (tLockRangeParam.fMinY < CurrentLockRange.MinY - cameraHHalf)
			{
				CurrentLockRange.MinY = tLockRangeParam.fMinY + cameraHHalf;
			}
			if (tLockRangeParam.fMaxY > CurrentLockRange.MaxY + cameraHHalf)
			{
				CurrentLockRange.MaxY = tLockRangeParam.fMaxY - cameraHHalf;
			}
			_focusArea = new FocusArea(Target.Collider2D.bounds, FocusAreaSize, CurrentLockRange);
			_focusPosition = _focusArea.Center + Vector2.up * VerticalOffset;
			base.transform.position = (Vector3)_focusPosition + _cameraMovementVal;
		}
		else if (tLockRangeParam.nMode == 2)
		{
			Vector3 vector = tLockRangeParam.vDir ?? Vector3.zero;
			int num5 = tLockRangeParam.nNoBack ?? 0;
			if (vector.x > 0f)
			{
				if (((uint)num5 & 2u) != 0 || (num5 & 0x1E) == 0)
				{
					CurrentLockRange.MinX += vector.x;
				}
				if (((uint)num5 & 4u) != 0)
				{
					CurrentLockRange.MaxX += vector.x;
				}
				if (CurrentLockRange.MaxX - CurrentLockRange.MinX <= 0f)
				{
					if ((num5 & 1) == 0)
					{
						CurrentLockRange.MinX = CurrentLockRange.MaxX;
					}
					else
					{
						CurrentLockRange.MaxX = CurrentLockRange.MinX;
					}
				}
			}
			else
			{
				if (((uint)num5 & 4u) != 0 || (num5 & 0x1E) == 0)
				{
					CurrentLockRange.MaxX += vector.x;
				}
				if (((uint)num5 & 2u) != 0)
				{
					CurrentLockRange.MinX += vector.x;
				}
				if (CurrentLockRange.MaxX - CurrentLockRange.MinX <= 0f)
				{
					if ((num5 & 1) == 0)
					{
						CurrentLockRange.MaxX = CurrentLockRange.MinX;
					}
					else
					{
						CurrentLockRange.MinX = CurrentLockRange.MaxX;
					}
				}
			}
			if (vector.y > 0f)
			{
				if (((uint)num5 & 0x10u) != 0 || (num5 & 0x1E) == 0)
				{
					CurrentLockRange.MinY += vector.y;
				}
				if (((uint)num5 & 8u) != 0)
				{
					CurrentLockRange.MaxY += vector.y;
				}
				if (CurrentLockRange.MaxY - CurrentLockRange.MinY <= 0f)
				{
					if ((num5 & 1) == 0)
					{
						CurrentLockRange.MinY = CurrentLockRange.MaxY;
					}
					else
					{
						CurrentLockRange.MaxY = CurrentLockRange.MinY;
					}
				}
			}
			else
			{
				if (((uint)num5 & 8u) != 0 || (num5 & 0x1E) == 0)
				{
					CurrentLockRange.MaxY += vector.y;
				}
				if (((uint)num5 & 0x10u) != 0)
				{
					CurrentLockRange.MinY += vector.y;
				}
				if (CurrentLockRange.MaxY - CurrentLockRange.MinY <= 0f)
				{
					if ((num5 & 1) == 0)
					{
						CurrentLockRange.MaxY = CurrentLockRange.MinY;
					}
					else
					{
						CurrentLockRange.MinY = CurrentLockRange.MaxY;
					}
				}
			}
			_focusArea.UpdateLockRange(CurrentLockRange);
		}
		else if (tLockRangeParam.nMode == 3)
		{
			float fCameraHHalf = ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
			float fCameraWHalf = ManagedSingleton<StageHelper>.Instance.fCameraWHalf;
			CurrentLockRange.MaxX -= fCameraWHalf - cameraWHalf;
			CurrentLockRange.MinX += fCameraWHalf - cameraWHalf;
			CurrentLockRange.MaxY -= fCameraHHalf - cameraHHalf;
			CurrentLockRange.MinY += fCameraHHalf - cameraHHalf;
			cameraWHalf = fCameraWHalf;
			cameraHHalf = fCameraHHalf;
		}
	}

	public void CameraMove(Vector3 vDis)
	{
		if (CurrentCameraEvent == CameraEvent.Shake)
		{
			BackPosition += vDis;
		}
		else
		{
			base.transform.position = base.transform.position + vDis;
		}
	}

	public void StopCameraShake()
	{
		if (CurrentCameraEvent == CameraEvent.Shake)
		{
			_shakeTime = 0f;
			CurrentCameraEvent = BackCameraEvent;
			if (BackCameraEvent != CameraEvent.Focus)
			{
				base.transform.position = BackPosition;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Camera component = GetComponent<Camera>();
		cameraHHalf = Mathf.Tan(0.5f * component.fieldOfView * ((float)Math.PI / 180f)) * Mathf.Abs(component.transform.position.z);
		cameraWHalf = cameraHHalf * component.aspect;
		Gizmos.color = new Color(1f, 1f, 1f);
		Gizmos.DrawWireCube(base.transform.position, new Vector3(cameraWHalf, cameraHHalf, 1.2f));
	}

	private void EventDeleteCheck(GameObject tObj)
	{
		if (tObj.GetInstanceID() == base.gameObject.GetInstanceID())
		{
			Target = null;
		}
	}

	private void EventFoucs(EventManager.StageCameraFocus tStageCameraFocus)
	{
		switch (tStageCameraFocus.nMode)
		{
		case 1:
			if (tRoomInPosCoroutine == null)
			{
				tRoomInPosCoroutine = StartCoroutine(RoomInPosCoroutine(tStageCameraFocus));
			}
			return;
		case 2:
		{
			Bounds targetBounds = new Bounds(new Vector3(tStageCameraFocus.roominpos.x, tStageCameraFocus.roominpos.y + FocusAreaSize.y / 2f, tStageCameraFocus.roominpos.z), FocusAreaSize);
			_focusArea = new FocusArea(targetBounds, FocusAreaSize, CurrentLockRange);
			base.transform.position = new Vector3(_focusArea.Center.x, base.transform.position.y, base.transform.position.z);
			return;
		}
		case 4:
			bSavingCheck = false;
			if (CurrentCameraEvent != CameraEvent.Shake)
			{
				CurrentCameraEvent = SaveCameraEvent;
				if (tStageCameraFocus.bRightNow)
				{
					if (Target == null)
					{
						StartCoroutine(FocusCoroutine());
					}
					else
					{
						_focusArea = new FocusArea(Target.Collider2D.bounds, FocusAreaSize, CurrentLockRange);
					}
				}
			}
			else
			{
				BackCameraEvent = SaveCameraEvent;
			}
			return;
		case 5:
			if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
			{
				CameraEvent currentCameraEvent = CurrentCameraEvent;
				if (currentCameraEvent == CameraEvent.Focus)
				{
					CameraFocus();
				}
			}
			return;
		}
		if (CurrentCameraEvent == CameraEvent.Shake)
		{
			CurrentCameraEvent = BackCameraEvent;
			if (BackCameraEvent != CameraEvent.Focus)
			{
				base.transform.position = BackPosition;
			}
		}
		if (tStageCameraFocus.bLock)
		{
			if (CurrentCameraEvent != CameraEvent.Shake)
			{
				if (tStageCameraFocus.nMode == 3)
				{
					SaveCameraEvent = CurrentCameraEvent;
				}
				CurrentCameraEvent = CameraEvent.Focus;
				if (bSavingCheck)
				{
					SaveCameraEvent = CurrentCameraEvent;
				}
				if (tStageCameraFocus.bRightNow)
				{
					if (Target == null)
					{
						StartCoroutine(FocusCoroutine());
					}
					else
					{
						CheckFocusBounds();
						_focusArea = new FocusArea(Target.Collider2D.bounds, FocusAreaSize, CurrentLockRange);
						_focusPosition = _focusArea.Center + Vector2.up * VerticalOffset;
						base.transform.position = (Vector3)_focusPosition + _cameraMovementVal;
					}
				}
			}
			else
			{
				if (tStageCameraFocus.nMode == 3)
				{
					SaveCameraEvent = BackCameraEvent;
				}
				BackCameraEvent = CameraEvent.Focus;
			}
		}
		else
		{
			if (CurrentCameraEvent != CameraEvent.Shake)
			{
				if (tStageCameraFocus.nMode == 3)
				{
					SaveCameraEvent = CurrentCameraEvent;
				}
				CurrentCameraEvent = CameraEvent.None;
				if (StageResManager.GetStageUpdate() != null)
				{
					StageResManager.GetStageUpdate().sLastLockRangeSyncID = "";
				}
				if (bSavingCheck)
				{
					SaveCameraEvent = CurrentCameraEvent;
				}
			}
			else
			{
				if (tStageCameraFocus.nMode == 3)
				{
					SaveCameraEvent = BackCameraEvent;
				}
				BackCameraEvent = CameraEvent.None;
				if (bSavingCheck)
				{
					SaveCameraEvent = BackCameraEvent;
				}
			}
			if (tStageCameraFocus.bUnRange)
			{
				Bounds targetBounds = new Bounds(new Vector3(base.transform.position.x, base.transform.position.y - Target.Collider2D.bounds.size.y / 2f, base.transform.position.z), Target.Collider2D.bounds.size);
				_focusArea = new FocusArea(targetBounds, FocusAreaSize, new LockRange(-9999f, 9999f, -9999f, 9999f));
			}
		}
		if (tStageCameraFocus.nMode == 3)
		{
			bSavingCheck = true;
		}
	}

	private void CheckFocusBounds()
	{
		Bounds bounds = Target.Collider2D.bounds;
		float num = bounds.center.x - FocusAreaSize.x / 2f;
		float num2 = bounds.center.x + FocusAreaSize.x / 2f;
		float y = bounds.min.y;
		float num3 = bounds.min.y + FocusAreaSize.y;
		if (num > CameraMaxRangeX)
		{
			CurrentLockRange.MaxX = (num + num2) / 2f;
			if (Target != null)
			{
				LockRangeObj component = Target.GetComponent<LockRangeObj>();
				if (component != null)
				{
					component.vLockLR.Set(CameraMinRangeX, CameraMaxRangeX);
				}
			}
		}
		if (num2 < CameraMinRangeX)
		{
			CurrentLockRange.MinX = (num + num2) / 2f;
			if (Target != null)
			{
				LockRangeObj component2 = Target.GetComponent<LockRangeObj>();
				if (component2 != null)
				{
					component2.vLockLR.Set(CameraMinRangeX, CameraMaxRangeX);
				}
			}
		}
		if (y > CameraMaxRangeY)
		{
			CurrentLockRange.MaxY = (y + num3) / 2f;
			if (Target != null)
			{
				LockRangeObj component3 = Target.GetComponent<LockRangeObj>();
				if (component3 != null)
				{
					component3.vLockTB.Set(CameraMinRangeY, CameraMaxRangeY);
				}
			}
		}
		if (!(num3 < CameraMinRangeY))
		{
			return;
		}
		CurrentLockRange.MinY = (y + num3) / 2f;
		if (Target != null)
		{
			LockRangeObj component4 = Target.GetComponent<LockRangeObj>();
			if (component4 != null)
			{
				component4.vLockTB.Set(CameraMinRangeY, CameraMaxRangeY);
			}
		}
	}

	private IEnumerator WaitPlayerReborn()
	{
		while (OrangeBattleUtility.CurrentCharacter != null && OrangeBattleUtility.CurrentCharacter.IsDead())
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private IEnumerator RoomInPosCoroutine(EventManager.StageCameraFocus tStageCameraFocus)
	{
		Vector3 roominpos = tStageCameraFocus.roominpos;
		float fRoomInTime = tStageCameraFocus.fRoomInTime;
		float fRoomOutTime = tStageCameraFocus.fRoomOutTime;
		float fRoomInFov = tStageCameraFocus.fRoomInFov;
		bool bDontPlayMotion = tStageCameraFocus.bDontPlayMotion;
		yield return WaitPlayerReborn();
		if (OrangeBattleUtility.CurrentCharacter.UsingVehicle)
		{
			tRoomInPosCoroutine = null;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
			yield break;
		}
		base.enabled = false;
		Vector3 backPos = base.transform.position;
		float fBackFov = _cameras[0].fieldOfView;
		roominpos.z = -18f;
		Vector3 vDir = roominpos - backPos;
		float fLen = vDir.magnitude;
		vDir /= fLen;
		float fdLen2 = fLen / fRoomInTime * 0.01f;
		float num = Mathf.Max(fRoomInFov, 15f);
		float fdFov2 = (num - fBackFov) / fRoomInTime * 0.01f;
		float fLeftTime2 = 0f;
		while (fRoomInTime > 0f)
		{
			fLeftTime2 += Time.deltaTime;
			while (fRoomInTime > 0f && fLeftTime2 > 0.01f)
			{
				fLeftTime2 -= 0.01f;
				fRoomInTime -= 0.01f;
				base.transform.position = base.transform.position + vDir * fdLen2;
				_cameras[0].fieldOfView = _cameras[0].fieldOfView + fdFov2;
			}
			yield return CoroutineDefine._waitForEndOfFrame;
			Shader.SetGlobalFloat(cameraFOV, _cameras[0].fieldOfView);
			yield return StageUpdate.WaitGamePauseProcess();
		}
		yield return WaitPlayerReborn();
		yield return StageUpdate.WaitGamePauseProcess();
		if (!bDontPlayMotion && OrangeBattleUtility.CurrentCharacter != null)
		{
			bool bLcok = true;
			OrangeBattleUtility.CurrentCharacter.SetWinPose(delegate
			{
				bLcok = false;
			});
			while (bLcok && (!(OrangeBattleUtility.CurrentCharacter != null) || !OrangeBattleUtility.CurrentCharacter.IsDead()))
			{
				if (OrangeBattleUtility.CurrentCharacter != null && !OrangeBattleUtility.CurrentCharacter.CheckActStatusEvt(12, 1))
				{
					OrangeBattleUtility.CurrentCharacter.SetWinPose(delegate
					{
						bLcok = false;
					});
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		yield return WaitPlayerReborn();
		yield return StageUpdate.WaitGamePauseProcess();
		if (fRoomOutTime != -1f)
		{
			fdLen2 = fLen / fRoomOutTime * 0.01f;
			fdFov2 = (fRoomInFov - fBackFov) / fRoomOutTime * 0.01f;
			fLeftTime2 = 0f;
			while (fRoomOutTime > 0f)
			{
				fLeftTime2 += Time.deltaTime;
				while (fRoomOutTime > 0f && fLeftTime2 > 0.01f)
				{
					fLeftTime2 -= 0.01f;
					fRoomOutTime -= 0.01f;
					base.transform.position = base.transform.position - vDir * fdLen2;
					_cameras[0].fieldOfView = _cameras[0].fieldOfView - fdFov2;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
				yield return StageUpdate.WaitGamePauseProcess();
			}
			_cameras[0].fieldOfView = fBackFov;
			base.transform.position = backPos;
			yield return CoroutineDefine._waitForEndOfFrame;
			yield return StageUpdate.WaitGamePauseProcess();
		}
		else
		{
			yield return StageUpdate.WaitGamePauseProcessTime(1f);
		}
		tRoomInPosCoroutine = null;
		if (tStageCameraFocus.bCallStageEnd)
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_END_REPORT);
		}
	}

	private IEnumerator FocusCoroutine()
	{
		while (Target == null)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		CurrentCameraEvent = CameraEvent.Focus;
		Bounds targetBounds = new Bounds(new Vector3(base.transform.position.x, base.transform.position.y - FocusAreaSize.y / 2f + Target.Collider2D.bounds.size.y / 2f, base.transform.position.z), Target.Collider2D.bounds.size);
		_focusArea = new FocusArea(targetBounds, FocusAreaSize, CurrentLockRange);
		_focusPosition = _focusArea.Center + Vector2.up * VerticalOffset;
		base.transform.position = (Vector3)_focusPosition + _cameraMovementVal;
	}

	private void CameraFocus()
	{
		if (Target == null)
		{
			return;
		}
		_focusArea.Update(Target.Collider2D.bounds.center, CurrentLockRange);
		switch (nNoBack)
		{
		case 1:
			if (_focusArea._left > CurrentLockRange.MinX)
			{
				CurrentLockRange.MinX = _focusArea._left;
				if (CurrentLockRange.MaxX < CurrentLockRange.MinX)
				{
					CurrentLockRange.MinX = CurrentLockRange.MaxX;
				}
			}
			break;
		case 2:
			if (_focusArea._right < CurrentLockRange.MaxX)
			{
				CurrentLockRange.MaxX = _focusArea._right;
				if (CurrentLockRange.MaxX < CurrentLockRange.MinX)
				{
					CurrentLockRange.MaxX = CurrentLockRange.MinX;
				}
			}
			break;
		case 3:
			if (CurrentLockRange.MinY > _focusArea._bottom)
			{
				CurrentLockRange.MinY = _focusArea._bottom;
			}
			break;
		case 4:
			if (CurrentLockRange.MaxY < _focusArea._top)
			{
				CurrentLockRange.MaxY = _focusArea._top;
			}
			break;
		}
		_focusPosition = _focusArea.Center + Vector2.up * VerticalOffset;
		_focusPosition.y = Mathf.SmoothDamp(base.transform.position.y, _focusPosition.y, ref _smoothVelocityY, VerticalSmoothTime);
		base.transform.position = (Vector3)_focusPosition + _cameraMovementVal;
	}

	private void EventCameraShake(float shakeLevel, bool bTriggerFall)
	{
		if (shakeLevel > 3f)
		{
			shakeLevel = 3f;
		}
		_shakeLevel = shakeLevel;
		if (bTriggerFall)
		{
			FallingFloor[] array = OrangeSceneManager.FindObjectsOfTypeCustom<FallingFloor>();
			foreach (FallingFloor fallingFloor in array)
			{
				Vector3 vector = fallingFloor.transform.position - base.transform.position;
				if (Mathf.Abs(vector.x) < ManagedSingleton<StageHelper>.Instance.fCameraWHalf * 4f && Mathf.Abs(vector.y) < ManagedSingleton<StageHelper>.Instance.fCameraWHalf * 4f)
				{
					fallingFloor.TriggerFall();
				}
			}
		}
		if (CurrentCameraEvent != CameraEvent.Shake)
		{
			BackCameraEvent = CurrentCameraEvent;
			BackPosition = base.transform.position;
			_shakeTime = 0.2f;
			CurrentCameraEvent = CameraEvent.Shake;
		}
	}

	private void CameraShake()
	{
		if (_shakeTime <= 0f)
		{
			CurrentCameraEvent = BackCameraEvent;
			if (BackCameraEvent != CameraEvent.Focus)
			{
				base.transform.position = BackPosition;
			}
		}
		else
		{
			base.transform.position += UnityEngine.Random.insideUnitSphere * 0.1f * _shakeLevel;
			_shakeTime -= Time.deltaTime * 1f;
		}
	}

	public void UpdateCameraFov(float p_targetFov)
	{
		if (Screen.width != 1920 || Screen.height != 1080)
		{
			float num = p_targetFov * ((float)Math.PI / 180f);
			float num2 = 1.7777778f;
			_hFovInRads = 2f * Mathf.Atan(Mathf.Tan(num / 2f) * num2);
			float num3 = (float)Screen.width / (float)Screen.height;
			float num4 = 2f * Mathf.Atan(Mathf.Tan(_hFovInRads / 2f) / num3);
			DesignFov = num4 * 57.29578f;
		}
		else
		{
			DesignFov = p_targetFov;
		}
		Shader.SetGlobalFloat(cameraFOV, _cameras[0].fieldOfView);
		ManagedSingleton<StageHelper>.Instance.fCameraHHalf = Mathf.Tan(0.5f * _cameras[0].fieldOfView * ((float)Math.PI / 180f)) * Mathf.Abs(_cameras[0].transform.position.z);
		ManagedSingleton<StageHelper>.Instance.fCameraWHalf = ManagedSingleton<StageHelper>.Instance.fCameraHHalf * _cameras[0].aspect;
		EventManager.LockRangeParam lockRangeParam = new EventManager.LockRangeParam();
		lockRangeParam.nMode = 3;
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCK_RANGE, lockRangeParam);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		lowResRenderTexture.DiscardContents();
		Graphics.Blit(source, lowResRenderTexture);
		Graphics.Blit(source, destination);
	}

	public RenderTexture GetScreenRenderTarget()
	{
		return lowResRenderTexture;
	}
}
