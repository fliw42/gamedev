using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    [SerializeField] private Vector2Int _size;
    [Header("Noise")]
    [SerializeField] private Level[] _levels;
    [Header("Mesh")]
    [SerializeField] private GameObject _platform;
    
    private Cell[,] _grid;
    private void Start()
    {
        Generate();
        StartCoroutine(Spawn());
    }

    private void Generate()
    {
        float[,] noiseMap = new float[_size.x, _size.y];
        for (int x=0; x<_size.x; x++)
        for (int y = 0; y < _size.x; y++)
            noiseMap[x, y] = Mathf.PerlinNoise(x + Random.Range(0, 1000), y + Random.Range(0, 1000));
        
        
        _grid = new Cell[_size.x, _size.y];
        for (int x = 0; x < _size.x; x++)
        {
            for (int y = 0; y < _size.y; y++)
            {

                float noiseLevel = noiseMap[x, y];
                Cell cell = new Cell();
                foreach (Level level in _levels)
                {
                    if (noiseLevel < level.maxLevel)
                    {
                        cell.LandType = level.landtype;
                        break;
                    }
                }
                _grid[x, y] = cell;
            }
        }
    }
    
    private IEnumerator Spawn()
    {
        int count = 0;
        
        for (int x = 0; x < _size.x; x++)
        {
            for (int y = 0; y< _size.y; y++)
            {
                Vector3 pos = new Vector3(x ,0,y);
                Instantiate(_platform, pos, Quaternion.identity);
                count++;

                if (count > 100)
                {
                    count = 0;
                    yield return null;
                }
            }
        }
    }
    
    [Serializable]
    private class Level
    {
        public float maxLevel;
        public LandType landtype;
    }
}
