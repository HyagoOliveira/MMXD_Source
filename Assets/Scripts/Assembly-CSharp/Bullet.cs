using DragonBones;
using UnityEngine;

[RequireComponent(typeof(UnityArmatureComponent))]
public class Bullet : MonoBehaviour
{
	private UnityArmatureComponent _armatureComponent;

	private UnityArmatureComponent _effectComponent;

	private Vector3 _speed;

	private void Awake()
	{
		_armatureComponent = base.gameObject.GetComponent<UnityArmatureComponent>();
	}

	public void Init(string effectArmatureName, float radian, float speed)
	{
		_speed.x = Mathf.Cos(radian) * speed * _armatureComponent.animation.timeScale;
		_speed.y = (0f - Mathf.Sin(radian)) * speed * _armatureComponent.animation.timeScale;
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.z = (0f - radian) * DragonBones.Transform.RAD_DEG;
		base.transform.localEulerAngles = localEulerAngles;
		_armatureComponent.armature.animation.Play("idle");
		if (effectArmatureName != null)
		{
			_effectComponent = UnityFactory.factory.BuildArmatureComponent(effectArmatureName);
			Vector3 localEulerAngles2 = _effectComponent.transform.localEulerAngles;
			Vector3 localScale = _effectComponent.transform.localScale;
			localEulerAngles2.z = (0f - radian) * DragonBones.Transform.RAD_DEG;
			if ((double)Random.Range(0f, 1f) < 0.5)
			{
				localEulerAngles2.x = 180f;
				localEulerAngles2.z = 0f - localEulerAngles2.z;
			}
			localScale.x = Random.Range(1f, 2f);
			localScale.y = Random.Range(1f, 1.5f);
			_effectComponent.animation.timeScale = _armatureComponent.animation.timeScale;
			_effectComponent.transform.localPosition = base.transform.localPosition;
			_effectComponent.transform.localEulerAngles = localEulerAngles2;
			_effectComponent.transform.localScale = localScale;
			_effectComponent.animation.Play("idle");
		}
	}

	private void Update()
	{
		if (_armatureComponent.armature == null)
		{
			return;
		}
		base.transform.localPosition += _speed;
		if (base.transform.localPosition.x < -7f || base.transform.localPosition.x > 7f || base.transform.localPosition.y < -7f || base.transform.localPosition.y > 7f)
		{
			_armatureComponent.armature.Dispose();
			if (_effectComponent != null)
			{
				_effectComponent.armature.Dispose();
			}
		}
	}
}
