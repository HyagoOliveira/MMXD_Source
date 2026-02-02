using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VHSMaterial : MonoBehaviour
{
	[SerializeField]
	private Image[] ImaeRender;

	private Material mpb;

	private Material oldmpb;

	private float minTime = 0.5f;

	private float thresh = 0.5f;

	private float minFrame;

	private float lastTime;

	private float FlashTime;

	private bool FlashOn;

	private float AutoFlashTimer;

	protected void Awake()
	{
		if (ImaeRender[0] != null)
		{
			mpb = Object.Instantiate(ImaeRender[0].material);
			mpb.SetFloat(Shader.PropertyToID("_VhsVOffect"), 128f);
			mpb.SetFloat(Shader.PropertyToID("_NoiseCutoff"), 0.7f);
			oldmpb = ImaeRender[0].material;
		}
		lastTime = 0f;
		FlashTime = OrangeBattleUtility.Random(5, 10);
		FlashOn = false;
	}

	private void Start()
	{
		AutoFlashTimer = 0f;
		StartCoroutine(OnFlashMenu());
	}

	private IEnumerator OnFlashMenu()
	{
		while (true)
		{
			if (base.transform.GetComponent<Canvas>().isActiveAndEnabled)
			{
				if (!FlashOn)
				{
					yield return CoroutineDefine._1sec;
					AutoFlashTimer += 1f;
					if (AutoFlashTimer > FlashTime && OrangeBattleUtility.Random(0, 100) < 30)
					{
						FlashOn = true;
						FlashTime = OrangeBattleUtility.Random(45, 75);
						AutoFlashTimer = 0f;
					}
					continue;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
				AutoFlashTimer += 1f;
				if (AutoFlashTimer > FlashTime)
				{
					for (int i = 0; i < ImaeRender.Length; i++)
					{
						ImaeRender[i].material = oldmpb;
					}
					FlashOn = false;
					FlashTime = OrangeBattleUtility.Random(5, 10);
					AutoFlashTimer = 0f;
					continue;
				}
				minFrame += 1f;
				if (minFrame < 30f)
				{
					if ((float)OrangeBattleUtility.Random(0, 100) < thresh * 100f)
					{
						for (int j = 0; j < ImaeRender.Length; j++)
						{
							ImaeRender[j].material = mpb;
						}
					}
					else
					{
						for (int k = 0; k < ImaeRender.Length; k++)
						{
							ImaeRender[k].material = oldmpb;
						}
					}
				}
				else
				{
					minFrame = 0f;
				}
			}
			else
			{
				yield return CoroutineDefine._1sec;
			}
		}
	}

	private void Update()
	{
	}
}
