using UnityEngine;

namespace Coffee.UIExtensions
{
	public class UIEffect_Demo_Dialog : MonoBehaviour
	{
		[SerializeField]
		private Animator m_Animator;

		public void Open()
		{
			base.gameObject.SetActive(true);
		}

		public void Close()
		{
			m_Animator.SetTrigger("Close");
		}

		public void Closed()
		{
			base.gameObject.SetActive(false);
		}
	}
}
