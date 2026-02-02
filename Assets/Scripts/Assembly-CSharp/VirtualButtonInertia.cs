using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualButtonInertia : MonoBehaviour
{
	private readonly int InertiaFrame = 100;

	private bool isAllowInertia;

	private int inertiaEndFrame;

	private Image _imgInertia;

	private Color colorVisible = new Color(1f, 1f, 1f, 1f);

	private Color colorInvisible = Color.clear;

	private ButtonId[] inertiaButtonIds = new ButtonId[7]
	{
		ButtonId.SHOOT,
		ButtonId.JUMP,
		ButtonId.DASH,
		ButtonId.SKILL0,
		ButtonId.SKILL1,
		ButtonId.FS_SKILL,
		ButtonId.CHIP_SWITCH
	};

	private VirtualButton vBtn;

	private List<RaycastResult> inertiaResults = new List<RaycastResult>();

	public bool _BtnStatus { get; private set; }

	public bool IsInertiaExist { get; set; }

	public void Init(VirtualButton p_btn)
	{
		vBtn = p_btn;
		ButtonId keyMapping = vBtn.KeyMapping;
		if ((uint)(keyMapping - 5) > 4u && keyMapping != ButtonId.FS_SKILL)
		{
			isAllowInertia = false;
		}
		else
		{
			isAllowInertia = true;
		}
		InitInertiaBtn();
		TriggerInertia(false);
	}

	private void InitInertiaBtn()
	{
		if (!(_imgInertia != null))
		{
			GameObject gameObject = new GameObject("imgInertia");
			gameObject.transform.SetParent(base.transform);
			_imgInertia = gameObject.AddComponent<Image>();
			Sprite assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>("ui/ui_battleinfo", "UI_battle_button_hold");
			if (assstSync != null)
			{
				_imgInertia.sprite = assstSync;
			}
			else
			{
				_imgInertia.sprite = GetComponent<Image>().sprite;
			}
			_imgInertia.rectTransform.sizeDelta = vBtn.GetComponent<RectTransform>().sizeDelta * 1.1f;
			_imgInertia.transform.localPosition = Vector3.zero;
			_imgInertia.raycastTarget = false;
			_imgInertia.color = colorInvisible;
		}
	}

	private void Update()
	{
		if (IsInertiaExist)
		{
			if (GameLogicUpdateManager.GameFrame >= inertiaEndFrame)
			{
				if (!IsAnyOtherBtnPressed())
				{
					PointerEventData eventData = new PointerEventData(EventSystem.current);
					VirtualButton.AllowPressAndDrag = false;
					ExecuteEvents.Execute(vBtn.gameObject, eventData, ExecuteEvents.pointerUpHandler);
				}
				TriggerInertia(false);
			}
			else
			{
				OnCheckInertia();
			}
		}
		else if (base.enabled)
		{
			TriggerInertia(false);
		}
	}

	public void CheckInertia()
	{
		if (isAllowInertia && !IsInertiaExist)
		{
			OnCheckInertia();
		}
	}

	private void OnCheckInertia()
	{
		inertiaResults.Clear();
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		EventSystem.current.RaycastAll(pointerEventData, inertiaResults);
		if (inertiaResults.Count > 0)
		{
			bool trigger = false;
			for (int i = 0; i < inertiaResults.Count; i++)
			{
				GameObject gameObject = inertiaResults[i].gameObject;
				if (!(gameObject == null) && (gameObject.GetComponent<Canvas>() != null || gameObject.GetComponent<VirtualAnalogStick>() != null))
				{
					trigger = true;
					break;
				}
			}
			TriggerInertia(trigger);
		}
		else
		{
			TriggerInertia(true);
		}
	}

	public void TriggerInertia(bool trigger)
	{
		if (vBtn.GetStickEnable() || !vBtn._BtnStatus)
		{
			SetSecondBtnStatus(false);
			IsInertiaExist = false;
			base.enabled = false;
		}
		else if (base.enabled != trigger)
		{
			if (trigger)
			{
				inertiaEndFrame = GameLogicUpdateManager.GameFrame + InertiaFrame;
				SetSecondBtnStatus(true);
			}
			else
			{
				inertiaEndFrame = 0;
				SetSecondBtnStatus(false);
			}
			IsInertiaExist = trigger;
			base.enabled = trigger;
		}
	}

	private void SetSecondBtnStatus(bool visible)
	{
		_imgInertia.enabled = visible;
		_imgInertia.color = (visible ? colorVisible : colorInvisible);
		_BtnStatus = visible;
	}

	public bool IsAnyOtherBtnPressed()
	{
		for (int i = 0; i < inertiaButtonIds.Length; i++)
		{
			if (vBtn.KeyMapping != inertiaButtonIds[i] && ManagedSingleton<InputStorage>.Instance.IsPressed(MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify, inertiaButtonIds[i]))
			{
				return true;
			}
		}
		return false;
	}
}
