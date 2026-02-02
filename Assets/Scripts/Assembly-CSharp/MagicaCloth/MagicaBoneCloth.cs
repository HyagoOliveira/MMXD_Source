using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
    [HelpURL("https://magicasoft.jp/magica-cloth-bone-cloth/")]
    [AddComponentMenu("MagicaCloth/MagicaBoneCloth")]
    public class MagicaBoneCloth : BaseCloth
    {
        private const int DATA_VERSION = 6;

        private const int ERR_DATA_VERSION = 3;

        [SerializeField]
        private MeshData meshData;

        [SerializeField]
        private int meshDataHash;

        [SerializeField]
        private int meshDataVersion;

        [SerializeField]
        private BoneClothTarget clothTarget = new BoneClothTarget();

        [SerializeField]
        private List<Transform> useTransformList = new List<Transform>();

        [SerializeField]
        private List<Vector3> useTransformPositionList = new List<Vector3>();

        [SerializeField]
        private List<Quaternion> useTransformRotationList = new List<Quaternion>();

        [SerializeField]
        private List<Vector3> useTransformScaleList = new List<Vector3>();

        public BoneClothTarget ClothTarget
        {
            get
            {
                return clothTarget;
            }
        }

        public MeshData MeshData
        {
            get
            {
                return meshData;
            }
        }

        private int UseTransformCount
        {
            get
            {
                return useTransformList.Count;
            }
        }

        public override int GetDataHash()
        {
            return base.GetDataHash() + MeshData.GetDataHash() + clothTarget.GetDataHash() + useTransformList.GetDataHash() + useTransformPositionList.GetDataHash() + useTransformRotationList.GetDataHash() + useTransformScaleList.GetDataHash();
        }

        protected override void Reset()
        {
            base.Reset();
            ResetParams();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
        }

        protected override void ClothInit()
        {
            ClothTarget.AddParentTransform();
            base.ClothInit();
        }

        protected override void ClothDispose()
        {
            ClothTarget.RemoveParentTransform();
            base.ClothDispose();
        }

        protected override void ClothActive()
        {
            base.ClothActive();
            if (CreateSingleton<MagicaPhysicsManager>.Instance.IsDelay && base.ActiveCount > 1)
            {
                ClothTarget.ResetFuturePredictionParentTransform();
            }
        }

        protected override uint UserFlag(int index)
        {
            bool flag = base.ClothData.IsFixedVertex(index);
            return 0x40000u | (flag ? 12288u : 0u) | 0x4000u | 0x20000u | 0x80000u;
        }

        protected override Transform UserTransform(int index)
        {
            return GetUseTransform(index);
        }

        protected override float3 UserTransformLocalPosition(int vindex)
        {
            int index = base.ClothData.useVertexList[vindex];
            return useTransformPositionList[index];
        }

        protected override quaternion UserTransformLocalRotation(int vindex)
        {
            int index = base.ClothData.useVertexList[vindex];
            return useTransformRotationList[index];
        }

        public override int GetDeformerCount()
        {
            return 0;
        }

        public override BaseMeshDeformer GetDeformer(int index)
        {
            return null;
        }

        protected override MeshData GetMeshData()
        {
            return MeshData;
        }

        protected override void WorkerInit()
        {
        }

        protected override void SetDeformerUseVertex(bool sw, BaseMeshDeformer deformer, int deformerIndex)
        {
        }

        public List<Transform> GetTransformList()
        {
            HashSet<Transform> hashSet = new HashSet<Transform>();
            int rootCount = clothTarget.RootCount;
            for (int i = 0; i < rootCount; i++)
            {
                Transform root = clothTarget.GetRoot(i);
                if (root != null)
                {
                    Transform[] componentsInChildren = root.GetComponentsInChildren<Transform>();
                    foreach (Transform item in componentsInChildren)
                    {
                        hashSet.Add(item);
                    }
                }
            }
            List<Transform> list = new List<Transform>();
            foreach (Transform item2 in hashSet)
            {
                list.Add(item2);
            }
            return list;
        }

        private Transform GetUseTransform(int index)
        {
            int index2 = base.ClothData.useVertexList[index];
            return useTransformList[index2];
        }

        public override int GetVersion()
        {
            return 6;
        }

        public override int GetErrorVersion()
        {
            return 3;
        }

        public override void CreateVerifyData()
        {
            base.CreateVerifyData();
            meshDataHash = MeshData.SaveDataHash;
            meshDataVersion = MeshData.SaveDataVersion;
        }

        public override Define.Error VerifyData()
        {
            Define.Error error = base.VerifyData();
            if (error != 0)
            {
                return error;
            }
            if (base.ClothData == null)
            {
                return Define.Error.ClothDataNull;
            }
            if (MeshData == null)
            {
                return Define.Error.MeshDataNull;
            }
            Define.Error error2 = MeshData.VerifyData();
            if (error2 != 0)
            {
                return error2;
            }
            if (meshDataHash != MeshData.SaveDataHash)
            {
                return Define.Error.MeshDataHashMismatch;
            }
            if (meshDataVersion != MeshData.SaveDataVersion)
            {
                return Define.Error.MeshDataVersionMismatch;
            }
            if (useTransformList.Count == 0)
            {
                return Define.Error.UseTransformCountZero;
            }
            if (UseTransformCount != MeshData.VertexCount)
            {
                return Define.Error.UseTransformCountMismatch;
            }
            foreach (Transform useTransform in useTransformList)
            {
                if (useTransform == null)
                {
                    return Define.Error.UseTransformNull;
                }
            }
            return Define.Error.None;
        }

        public override string GetInformation()
        {
            StaticStringBuilder.Clear();
            Define.Error error = VerifyData();
            switch (error)
            {
                case Define.Error.None:
                    {
                        ClothData clothData = base.ClothData;
                        StaticStringBuilder.AppendLine("Active: ", base.Status.IsActive);
                        StaticStringBuilder.AppendLine("Transform: ", MeshData.VertexCount);
                        StaticStringBuilder.AppendLine("Line: ", MeshData.LineCount);
                        StaticStringBuilder.AppendLine("Triangle: ", MeshData.TriangleCount);
                        StaticStringBuilder.AppendLine("Clamp Distance: ", clothData.ClampDistanceConstraintCount);
                        StaticStringBuilder.AppendLine("Clamp Position: ", clothParams.UseClampPositionLength ? clothData.VertexUseCount : 0);
                        StaticStringBuilder.AppendLine("Clamp Rotation: ", clothData.ClampRotationConstraintRootCount, " - ", clothData.ClampRotationConstraintDataCount);
                        StaticStringBuilder.AppendLine("Struct Distance: ", clothData.StructDistanceConstraintCount / 2);
                        StaticStringBuilder.AppendLine("Bend Distance: ", clothData.BendDistanceConstraintCount / 2);
                        StaticStringBuilder.AppendLine("Near Distance: ", clothData.NearDistanceConstraintCount / 2);
                        StaticStringBuilder.AppendLine("Restore Rotation: ", clothData.RestoreRotationConstraintCount);
                        StaticStringBuilder.AppendLine("Triangle Bend: ", clothData.TriangleBendConstraintCount);
                        StaticStringBuilder.Append("Rotation Interpolation: ");
                        if (clothData.LineRotationWorkerCount > 0)
                        {
                            StaticStringBuilder.Append("Line ");
                        }
                        if (clothData.TriangleRotationWorkerCount > 0)
                        {
                            StaticStringBuilder.Append("Triangle ");
                        }
                        StaticStringBuilder.AppendLine();
                        StaticStringBuilder.Append("Collider: ", teamData.ColliderCount);
                        break;
                    }
                case Define.Error.EmptyData:
                    StaticStringBuilder.Append(Define.GetErrorMessage(error));
                    break;
                default:
                    StaticStringBuilder.AppendLine("This bone cloth is in a state error!");
                    if (Application.isPlaying)
                    {
                        StaticStringBuilder.AppendLine("Execution stopped.");
                    }
                    else
                    {
                        StaticStringBuilder.AppendLine("Please recreate the bone cloth data.");
                    }
                    StaticStringBuilder.Append(Define.GetErrorMessage(error));
                    break;
            }
            return StaticStringBuilder.ToString();
        }

        public override void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict)
        {
            base.ReplaceBone(boneReplaceDict);
            for (int i = 0; i < useTransformList.Count; i++)
            {
                useTransformList[i] = MeshUtility.GetReplaceBone(useTransformList[i], boneReplaceDict);
            }
            clothTarget.ReplaceBone(boneReplaceDict);
        }

        public override int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
        {
            wposList = new List<Vector3>();
            wnorList = new List<Vector3>();
            wtanList = new List<Vector3>();
            foreach (Transform transform in GetTransformList())
            {
                wposList.Add(transform.position);
                wnorList.Add(transform.TransformDirection(Vector3.forward));
                Vector3 item = transform.TransformDirection(Vector3.up);
                wtanList.Add(item);
            }
            return wposList.Count;
        }

        public override List<int> GetEditorTriangleList()
        {
            List<int> result = new List<int>();
            MeshData meshData = MeshData;
            if (meshData != null && meshData.triangleList != null)
            {
                result = new List<int>(meshData.triangleList);
            }
            return result;
        }

        public override List<int> GetEditorLineList()
        {
            List<int> result = new List<int>();
            MeshData meshData = MeshData;
            if (meshData != null && meshData.lineList != null)
            {
                result = new List<int>(meshData.lineList);
            }
            return result;
        }

        public override List<int> GetSelectionList()
        {
            if (base.ClothSelection != null && MeshData != null)
            {
                return base.ClothSelection.GetSelectionData(MeshData, null);
            }
            return null;
        }

        public override List<int> GetUseList()
        {
            return null;
        }

        public override List<ShareDataObject> GetAllShareDataObject()
        {
            List<ShareDataObject> allShareDataObject = base.GetAllShareDataObject();
            allShareDataObject.Add(MeshData);
            return allShareDataObject;
        }

        public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
        {
            ShareDataObject shareDataObject = base.DuplicateShareDataObject(source);
            if (shareDataObject != null)
            {
                return shareDataObject;
            }
            if (MeshData == source)
            {
                meshData = ShareDataObject.Clone(MeshData);
                return meshData;
            }
            return null;
        }

        private void ResetParams()
        {
            clothParams.SetRadius(0.02f, 0.02f);
            clothParams.SetMass(10f, 1f, true, -0.5f, true);
            clothParams.SetGravity(true);
            clothParams.SetDrag(true, 0.01f, 0.01f);
            clothParams.SetMaxVelocity(true);
            clothParams.SetWorldInfluence(10f, 0.5f, 0.5f);
            clothParams.SetTeleport(false);
            clothParams.SetClampDistanceRatio(true, 0.7f, 1.1f);
            clothParams.SetClampPositionLength(false, 0f, 0.4f);
            clothParams.SetClampRotationAngle(false);
            clothParams.SetRestoreDistance();
            clothParams.SetRestoreRotation(false, 0.01f, 0f, 0.5f);
            clothParams.SetSpring(false);
            clothParams.SetAdjustRotation();
            clothParams.SetTriangleBend(true, 0.9f, 0.9f);
            clothParams.SetVolume(false);
            clothParams.SetCollision(false);
            clothParams.SetExternalForce(0.3f, 1f, 0.7f);
        }
    }
}
