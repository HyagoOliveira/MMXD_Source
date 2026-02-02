using StageLib;

public class KillActivityUI2 : KillActivityUI
{
	private OrangeCharacter tOC;

	private int nLastStack;

	private int nLastMaxStack;

	public override void Init()
	{
		KillActivityEft.Play();
		KillScoreBar.fillAmount = 0f;
		KillScoreText.text = "0%";
	}

	public override void LateUpdate()
	{
		if (tOC == null)
		{
			tOC = StageUpdate.GetMainPlayerOC();
			if (tOC == null)
			{
				return;
			}
		}
		int count = tOC.selfBuffManager.listBuffs.Count;
		int num = 1;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			PerBuff perBuff = tOC.selfBuffManager.listBuffs[i];
			if (perBuff.refCTable.n_EFFECT == 120)
			{
				num2 += perBuff.nStack;
				if (perBuff.refCTable.n_MAX_STACK > num)
				{
					num = perBuff.refCTable.n_MAX_STACK;
				}
			}
		}
		if (num2 > nLastStack)
		{
			nLastStack = num2;
		}
		if (num > nLastMaxStack)
		{
			nLastMaxStack = num;
		}
		if (nLastStack > nLastMaxStack)
		{
			nLastStack = nLastMaxStack;
		}
		float num3 = (float)nLastStack / (float)nLastMaxStack;
		KillScoreBar.fillAmount = num3;
		KillScoreText.text = (num3 * 100f).ToString("0.0") + "%";
	}
}
