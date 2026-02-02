using UnityEngine;

public class AnimationPos : MonoBehaviour
{
	public float MinPosX;

	public float MaxPosX;

	public float MinPosY;

	public float MaxPosY;

	public float MinPosZ;

	public float MaxPosZ;

	private void Start()
	{
		Animation animation = base.transform.gameObject.AddComponent<Animation>();
		AnimationClip animationClip = new AnimationClip
		{
			legacy = true
		};
		Keyframe[] array = new Keyframe[3]
		{
			new Keyframe(0f, base.transform.position.y),
			default(Keyframe),
			default(Keyframe)
		};
		if (Random.Range(0, 100) < 50)
		{
			array[1] = new Keyframe(2f, base.transform.position.y + Random.Range(MinPosY, MaxPosY));
		}
		else
		{
			array[1] = new Keyframe(2f, base.transform.position.y + Random.Range(0f - MinPosY, 0f - MaxPosY));
		}
		array[2] = new Keyframe(4f, base.transform.position.y);
		animationClip.SetCurve(curve: new AnimationCurve(array), relativePath: "", type: typeof(Transform), propertyName: "localPosition.y");
		array = new Keyframe[3]
		{
			new Keyframe(0f, base.transform.position.x),
			default(Keyframe),
			default(Keyframe)
		};
		if (Random.Range(0, 100) < 50)
		{
			array[1] = new Keyframe(2f, base.transform.position.x + Random.Range(MinPosX, MaxPosX));
		}
		else
		{
			array[1] = new Keyframe(2f, base.transform.position.x + Random.Range(0f - MinPosX, 0f - MaxPosX));
		}
		array[2] = new Keyframe(4f, base.transform.position.x);
		animationClip.SetCurve(curve: new AnimationCurve(array), relativePath: "", type: typeof(Transform), propertyName: "localPosition.x");
		array = new Keyframe[3]
		{
			new Keyframe(0f, base.transform.position.z),
			default(Keyframe),
			default(Keyframe)
		};
		if (Random.Range(0, 100) < 50)
		{
			array[1] = new Keyframe(2f, base.transform.position.z + Random.Range(MinPosZ, MaxPosZ));
		}
		else
		{
			array[1] = new Keyframe(2f, base.transform.position.z + Random.Range(0f - MinPosZ, 0f - MaxPosZ));
		}
		array[2] = new Keyframe(4f, base.transform.position.z);
		animationClip.SetCurve(curve: new AnimationCurve(array), relativePath: "", type: typeof(Transform), propertyName: "localPosition.z");
		animationClip.wrapMode = WrapMode.Loop;
		animation.AddClip(animationClip, animationClip.name);
		animation.Play(animationClip.name);
	}
}
