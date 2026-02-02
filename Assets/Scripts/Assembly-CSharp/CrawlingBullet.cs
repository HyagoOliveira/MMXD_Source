using UnityEngine;

public class CrawlingBullet : BasicBullet
{
	[SerializeField]
	private Transform headAnchor;

	[SerializeField]
	private Transform centerAnchor;

	[SerializeField]
	private float attachGap;

	private int downDirection;

	private Collider2D currentAttachCollider;

	public override void Active(Vector3 pPos, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pPos, pDirection, pTargetMask, pTarget);
		Initialize();
	}

	public override void Active(Transform pTransform, Vector3 pDirection, LayerMask pTargetMask, IAimTarget pTarget = null)
	{
		base.Active(pTransform, pDirection, pTargetMask, pTarget);
		Initialize();
	}

	private void Initialize()
	{
		Vector2.Distance(headAnchor.position, centerAnchor.position);
		float distance = 100f;
		float distance2 = 100f;
		Vector3 position = _transform.position;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(centerAnchor.position, centerAnchor.up, distance, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(centerAnchor.position, -centerAnchor.up, distance2, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		if (Vector2.Distance(centerAnchor.position, raycastHit2D.point) < Vector2.Distance(centerAnchor.position, raycastHit2D2.point))
		{
			downDirection = 1;
			position = raycastHit2D.point + raycastHit2D.normal.normalized * 0.3f;
			currentAttachCollider = raycastHit2D.collider;
		}
		else
		{
			downDirection = -1;
			position = raycastHit2D2.point + raycastHit2D2.normal.normalized * 0.3f;
			currentAttachCollider = raycastHit2D2.collider;
		}
		attachGap = 0.25f;
		_transform.position = position;
	}

	protected override void MoveBullet()
	{
		base.MoveBullet();
		Crawling();
	}

	private void Crawling()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(centerAnchor.position, centerAnchor.up * downDirection, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		float num = Vector2.Distance(_transform.position, raycastHit2D.point);
		if (num > attachGap)
		{
			if (currentAttachCollider == raycastHit2D.collider)
			{
				_transform.position += _transform.up * downDirection * (num - attachGap);
			}
			else
			{
				raycastHit2D = Physics2D.Raycast(headAnchor.position, headAnchor.up * downDirection, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
				if (Vector2.Distance(_transform.position, raycastHit2D.point) > attachGap)
				{
					_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Quaternion.Euler(0f, 0f, 90 * downDirection) * _transform.right));
					_transform.position += _transform.right * attachGap * 2f;
					raycastHit2D = Physics2D.Raycast(centerAnchor.position, centerAnchor.up * downDirection, 100f, 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
					if (raycastHit2D.collider != null)
					{
						currentAttachCollider = raycastHit2D.collider;
						_transform.position = raycastHit2D.point + raycastHit2D.normal.normalized * attachGap;
					}
				}
			}
		}
		else if (num < attachGap && raycastHit2D.collider == currentAttachCollider)
		{
			_transform.position -= _transform.up * downDirection * (attachGap - num);
		}
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(centerAnchor.position, headAnchor.position - centerAnchor.position, Vector2.Distance(headAnchor.position, centerAnchor.position), 1 << ManagedSingleton<OrangeLayerManager>.Instance.BlockLayer);
		if (raycastHit2D2.collider != null)
		{
			currentAttachCollider = raycastHit2D2.collider;
			_transform.position = raycastHit2D2.point + raycastHit2D2.normal.normalized * attachGap;
			_transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, Quaternion.Euler(0f, 0f, -90 * downDirection) * _transform.right));
		}
	}
}
