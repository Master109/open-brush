#if UNITY_EDITOR
using UnityEngine;

namespace EternityEngine
{
	public class MakeMeshAsset : MakeAsset
	{
		public MeshFilter meshFilter;
        public bool useSharedMesh;

		public override void Do ()
		{
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;
            if (useSharedMesh)
                mesh = meshFilter.sharedMesh;
			_Do (mesh, assetPath);
		}
	}
}
#else
namespace EternityEngine
{
	public class MakeMeshAsset : EditorScript
	{
	}
}
#endif