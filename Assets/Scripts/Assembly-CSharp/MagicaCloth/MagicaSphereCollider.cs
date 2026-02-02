using UnityEngine;

namespace MagicaCloth
{
    [HelpURL("https://magicasoft.jp/magica-cloth-sphere-collider/")]
    [AddComponentMenu("MagicaCloth/MagicaSphereCollider")]
    public class MagicaSphereCollider : ColliderComponent
    {
        [SerializeField]
        [Range(0.001f, 0.5f)]
        private float radius = 0.05f;

        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            foreach (ChunkData value in particleDict.Values)
            {
                for (int i = 0; i < value.dataLength; i++)
                {
                    CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetRadius(value.startIndex + i, radius);
                    CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetLocalPos(value.startIndex + i, base.Center);
                }
            }
        }

        public override int GetDataHash()
        {
            return base.GetDataHash() + radius.GetDataHash();
        }

        protected override ChunkData CreateColliderParticleReal(int teamId)
        {
            uint num = 0u;
            num |= 2u;
            num |= 0x10u;
            num |= 0x4000u;
            num |= 0x1000000u;
            num |= 0x2000000u;
            num |= 0x8000u;
            ChunkData result = CreateParticle(num, teamId, 0f, radius, base.Center);
            CreateSingleton<MagicaPhysicsManager>.Instance.Team.AddCollider(teamId, result.startIndex);
            return result;
        }

        public float GetScale()
        {
            return base.transform.lossyScale.x;
        }

        public override bool CalcNearPoint(Vector3 pos, out Vector3 p, out Vector3 dir, out Vector3 d, bool skinning)
        {
            dir = Vector3.zero;
            float scale = GetScale();
            d = base.transform.TransformPoint(base.Center);
            Vector3 vector = pos - d;
            float magnitude = vector.magnitude;
            if (magnitude <= Radius * scale)
            {
                p = pos;
                if (magnitude > 0f)
                {
                    dir = vector.normalized;
                }
            }
            else
            {
                dir = vector.normalized;
                p = d + dir * Radius;
            }
            return true;
        }
    }
}
