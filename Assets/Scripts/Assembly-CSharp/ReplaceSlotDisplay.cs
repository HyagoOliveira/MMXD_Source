using DragonBones;
using UnityEngine;

public class ReplaceSlotDisplay : BaseDemo
{
	private static readonly string[] WEAPON_RIGHT_LIST = new string[5] { "weapon_1004_r", "weapon_1004b_r", "weapon_1004c_r", "weapon_1004d_r", "weapon_1004e_r" };

	private GameObject _logoReplaceTxt;

	private UnityArmatureComponent _armatureComp;

	private Slot _leftWeaponSlot;

	private Slot _rightWeaponSlot;

	private GameObject _sourceLogoDisplay;

	private int _leftWeaponIndex = -1;

	private int _rightWeaponIndex = -1;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("mecha_1004d_show/mecha_1004d_show_ske");
		UnityFactory.factory.LoadTextureAtlasData("mecha_1004d_show/mecha_1004d_show_tex");
		UnityFactory.factory.LoadDragonBonesData("weapon_1004_show/weapon_1004_show_ske");
		UnityFactory.factory.LoadTextureAtlasData("weapon_1004_show/weapon_1004_show_tex");
		_armatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_1004d");
		_armatureComp.CloseCombineMeshs();
		_armatureComp.animation.Play("idle");
		_armatureComp.transform.localPosition = new Vector3(0f, -2f, 0f);
		_leftWeaponSlot = _armatureComp.armature.GetSlot("weapon_hand_l");
		_rightWeaponSlot = _armatureComp.armature.GetSlot("weapon_hand_r");
		_sourceLogoDisplay = _armatureComp.armature.GetSlot("logo").display as GameObject;
		_leftWeaponIndex = 0;
		_rightWeaponIndex = 0;
	}

	protected override void OnUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			float num = 0f + (float)Screen.width / 2f - (float)Screen.width / 6f;
			float num2 = (float)Screen.width / 2f + (float)Screen.width / 6f;
			bool num3 = Input.mousePosition.x < num2 && Input.mousePosition.x > num;
			bool flag = Input.mousePosition.x > num2;
			if (num3)
			{
				_ReplaceDisplay(0);
			}
			else
			{
				_ReplaceDisplay(flag ? 1 : (-1));
			}
		}
	}

	private void _ReplaceDisplay(int type)
	{
		switch (type)
		{
		case 1:
			_leftWeaponIndex++;
			_leftWeaponIndex %= _leftWeaponSlot.displayList.Count;
			_leftWeaponSlot.displayIndex = _leftWeaponIndex;
			break;
		case -1:
		{
			_rightWeaponIndex++;
			_rightWeaponIndex %= WEAPON_RIGHT_LIST.Length;
			string displayName = WEAPON_RIGHT_LIST[_rightWeaponIndex];
			UnityFactory.factory.ReplaceSlotDisplay("weapon_1004_show", "weapon", "weapon_r", displayName, _rightWeaponSlot);
			break;
		}
		default:
		{
			UnitySlot unitySlot = _armatureComp.armature.GetSlot("logo") as UnitySlot;
			if (unitySlot.renderDisplay.GetComponent<TextMesh>() != null)
			{
				unitySlot.display = _sourceLogoDisplay;
			}
			else
			{
				unitySlot.display = _GetTextLogo();
			}
			break;
		}
		}
	}

	private GameObject _GetTextLogo()
	{
		if (_logoReplaceTxt == null)
		{
			_logoReplaceTxt = new GameObject("txt_logo");
			TextMesh textMesh = _logoReplaceTxt.AddComponent<TextMesh>();
			textMesh.characterSize = 0.2f;
			textMesh.fontSize = 20;
			textMesh.text = "Core Element";
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.alignment = TextAlignment.Center;
			textMesh.richText = false;
		}
		return _logoReplaceTxt;
	}
}
