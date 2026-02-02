using UnityEngine;

public class CH096_BeamBullet : BeamBullet
{
	protected float defLength = 4.62f;

	[SerializeField]
	private LineRenderer fxLine00;

	[SerializeField]
	private LineRenderer fxMesh01;

	[SerializeField]
	private ParticleSystem fxLine02;

	[SerializeField]
	private ParticleSystem fxLine03;

	[SerializeField]
	private LineRenderer fxLine01L;

	[SerializeField]
	private LineRenderer fxLine01R;

	[SerializeField]
	private ParticleSystem fxLine02L;

	[SerializeField]
	private ParticleSystem fxLine02R;

	protected override void Update_Effect()
	{
		defLength = fxEndpoint.transform.localPosition.z;
		bIsEnd = false;
		float num = ((BoxCollider2D)_hitCollider).size.x - defLength;
		fxEndpoint.localPosition += new Vector3(0f, 0f, num);
		Transform[] array = fxLine;
		for (int i = 0; i < array.Length; i++)
		{
			LineRenderer component = array[i].GetComponent<LineRenderer>();
			component.SetPosition(0, new Vector3(component.GetPosition(0).x - num, 0f, 0f));
		}
		fxLine00.SetPosition(0, new Vector3(fxLine00.GetPosition(0).x - num, 0f, 0f));
		fxMesh01.SetPosition(0, new Vector3(fxMesh01.GetPosition(0).x - num, 0f, 0f));
		fxLine01L.SetPosition(0, new Vector3(0f - (num + 2f), 0f, 0f));
		fxLine01R.transform.localPosition += new Vector3(0f - num, 0f, 0f);
		fxLine01R.SetPosition(1, new Vector3(num + 2f, 0f, 0f));
		SetLine02(ref fxLine02, num);
		SetLine02(ref fxLine03, num);
		SetLine02(ref fxLine02L, num);
		SetLine02(ref fxLine02R, num);
	}

	private void SetLine02(ref ParticleSystem ps, float difLength)
	{
		ParticleSystem.ShapeModule shape = ps.shape;
		shape.scale = new Vector3(1f, shape.scale.y + difLength, 1f);
		ps.transform.localPosition += new Vector3((0f - difLength) * 0.5f, 0f, 0f);
	}
}
