using UnityEngine;

public class Beam016Bullet : BeamBullet
{
	protected float defLength = 4.53f;

	[SerializeField]
	private ParticleSystem fxLine00;

	[SerializeField]
	private ParticleSystem fxLine01;

	[SerializeField]
	private ParticleSystem fxLine02;

	[SerializeField]
	private ParticleSystem fxLine03;

	protected override void Update_Effect()
	{
		defLength = fxEndpoint.transform.localPosition.z;
		bIsEnd = false;
		float x = ((BoxCollider2D)_hitCollider).size.x;
		float num = x - defLength;
		fxEndpoint.localPosition += new Vector3(0f, 0f, num);
		Transform[] array = fxLine;
		for (int i = 0; i < array.Length; i++)
		{
			LineRenderer component = array[i].GetComponent<LineRenderer>();
			component.SetPosition(0, new Vector3(component.GetPosition(0).x + num, 0f, 0f));
		}
		SetLine00(ref fxLine00, num);
		SetLine01(ref fxLine01, x);
		SetLine02(ref fxLine02, x);
		SetLine02(ref fxLine03, x);
	}

	private void SetLine00(ref ParticleSystem ps, float difLength)
	{
		ParticleSystem.ShapeModule shape = ps.shape;
		shape.position = new Vector3(shape.position.x, shape.position.y, shape.position.z + difLength * 0.5f);
		shape.scale = new Vector3(shape.scale.x, shape.scale.y, shape.scale.z + difLength);
	}

	private void SetLine01(ref ParticleSystem ps, float length)
	{
		ParticleSystem.MainModule main = ps.main;
		main.startSizeXMultiplier = length / 6.695f;
	}

	private void SetLine02(ref ParticleSystem ps, float length)
	{
		ParticleSystem.MainModule main = ps.main;
		main.startSizeXMultiplier = length / 4.47f;
	}
}
