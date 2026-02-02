using UnityEngine;

public class BS052_ShaderController : MonoBehaviour
{
	public Renderer targetRenderer;

	public float fadeTime = 1f;

	public float fadeValueStart;

	public float fadeValueEnd;

	private bool isFading;

	private float fadeSpeed;

	private float fadeDelta;

	private float currentFadeValue;

	public void Activate()
	{
		base.gameObject.SetActive(true);
		currentFadeValue = fadeValueStart;
		fadeDelta = Mathf.Abs(fadeValueEnd - fadeValueStart);
		fadeSpeed = (fadeValueEnd - fadeValueStart) / fadeTime;
		targetRenderer.material.SetFloat("_Threshold", fadeValueStart);
		isFading = true;
	}

	public void UpdateProgress(float deltaTime)
	{
		if (isFading)
		{
			float num = deltaTime * fadeSpeed;
			currentFadeValue += num;
			fadeDelta -= Mathf.Abs(num);
			if (fadeDelta <= 0f)
			{
				currentFadeValue = fadeValueEnd;
				isFading = false;
			}
			targetRenderer.material.SetFloat("_Threshold", currentFadeValue);
		}
	}

	public bool IsDone()
	{
		return !isFading;
	}

	public void Deactivate()
	{
		targetRenderer.material.SetFloat("_Threshold", fadeValueEnd);
		base.gameObject.SetActive(false);
		isFading = false;
	}
}
