using DragonBones;
using UnityEngine;

public class CoreElement : BaseDemo
{
	public const float G = -0.005f;

	public const float GROUND = 0f;

	public const KeyCode left = KeyCode.A;

	public const KeyCode right = KeyCode.D;

	public const KeyCode up = KeyCode.W;

	public const KeyCode down = KeyCode.S;

	public const KeyCode switchSkin = KeyCode.Space;

	public const KeyCode switchLeftWeapon = KeyCode.Q;

	public const KeyCode switchRightWeapon = KeyCode.E;

	private Mecha _player;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("mecha_1502b/mecha_1502b_ske");
		UnityFactory.factory.LoadTextureAtlasData("mecha_1502b/mecha_1502b_tex");
		UnityFactory.factory.LoadDragonBonesData("skin_1502b/skin_1502b_ske");
		UnityFactory.factory.LoadTextureAtlasData("skin_1502b/skin_1502b_tex");
		UnityFactory.factory.LoadDragonBonesData("weapon_1000/weapon_1000_ske");
		UnityFactory.factory.LoadTextureAtlasData("weapon_1000/weapon_1000_tex");
		_player = new Mecha();
	}

	protected override void OnUpdate()
	{
		bool key = Input.GetKey(KeyCode.A);
		bool key2 = Input.GetKey(KeyCode.D);
		if (key == key2)
		{
			_player.Move(0);
		}
		else if (key)
		{
			_player.Move(-1);
		}
		else
		{
			_player.Move(1);
		}
		if (Input.GetKeyDown(KeyCode.W))
		{
			_player.Jump();
		}
		_player.Squat(Input.GetKey(KeyCode.S));
		if (Input.GetKeyDown(KeyCode.Space))
		{
			_player.SwitchSkin();
		}
		if (Input.GetKeyDown(KeyCode.Q))
		{
			_player.SwitchWeaponL();
		}
		if (Input.GetKeyDown(KeyCode.E))
		{
			_player.SwitchWeaponR();
		}
		Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, Camera.main.farClipPlane));
		_player.Aim(vector.x, vector.y);
		_player.Attack(Input.GetMouseButton(0));
		_player.Update();
	}
}
