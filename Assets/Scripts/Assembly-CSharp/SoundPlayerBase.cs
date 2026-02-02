using System.Collections;
using UnityEngine;

public class SoundPlayerBase : MonoBehaviour
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
		Loop = 1,
		Visible = 2
	}

	[SerializeField]
	public bool CheckVisible;

	[SerializeField]
	public bool NeedEndSE;

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
	public float f_fristTime;

	[SerializeField]
	[HideInInspector]
	public float f_loopTime = 0.5f;

	[SerializeField]
	public SoundPlayType m_playType;

	protected bool bSoundPlaying;

	protected bool bVisible;

	private string s_acb = "";

	private string cuename = "";

	[HideInInspector]
	private Renderer[] AllRenderers;

	[SerializeField]
	public OrangeCriSource SoundSource;

	private void Awake()
	{
	}

	private void Start()
	{
		if (SoundSource == null)
		{
			SoundSource = base.gameObject.AddOrGetComponent<OrangeCriSource>();
		}
		AllRenderers = GetComponentsInChildren<Renderer>();
		GetCriString();
		StartCoroutine(StartPlay());
	}

	private void Update()
	{
	}

	protected void OnEnable()
	{
	}

	protected void OnDisable()
	{
	}

	private void OnDestroy()
	{
		SetStop();
	}

	public void SetStop()
	{
		StopAllCoroutines();
		bSoundPlaying = false;
	}

	public IEnumerator StartPlay()
	{
		while (MonoBehaviourSingleton<UIManager>.Instance.IsLoading || MonoBehaviourSingleton<OrangeSceneManager>.Instance.NowScene != "StageTest")
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		switch (m_playType)
		{
		case SoundPlayType.playOnce:
			StartCoroutine(PlayOnce());
			break;
		case SoundPlayType.Loop:
			SoundSource.AddLoopSE(s_acb, cuename, f_loopTime, f_fristTime);
			break;
		case SoundPlayType.Visible:
			StartCoroutine(PlayVisible());
			break;
		}
		bSoundPlaying = true;
	}

	protected virtual IEnumerator PlayLoop()
	{
		if (f_fristTime > 0f)
		{
			yield return new WaitForSeconds(f_fristTime);
		}
		while (base.gameObject.activeSelf && bVisible)
		{
			yield return new WaitForSeconds(f_loopTime);
			PlaySE();
		}
		bSoundPlaying = false;
	}

	protected virtual IEnumerator PlayOnce()
	{
		if (f_fristTime > 0f)
		{
			yield return new WaitForSeconds(f_fristTime);
		}
		PlaySE();
		bSoundPlaying = false;
	}

	protected virtual IEnumerator PlayVisible()
	{
		if (f_fristTime > 0f)
		{
			yield return new WaitForSeconds(f_fristTime);
		}
		bool bIsPlay = false;
		while (true)
		{
			if (base.gameObject.activeSelf)
			{
				if (bIsVisible())
				{
					if (!bIsPlay)
					{
						bIsPlay = true;
						PlaySE();
					}
				}
				else if (bIsPlay)
				{
					bIsPlay = false;
					if (NeedEndSE)
					{
						PlayEndSE();
					}
				}
			}
			else if (bIsPlay)
			{
				bIsPlay = false;
				if (NeedEndSE)
				{
					PlayEndSE();
				}
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
	}

	private bool bIsVisible()
	{
		Renderer[] allRenderers = AllRenderers;
		for (int i = 0; i < allRenderers.Length; i++)
		{
			if (allRenderers[i].isVisible)
			{
				return true;
			}
		}
		return false;
	}

	private void GetCriString()
	{
		switch (m_acbType)
		{
		case CriType.battle:
			s_acb = "BattleSE";
			cuename = AudioManager.FormatEnum2Name(m_battleSE.ToString());
			break;
		case CriType.battle02:
			s_acb = "BattleSE02";
			cuename = AudioManager.FormatEnum2Name(m_battleSE02.ToString());
			break;
		case CriType.boss:
			s_acb = "BossSE";
			cuename = AudioManager.FormatEnum2Name(m_bossSE.ToString());
			break;
		case CriType.enemy:
			s_acb = "EnemySE";
			cuename = AudioManager.FormatEnum2Name(m_enemySE.ToString());
			break;
		case CriType.hit:
			s_acb = "HitSE";
			cuename = AudioManager.FormatEnum2Name(m_hitSE.ToString());
			break;
		case CriType.weapon:
			s_acb = "WeaponSE";
			cuename = AudioManager.FormatEnum2Name(m_weaponSE.ToString());
			break;
		case CriType.custom:
			s_acb = m_customSE[0];
			cuename = m_customSE[1];
			break;
		}
	}

	private void PlaySE()
	{
		SoundSource.PlaySE(s_acb, cuename);
	}

	private void PlayEndSE()
	{
		string text = m_endSE[0];
		string text2 = m_endSE[1];
		SoundSource.PlaySE(text, text2);
	}
}
