public class CH143_Controller : RMXController
{
	public override void Start()
	{
		base.Start();
		if (_refEntity.IsLocalPlayer)
		{
			_refChargeShootObj.ChargeLV3SE = "x_chargemax";
		}
		else
		{
			_refChargeShootObj.ChargeLV3SE = "bt_x_chargemax";
		}
	}

	public override void GetUniqueMotion(out string[] source, out string[] target)
	{
		source = new string[4] { "buster_stand_charge_atk", "buster_fall_charge_atk", "buster_wallgrab_charge_atk", "buster_crouch_charge_atk" };
		target = new string[4] { "ch143_skill_01_stand", "ch143_skill_01_fall", "ch143_skill_01_wallgrab", "ch143_skill_01_crouch" };
	}
}
