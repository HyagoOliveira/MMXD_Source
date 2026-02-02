using UnityEngine;

public class SpringBoneNew : MonoBehaviour
{
	public Vector3 springEnd = Vector3.left;

	public bool useSpecifiedRotation;

	public Vector3 customRotation;

	public float stiffness = 1f;

	public float bounciness = 40f;

	[Range(0f, 0.9f)]
	public float dampness = 0.1f;

	private bool updated;

	private Vector3 currentTipPos;

	private SpringBoneNew parBone;

	private Vector3 velocity;

	private float springLength
	{
		get
		{
			return springEnd.magnitude;
		}
	}

	private void Start()
	{
		currentTipPos = base.transform.TransformPoint(springEnd);
		if (base.transform.parent != null)
		{
			parBone = base.transform.parent.GetComponentInParent<SpringBoneNew>();
		}
	}

	private void Update()
	{
		updated = false;
	}

	private void LateUpdate()
	{
		UpdateSpring();
	}

	private void UpdateSpring()
	{
		if (!updated)
		{
			if (parBone != null)
			{
				parBone.UpdateSpring();
			}
			updated = true;
			Vector3 vector = currentTipPos;
			if (useSpecifiedRotation)
			{
				base.transform.localRotation = Quaternion.Euler(customRotation);
			}
			currentTipPos = base.transform.TransformPoint(springEnd);
			Vector3 vector2 = bounciness * (currentTipPos - vector);
			vector2 += stiffness * (currentTipPos - base.transform.position).normalized;
			vector2 -= dampness * velocity;
			velocity += vector2 * Time.deltaTime;
			currentTipPos = vector + velocity * Time.deltaTime;
			currentTipPos = springLength * (currentTipPos - base.transform.position).normalized + base.transform.position;
			base.transform.rotation = Quaternion.FromToRotation(base.transform.TransformDirection(springEnd), (currentTipPos - base.transform.position).normalized) * base.transform.rotation;
		}
	}
}
