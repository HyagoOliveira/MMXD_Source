using UnityEngine;

public class CH058_BeamBullet : BeamBullet
{
	protected float defLength = 4.62f;

	[SerializeField]
	private LineRenderer fxLine00;

	[SerializeField]
	private LineRenderer fxLine01L;

	[SerializeField]
	private LineRenderer fxLine01R;

	[SerializeField]
	private ParticleSystem fxLine02L;

	[SerializeField]
	private ParticleSystem fxLine02R;

	[SerializeField]
	private ParticleSystem fxLoveLove;

	[SerializeField]
	private ParticleSystem fxLoveLove2;

	[SerializeField]
	private ParticleSystem fxLoveLove3;

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
			component.SetPosition(0, new Vector3(component.GetPosition(0).x - num, 0f, 0f));
		}
		fxLine00.SetPosition(0, new Vector3(fxLine00.GetPosition(0).x - num, 0f, 0f));
		fxLine01L.SetPosition(0, new Vector3(0f - (num + 2f), 0f, 0f));
		fxLine01R.transform.localPosition += new Vector3(0f - num, 0f, 0f);
		fxLine01R.SetPosition(1, new Vector3(num + 2f, 0f, 0f));
		SetLine02(ref fxLine02L, num);
		SetLine02(ref fxLine02R, num);
		SetLoveLove(ref fxLoveLove, num, x);
		SetLoveLove2(ref fxLoveLove2, num);
		SetLoveLove2(ref fxLoveLove3, num);
	}

	private void SetLine02(ref ParticleSystem ps, float difLength)
	{
		ParticleSystem.ShapeModule shape = ps.shape;
		shape.scale = new Vector3(1f, shape.scale.y + difLength, 1f);
		ps.transform.localPosition += new Vector3((0f - difLength) * 0.5f, 0f, 0f);
	}

    [System.Obsolete]
    private void SetLoveLove(ref ParticleSystem ps, float difLength, float len)
	{
		ParticleSystem.ShapeModule shape = ps.shape;
		shape.scale = new Vector3(difLength * 0.5f + shape.scale.x, 1f, 1f);
		int num = 5 + Mathf.CeilToInt(len - 1f);
		ps.emission.SetBurst(0, new ParticleSystem.Burst(0f, num));
		ps.maxParticles = num * 2;
		ps.transform.localPosition += new Vector3((0f - difLength) * 0.5f, 0f, 0f);
	}

	private void SetLoveLove2(ref ParticleSystem ps, float difLenght)
	{
		ParticleSystem.ShapeModule shape = ps.shape;
		shape.scale = new Vector3(shape.scale.x + difLenght, shape.scale.y, shape.scale.z);
		ps.transform.localPosition += new Vector3((0f - difLenght) * 0.5f, 0f, 0f);
	}
}
