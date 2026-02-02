using System.Collections;
using UnityEngine;

public class FXSoundPlayer : MonoBehaviour, IManagedUpdateBehavior
{
	public enum CriType
	{
		hit = 0,
		boss = 1,
		enemy = 2,
		battle = 3,
		battle02 = 4,
		weapon = 5,
		custom = 6
	}

	public enum SoundPlayType
	{
		playOnce = 0,
		Loop = 1
	}

	[SerializeField]
	[Tooltip("檢查是否可見")]
	public bool CheckVisible;

	[SerializeField]
	[Tooltip("需要結束音效")]
	public bool NeedEndSE;

	[SerializeField]
	[Tooltip("強制音量")]
	public bool ForceVolume;

	[SerializeField]
	public CriType m_acbType;

	[SerializeField]
	[HideInInspector]
	public BossSE m_bossSE;

	[SerializeField]
	[HideInInspector]
	public EnemySE m_enemySE;

	[SerializeField]
	[HideInInspector]
	public HitSE m_hitSE;

	[SerializeField]
	[HideInInspector]
	public BattleSE m_battleSE;

	[SerializeField]
	[HideInInspector]
	public BattleSE02 m_battleSE02;

	[SerializeField]
	[HideInInspector]
	public WeaponSE m_weaponSE;

	[SerializeField]
	[HideInInspector]
	public string[] m_customSE;

	[SerializeField]
	[HideInInspector]
	public string[] m_endSE;

	[SerializeField]
	[Tooltip("Delay Time")]
	public float f_fristTime;

	[SerializeField]
	[Tooltip("Play Time Loop")]
	[HideInInspector]
	public float f_loopTime = 0.5f;

	[SerializeField]
	public SoundPlayType m_playType;

	[SerializeField]
	private OrangeCriSource _SSource;

	private ParticleSystem mFX;

	private ParticleSystem.MainModule fxMain;

	protected bool fxPlaying;

	private float oldTime;

	[SerializeField]
	public OrangeCriSource SoundSource
	{
		get
		{
			if (_SSource == null)
			{
				_SSource = base.gameObject.GetComponent<OrangeCriSource>();
				if (_SSource == null)
				{
					_SSource = base.gameObject.AddComponent<OrangeCriSource>();
					_SSource.Initial(OrangeSSType.ENEMY);
				}
			}
			return _SSource;
		}
		set
		{
			OrangeCriSource component = base.gameObject.GetComponent<OrangeCriSource>();
			if (component != null)
			{
				Object.Destroy(component);
			}
			_SSource = value;
		}
	}

	private void Awake()
	{
		mFX = base.transform.GetComponent<ParticleSystem>();
		if (SoundSource == null)
		{
			SoundSource = base.gameObject.AddOrGetComponent<OrangeCriSource>();
			SoundSource.Initial(OrangeSSType.HIT);
		}
		if ((bool)mFX)
		{
			fxMain = mFX.main;
			fxMain.stopAction = ParticleSystemStopAction.Callback;
		}
	}

	public void OnParticleSystemStopped()
	{
		StopPlay();
	}

	private void Start()
	{
	}

	private void PlayBattleSE(string s_acb, string cuename, float vol)
	{
		SoundSource.PlaySE(s_acb, cuename);
	}

	private void StopBattleSE(string s_acb, string cuename)
	{
		SoundSource.PlaySE(s_acb, cuename);
	}

	public void CallPlay()
	{
		if (MonoBehaviourSingleton<UIManager>.Instance.IsLoading || MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "StageTest")
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
	}

	public void StopPlay()
	{
		StopAllCoroutines();
		if (NeedEndSE)
		{
			CriType acbType = m_acbType;
			if (acbType == CriType.custom)
			{
				SoundSource.PlaySE(m_endSE[0], m_endSE[1]);
			}
		}
	}

	private void OnEnable()
	{
		if (mFX == null)
		{
			mFX = base.transform.GetComponentInChildren<ParticleSystem>();
		}
		if ((bool)mFX)
		{
			fxMain = mFX.main;
			fxMain.stopAction = ParticleSystemStopAction.Callback;
		}
		MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(this);
	}

	private void OnDisable()
	{
		if (fxPlaying)
		{
			StopPlay();
		}
		fxPlaying = false;
		MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(this);
	}

	public void UpdateFunc()
	{
		if (mFX == null)
		{
			return;
		}
		if (mFX.isPlaying)
		{
			if (!fxPlaying)
			{
				fxPlaying = true;
				CallPlay();
			}
		}
		else
		{
			fxPlaying = false;
		}
	}

	protected virtual IEnumerator PlayLoop()
	{
		if (f_fristTime > 0f)
		{
			yield return new WaitForSeconds(f_fristTime);
		}
		while (base.gameObject.activeSelf)
		{
			if (SoundSource.IsVisiable || ForceVolume)
			{
				PlaySE();
			}
			yield return new WaitForSeconds(f_loopTime);
		}
	}

	protected virtual IEnumerator PlayOnce()
	{
		if (f_fristTime > 0f)
		{
			yield return new WaitForSeconds(f_fristTime);
		}
		PlaySE();
	}

	public void PlaySE()
	{
		if (MonoBehaviourSingleton<UpdateManager>.Instance.Pause)
		{
			return;
		}
		switch (m_acbType)
		{
		case CriType.battle:
			SoundSource.PlaySE("BattleSE", AudioManager.FormatEnum2Name(m_battleSE.ToString()));
			break;
		case CriType.battle02:
			SoundSource.PlaySE("BattleSE02", AudioManager.FormatEnum2Name(m_battleSE02.ToString()));
			break;
		case CriType.boss:
			SoundSource.PlaySE("BossSE", (int)m_bossSE);
			break;
		case CriType.enemy:
			SoundSource.PlaySE("BossSE", (int)m_enemySE);
			break;
		case CriType.hit:
			SoundSource.PlaySE("HitSE", (int)m_hitSE);
			break;
		case CriType.weapon:
			SoundSource.PlaySE("WeaponSE", AudioManager.FormatEnum2Name(m_weaponSE.ToString()));
			break;
		case CriType.custom:
			if (ForceVolume)
			{
				SoundSource.ForcePlaySE(m_customSE[0], m_customSE[1]);
			}
			else
			{
				SoundSource.PlaySE(m_customSE[0], m_customSE[1]);
			}
			break;
		}
	}
}
