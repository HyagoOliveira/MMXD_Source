using System;
using System.Collections;
using UnityEngine;

public class Rotater : MonoBehaviour
{
	private enum Quadrant
	{
		FIRST = 0,
		SECOND = 1,
		THIRD = 2,
		FOUR = 3
	}

	[SerializeField]
	private Transform player;

	private Quadrant quadrant;

	private bool facingRight = true;

	private float radiusX = 3f;

	private float radiusY = 2f;

	private Transform _transform;

	private void Start()
	{
		Vector3 localPosition = new Vector3(radiusX * Mathf.Sin(0f), radiusY * Mathf.Cos(0f), 0f);
		base.gameObject.transform.localPosition = localPosition;
		StartCoroutine(MoveTween());
	}

	private IEnumerator MoveTween()
	{
		bool goNext = false;
		for (int i = 0; i < 72; i++)
		{
			goNext = false;
			float num = (float)i * 5f;
			Vector3 vector = new Vector3(radiusX * Mathf.Sin(num * ((float)Math.PI / 180f)), radiusY * Mathf.Cos(num * ((float)Math.PI / 180f)), 0f);
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			go.transform.position = vector;
			LeanTween.moveLocal(base.gameObject, vector, 0.1f).setOnComplete((Action)delegate
			{
				Vector3 localPosition = go.transform.localPosition;
				if (localPosition.x >= 0f && localPosition.y >= 0f)
				{
					quadrant = Quadrant.FIRST;
				}
				else if (localPosition.x < 0f && localPosition.y > 0f)
				{
					quadrant = Quadrant.SECOND;
				}
				else if (localPosition.x < 0f && localPosition.y < 0f)
				{
					quadrant = Quadrant.THIRD;
				}
				else if (localPosition.x > 0f && localPosition.y < 0f)
				{
					quadrant = Quadrant.FOUR;
				}
				if (facingRight && (quadrant == Quadrant.SECOND || quadrant == Quadrant.THIRD))
				{
					facingRight = !facingRight;
					player.localScale = new Vector3(1f, 1f, -1f);
				}
				else if (!facingRight && (quadrant == Quadrant.FIRST || quadrant == Quadrant.FOUR))
				{
					facingRight = !facingRight;
					player.localScale = new Vector3(1f, 1f, 1f);
				}
				goNext = true;
				UnityEngine.Object.Destroy(go);
			});
			while (!goNext)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		yield return null;
		StartCoroutine(MoveTween());
	}
}
