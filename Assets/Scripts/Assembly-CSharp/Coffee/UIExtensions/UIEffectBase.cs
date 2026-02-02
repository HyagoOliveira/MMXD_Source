using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	[DisallowMultipleComponent]
	public abstract class UIEffectBase : BaseMeshEffect, IParameterTexture
	{
		protected static readonly Vector2[] splitedCharacterPosition = new Vector2[4]
		{
			Vector2.up,
			Vector2.one,
			Vector2.right,
			Vector2.zero
		};

		protected static readonly List<UIVertex> tempVerts = new List<UIVertex>();

		[HideInInspector]
		[SerializeField]
		private int m_Version;

		[SerializeField]
		protected Material m_EffectMaterial;

		public int parameterIndex { get; set; }

		public virtual ParameterTexture ptex
		{
			get
			{
				return null;
			}
		}

		public Graphic targetGraphic
		{
			get
			{
				return base.graphic;
			}
		}

		public Material effectMaterial
		{
			get
			{
				return m_EffectMaterial;
			}
		}

		public virtual void ModifyMaterial()
		{
			targetGraphic.material = (base.isActiveAndEnabled ? m_EffectMaterial : null);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (ptex != null)
			{
				ptex.Register(this);
			}
			ModifyMaterial();
			SetVerticesDirty();
			SetDirty();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			ModifyMaterial();
			SetVerticesDirty();
			if (ptex != null)
			{
				ptex.Unregister(this);
			}
		}

		protected virtual void SetDirty()
		{
			SetVerticesDirty();
		}

		protected override void OnDidApplyAnimationProperties()
		{
			SetDirty();
		}
	}
}
