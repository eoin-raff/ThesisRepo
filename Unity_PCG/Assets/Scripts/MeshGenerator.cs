using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys) ;

        int meshSimplificationIncremement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2*meshSimplificationIncremement;
        int meshSizeUnsimlified = borderedSize - 2;
        float topLeftX = (meshSizeUnsimlified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimlified - 1) / 2f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncremement + 1;

        MeshData meshData = new MeshData(verticesPerLine);
        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];

        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        /*
         * Border vertices are used to accurately calculate normals at edges, but are not part of the mesh.
         * 
         * The are marked with negative indices so that they will not get added to the mesh generation
         * but can still be used for normal calculation
         */

        for (int y = 0; y < borderedSize; y += meshSimplificationIncremement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncremement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;
                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y += meshSimplificationIncremement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncremement)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 percent = new Vector2(
                   (x - meshSimplificationIncremement) / (float)meshSize,
                   (y - meshSimplificationIncremement) / (float)meshSize
                );

                float height = heightMap[x, y] * heightMultiplier * heightCurve.Evaluate(heightMap[x, y]);

                Vector3 vertexPosition = new Vector3(
                    topLeftX + percent.x * meshSizeUnsimlified,
                    height, 
                    topLeftZ - percent.y * meshSizeUnsimlified
                );

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[
                        x, 
                        y
                    ];
                    int b = vertexIndicesMap[
                        x + meshSimplificationIncremement, 
                        y
                    ];
                    int c = vertexIndicesMap[
                        x, 
                        y + meshSimplificationIncremement
                    ];
                    int d = vertexIndicesMap[
                        x + meshSimplificationIncremement,
                        y + meshSimplificationIncremement
                    ];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

                vertexIndex++;
            }
        }
        meshData.BakeNormals();

        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex;
    int borderTrianglesIndex;

    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
        //triangleIndex = 0;
    }
    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            //is border index
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            //is border triangle
            borderTriangles[borderTrianglesIndex] = a;
            borderTriangles[borderTrianglesIndex + 1] = b;
            borderTriangles[borderTrianglesIndex + 2] = c;
            borderTrianglesIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    //Maunal Normal Calculation to fix incorrect lighting at seams between meshes
    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = bakedNormals;
        return mesh;
    }
}