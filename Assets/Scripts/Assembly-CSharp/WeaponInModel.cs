#define RELEASE
using UnityEngine;
using enums;

public class WeaponInModel : MonoBehaviour
{
	[SerializeField]
	private WeaponType _weaponType;

	[SerializeField]
	private int[] _skillID = new int[0];

	[SerializeField]
	private OrangeCharacter _charactercontroller;

	[SerializeField]
	private CharacterMaterial _weaponMesh;

	public void Initialize(OrangeCharacter p_charactercontroller)
	{
		WeaponType weaponType = _weaponType;
		if (weaponType == WeaponType.Dummy || weaponType == WeaponType.All)
		{
			Debug.LogError("WeaponInModel 需要設定武器類型，該參數會影響之後載入的動作");
			return;
		}
		if (_skillID.Length < 1)
		{
			Debug.LogError("WeaponInModel 需要設定技能ID，間接影響 PlayerShotBuster 裡面要切換武器的技能");
			return;
		}
		_charactercontroller = p_charactercontroller;
		if (_weaponMesh == null)
		{
			_weaponMesh = GetComponent<CharacterMaterial>();
		}
		int[] skillID = _skillID;
		foreach (int num in skillID)
		{
			WeaponStruct weaponStruct = _charactercontroller.PlayerSkills[num];
			if (weaponStruct.WeaponData == null)
			{
				weaponStruct.WeaponData = new WEAPON_TABLE();
				weaponStruct.WeaponData.n_TYPE = (int)_weaponType;
			}
			weaponStruct.WeaponMesh[0] = _weaponMesh;
		}
	}
}
