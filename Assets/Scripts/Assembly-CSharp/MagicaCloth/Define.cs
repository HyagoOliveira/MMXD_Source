using System.Text;

namespace MagicaCloth
{
	public static class Define
	{
		public static class Compute
		{
			public const float CollisionFrictionRange = 0.03f;

			public const float FrictionDampingRate = 0.6f;

			public const float FrictionMoveRatio = 1f;

			public const float FrictionPower = 4f;

			public const float ClampPositionMaxVelocity = 1f;

			public const float ClampRotationMaxVelocity = 1f;

			public const int BaseSkinningWeightCount = 4;

			public const float GlobalColliderMaxMoveDistance = 0.2f;

			public const float GlobalColliderMaxRotationAngle = 10f;

			public const float ColliderExtrusionDist = 0.02f;

			public const float ColliderExtrusionDirectionPower = 0.5f;

			public const float ColliderExtrusionDistPower = 2f;

			public const float ColliderExtrusionVelocityInfluence = 0.25f;
		}

		public enum Error
		{
			None = 0,
			EmptyData = 100,
			InvalidDataHash = 101,
			TooOldDataVersion = 102,
			MeshDataNull = 200,
			MeshDataHashMismatch = 201,
			MeshDataVersionMismatch = 202,
			ClothDataNull = 300,
			ClothDataHashMismatch = 301,
			ClothDataVersionMismatch = 302,
			ClothSelectionHashMismatch = 400,
			ClothSelectionVersionMismatch = 401,
			ClothTargetRootCountMismatch = 500,
			UseTransformNull = 600,
			UseTransformCountZero = 601,
			UseTransformCountMismatch = 602,
			DeformerNull = 700,
			DeformerHashMismatch = 701,
			DeformerVersionMismatch = 702,
			DeformerCountZero = 703,
			DeformerCountMismatch = 704,
			VertexCountZero = 800,
			VertexUseCountZero = 801,
			VertexCountMismatch = 802,
			RootListCountMismatch = 900,
			SelectionDataCountMismatch = 1000,
			SelectionCountZero = 1001,
			CenterTransformNull = 1100,
			SpringDataNull = 1200,
			SpringDataHashMismatch = 1201,
			SpringDataVersionMismatch = 1202,
			TargetObjectNull = 1300,
			SharedMeshNull = 1400,
			SharedMeshCannotRead = 1401,
			MeshOptimizeMismatch = 1500,
			MeshVertexCount65535Over = 1501,
			BoneListZero = 1600,
			BoneListNull = 1601,
			OverlappingTransform = 20000,
			AddOverlappingTransform = 20001,
			OldDataVersion = 20002
		}

		public static class OptimizeMesh
		{
			public const int Unknown = 0;

			public const int Nothing = 1;

			public const int Unity2018_On = 16;

			public const int Unity2019_PolygonOrder = 256;

			public const int Unity2019_VertexOrder = 512;
		}

		public static bool IsNormal(Error err)
		{
			return err == Error.None;
		}

		public static bool IsError(Error err)
		{
			if (err != 0)
			{
				return err < Error.OverlappingTransform;
			}
			return false;
		}

		public static bool IsWarning(Error err)
		{
			return err >= Error.OverlappingTransform;
		}

		public static string GetErrorMessage(Error err)
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			stringBuilder.AppendFormat("{0} ({1}) : {2}", IsError(err) ? "Error" : "Warning", (int)err, err.ToString());
			switch (err)
			{
			case Error.SharedMeshCannotRead:
				stringBuilder.AppendLine();
				stringBuilder.Append("Please turn On the [Read/Write Enabled] flag of the mesh importer.");
				break;
			case Error.OldDataVersion:
				stringBuilder.Clear();
				stringBuilder.Append("Old data format!");
				stringBuilder.AppendLine();
				stringBuilder.Append("It may not work or the latest features may not be available.");
				stringBuilder.AppendLine();
				stringBuilder.Append("It is recommended to press the [Create] button and rebuild the data.");
				break;
			case Error.EmptyData:
				stringBuilder.Clear();
				stringBuilder.Append("No Data.");
				break;
			}
			return stringBuilder.ToString();
		}
	}
}
