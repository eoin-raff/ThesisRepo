using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public Renderer TextureRenderer;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    public DrawMode DrawMode;

    public MeshSettings MeshSettings;
    public HeightMapSettings HeightMapSettings;
    public TextureData TextureData;

    public Material TerrainMaterial;

    [Range(0, MeshSettings.NumSupportedLODs - 1)]
    public int EditorPreviewLOD;
    public bool AutoUpdate;

    public void DrawMapInEditor()
    {
        TextureData.ApplyToMaterial(TerrainMaterial);
        TextureData.UpdateMeshHeights(TerrainMaterial, HeightMapSettings.MinHeight, HeightMapSettings.MaxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(MeshSettings.NumVertsPerLine, MeshSettings.NumVertsPerLine, HeightMapSettings, Vector2.zero);

        if (DrawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightmap(heightMap));
        }
        else if (DrawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.Values, MeshSettings, EditorPreviewLOD));
        }
        else if (DrawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightmap(new HeightMap( FalloffGenerator.GenerateFalloffMap(MeshSettings.NumVertsPerLine), 0, 1)));
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        TextureRenderer.sharedMaterial.mainTexture = texture;
        TextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        TextureRenderer.gameObject.SetActive(true);
        MeshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        MeshFilter.sharedMesh = meshData.CreateMesh();
        TextureRenderer.gameObject.SetActive(false);
        MeshFilter.gameObject.SetActive(true);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }
    void OnTextureValuesUpdated()
    {
        TextureData.ApplyToMaterial(TerrainMaterial);
    }

    private void OnValidate()
    {
        if (MeshSettings != null)
        {
            MeshSettings.OnValuesUpdated -= OnValuesUpdated; //if not subscribed, then this does nothing. If you are subscribed, it stops multiple calls being made
            MeshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (HeightMapSettings != null)
        {
            HeightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            HeightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (TextureData != null)
        {
            TextureData.OnValuesUpdated -= OnTextureValuesUpdated;
            TextureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}
public enum DrawMode
{
    NoiseMap,
    Mesh,
    FalloffMap
};