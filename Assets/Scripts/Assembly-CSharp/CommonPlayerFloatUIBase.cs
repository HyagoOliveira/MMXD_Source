using UnityEngine;

public abstract class CommonPlayerFloatUIBase : CommonFloatUIBase
{
	protected string _playerId;

	public virtual void Setup(string playerId, Vector3 tarPos)
	{
		_playerId = playerId;
		base.Setup(tarPos);
	}
}
