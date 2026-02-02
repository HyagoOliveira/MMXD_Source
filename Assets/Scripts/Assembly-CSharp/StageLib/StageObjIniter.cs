using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageObjIniter : MonoBehaviour
	{
		public string sPrefabPath = "";

		public string sImagePath = "";

		private string sRealImagePath = "";

		public bool CheckMaterial(string path)
		{
			return false;
		}

		private void Start()
		{
		}

		private void LateUpdate()
		{
		}

		public void AddCollider()
		{
			GameObject obj = new GameObject();
			obj.transform.localPosition = base.transform.position;
			obj.transform.localRotation = Quaternion.identity;
			obj.transform.localScale = Vector3.one;
			obj.name = "colliderobj";
			BoxCollider2D boxCollider2D = obj.AddComponent<BoxCollider2D>();
			obj.layer = base.gameObject.layer;
			BoxCollider boxCollider = base.transform.gameObject.AddComponent<BoxCollider>();
			boxCollider2D.offset = new Vector2(boxCollider.center.x, boxCollider.center.z);
			boxCollider2D.size = new Vector2(boxCollider.size.x, boxCollider.size.z);
			Object.DestroyImmediate(boxCollider);
			obj.transform.parent = base.transform;
		}

		public void SetFallModeCollider()
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.localPosition = base.transform.position;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.name = base.gameObject.name + "_fall";
			BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
			gameObject.layer = base.gameObject.layer;
			BoxCollider boxCollider = base.transform.gameObject.AddComponent<BoxCollider>();
			boxCollider2D.offset = new Vector2(boxCollider.center.x, boxCollider.center.z);
			boxCollider2D.size = new Vector2(boxCollider.size.x, boxCollider.size.z);
			Object.DestroyImmediate(boxCollider);
			base.transform.parent = gameObject.transform;
			Controller2D controller2D = gameObject.AddComponent<Controller2D>();
			controller2D.collisionMask = LayerMask.GetMask("Block");
			controller2D.HorizontalRayCount = 4;
			controller2D.VerticalRayCount = 4;
			gameObject.AddComponent<FallingFloor>();
		}
	}
}
