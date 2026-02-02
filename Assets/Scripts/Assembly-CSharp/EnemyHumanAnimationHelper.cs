public static class EnemyHumanAnimationHelper
{
	public enum ControllerType
	{
		Valentine2021HumanSwordController = 0,
		Valentine2021HumanController = 1,
		EnemyHumanShieldController = 2,
		EnemyHumanSwordController = 3,
		EnemyHumanController = 4,
		Event4_Human2Controller = 5,
		Event5_HumanController = 6,
		Event6_NPC_Controller = 7,
		Event7_NPC_Controller = 8
	}

	public static string[] GetHumanDependAnimations(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Valentine2021HumanSwordController:
		case ControllerType.Valentine2021HumanController:
			return new string[12]
			{
				"event_enemy_otaku_idle_01_loop", "event_enemy_otaku_idle_02_loop", "event_enemy_otaku_idle_03_loop", "event_enemy_otaku_idle_04_loop", "event_enemy_otaku_idle_05_loop", "event_enemy_otaku_idle_06_loop", "event_enemy_otaku_idle_07_loop", "event_enemy_otaku_idle_08_loop", "event_enemy_otaku_idle_09_loop", "event_enemy_otaku_idle_10_loop",
				"event_enemy_otaku_idle_11_loop", "event_enemy_otaku_idle_12_loop"
			};
		case ControllerType.EnemyHumanShieldController:
			return new string[1] { "spray_run_loop" };
		case ControllerType.Event4_Human2Controller:
			return new string[1] { "buster_stand_weak_loop" };
		case ControllerType.Event5_HumanController:
			return new string[3] { "enemy_sf_standby_loop", "enemy_sf_debut", "enemy_sf_atk_mid" };
		case ControllerType.Event6_NPC_Controller:
			return new string[2] { "ch039_skill_01_stand", "buster_stand_weak_loop" };
		case ControllerType.Event7_NPC_Controller:
			return new string[3] { "ch007_NPC_skill_01", "ch007_NPC_skill_02", "ch007_NPC_idl" };
		default:
			return new string[0];
		}
	}
}
