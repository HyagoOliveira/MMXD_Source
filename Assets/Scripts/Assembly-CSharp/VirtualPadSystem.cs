using System;
using System.Collections.Generic;
using System.Linq;
using Better;
using UnityEngine;
using UnityEngine.UI;

public class VirtualPadSystem : MonoBehaviour
{
	[HideInInspector]
	public VirtualAnalogStick VirtualAnalogStickInstance;

	[HideInInspector]
	public System.Collections.Generic.Dictionary<ButtonId, VirtualButton> VirtualButtonInstances = new Better.Dictionary<ButtonId, VirtualButton>();

	[HideInInspector]
	public VirtualButton[] ListVirtualButtonInstances = new VirtualButton[0];

	private VirtualButton _selectedVirtualButton;

	public System.Collections.Generic.Dictionary<string, float> DefaultValueDictionary = new Better.Dictionary<string, float>();

	private Transform TargetTransform;

	[HideInInspector]
	public Slider SliderScale;

	[HideInInspector]
	public OrangeText GamePadMappingNameText;

	[HideInInspector]
	public OrangeText MainGamePadMappingText;

	[HideInInspector]
	public OrangeText SubGamePadMappingText;

	public bool IsInit { get; set; }

	private void Awake()
	{
		IsInit = false;
	}

	private void OnEnable()
	{
		cInput.OnKeyChanged += OnUpdateKeyDisplay;
	}

	private void OnDisable()
	{
		cInput.OnKeyChanged -= OnUpdateKeyDisplay;
	}

	public void OnUpdateKeyDisplay()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK02);
		UpdateKeyDisplay();
	}

	public void UpdateKeyDisplay()
	{
		SetMappingText(ref MainGamePadMappingText, true);
		SetMappingText(ref SubGamePadMappingText, false);
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			ResetStatus();
		}
	}

	public void InitializeVirtualPad(bool editMode = false)
	{
		VirtualAnalogStickInstance = GetComponentInChildren<VirtualAnalogStick>();
		VirtualButton[] componentsInChildren = GetComponentsInChildren<VirtualButton>(true);
		VirtualButtonInstances.Clear();
		VirtualButton[] array = componentsInChildren;
		foreach (VirtualButton virtualButton in array)
		{
			VirtualButtonInstances.Add(virtualButton.KeyMapping, virtualButton);
		}
		ListVirtualButtonInstances = VirtualButtonInstances.Values.ToArray();
		foreach (Transform item in base.transform)
		{
			VirtualButton component = item.GetComponent<VirtualButton>();
			VirtualAnalogStick component2 = item.GetComponent<VirtualAnalogStick>();
			Transform transform = null;
			string text = "";
			if ((bool)component)
			{
				if (editMode)
				{
					VirtualPadEditorDrag virtualPadEditorDrag = component.gameObject.AddComponent<VirtualPadEditorDrag>();
					virtualPadEditorDrag.IsFixed = component.isFixed;
					virtualPadEditorDrag.Setup(OnPointerDownCB);
					component.enabled = false;
				}
				else if (component.isFixed)
				{
					component.gameObject.SetActive(false);
				}
				transform = component.transform;
				text = component.KeyMapping.ToString();
			}
			else if ((bool)component2)
			{
				if (editMode)
				{
					component2.Radius.gameObject.AddComponent<VirtualPadEditorDrag>().Setup(OnPointerDownCB);
					component2.enabled = false;
				}
				transform = component2.Radius.transform;
				text = "ANALOG";
			}
			if ((bool)transform)
			{
				if (!DefaultValueDictionary.ContainsKey("CustomizedLayout_" + text + "X"))
				{
					DefaultValueDictionary.Add("CustomizedLayout_" + text + "X", transform.localPosition.x);
					DefaultValueDictionary.Add("CustomizedLayout_" + text + "Y", transform.localPosition.y);
					DefaultValueDictionary.Add("CustomizedLayout_" + text + "Scale", transform.localScale.x);
				}
				EnableButtonTips(MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting.ButtonTip != 0);
			}
		}
		IsInit = true;
	}

	public void UpdateTargetScale(float scaleValue)
	{
		if (TargetTransform != null)
		{
			TargetTransform.localScale = new Vector3(scaleValue, scaleValue, 1f);
		}
	}

	private void OnPointerDownCB(object p_param)
	{
		Transform transform = p_param as Transform;
		if ((bool)transform)
		{
			if (TargetTransform != transform)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
			}
			TargetTransform = transform.transform;
			if (SliderScale != null)
			{
				SliderScale.value = transform.transform.localScale.x;
			}
			_selectedVirtualButton = transform.gameObject.GetComponentInChildren<VirtualButton>();
			UpdateKeyDisplay();
		}
	}

	private void SetMappingText(ref OrangeText text, bool isMain)
	{
		if (text != null)
		{
			if (_selectedVirtualButton == null)
			{
				text.text = "---";
			}
			else
			{
				text.text = cInput.GetText(_selectedVirtualButton.KeyMapping.ToString(), isMain ? 1 : 2);
			}
		}
	}

	public void SetButton(bool isMain)
	{
		if (!(_selectedVirtualButton == null))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR08);
			cInput.ChangeKey(_selectedVirtualButton.KeyMapping.ToString(), isMain ? 1 : 2);
			UpdateKeyDisplay();
		}
	}

	public void DestroyVirtualPad()
	{
		UnityEngine.Object.Destroy(base.gameObject);
		VirtualAnalogStickInstance = null;
		VirtualButtonInstances = null;
	}

	public KeyCode GetCurrentPadPressed()
	{
		if (Input.anyKeyDown)
		{
			for (int i = 0; i < 20; i++)
			{
				if (Input.GetKeyDown("joystick 1 button " + i))
				{
					return (KeyCode)Enum.Parse(typeof(KeyCode), "Joystick1Button" + i);
				}
			}
		}
		return KeyCode.None;
	}

	public VirtualButton GetButton(ButtonId id)
	{
		VirtualButton value = null;
		VirtualButtonInstances.TryGetValue(id, out value);
		return value;
	}

	public void ResetStatus()
	{
		MonoBehaviourSingleton<InputManager>.Instance.ClearTouchChain();
		if (VirtualAnalogStickInstance != null)
		{
			VirtualAnalogStickInstance.ClearAnalogStick();
		}
		VirtualButton[] listVirtualButtonInstances = ListVirtualButtonInstances;
		for (int i = 0; i < listVirtualButtonInstances.Length; i++)
		{
			listVirtualButtonInstances[i].ClearButton();
		}
	}

	public void EnableButtonTips(bool bEnable)
	{
		VirtualButton[] componentsInChildren = GetComponentsInChildren<VirtualButton>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].EnableButtonTipIcon(bEnable);
		}
	}
}
