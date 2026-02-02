using UnityEngine;

public class CameraRainEffectController : CameraEffectControllerBase
{
	public ParticleSystem fx;

	public override void Initialize(Camera camera)
	{
		if (!(camera == null))
		{
			base.transform.position = camera.transform.position;
			base.transform.parent = camera.transform;
		}
	}

	public override void Refresh()
	{
	}

	public override void Play()
	{
		fx.Play();
	}

	public override void Stop()
	{
		if (fx.isPaused)
		{
			fx.Play();
		}
		fx.Stop();
	}

	public override bool IsPlaying()
	{
		return fx.isPlaying;
	}
}
