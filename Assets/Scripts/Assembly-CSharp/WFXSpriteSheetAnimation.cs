using System.Collections;
using UnityEngine;

internal class WFXSpriteSheetAnimation : MonoBehaviour
{
	public int TilesX = 4;

	public int TilesY = 4;

	public float AnimationFPS = 30f;

	public bool IsInterpolateFrames;

	public int StartFrameOffset;

	public bool IsLoop = true;

	public float StartDelay;

	public string[] ShaderProperties = new string[1] { "_MainTex" };

	public AnimationCurve FrameOverTime = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	private bool isInizialised;

	private int index;

	private int count;

	private int allCount;

	private float animationLifeTime;

	private bool isVisible;

	private bool isCorutineStarted;

	private WFX_MeshMaterialEffect wfxMeshMaterialEffect;

	private float currentInterpolatedTime;

	private float animationStartTime;

	private bool animationStoped;

	private void Start()
	{
		wfxMeshMaterialEffect = GetComponent<WFX_MeshMaterialEffect>();
		InitDefaultVariables();
		isInizialised = true;
		isVisible = true;
		Play();
	}

	private void InitDefaultVariables()
	{
		allCount = 0;
		animationStoped = false;
		animationLifeTime = (float)(TilesX * TilesY) / AnimationFPS;
		count = TilesY * TilesX;
		index = TilesX - 1;
		Vector3 zero = Vector3.zero;
		StartFrameOffset -= StartFrameOffset / count * count;
		Vector2 value = new Vector2(1f / (float)TilesX, 1f / (float)TilesY);
		if (wfxMeshMaterialEffect != null)
		{
			string[] shaderProperties = ShaderProperties;
			foreach (string text in shaderProperties)
			{
				wfxMeshMaterialEffect.Material.SetTextureScale(text, value);
				wfxMeshMaterialEffect.Material.SetTextureOffset(text, zero);
			}
		}
	}

	private void Play()
	{
		if (!isCorutineStarted)
		{
			if (StartDelay > 0.0001f)
			{
				Invoke("PlayDelay", StartDelay);
			}
			else
			{
				StartCoroutine(UpdateCorutine());
			}
			isCorutineStarted = true;
		}
	}

	private void PlayDelay()
	{
		StartCoroutine(UpdateCorutine());
	}

	private void OnEnable()
	{
		if (isInizialised)
		{
			InitDefaultVariables();
			isVisible = true;
			Play();
		}
	}

	private void OnDisable()
	{
		isCorutineStarted = false;
		isVisible = false;
		StopAllCoroutines();
		CancelInvoke("PlayDelay");
	}

	private IEnumerator UpdateCorutine()
	{
		animationStartTime = Time.time;
		while (isVisible && (IsLoop || !animationStoped))
		{
			UpdateFrame();
			if (!IsLoop && animationStoped)
			{
				break;
			}
			float value = (Time.time - animationStartTime) / animationLifeTime;
			float num = FrameOverTime.Evaluate(Mathf.Clamp01(value));
			yield return new WaitForSeconds(1f / (AnimationFPS * num));
		}
		isCorutineStarted = false;
	}

	private void UpdateFrame()
	{
		allCount++;
		index++;
		if (index >= count)
		{
			index = 0;
		}
		if (count == allCount)
		{
			animationStartTime = Time.time;
			allCount = 0;
			animationStoped = true;
		}
		Vector2 value = new Vector2((float)index / (float)TilesX - (float)(index / TilesX), 1f - (float)(index / TilesX) / (float)TilesY);
		if (wfxMeshMaterialEffect != null)
		{
			string[] shaderProperties = ShaderProperties;
			foreach (string text in shaderProperties)
			{
				wfxMeshMaterialEffect.Material.SetTextureOffset(text, value);
			}
		}
		if (IsInterpolateFrames)
		{
			currentInterpolatedTime = 0f;
		}
	}

	private void Update()
	{
		if (IsInterpolateFrames)
		{
			currentInterpolatedTime += Time.deltaTime;
			int num = index + 1;
			if (allCount == 0)
			{
				num = index;
			}
			Vector4 value = new Vector4(1f / (float)TilesX, 1f / (float)TilesY, (float)num / (float)TilesX - (float)(num / TilesX), 1f - (float)(num / TilesX) / (float)TilesY);
			if (wfxMeshMaterialEffect != null)
			{
				wfxMeshMaterialEffect.Material.SetVector("_MainTex_NextFrame", value);
				float value2 = (Time.time - animationStartTime) / animationLifeTime;
				float num2 = FrameOverTime.Evaluate(Mathf.Clamp01(value2));
				wfxMeshMaterialEffect.Material.SetFloat("InterpolationValue", Mathf.Clamp01(currentInterpolatedTime * AnimationFPS * num2));
			}
		}
	}
}
