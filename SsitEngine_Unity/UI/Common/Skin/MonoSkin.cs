using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Skin
{
    /// <summary>Render dynamic skinned mesh.</summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public abstract class MonoSkin : MonoBehaviour, ISkin
    {
        /// <summary>Mesh of skin.</summary>
        protected Mesh mesh;

        /// <summary>Mesh collider of skin.</summary>
        protected MeshCollider meshCollider;

        /// <summary>Skinned mesh renderer of skin.</summary>
        protected SkinnedMeshRenderer meshRenderer;

        /// <summary>Skinned mesh renderer of skin.</summary>
        public SkinnedMeshRenderer Renderer => meshRenderer;

        /// <summary>Mesh collider of skin.</summary>
        public MeshCollider Collider => meshCollider;

        /// <summary>Rebuild the mesh of skin.</summary>
        public virtual void Rebuild()
        {
            RebuildMesh(mesh);
            meshRenderer.sharedMesh = mesh;
            meshRenderer.localBounds = mesh.bounds;
            if (!(bool) meshCollider)
                return;
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        /// <summary>Attach MeshCollider to skin.</summary>
        public void AttachCollider()
        {
            if (!(GetComponent<MeshCollider>() == null))
                return;
            gameObject.AddComponent<MeshCollider>();
        }

        /// <summary>Remove MeshCollider from skin.</summary>
        public void RemoveCollider()
        {
            if (!(bool) meshCollider)
                return;
            Destroy(meshCollider);
            meshCollider = null;
        }

        /// <summary>Reset component.</summary>
        protected virtual void Reset()
        {
            Initialize();
            Rebuild();
        }

        /// <summary>Awake component.</summary>
        protected virtual void Awake()
        {
            Initialize();
            Rebuild();
        }

        /// <summary>Initialize mono skin.</summary>
        protected virtual void Initialize()
        {
            meshRenderer = GetComponent<SkinnedMeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
            if (this.mesh != null) return;
            var mesh = new Mesh();
            mesh.name = "Skin";
            this.mesh = mesh;
        }

        /// <summary>Rebuild the mesh of skin.</summary>
        /// <param name="mesh">Mesh of skin.</param>
        protected abstract void RebuildMesh( Mesh mesh );
    }
}