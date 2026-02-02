#define RELEASE
using System.Collections;
using UnityEngine;

public class WantedRateInfoHelper : OrangeChildUIBase
{
	[SerializeField]
	private WantedRateInfoUnitHelper[] _rateInfoUnitHelpers;

	[SerializeField]
	public float _randomRateTime;

	private Coroutine _randomRateCoroutine;

	private void Awake()
	{
		_rateInfoUnitHelpers[0].Setup(0);
		_rateInfoUnitHelpers[1].Setup(1);
		_rateInfoUnitHelpers[2].Setup(2);
	}

	private void OnDestroy()
	{
		StopRandomRate();
	}

	public void Setup(WANTED_SUCCESS_TABLE successAttrData, WantedConditionFlag conditionFlag, int conditionLevel)
	{
		StopRandomRate();
		if (conditionFlag.HasFlag(WantedConditionFlag.BasicCondition))
		{
			switch (conditionLevel)
			{
			case 1:
				_rateInfoUnitHelpers[0].SetRate(successAttrData.n_GUILD_WANTED_RATE_1);
				_rateInfoUnitHelpers[1].SetRate(successAttrData.n_GUILD_WANTED_RATE_2);
				_rateInfoUnitHelpers[2].SetRate(successAttrData.n_GUILD_WANTED_RATE_3);
				break;
			case 2:
				_rateInfoUnitHelpers[0].SetRate(successAttrData.n_GUILD_WANTED_RATE_4);
				_rateInfoUnitHelpers[1].SetRate(successAttrData.n_GUILD_WANTED_RATE_5);
				_rateInfoUnitHelpers[2].SetRate(successAttrData.n_GUILD_WANTED_RATE_6);
				break;
			case 3:
				_rateInfoUnitHelpers[0].SetRate(successAttrData.n_GUILD_WANTED_RATE_7);
				_rateInfoUnitHelpers[1].SetRate(successAttrData.n_GUILD_WANTED_RATE_8);
				_rateInfoUnitHelpers[2].SetRate(successAttrData.n_GUILD_WANTED_RATE_9);
				break;
			case 4:
				_rateInfoUnitHelpers[0].SetRate(successAttrData.n_GUILD_WANTED_RATE_10);
				_rateInfoUnitHelpers[1].SetRate(successAttrData.n_GUILD_WANTED_RATE_11);
				_rateInfoUnitHelpers[2].SetRate(successAttrData.n_GUILD_WANTED_RATE_12);
				break;
			default:
				Debug.LogError(string.Format("Invalid ConditionLevel : {0}", conditionLevel));
				ClearRate();
				break;
			}
		}
		else
		{
			ClearRate();
		}
	}

	public void ClearRate()
	{
		StopRandomRate();
		_rateInfoUnitHelpers[0].SetRate(0);
		_rateInfoUnitHelpers[1].SetRate(0);
		_rateInfoUnitHelpers[2].SetRate(0);
	}

	public void StartRandomRate()
	{
		if (_randomRateCoroutine == null)
		{
			_randomRateCoroutine = StartCoroutine(RandomRateCoroutine());
		}
	}

	private void StopRandomRate()
	{
		if (_randomRateCoroutine != null)
		{
			StopCoroutine(_randomRateCoroutine);
			_randomRateCoroutine = null;
		}
	}

	private IEnumerator RandomRateCoroutine()
	{
		while (true)
		{
			int num = Random.Range(0, 100);
			_rateInfoUnitHelpers[0].SetRandomRate(num);
			int num2 = Random.Range(0, 100 - num);
			_rateInfoUnitHelpers[1].SetRandomRate(num2);
			int randomRate = 100 - num - num2;
			_rateInfoUnitHelpers[2].SetRandomRate(randomRate);
			yield return new WaitForSeconds(_randomRateTime);
		}
	}
}
