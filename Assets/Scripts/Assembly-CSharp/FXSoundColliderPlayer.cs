using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FXSoundColliderPlayer : FXSoundPlayer, ILogicUpdate
{
	public CollideBullet m_bullet;

	public bool waitTimeFrist = true;

	private bool isPlayed;

	protected void OnEnable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.AddUpdate(this);
	}

	protected void OnDisable()
	{
		MonoBehaviourSingleton<GameLogicUpdateManager>.Instance.RemoveUpdate(this);
	}

	protected override IEnumerator PlayLoop()
	{
		if (f_fristTime > 0f)
		{
			yield return new WaitForSeconds(f_fristTime);
		}
		while (base.gameObject.activeSelf && m_bullet.IsActivate)
		{
			if (waitTimeFrist)
			{
				yield return new WaitForSeconds(f_loopTime);
				PlaySE();
			}
			else
			{
				PlaySE();
				yield return new WaitForSeconds(f_loopTime);
			}
		}
		isPlayed = false;
	}

	protected override IEnumerator PlayOnce()
	{
		_003C_003En__0();
		isPlayed = false;
		yield return 0;
	}

	public void LogicUpdate()
	{
		if (!m_bullet.IsActivate || isPlayed)
		{
			return;
		}
		switch (m_playType)
		{
		case SoundPlayType.playOnce:
			if (f_fristTime > 0f)
			{
				StartCoroutine(PlayOnce());
			}
			else
			{
				PlaySE();
			}
			break;
		case SoundPlayType.Loop:
			StartCoroutine(PlayLoop());
			break;
		}
		isPlayed = true;
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0()
	{
		return base.PlayOnce();
	}
}
