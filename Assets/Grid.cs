using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private Vector2Int _size;
    [SerializeField] private Level[] _levels;
    [SerializeField] private float _perlinNoiseScale = 0.1f;
    private Cell[,] _grid;

    private void Start()
    {
        float[,] noiseMap = new float[_size.x, _size.y];
        for (int x=0; x<_size.x; x++)
        for (int y = 0; y < _size.x; y++)
            noiseMap[x, y] = Mathf.PerlinNoise(x * _perlinNoiseScale, y * _perlinNoiseScale);
        
        
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        for (int x = 0; x < _size.x; x++)
        {
            for (int y = 0; y< _size.y; y++)
            {
                Cell cell = _grid[x, y];
                switch (cell.LandType)
                {
                    case LandType.Grass:
                        Gizmos.color = Color.green;
                        break;
                    case LandType.Water:
                        Gizmos.color = Color.blue;
                        break;
                    case LandType.Rock:
                        Gizmos.color = Color.grey;
                        break;
                }
                Vector3 pos = new Vector3(x ,0,y);
                Gizmos.DrawCube(pos, Vector3.one);
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
