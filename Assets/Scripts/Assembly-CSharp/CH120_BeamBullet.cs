using UnityEngine;

public class CH120_BeamBullet : BeamBullet
{
	public float EndPointPositionOffset;

	public float LineRendererLengthOffset;

	protected override void Update_Effect()
	{
		bIsEnd = false;
		float num = ((BoxCollider2D)_hitCollider).size.x + EndPointPositionOffset;
		fxEndpoint.localPosition = new Vector3(0f, 0f, num);
		Transform[] array = fxLine;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<LineRenderer>().SetPosition(0, new Vector3(0f - (num + LineRendererLengthOffset), 0f, 0f));
		}
	}
}
