using UnityEngine;

public class AutomaticWireframeObjects : MonoBehaviour
{
    [Header("Gathers all mesh filters from children and makes them wireframe objects.")]

    [Header("Edges with angle below this threshold will not be rendered.")]
    [SerializeField] float edgeAngleLimit;

    [Header("Should we override existing wireframe objects from children?")]
    [SerializeField] bool overrideExistingObjects;

    [Header("Should we delete materials?")]
    [SerializeField] bool deleteMaterials;

    void Start()
    {
        if (WireframeRenderer.Instance)
        {
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(false);
            foreach (MeshFilter meshFilter in meshFilters)
            {
                WireframeObject wireFrameObject = meshFilter.gameObject.GetComponent<WireframeObject>();
                if (!wireFrameObject)
                {
                    wireFrameObject = meshFilter.gameObject.AddComponent<WireframeObject>();
                    wireFrameObject.UpdateProperties(edgeAngleLimit);
                }
                else if (overrideExistingObjects)
                {
                    wireFrameObject.UpdateProperties(edgeAngleLimit);
                }

                if (deleteMaterials)
                {
                    MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                    if (meshRenderer)
                    {
                        meshRenderer.materials = new Material[0];
                    }
                }
            }
        }
        Destroy(this);
    }
}
