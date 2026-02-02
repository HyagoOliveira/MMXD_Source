using UnityEngine;

namespace DigitalRuby.RainMaker
{
	public class BaseRainScript : MonoBehaviour
	{
		[Tooltip("Camera the rain should hover over, defaults to main camera")]
		public Camera Camera;

		[Tooltip("Ambient Color.")]
		public Color AmbientColor = new Color(0.2f, 0.2f, 0.2f);

		[Tooltip("lightning enabled.")]
		public bool EnableLightning = true;

		private float minTime = 0.5f;

		private float thresh = 0.5f;

		private float lastTime;

		private float LightningTime;

		private bool LightningOn;

		private Light MainDirectLight;

		private Color BackupDirectLightColor;

		[Tooltip("Whether rain should follow the camera. If false, rain must be moved manually and will not follow the camera.")]
		public bool FollowCamera = true;

		[Tooltip("Intensity of rain (0-1)")]
		[Range(0f, 1f)]
		public float RainIntensity;

		[Tooltip("Rain particle system")]
		public ParticleSystem RainFallParticleSystem;

		[Tooltip("Particles system for when rain hits something")]
		public ParticleSystem RainExplosionParticleSystem;

		[Tooltip("Particle system to use for rain mist")]
		public ParticleSystem RainMistParticleSystem;

		[Tooltip("The threshold for intensity (0 - 1) at which mist starts to appear")]
		[Range(0f, 1f)]
		public float RainMistThreshold = 0.5f;

		[Tooltip("X = minimum wind speed. Y = maximum wind speed. Z = sound multiplier. Wind speed is divided by Z to get sound multiplier value. Set Z to lower than Y to increase wind sound volume, or higher to decrease wind sound volume.")]
		public Vector3 WindSpeedRange = new Vector3(50f, 500f, 500f);

		[Tooltip("How often the wind speed and direction changes (minimum and maximum change interval in seconds)")]
		public Vector2 WindChangeInterval = new Vector2(5f, 30f);

		[Tooltip("Whether wind should be enabled.")]
		public bool EnableWind = true;

		protected Material rainMaterial;

		protected Material rainExplosionMaterial;

		protected Material rainMistMaterial;

		private float lastRainIntensityValue = -1f;

		private float nextWindTime;

		protected virtual bool UseRainMistSoftParticles
		{
			get
			{
				return true;
			}
		}

		private void CheckForRainChange()
		{
			if (lastRainIntensityValue == RainIntensity)
			{
				return;
			}
			lastRainIntensityValue = RainIntensity;
			if (RainIntensity <= 0.01f)
			{
				if (RainFallParticleSystem != null)
				{
					ParticleSystem.EmissionModule emission = RainFallParticleSystem.emission;
					emission.enabled = false;
					RainFallParticleSystem.Stop();
				}
				if (RainMistParticleSystem != null)
				{
					ParticleSystem.EmissionModule emission2 = RainMistParticleSystem.emission;
					emission2.enabled = false;
					RainMistParticleSystem.Stop();
				}
				return;
			}
			if (RainFallParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission3 = RainFallParticleSystem.emission;
				bool flag2 = (RainFallParticleSystem.GetComponent<Renderer>().enabled = true);
				emission3.enabled = flag2;
				if (!RainFallParticleSystem.isPlaying)
				{
					RainFallParticleSystem.Play();
				}
				ParticleSystem.MinMaxCurve rateOverTime = emission3.rateOverTime;
				rateOverTime.mode = ParticleSystemCurveMode.Constant;
				float constantMin = (rateOverTime.constantMax = RainFallEmissionRate());
				rateOverTime.constantMin = constantMin;
				emission3.rateOverTime = rateOverTime;
			}
			if (RainMistParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission4 = RainMistParticleSystem.emission;
				bool flag2 = (RainMistParticleSystem.GetComponent<Renderer>().enabled = true);
				emission4.enabled = flag2;
				if (!RainMistParticleSystem.isPlaying)
				{
					RainMistParticleSystem.Play();
				}
				float num2 = ((!(RainIntensity < RainMistThreshold)) ? MistEmissionRate() : 0f);
				ParticleSystem.MinMaxCurve rateOverTime2 = emission4.rateOverTime;
				rateOverTime2.mode = ParticleSystemCurveMode.Constant;
				float constantMin = (rateOverTime2.constantMax = num2);
				rateOverTime2.constantMin = constantMin;
				emission4.rateOverTime = rateOverTime2;
			}
		}

		public void EffectSubjoin(int IntensityLevel, bool isFade)
		{
			if (MainDirectLight.color.r > AmbientColor.r)
			{
				LeanTween.value(MainDirectLight.color.r, AmbientColor.r, 4f).setOnUpdate(delegate(float f)
				{
					MainDirectLight.color = new Color(f, f, f);
				}).setIgnoreTimeScale(true);
			}
			switch (IntensityLevel)
			{
			case 1:
				if (isFade)
				{
					if (RainIntensity < 0.25f)
					{
						LeanTween.value(RainIntensity, 0.25f, 4f).setOnUpdate(delegate(float f)
						{
							RainIntensity = f;
						}).setIgnoreTimeScale(true);
					}
				}
				else
				{
					RainIntensity = 0.25f;
				}
				break;
			case 2:
				if (isFade)
				{
					if (RainIntensity < 0.5f)
					{
						LeanTween.value(RainIntensity, 0.5f, 6f).setOnUpdate(delegate(float f)
						{
							RainIntensity = f;
						}).setIgnoreTimeScale(true);
					}
				}
				else
				{
					RainIntensity = 0.5f;
				}
				break;
			case 3:
				if (isFade)
				{
					if (!(RainIntensity < 0.75f))
					{
						break;
					}
					LeanTween.value(RainIntensity, 0.75f, 8f).setOnUpdate(delegate(float f)
					{
						RainIntensity = f;
						if (RainIntensity > 0.5f)
						{
							EnableLightning = true;
						}
					}).setIgnoreTimeScale(true);
				}
				else
				{
					RainIntensity = 0.75f;
					EnableLightning = true;
				}
				break;
			case 4:
				if (isFade)
				{
					if (!(RainIntensity < 1f))
					{
						break;
					}
					LeanTween.value(RainIntensity, 1f, 8f).setOnUpdate(delegate(float f)
					{
						RainIntensity = f;
						if (RainIntensity > 0.5f)
						{
							EnableLightning = true;
						}
					}).setIgnoreTimeScale(true);
				}
				else
				{
					RainIntensity = 1f;
					EnableLightning = true;
				}
				break;
			}
		}

		public void EffectLessen(int IntensityLevel, bool isFade)
		{
			if (IntensityLevel == 0 && MainDirectLight.color.r < BackupDirectLightColor.r)
			{
				LeanTween.value(MainDirectLight.color.r, BackupDirectLightColor.r, 4f).setOnUpdate(delegate(float f)
				{
					MainDirectLight.color = new Color(f, f, f);
				}).setIgnoreTimeScale(true);
			}
			switch (IntensityLevel)
			{
			case 0:
				if (isFade)
				{
					if (!(RainIntensity > 0f))
					{
						break;
					}
					LeanTween.value(RainIntensity, 0f, 8f).setOnUpdate(delegate(float f)
					{
						RainIntensity = f;
						if (RainIntensity < 0.5f)
						{
							EnableLightning = false;
						}
					}).setIgnoreTimeScale(true);
				}
				else
				{
					RainIntensity = 0f;
					EnableLightning = false;
				}
				break;
			case 1:
				if (isFade)
				{
					if (!(RainIntensity > 0.25f))
					{
						break;
					}
					LeanTween.value(RainIntensity, 0.25f, 4f).setOnUpdate(delegate(float f)
					{
						RainIntensity = f;
						if (RainIntensity < 0.5f)
						{
							EnableLightning = false;
						}
					}).setIgnoreTimeScale(true);
				}
				else
				{
					RainIntensity = 0.25f;
					EnableLightning = false;
				}
				break;
			case 2:
				if (isFade)
				{
					if (!(RainIntensity > 0.5f))
					{
						break;
					}
					LeanTween.value(RainIntensity, 0.5f, 6f).setOnUpdate(delegate(float f)
					{
						RainIntensity = f;
						if (RainIntensity < 0.5f)
						{
							EnableLightning = false;
						}
					}).setIgnoreTimeScale(true);
				}
				else
				{
					RainIntensity = 0.5f;
					EnableLightning = false;
				}
				break;
			case 3:
				if (isFade)
				{
					if (RainIntensity > 0.75f)
					{
						LeanTween.value(RainIntensity, 0.75f, 8f).setOnUpdate(delegate(float f)
						{
							RainIntensity = f;
						}).setIgnoreTimeScale(true);
					}
				}
				else
				{
					RainIntensity = 0.75f;
				}
				break;
			case 4:
				if (isFade)
				{
					if (RainIntensity > 1f)
					{
						LeanTween.value(RainIntensity, 1f, 8f).setOnUpdate(delegate(float f)
						{
							RainIntensity = f;
						}).setIgnoreTimeScale(true);
					}
				}
				else
				{
					RainIntensity = 1f;
				}
				break;
			}
		}

		protected virtual void Start()
		{
			if (Camera == null)
			{
				Camera = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera;
			}
			MainDirectLight = GameObject.Find("StageLight01").GetComponent<Light>();
			BackupDirectLightColor = MainDirectLight.color;
			if (RainFallParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission = RainFallParticleSystem.emission;
				emission.enabled = false;
				Renderer component = RainFallParticleSystem.GetComponent<Renderer>();
				component.enabled = false;
				rainMaterial = new Material(component.material);
				rainMaterial.EnableKeyword("SOFTPARTICLES_OFF");
				component.material = rainMaterial;
			}
			if (RainExplosionParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission2 = RainExplosionParticleSystem.emission;
				emission2.enabled = false;
				Renderer component2 = RainExplosionParticleSystem.GetComponent<Renderer>();
				rainExplosionMaterial = new Material(component2.material);
				rainExplosionMaterial.EnableKeyword("SOFTPARTICLES_OFF");
				component2.material = rainExplosionMaterial;
			}
			if (RainMistParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission3 = RainMistParticleSystem.emission;
				emission3.enabled = false;
				Renderer component3 = RainMistParticleSystem.GetComponent<Renderer>();
				component3.enabled = false;
				rainMistMaterial = new Material(component3.material);
				if (UseRainMistSoftParticles)
				{
					rainMistMaterial.EnableKeyword("SOFTPARTICLES_ON");
				}
				else
				{
					rainMistMaterial.EnableKeyword("SOFTPARTICLES_OFF");
				}
				component3.material = rainMistMaterial;
			}
			WeatherSystem component4 = base.transform.parent.GetComponent<WeatherSystem>();
			if (component4.isStartActive)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.WEATHER_SYSTEM_CTRL, true, component4.StartActiveLivel, component4.isStartActiveFade);
			}
		}

		protected virtual void Update()
		{
			CheckForRainChange();
			if (!EnableLightning)
			{
				return;
			}
			if (!LightningOn)
			{
				if (Time.time > LightningTime && OrangeBattleUtility.Random(0, 100) < 50)
				{
					LightningOn = true;
					LightningTime = (float)OrangeBattleUtility.Random(1, 2) + Time.time;
				}
			}
			else if (Time.time > LightningTime)
			{
				MainDirectLight.color = AmbientColor;
				LightningOn = false;
				LightningTime = (float)OrangeBattleUtility.Random(2, 5) + Time.time;
			}
			else if (Time.time - lastTime < minTime)
			{
				if ((float)OrangeBattleUtility.Random(0, 100) < thresh * 100f)
				{
					MainDirectLight.color = new Color(1f, 1f, 1f);
				}
				else
				{
					MainDirectLight.color = AmbientColor;
				}
			}
			else
			{
				lastTime = Time.time;
			}
		}

		protected virtual float RainFallEmissionRate()
		{
			return (float)RainFallParticleSystem.main.maxParticles / RainFallParticleSystem.main.startLifetime.constant * RainIntensity;
		}

		protected virtual float MistEmissionRate()
		{
			return (float)RainMistParticleSystem.main.maxParticles / RainMistParticleSystem.main.startLifetime.constant * RainIntensity * RainIntensity;
		}
	}
}
