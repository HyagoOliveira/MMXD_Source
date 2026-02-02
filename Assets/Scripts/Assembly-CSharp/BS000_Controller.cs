#define RELEASE
public class BS000_Controller : EnemyHumanController
{
	protected override void Awake()
	{
		base.Awake();
		Debug.Log("I'm boss zero controller !");
	}
}
