using UnityEngine;

public interface IAimTarget
{
	Transform AimTransform { get; set; }

	Vector3 AimPoint { get; set; }

	Vector3 AimPosition { get; set; }

	bool Activate { get; set; }

	bool AllowAutoAim { get; set; }

	bool VanishStatus { get; set; }

	AimTargetType AutoAimType { get; set; }

	PerBuffManager BuffManager { get; set; }
}
