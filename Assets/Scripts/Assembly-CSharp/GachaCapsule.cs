using System;
using System.Collections.Generic;
using CallbackDefs;
using Cinemachine;
using UnityEngine;
using enums;

public class GachaCapsule : MonoBehaviour
{
	private readonly int i_TintColor = Shader.PropertyToID("_TintColor");

	private readonly int i_EmissionColor = Shader.PropertyToID("_EmissionColor");

	[SerializeField]
	private CinemachineImpulseSource impulseSource;

	[SerializeField]
	private Renderer[] capsuleRenderers;

	[SerializeField]
	private Renderer lightRenderer;

	[SerializeField]
	private ParticleSystem transferParticle;

	[SerializeField]
	private ParticleSystem lightningParticle;

	[SerializeField]
	private ParticleSystem rareSParticle;

	[SerializeField]
	private Animation animCabinDoor;

	[SerializeField]
	private Color[] colors = new Color[8]
	{
		new Color(0f, 0f, 0f),
		new Color(1f / 17f, 1f / 17f, 1f / 17f),
		new Color(0.015686275f, 5f / 51f, 6f / 85f),
		new Color(0.043137256f, 0.11764706f, 0.2f),
		new Color(13f / 51f, 0.10980392f, 0.32156864f),
		new Color(0.38039216f, 0.2f, 6f / 85f),
		new Color(0.38039216f, 0.2f, 6f / 85f),
		new Color(0.38039216f, 0.2f, 6f / 85f)
	};

	private GachaRareVisualInfo targetRareInfo;

	private int tweenUid = -1;

	private MaterialPropertyBlock mpbCapsule;

	private MaterialPropertyBlock mpbRareRenderer;

	private MaterialPropertyBlock mpbLightRenderer;

	private ParticleSystemRenderer transferRenderer;

	[SerializeField]
	private Renderer[] rarePsRenderer;

	private Callback m_cb;

	private bool isCabinPlay;

	private List<Color> rendererColor = new List<Color>();

	public bool IsBlock { get; set; }

	public void Awake()
	{
		IsBlock = false;
		mpbCapsule = new MaterialPropertyBlock();
		mpbRareRenderer = new MaterialPropertyBlock();
		mpbLightRenderer = new MaterialPropertyBlock();
		transferRenderer = transferParticle.GetComponent<ParticleSystemRenderer>();
		rareSParticle.Stop();
		UpdateCapsuleProperty(ItemRarity.Dummy);
	}

	public void Init(GachaRareVisualInfo p_rareVisualInfo, Callback p_cb)
	{
		targetRareInfo = p_rareVisualInfo;
		m_cb = p_cb;
		ClearRareFx(targetRareInfo.Idx);
		impulseSource.GenerateImpulse(Vector3.back);
		tweenUid = Play(true, EftComplete);
	}

	private void EftComplete()
	{
		if (targetRareInfo.IsSpChange)
		{
			lightningParticle.Play(true);
			targetRareInfo.IsSpChange = false;
			impulseSource.GenerateImpulse(Vector3.back);
			if (!isCabinPlay)
			{
				isCabinPlay = true;
				animCabinDoor.Play();
			}
		}
		else
		{
			IsBlock = true;
			Stop();
		}
	}

	public void SkipEft(bool triggerCallback = true)
	{
		if (!triggerCallback)
		{
			m_cb = null;
		}
		LeanTween.cancel(base.gameObject, false);
		tweenUid = -1;
		Stop(false);
	}

	public void SetRareFx(ref List<GachaRareVisualInfo> listGachaRareVisualInfo)
	{
		rendererColor.Clear();
		int count = listGachaRareVisualInfo.Count;
		for (int i = 0; i < count; i++)
		{
			int index = i;
			GachaRareVisualInfo gachaRareVisualInfo = listGachaRareVisualInfo[index];
			Color item = 1.1f * colors[gachaRareVisualInfo.Rare - (gachaRareVisualInfo.IsSpChange ? 1 : 0)];
			rendererColor.Add(item);
		}
	}

	public void PlayRareFx()
	{
		int count = rendererColor.Count;
		for (int i = 0; i < count; i++)
		{
			int idx = i;
			Renderer psRenderer = rarePsRenderer[idx];
			LeanTween.value(psRenderer.gameObject, 0f, 1f, 0.1f).setDelay((float)idx * 0.1f).setOnComplete((Action)delegate
			{
				mpbRareRenderer.SetColor(i_TintColor, rendererColor[idx]);
				psRenderer.SetPropertyBlock(mpbRareRenderer);
				if (idx == 10)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Play("SystemSE", 109);
				}
			});
		}
		if (count == 1)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GACHA02_1);
		}
		else if (count >= 10)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GACHA02_10);
		}
	}

	public void ClearRareFx(int idx)
	{
		mpbRareRenderer.SetColor(i_TintColor, colors[0]);
	}

	public void PlayTransferFx(int rarity)
	{
		mpbCapsule.SetColor(i_TintColor, colors[rarity]);
		transferRenderer.SetPropertyBlock(mpbCapsule);
		transferParticle.Play();
		mpbLightRenderer.SetColor(i_EmissionColor, colors[rarity]);
	}

	private int Play(bool clearColor, Callback p_cb)
	{
		int num = targetRareInfo.Rare;
		if (targetRareInfo.IsSpChange)
		{
			num--;
		}
		Color from = colors[0];
		if (!clearColor)
		{
			from = mpbCapsule.GetColor(i_TintColor);
		}
		PlayTransferFx(num);
		IsBlock = false;
		if (num >= 5)
		{
			rareSParticle.Play();
		}
		if (targetRareInfo.IsSpChange)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GACHA03_H);
		}
		else
		{
			ItemRarity itemRarity = (ItemRarity)num;
			if ((uint)(itemRarity - 5) <= 2u)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GACHA03_M);
			}
			else
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GACHA03_L);
			}
		}
		return LeanTween.value(base.gameObject, from, colors[num], 3f).setOnUpdate(delegate(Color val)
		{
			mpbCapsule.SetColor(i_TintColor, val);
			mpbLightRenderer.SetColor(i_EmissionColor, val);
			UpdateCapsulePropertyBlock();
		}).setOnComplete((Action)delegate
		{
			p_cb.CheckTargetToInvoke();
		})
			.uniqueId;
	}

	public void Stop(bool playPowerOff = true)
	{
		transferParticle.Stop();
		if (playPowerOff)
		{
			lightningParticle.Play(true);
			LeanTween.value(base.gameObject, mpbCapsule.GetColor(i_TintColor), colors[0], 2f).setOnUpdate(delegate(Color val)
			{
				mpbCapsule.SetColor(i_TintColor, val);
				mpbLightRenderer.SetColor(i_EmissionColor, val);
				UpdateCapsulePropertyBlock();
			}).setOnComplete((Action)delegate
			{
				if (!rareSParticle.isStopped)
				{
					rareSParticle.Stop();
				}
				UpdateCapsuleProperty(ItemRarity.Dummy);
				m_cb.CheckTargetToInvoke();
			});
		}
		else
		{
			if (!rareSParticle.isStopped)
			{
				rareSParticle.Stop();
			}
			UpdateCapsuleProperty(ItemRarity.Dummy);
			m_cb.CheckTargetToInvoke();
		}
	}

	public void UpdateCapsuleProperty(ItemRarity rarity)
	{
		mpbCapsule.SetColor(i_TintColor, colors[(int)rarity]);
		mpbLightRenderer.SetColor(i_EmissionColor, colors[(int)rarity]);
		UpdateCapsulePropertyBlock();
	}

	private void UpdateCapsulePropertyBlock()
	{
		Renderer[] array = capsuleRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(mpbCapsule);
		}
		transferRenderer.SetPropertyBlock(mpbCapsule);
		lightRenderer.SetPropertyBlock(mpbLightRenderer);
	}
}
