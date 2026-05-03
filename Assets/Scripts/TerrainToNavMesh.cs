using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class TerrainToNavMesh : MonoBehaviour
{
    public Terrain terrain;
    public int targetTextureIndex = 2; // 道テクスチャ
    public float threshold = 0.5f;
    public GameObject tilePrefab; // Quadなど
    public NavMeshSurface surface;

    void Start()
    {
    }


#if UNITY_EDITOR
    [ContextMenu("NavMeshを消す")]
    void DeleteNavMeshTiles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (child.name == "CPU_NavMesh(Clone)")
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    [ContextMenu("NavMeshを整える")]
    void GenerateWalkableTiles()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "CPU_NavMesh(Clone)")
            {
                DestroyImmediate(child.gameObject);
            }
        }
        TerrainData data = terrain.terrainData;

        int width = data.alphamapWidth;
        int height = data.alphamapHeight;

        float[,,] alphaMaps = data.GetAlphamaps(0, 0, width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = alphaMaps[y, x, targetTextureIndex];

                if (value > threshold)
                {
                    Vector3 pos = new Vector3(
                        (float)x / width * data.size.x,
                        0,
                        (float)y / height * data.size.z
                    );

                    pos += terrain.transform.position;

                    pos.y = terrain.SampleHeight(pos) + 1f;

                    Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                }
            }
        }
        surface.UpdateNavMesh(surface.navMeshData);
    }
#endif
}