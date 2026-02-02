using UnityEngine;

public class CH034_ArcBlade : BasicBullet
{
	[SerializeField]
	protected Vector3 subBulletShiftPos = new Vector3(0f, 0.5f, 0f);

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		if (!isSubBullet)
		{
			pPos += subBulletShiftPos;
			pDirection = Vector3.down;
		}
		base.Active(pPos, pDirection, pTargetMask, pTarget);
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		Active(pTransform.position, pDirection, pTargetMask, pTarget);
	}

	protected override void BulletReflect()
	{
		if (reflectCount <= 0 || Velocity.Equals(Vector3.zero))
		{
			hasReflect = false;
			return;
		}
		Vector3 vector = ((Velocity.x > 0f) ? Vector3.right : Vector3.left);
		Vector3 vector2 = _transform.TransformDirection(vector);
		bool flag = false;
		RaycastHit2D[] array = Physics2D.RaycastAll(reflectPoint, vector2, float.PositiveInfinity, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (!IsStageHurtObject(raycastHit2D.collider))
			{
				reflectCount--;
				reflectPoint = raycastHit2D.point + raycastHit2D.normal * (_colliderSize.x * 0.5f);
				Vector2 vector3 = Vector2.Reflect(vector2, raycastHit2D.normal);
				reflectRotation = Quaternion.FromToRotation(vector, vector3) * BulletQuaternion;
				lastReflectTrans = raycastHit2D.transform;
				hasReflect = true;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			flag = CheckReflect(true);
			if (!flag)
			{
				flag = CheckReflect(false);
			}
		}
		if (!flag)
		{
			reflectCount = 0;
			hasReflect = false;
		}
	}

	protected bool CheckReflect(bool bUp)
	{
		float z = (bUp ? 90 : (-90));
		Vector3 vector = ((Velocity.x > 0f) ? Vector3.right : Vector3.left);
		Vector3 vector2 = _transform.TransformDirection(vector);
		Vector3 vector3 = Quaternion.Euler(0f, 0f, z) * vector2;
		Vector3 vector4 = reflectPoint + (vector3.normalized * (_colliderSize.x * 0.5f)).xy();
		Vector2 vector5 = reflectPoint - vector4.xy();
		RaycastHit2D[] array = Physics2D.RaycastAll(vector4, vector2, float.PositiveInfinity, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit2D raycastHit2D = array[i];
			if (!IsStageHurtObject(raycastHit2D.collider))
			{
				reflectCount--;
				reflectPoint = raycastHit2D.point + vector5 + raycastHit2D.normal * (_colliderSize.x * 0.5f);
				Vector2 vector6 = Vector2.Reflect(vector2, raycastHit2D.normal);
				reflectRotation = Quaternion.FromToRotation(vector, vector6) * BulletQuaternion;
				lastReflectTrans = raycastHit2D.transform;
				hasReflect = true;
				return true;
			}
		}
		return false;
	}
}
