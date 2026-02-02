public class EnemyHumanShieldController : EnemyHumanController
{
	public override string[] GetHumanDependAnimations()
	{
		return new string[1] { "spray_run_loop" };
	}
}
