public class StoryMainUI : OrangeUIBase
{
	private void Start()
	{
		ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.GetEnumerator();
	}

	private void Update()
	{
	}

	public void OnChangeLStage()
	{
	}

	public void OnChangeRStage()
	{
	}
}
