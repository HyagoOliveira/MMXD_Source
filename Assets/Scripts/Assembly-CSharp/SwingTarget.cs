using UnityEngine;

public class SwingTarget : MonoBehaviour
{
	public Transform Line_Object;

	public ParticleSystem Arrow_Objec;

	private LineRenderer LineR;

	public Transform Pint_Object;

	public float fShowTime = 999f;

	public Vector3 point1;

	public Vector3 point2;

	public float mscl = 1f;

	private void Start()
	{
		LineR = Line_Object.GetComponent<LineRenderer>();
		Arrow_Objec.Stop();
	}

	public void SetActiveLine(bool value, float infShowTime = 999f)
	{
		Line_Object.gameObject.SetActive(true);
		Arrow_Objec.gameObject.SetActive(true);
		fShowTime = infShowTime;
	}

	public void SetLine_Scl(Vector3 scl)
	{
	}

	public void SetLine_Pos(Vector3 Source, Vector3 Target)
	{
		LineR.SetPosition(0, new Vector3(Target.x, Target.z, 0f - Source.y));
		LineR.SetPosition(1, new Vector3(Source.x, Source.z, 0f - Source.y));
	}

	public void SetLine_Pos(Vector3 Source, Vector3 Targe, Color mcol, float scl = 1f)
	{
		Arrow_Objec.Stop();
		float num = Vector2.Distance(Source, Targe);
		Vector3 vector = (Source.xy() - Targe.xy()).normalized;
		Line_Object.transform.localScale = new Vector3(1f, 0.3f * scl, 1f);
		Arrow_Objec.transform.localScale = new Vector3(scl, scl, scl);
		LineR.transform.localPosition = Source;
		LineR.transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, -vector));
		LineR.SetPosition(0, new Vector3(0f, 0f, 0f));
		LineR.SetPosition(1, new Vector3(num, 0f, 0f));
		LineR.startColor = mcol;
		LineR.endColor = mcol;
		Arrow_Objec.transform.localPosition = Source;
		Arrow_Objec.transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector));
		ParticleSystem.MainModule main = Arrow_Objec.main;
		ParticleSystemRenderer component = Arrow_Objec.gameObject.GetComponent<ParticleSystemRenderer>();
		float num2 = num / 3.4f / scl;
		main.startLifetime = num2;
		main.startColor = mcol;
		if ((bool)component)
		{
			component.pivot = new Vector3(num2 / 2.4767f * -1f, 0f, 0f);
		}
		Arrow_Objec.Play();
	}

	private void Update()
	{
		if (fShowTime > 0f)
		{
			fShowTime -= Time.deltaTime;
			if (fShowTime <= 0f)
			{
				Line_Object.gameObject.SetActive(false);
				Arrow_Objec.gameObject.SetActive(false);
				Arrow_Objec.Stop();
			}
		}
	}
}
