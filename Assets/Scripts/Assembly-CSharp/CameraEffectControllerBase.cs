using UnityEngine;

public abstract class CameraEffectControllerBase : MonoBehaviour
{
	public abstract void Initialize(Camera camera);

	public abstract void Play();

	public abstract void Stop();

	public abstract bool IsPlaying();

	public abstract void Refresh();
}
