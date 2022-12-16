using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    [SerializeField] private int _size;
    [SerializeField] private Material _terrain;
    [SerializeField] private Material _edge;
    [Header("Noise")] 
    [SerializeField] private Level[] _levels;

    private Cell[,] _grid;
    private float _perlinScale;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        Generate();
        DrawTerrainMesh();
        DrawEdgeMesh();
        DrawTexture();
    }

    private void Generate()
    {
        _perlinScale = Random.Range(0.05f, 0.1f);
        
        float[,] noiseMap = new float[_size, _size];
        for (int y = 0; y < _size; y++)
        for (int x = 0; x < _size; x++)
            noiseMap[x, y] = Mathf.PerlinNoise(_perlinScale * y, _perlinScale * x);

        float[,] falloffMap = new float[_size, _size];
        for(int y = 0; y < _size; y++) {
            for(int x = 0; x < _size; x++) {
                float xv = x / (float)_size * 2 - 1;
                float yv = y / (float)_size * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }
        
        _grid = new Cell[_size, _size];
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                float noiseLevel = noiseMap[y, x];
                noiseLevel -= falloffMap[x, y];
                Cell cell = new Cell();
                foreach (Level level in _levels)
                {
                    if (noiseLevel < level.maxLevel)
                    {
                        cell.LandType = level.landType;
                        break;
                    }
                }

                _grid[x, y] = cell;
            }
        }
    }

    private void DrawTerrainMesh() {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for(int y = 0; y < _size; y++) {
            for(int x = 0; x < _size; x++) {
                Cell cell = _grid[x, y];
                if(cell.LandType != LandType.Water) {
                    Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                    Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                    Vector3 c = new Vector3(x - .5f, 0, y - .5f);
                    Vector3 d = new Vector3(x + .5f, 0, y - .5f);
                    Vector2 uvA = new Vector2(x / (float)_size, y / (float)_size);
                    Vector2 uvB = new Vector2((x + 1) / (float)_size, y / (float)_size);
                    Vector2 uvC = new Vector2(x / (float)_size, (y + 1) / (float)_size);
                    Vector2 uvD = new Vector2((x + 1) / (float)_size, (y + 1) / (float)_size);
                    Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                    Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
                    for(int k = 0; k < 6; k++) {
                        vertices.Add(v[k]);
                        triangles.Add(triangles.Count);
                        uvs.Add(uv[k]);
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        
        _meshFilter.mesh = mesh;
    }

    private void DrawEdgeMesh() {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for(int y = 0; y < _size; y++) {
            for(int x = 0; x < _size; x++) {
                Cell cell = _grid[x, y];
                if(cell.LandType != LandType.Water) {
                    if(x > 0) {
                        Cell left = _grid[x - 1, y];
                        if(left.LandType == LandType.Water) {
                            Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -1, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -1, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for(int k = 0; k < 6; k++) {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                        
                    }
                    if(x < _size - 1) {
                        Cell right = _grid[x + 1, y];
                        if(right.LandType == LandType.Water) {
                            Vector3 a = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -1, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -1, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for(int k = 0; k < 6; k++) {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if(y > 0) {
                        Cell down = _grid[x, y - 1];
                        if(down.LandType == LandType.Water) {
                            Vector3 a = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -1, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -1, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for(int k = 0; k < 6; k++) {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if(y < _size - 1) {
                        Cell up = _grid[x, y + 1];
                        if(up.LandType == LandType.Water) {
                            Vector3 a = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -1, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -1, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for(int k = 0; k < 6; k++) {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(transform);

        MeshFilter meshFilter = edgeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = _edge;
    }

    void DrawTexture() {
        Texture2D texture = new Texture2D(_size, _size);
        Color[] colorMap = new Color[_size * _size];
        for(int y = 0; y < _size; y++) {
            for(int x = 0; x < _size; x++) {
                Cell cell = _grid[x, y];
                switch (cell.LandType)
                {
                    case LandType.Grass:
                        colorMap[y * _size + x] = Color.green;
                        break;
                    case LandType.Rock:
                        colorMap[y * _size + x] = Color.gray;
                        break;
                    case LandType.Water:
                        colorMap[y * _size + x] = Color.blue;
                        break;
                }
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = _terrain;
        meshRenderer.material.mainTexture = texture;
    }


    [Serializable]
    private class Level
    {
        public float maxLevel;
        public LandType landType;
    }
}