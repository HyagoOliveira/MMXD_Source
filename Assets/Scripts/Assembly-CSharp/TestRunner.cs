using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestRunner : MonoBehaviour
{
	[SerializeField]
	private Transform[] transforms;

	[SerializeField]
	private Transform[] paths;

	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private float time = 2.1f;

	public Slider sliderSpd;

	public Slider sliderZoom;

	private bool goNext = true;

	private void Start()
	{
		List<Vector3> list = new List<Vector3>();
		List<float> list2 = new List<float>();
		for (int i = 0; i < paths.Length; i++)
		{
			list.Add(paths[i].position);
			list2.Add(paths[i].eulerAngles.y);
		}
		Vector3[] pointsArray = list.ToArray();
		float[] rotationArray = list2.ToArray();
		StartCoroutine(UpdateRotate(pointsArray, rotationArray));
		sliderSpd.onValueChanged.AddListener(delegate
		{
			OnSpdChange();
		});
		sliderZoom.onValueChanged.AddListener(delegate
		{
			OnZoomChange();
		});
	}

	private IEnumerator UpdateRotate(Vector3[] pointsArray, float[] rotationArray)
	{
		for (int i = 0; i < rotationArray.Length; i++)
		{
			LeanTween.move(transforms[0].gameObject, pointsArray[i], time);
			LeanTween.move(transforms[1].gameObject, pointsArray[i], time);
			LeanTween.rotateY(transforms[0].gameObject, rotationArray[i], time).setOnComplete((Action)delegate
			{
				goNext = true;
			});
			goNext = false;
			while (!goNext)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		yield return null;
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("title");
	}

	public void OnSpdChange()
	{
		time = sliderSpd.minValue + sliderSpd.maxValue - sliderSpd.value;
	}

	public void OnZoomChange()
	{
		_camera.gameObject.transform.localPosition = new Vector3(_camera.gameObject.transform.localPosition.x, _camera.gameObject.transform.localPosition.y, sliderZoom.value);
	}
}
