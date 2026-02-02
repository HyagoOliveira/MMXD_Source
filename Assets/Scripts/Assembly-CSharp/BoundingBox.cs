using DragonBones;
using UnityEngine;

public class BoundingBox : BaseDemo
{
	public const float LINE_WIDTH = 4f;

	private readonly Point _intersectionPointA = new Point();

	private readonly Point _intersectionPointB = new Point();

	private readonly Point _normalRadians = new Point();

	private Vector3 _helpPointA;

	private Vector3 _helpPointB;

	private UnityArmatureComponent _armatureComp;

	private UnityArmatureComponent _boundingBoxComp;

	private UnityArmatureComponent _targetA;

	private UnityArmatureComponent _targetB;

	private GameObject _lineSlot;

	private GameObject _pointSlotA;

	private GameObject _pointSlotB;

	protected override void OnStart()
	{
		UnityFactory.factory.LoadDragonBonesData("mecha_2903/mecha_2903_ske");
		UnityFactory.factory.LoadTextureAtlasData("mecha_2903/mecha_2903_tex");
		UnityFactory.factory.LoadDragonBonesData("bounding_box_tester/bounding_box_tester_ske");
		UnityFactory.factory.LoadTextureAtlasData("bounding_box_tester/bounding_box_tester_tex");
		_armatureComp = UnityFactory.factory.BuildArmatureComponent("mecha_2903d");
		_boundingBoxComp = UnityFactory.factory.BuildArmatureComponent("tester");
		_targetA = _boundingBoxComp.armature.GetSlot("target_a").childArmature.proxy as UnityArmatureComponent;
		_targetB = _boundingBoxComp.armature.GetSlot("target_b").childArmature.proxy as UnityArmatureComponent;
		_pointSlotA = _boundingBoxComp.armature.GetSlot("point_a").display as GameObject;
		_pointSlotB = _boundingBoxComp.armature.GetSlot("point_b").display as GameObject;
		_lineSlot = _boundingBoxComp.armature.GetSlot("line").display as GameObject;
		_armatureComp.debugDraw = true;
		_targetA.armature.inheritAnimation = false;
		_targetB.armature.inheritAnimation = false;
		_armatureComp.sortingOrder = 0;
		_boundingBoxComp.sortingOrder = 1;
		_pointSlotA.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
		_pointSlotB.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
		_armatureComp.animation.Play("walk");
		_boundingBoxComp.animation.Play("0");
		_targetA.animation.Play("0");
		_targetB.animation.Play("0");
		EnableDrag(_targetA.gameObject);
		EnableDrag(_targetB.gameObject);
	}

	protected override void OnUpdate()
	{
		BoundingBoxCheck();
	}

	private void BoundingBoxCheck()
	{
		_helpPointA = _armatureComp.transform.InverseTransformPoint(_targetA.transform.position);
		_helpPointB = _armatureComp.transform.InverseTransformPoint(_targetB.transform.position);
		Slot slot = _armatureComp.armature.ContainsPoint(_helpPointA.x, _helpPointA.y);
		Slot slot2 = _armatureComp.armature.ContainsPoint(_helpPointB.x, _helpPointB.y);
		Slot slot3 = _armatureComp.armature.IntersectsSegment(_helpPointA.x, _helpPointA.y, _helpPointB.x, _helpPointB.y, _intersectionPointA, _intersectionPointB, _normalRadians);
		string text = ((slot != null) ? "1" : "0");
		if (_targetA.animation.lastAnimationName != text)
		{
			_targetA.animation.FadeIn(text, 0.2f).resetToPose = false;
		}
		text = ((slot2 != null) ? "1" : "0");
		if (_targetB.animation.lastAnimationName != text)
		{
			_targetB.animation.FadeIn(text, 0.2f).resetToPose = false;
		}
		text = ((slot3 != null) ? "1" : "0");
		if (_boundingBoxComp.animation.lastAnimationName != text)
		{
			_boundingBoxComp.animation.FadeIn(text, 0.2f).resetToPose = false;
		}
		Vector3 localPosition = _targetA.transform.localPosition;
		Vector3 vector = _targetB.transform.localPosition - localPosition;
		float num = vector.magnitude / 4f;
		Vector3 vector2 = localPosition + vector.normalized * 4f * num / 2f;
		_lineSlot.transform.localPosition = new Vector3(vector2.x, vector2.y, _lineSlot.transform.localPosition.z);
		_lineSlot.transform.localScale = new Vector3(num, 1f, 1f);
		_lineSlot.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(vector.y, vector.x) * 57.29578f);
		if (slot3 != null)
		{
			_helpPointA = _armatureComp.transform.TransformPoint(_intersectionPointA.x, _intersectionPointA.y, 0f);
			_helpPointB = _armatureComp.transform.TransformPoint(_intersectionPointB.x, _intersectionPointB.y, 0f);
			_helpPointA = _boundingBoxComp.transform.InverseTransformPoint(_helpPointA);
			_helpPointB = _boundingBoxComp.transform.InverseTransformPoint(_helpPointB);
			_helpPointA.z = _pointSlotA.transform.localPosition.z;
			_helpPointB.z = _pointSlotB.transform.localPosition.z;
			_pointSlotA.SetActive(true);
			_pointSlotB.SetActive(true);
			_pointSlotA.transform.localPosition = _helpPointA;
			_pointSlotB.transform.localPosition = _helpPointB;
			_pointSlotA.transform.localEulerAngles = new Vector3(0f, 0f, _normalRadians.x * 57.29578f);
			_pointSlotB.transform.localEulerAngles = new Vector3(0f, 0f, _normalRadians.y * 57.29578f);
		}
		else
		{
			_pointSlotA.SetActive(false);
			_pointSlotB.SetActive(false);
		}
	}
}
