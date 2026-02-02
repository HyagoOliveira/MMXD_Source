using UnityEngine;

public class CH062_BeamBullet : BeamBullet
{
	[SerializeField]
	private Transform fxStartpoint;

	[SerializeField]
	private Transform fxLinepoint;

	[SerializeField]
	private LineRenderer[] lineRenderers;

	[SerializeField]
	private Transform fxStartParticle;

	[SerializeField]
	private Transform fxEndParticle;

	private bool _isEffectUpdated;

	protected override void Update_Effect()
	{
		if (!_isEffectUpdated)
		{
			float x = ((BoxCollider2D)_hitCollider).size.x;
			Vector3 vector = fxEndpoint.localPosition - fxStartpoint.localPosition;
			float num = x / vector.magnitude;
			Vector3 vector2 = fxLinepoint.localPosition - fxStartpoint.localPosition;
			float magnitude = vector2.magnitude;
			float magnitude2 = (fxLinepoint.localPosition - fxEndpoint.localPosition).magnitude;
			fxEndpoint.localPosition = fxStartpoint.localPosition + vector * num;
			fxLinepoint.localPosition = fxStartpoint.localPosition + vector2 * num;
			float magnitude3 = (fxLinepoint.localPosition - fxStartpoint.localPosition).magnitude;
			float magnitude4 = (fxLinepoint.localPosition - fxEndpoint.localPosition).magnitude;
			LineRenderer[] array = lineRenderers;
			foreach (LineRenderer obj in array)
			{
				Vector3 position = obj.GetPosition(1);
				Vector3 position2 = obj.GetPosition(0);
				float magnitude5 = position.magnitude;
				float magnitude6 = position2.magnitude;
				float num2 = magnitude3 - (magnitude - magnitude5);
				position *= num2 / magnitude5;
				float num3 = magnitude4 - (magnitude2 - magnitude6);
				position2 *= num3 / magnitude6;
				obj.SetPosition(1, position);
				obj.SetPosition(0, position2);
			}
			float num4 = magnitude - (magnitude - fxStartParticle.localPosition.magnitude);
			fxStartParticle.localPosition = fxStartParticle.localPosition.normalized * num4;
			float num5 = magnitude4 - (magnitude - fxEndParticle.localPosition.magnitude);
			fxEndParticle.localPosition = fxEndParticle.localPosition.normalized * num5;
			_isEffectUpdated = true;
		}
	}
}
