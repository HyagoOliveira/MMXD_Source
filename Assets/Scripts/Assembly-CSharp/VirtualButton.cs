using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using enums;

[RequireComponent(typeof(Image))]
public class VirtualButton : MonoBehaviour, IDragHandler, IEventSystemHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler
{
	public static bool AllowPressAndDrag = true;

	public bool isFixed;

	public ButtonId KeyMapping;

	public bool AllowAnalog;

	public AnalogSticks AnalogID;

	public bool AllowSlide;

	public Image ButtonIcon;

	[HideInInspector]
	public bool ActivateAnalog;

	private InputManager _inputManager;

	private OrangeText _bulletText;

	private Color _bulletTextColor;

	private OrangeText _maskText;

	private Color _maskTextColor;

	private RectTransform _rectTransform;

	private float _radius;

	private Vector2 _stickOriginalPos;

	private Vector2 _stickCurrentPos;

	private Vector2 _touchOriginalPos;

	private Vector2 _touchCurrentPos;

	private Image _icon;

	private Image _reloadProgress;

	private Image _maskProgress;

	private Image _stickBg;

	private Image _stick;

	private Image _banMask;

	private VirtualButtonInertia virtualButtonInertia;

	private bool _buttonTipEnable = true;

	private GameObject _spMeterParent;

	private Vector2 _stickRelativePosition;

	private VInt _magazineRemainPrev = new VInt(int.MaxValue);

	public bool AllowUpdate { get; set; }

	public bool _BtnStatus { get; private set; }

	private void Awake()
	{
		_inputManager = MonoBehaviourSingleton<InputManager>.Instance;
		_rectTransform = (RectTransform)base.transform;
		_BtnStatus = false;
		_radius = _rectTransform.sizeDelta.x * 0.5f;
		_stickOriginalPos = _rectTransform.position;
		_stickCurrentPos = Vector2.zero;
		_touchOriginalPos = Vector2.zero;
		_touchCurrentPos = Vector2.zero;
		Transform[] target = base.transform.GetComponentsInChildren<Transform>(true);
		Transform transform = OrangeBattleUtility.FindChildRecursive(ref target, "Icon", true);
		if ((bool)transform)
		{
			_icon = transform.GetComponent<Image>();
		}
		Transform transform2 = OrangeBattleUtility.FindChildRecursive(ref target, "BulletText", true);
		if ((bool)transform2)
		{
			_bulletText = transform2.GetComponent<OrangeText>();
			_bulletTextColor = _bulletText.color;
			_bulletText.text = "";
			transform2.SetSiblingIndex(transform2.GetSiblingIndex() + 2);
		}
		Transform transform3 = OrangeBattleUtility.FindChildRecursive(ref target, "MaskText", true);
		if ((bool)transform3)
		{
			_maskText = transform3.GetComponent<OrangeText>();
			_maskTextColor = _maskText.color;
			_maskText.text = "";
		}
		Transform transform4 = OrangeBattleUtility.FindChildRecursive(ref target, "Progress", true);
		if ((bool)transform4)
		{
			_reloadProgress = (transform4 ? transform4.GetComponent<Image>() : null);
		}
		Transform transform5 = OrangeBattleUtility.FindChildRecursive(ref target, "Mask", true);
		if ((bool)transform5)
		{
			_maskProgress = (transform5 ? transform5.GetComponent<Image>() : null);
		}
		Transform transform6 = OrangeBattleUtility.FindChildRecursive(ref target, "Radius", true);
		if ((bool)transform6)
		{
			_stickBg = (transform6 ? transform6.GetComponent<Image>() : null);
			if (_stickBg != null)
			{
				_stickBg.enabled = false;
			}
		}
		Transform transform7 = OrangeBattleUtility.FindChildRecursive(ref target, "Stick", true);
		if ((bool)transform7)
		{
			_stick = (transform7 ? transform7.GetComponent<Image>() : null);
			if (_stick != null)
			{
				_stick.enabled = false;
			}
		}
		Transform transform8 = OrangeBattleUtility.FindChildRecursive(ref target, "BanMask", true);
		if ((bool)transform8)
		{
			_banMask = (transform8 ? transform8.GetComponent<Image>() : null);
			if (_banMask != null)
			{
				_banMask.gameObject.SetActive(false);
			}
		}
		virtualButtonInertia = base.gameObject.AddOrGetComponent<VirtualButtonInertia>();
		virtualButtonInertia.Init(this);
		AllowUpdate = true;
		_spMeterParent = BattleInfoUI.Instance.playersp.transform.parent.gameObject;
		MonoBehaviourSingleton<InputManager>.Instance.inputKeyChangeEvent += InputKeyChangeHandler;
		MonoBehaviourSingleton<InputManager>.Instance.gamepadMappingEvent += GamepadChangeEventHandler;
		UpdateKeyDisplay();
	}

	private void GamepadChangeEventHandler(GamepadType gamepad)
	{
		UpdateKeyDisplay();
	}

	private void InputKeyChangeHandler()
	{
		UpdateKeyDisplay();
	}

	private void UpdateKeyDisplay()
	{
		if (!(ButtonIcon != null))
		{
			return;
		}
		if (_buttonTipEnable)
		{
			Sprite buttonIcon = MonoBehaviourSingleton<InputManager>.Instance.GetButtonIcon(KeyMapping);
			ButtonIcon.gameObject.SetActive(buttonIcon != null);
			if (buttonIcon != null)
			{
				ButtonIcon.sprite = buttonIcon;
			}
		}
		else
		{
			ButtonIcon.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		MonoBehaviourSingleton<InputManager>.Instance.gamepadMappingEvent -= GamepadChangeEventHandler;
		MonoBehaviourSingleton<InputManager>.Instance.inputKeyChangeEvent -= InputKeyChangeHandler;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (AllowPressAndDrag && AllowSlide && !(eventData.pointerDrag == null) && eventData.eligibleForClick)
		{
			if (!_BtnStatus)
			{
				_inputManager.AddTouchChain(this);
			}
			_BtnStatus = true;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			_BtnStatus = true;
			if (AllowAnalog)
			{
				_touchOriginalPos = eventData.position;
				UpdateStick(ref eventData);
				RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out _stickRelativePosition);
				_stickBg.rectTransform.anchoredPosition = _stickRelativePosition;
				_stick.rectTransform.anchoredPosition = _touchCurrentPos;
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && AllowPressAndDrag)
		{
			virtualButtonInertia.CheckInertia();
			if (AllowAnalog && _BtnStatus)
			{
				UpdateStick(ref eventData);
				_stick.rectTransform.anchoredPosition = _touchCurrentPos;
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			AllowPressAndDrag = true;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			virtualButtonInertia.TriggerInertia(false);
			_inputManager.ClearTouchChain();
			ClearButton();
		}
	}

	public void ClearButton()
	{
		_BtnStatus = false;
		ActivateAnalog = false;
		if (AllowAnalog)
		{
			_stickCurrentPos = Vector2.zero;
			_touchCurrentPos = Vector2.zero;
		}
	}

	private void UpdateStick(ref PointerEventData eventData)
	{
		_stickCurrentPos = eventData.position - _stickOriginalPos;
		if (_stickCurrentPos.sqrMagnitude > _radius * _radius)
		{
			_stickCurrentPos = _stickCurrentPos.normalized * _radius;
		}
		_touchCurrentPos = eventData.position - _touchOriginalPos;
		if (_touchCurrentPos.sqrMagnitude > _radius * _radius)
		{
			_touchCurrentPos = _touchCurrentPos.normalized * _radius;
		}
	}

	public Vector2 GetRelativeStickValue()
	{
		if (!AllowAnalog)
		{
			return Vector2.zero;
		}
		return _touchCurrentPos / _radius;
	}

	public Vector2 GetAbsoluteStickValue()
	{
		if (!AllowAnalog)
		{
			return Vector2.zero;
		}
		return _stickCurrentPos / _radius;
	}

	public bool GetButtonStatus()
	{
		if (virtualButtonInertia == null)
		{
			return _BtnStatus;
		}
		if (virtualButtonInertia.IsInertiaExist)
		{
			return virtualButtonInertia._BtnStatus;
		}
		return _BtnStatus;
	}

	public void SetBulletText(string tex)
	{
		_bulletText.color = _bulletTextColor;
		_bulletText.text = tex;
	}

	public void SetBulletText(string tex, Color col)
	{
		_bulletText.text = tex;
		_bulletText.color = col;
	}

	public void SetMaskText(string tex)
	{
		_maskText.color = _maskTextColor;
		_maskText.text = tex;
	}

	public void SetProgress(float percent)
	{
		if ((bool)_reloadProgress)
		{
			_reloadProgress.fillAmount = percent;
		}
	}

	public float GetReloadProgressFillAmount()
	{
		if ((bool)_reloadProgress)
		{
			return _reloadProgress.fillAmount;
		}
		return 0f;
	}

	public void SetMaskProgress(float percent)
	{
		if ((bool)_maskProgress)
		{
			_maskProgress.fillAmount = 1f - percent;
		}
	}

	public Sprite GetIcon()
	{
		return _icon.sprite;
	}

	public void SetIcon(Sprite targetImage)
	{
		_icon.sprite = targetImage;
	}

	public void SetAnalogVisible(bool visible)
	{
		_stickBg.enabled = visible;
		_stick.enabled = visible;
	}

	public bool GetStickEnable()
	{
		if (_stick == null)
		{
			return false;
		}
		return _stick.enabled;
	}

	public void ClearStick()
	{
		ActivateAnalog = false;
		SetAnalogVisible(false);
		_touchCurrentPos = Vector2.zero;
	}

	public void SetBanMask(bool enable)
	{
		if (_banMask != null)
		{
			_banMask.gameObject.SetActive(enable);
		}
	}

	public void SetMagazineRemainDirty()
	{
		_magazineRemainPrev = new VInt(int.MaxValue);
	}

	public void UpdateValue(VirtualButtonId virtualButtonId, WeaponStruct currentWeaponStruct, OrangeCharacter refCharacter, RefPassiveskill tRefPassiveskill = null)
	{
		if (this == null || !AllowUpdate)
		{
			return;
		}
		if (virtualButtonId == VirtualButtonId.CHIP_SWITCH)
		{
			SetBulletText(tRefPassiveskill.bUsePassiveskill ? "ON" : "OFF");
			SetMaskText("");
			SetMaskProgress(tRefPassiveskill.GetWeaponChipSkillCD(currentWeaponStruct.weaponStatus.nWeaponCheck));
			SetProgress(tRefPassiveskill.GetWeaponChipUseCD(currentWeaponStruct.weaponStatus.nWeaponCheck));
			if (BattleInfoUI.Instance != null)
			{
				if ((bool)_spMeterParent)
				{
					_spMeterParent.SetActive(tRefPassiveskill.bUsePassiveskill);
				}
				BattleInfoUI.Instance.playersp.fillAmount = tRefPassiveskill.GetWeaponChipBulletPercent(currentWeaponStruct.weaponStatus.nWeaponCheck);
			}
		}
		else
		{
			if (currentWeaponStruct == null)
			{
				return;
			}
			float num = (float)(currentWeaponStruct.BulletData.n_FIRE_SPEED - currentWeaponStruct.LastUseTimer.GetMillisecond()) / 1000f;
			float maskProgress = (float)currentWeaponStruct.LastUseTimer.GetMillisecond() / (float)currentWeaponStruct.BulletData.n_FIRE_SPEED;
			float num2 = (float)(currentWeaponStruct.FastBulletDatas[currentWeaponStruct.Reload_index].n_RELOAD - currentWeaponStruct.LastUseTimer.GetMillisecond()) / 1000f;
			float num3 = (float)currentWeaponStruct.LastUseTimer.GetMillisecond() / (float)currentWeaponStruct.FastBulletDatas[currentWeaponStruct.Reload_index].n_RELOAD;
			switch (currentWeaponStruct.BulletData.n_MAGAZINE_TYPE)
			{
			case 0:
				if (currentWeaponStruct.MagazineRemain > 0f)
				{
					if (virtualButtonId == VirtualButtonId.FS_SKILL)
					{
						if (_magazineRemainPrev != (VInt)currentWeaponStruct.MagazineRemain)
						{
							if (currentWeaponStruct.MagazineRemainMax > 1f)
							{
								SetBulletText(string.Format("{0}/{1}", currentWeaponStruct.MagazineRemain, currentWeaponStruct.MagazineRemainMax));
							}
							else
							{
								SetBulletText(string.Empty);
							}
						}
						if ((float)currentWeaponStruct.LastUseTimer.GetMillisecond() < (float)currentWeaponStruct.BulletData.n_FIRE_SPEED && currentWeaponStruct.BulletData.n_FIRE_SPEED >= 600 && num2 > 0f)
						{
							SetMaskProgress(maskProgress);
							SetMaskText(string.Format("{0:N1}", num));
						}
						else
						{
							SetMaskProgress(1f);
							SetMaskText("");
						}
						break;
					}
					if (_magazineRemainPrev != (VInt)currentWeaponStruct.MagazineRemain)
					{
						SetBulletText(string.Format("{0}/{1}", currentWeaponStruct.MagazineRemain, currentWeaponStruct.BulletData.n_MAGAZINE));
					}
					if ((float)currentWeaponStruct.LastUseTimer.GetMillisecond() < (float)currentWeaponStruct.BulletData.n_FIRE_SPEED && currentWeaponStruct.BulletData.n_FIRE_SPEED >= 600 && num2 > 0f)
					{
						SetMaskProgress(maskProgress);
						SetMaskText(string.Format("{0:N1}", num));
					}
					else
					{
						SetMaskProgress(1f);
						SetMaskText("");
					}
					if (currentWeaponStruct.MagazineRemain != (float)currentWeaponStruct.BulletData.n_MAGAZINE && currentWeaponStruct.LastUseTimer.GetMillisecond() > OrangeCharacter.AutoReloadDelay)
					{
						SetProgress((float)(currentWeaponStruct.LastUseTimer.GetMillisecond() - OrangeCharacter.AutoReloadDelay) / ((float)currentWeaponStruct.FastBulletDatas[currentWeaponStruct.Reload_index].n_RELOAD * OrangeCharacter.AutoReloadPercent));
					}
					else
					{
						SetProgress(0f);
					}
				}
				else if (virtualButtonId == VirtualButtonId.FS_SKILL)
				{
					SetBulletText(string.Empty);
					SetProgress(0f);
					SetMaskProgress(0f);
					SetMaskText(string.Empty);
				}
				else
				{
					if (_magazineRemainPrev != (VInt)currentWeaponStruct.MagazineRemain)
					{
						SetBulletText(string.Format("{0}/{1}", currentWeaponStruct.MagazineRemain, currentWeaponStruct.BulletData.n_MAGAZINE), Color.red);
					}
					SetProgress(num3);
					SetMaskProgress(num3);
					SetMaskText(string.Format("{0:N1}", num2));
				}
				break;
			case 1:
				if (!currentWeaponStruct.ForceLock)
				{
					if (currentWeaponStruct.WeaponData != null && (short)currentWeaponStruct.WeaponData.n_TYPE != 8 && (float)currentWeaponStruct.LastUseTimer.GetMillisecond() < (float)currentWeaponStruct.BulletData.n_FIRE_SPEED && currentWeaponStruct.BulletData.n_FIRE_SPEED >= 600)
					{
						SetMaskProgress(maskProgress);
						SetMaskText(string.Format("{0:N1}", num));
					}
					else
					{
						SetMaskProgress(1f);
						SetMaskText("");
					}
					if (_magazineRemainPrev != (VInt)currentWeaponStruct.MagazineRemain)
					{
						if (currentWeaponStruct.MagazineRemain >= (float)currentWeaponStruct.BulletData.n_USE_COST)
						{
							SetBulletText(string.Format("{0:N1}", currentWeaponStruct.MagazineRemain));
						}
						else
						{
							SetBulletText(string.Format("{0:N1}", currentWeaponStruct.MagazineRemain), Color.yellow);
						}
					}
					SetProgress(currentWeaponStruct.MagazineRemain / (float)currentWeaponStruct.BulletData.n_MAGAZINE);
				}
				else
				{
					SetBulletText(string.Format("Overheat {0:N2}", num2), Color.red);
					SetBulletText("0", Color.red);
					SetProgress(num3);
					SetMaskProgress(num3);
					SetMaskText(string.Format("{0:N1}", num2));
				}
				break;
			case 2:
			{
				int nMeasureMax = refCharacter.selfBuffManager.nMeasureMax;
				int nMeasureNow = refCharacter.selfBuffManager.nMeasureNow;
				int n_USE_COST = currentWeaponStruct.BulletData.n_USE_COST;
				if (nMeasureMax > 0)
				{
					if (nMeasureMax == n_USE_COST)
					{
						num3 = ((nMeasureNow < n_USE_COST) ? ((float)nMeasureNow / (float)n_USE_COST) : 1f);
						SetBulletText("");
					}
					else
					{
						int num4 = nMeasureMax / n_USE_COST;
						int num5 = nMeasureNow / n_USE_COST;
						if (num4 == num5)
						{
							num3 = 1f;
							if (num4 > 1)
							{
								SetBulletText(string.Format("{0}/{1}", num5, num4), Color.white);
							}
							else
							{
								SetBulletText("");
							}
						}
						else if (num5 > 0)
						{
							num3 = (float)nMeasureNow / (float)nMeasureMax;
							if (num4 > 1)
							{
								SetBulletText(string.Format("{0}/{1}", num5, num4), Color.white);
							}
							else
							{
								SetBulletText("");
							}
						}
						else
						{
							num3 = (float)nMeasureNow / (float)nMeasureMax;
							if (num4 > 1)
							{
								SetBulletText(string.Format("0/{0}", num4), Color.red);
							}
							else
							{
								SetBulletText("");
							}
						}
					}
				}
				else
				{
					num3 = 0f;
					SetBulletText("");
				}
				SetProgress(num3);
				SetMaskProgress(num3);
				SetMaskText("");
				break;
			}
			}
			_magazineRemainPrev = (VInt)currentWeaponStruct.MagazineRemain;
			switch (virtualButtonId)
			{
			case VirtualButtonId.SHOOT:
				SetBanMask(refCharacter.LockWeapon);
				break;
			case VirtualButtonId.SKILL0:
			case VirtualButtonId.SKILL1:
				SetBanMask(refCharacter.LockSkill);
				break;
			}
		}
	}

	public void UpdateIconBySklTable(SKILL_TABLE tSKILL_TABLE)
	{
		if (tSKILL_TABLE != null)
		{
			WeaponType weaponType = (WeaponType)tSKILL_TABLE.n_TYPE;
			if (weaponType != WeaponType.Melee)
			{
				AllowAnalog = tSKILL_TABLE.n_USE_TYPE == 1;
			}
			else
			{
				AllowAnalog = false;
			}
			base.gameObject.SetActive(false);
			if (tSKILL_TABLE.s_ICON != "null" && tSKILL_TABLE.s_ICON != "" && tSKILL_TABLE.s_ICON != null)
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(tSKILL_TABLE.s_ICON), tSKILL_TABLE.s_ICON, delegate(Sprite obj)
				{
					SetIcon(obj);
					base.gameObject.SetActive(true);
				});
			}
		}
		else
		{
			AllowAnalog = false;
		}
	}

	public void UpdateIconByBundlePath(string bundleName, string assetName)
	{
		base.gameObject.SetActive(false);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(bundleName, assetName, delegate(Sprite obj)
		{
			if (obj != null)
			{
				SetIcon(obj);
				base.gameObject.SetActive(true);
				SetBulletText("");
			}
		});
	}

	public void EnableButtonTipIcon(bool bEnable)
	{
		if (ButtonIcon != null)
		{
			_buttonTipEnable = bEnable;
			ButtonIcon.gameObject.SetActive(_buttonTipEnable);
		}
	}
}
