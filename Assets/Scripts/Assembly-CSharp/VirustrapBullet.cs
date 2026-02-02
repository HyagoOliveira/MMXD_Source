using UnityEngine;

public class VirustrapBullet : LockingBullet
{
	[SerializeField]
	private float seDelay = 0.55f;

	public override void PlayUseSE(bool force = false)
	{
		if (!isMuteSE)
		{
			base.SoundSource.PlaySE(_UseSE[0], _UseSE[1], seDelay);
		}
	}
}
