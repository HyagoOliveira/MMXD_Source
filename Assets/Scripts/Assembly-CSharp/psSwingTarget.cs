using UnityEngine;

public class psSwingTarget : FxBase
{
	public Transform Line_Object;

	public ParticleSystem Arrow_Objec;

	public void SetEffect(float distance, Color mAcol, Color mbg, float lefttime, float scl = 1f)
	{
		Arrow_Objec.Stop();
		LineRenderer component = Line_Object.GetComponent<LineRenderer>();
		timeBackToPool = lefttime;
		Line_Object.transform.localScale = new Vector3(1f, 0.3f * scl, 1f);
		Arrow_Objec.transform.localScale = new Vector3(scl, scl, scl);
		component.SetPosition(0, new Vector3(0f, 0f, 0f));
		component.SetPosition(1, new Vector3(distance, 0f, 0f));
		component.startColor = mbg;
		component.endColor = mbg;
		ParticleSystem.MainModule main = Arrow_Objec.main;
		ParticleSystemRenderer component2 = Arrow_Objec.gameObject.GetComponent<ParticleSystemRenderer>();
		float num = distance / 3.4f / scl;
		main.startLifetime = num;
		main.startColor = mAcol;
		if ((bool)component2)
		{
			component2.pivot = new Vector3(num / 2.4767f * -1f, 0f, 0f);
		}
		Arrow_Objec.Play();
	}
}
