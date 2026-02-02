public interface ILoadingState
{
	bool IsComplete { get; set; }

	object[] Params { get; set; }
}
