using System.Collections.Generic;
using UnityEngine;

public class GuildEddieRewardProgressHelper : MonoBehaviour
{
	[SerializeField]
	private GuildEddieRewardProgressValueHelper[] _thresholdValueHelpers;

	public void Setup(int currentValue, List<GuildEddieDonateSetting> settings)
	{
		float num = 0f;
		for (int i = 0; i < _thresholdValueHelpers.Length; i++)
		{
			float minValue = num;
			num = ((i < settings.Count) ? ((float)settings[i].Threshold) : num);
			_thresholdValueHelpers[i].Setup(minValue, num, currentValue, i, i > 0);
		}
	}
}
