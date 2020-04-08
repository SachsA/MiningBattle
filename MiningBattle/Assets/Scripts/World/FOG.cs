using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class FOG : MonoBehaviour {

    
    #region Public Fields

    public Tilemap blurredTileMap;
    
    public Tile blurredTile;

    
    public static FOG Instance;
    
    #endregion

    #region Private Fields

    private int[,] _blurredMap;
    
    private Vector2Int TileIndex = new Vector2Int(0, 0);
    
    #endregion
    
    
    #region Monobehaviour Callbacks

    void Awake()
    {
        Instance = this;
        _blurredMap = new int[500, 500];
        
    }
    
    void Start ()
    {
        blurredTileMap.origin = new Vector3Int(0, 0, 0);

        for (int x = -250; x < 250; x++)
        {
            for (int y = -250; y < 250; y++)
            {
                blurredTileMap.SetTile(new Vector3Int(x, y, 0), blurredTile);
            }
        }
    }
    
    #endregion
    
    #region Public Methods

    public void SeeTile(Vector2 position, Vector2Int index)
    {
        _blurredMap[index.x, index.y] += 1;
        if (_blurredMap[index.x, index.y] == 1)
            blurredTileMap.RefreshTile(Vector3Int.FloorToInt(position));
    } 
    
    public void SeeTile(Vector2 position)
    {
        Vector2Int index = World.Instance.getTileIndex(position.x, position.y);
        _blurredMap[index.x, index.y] += 1;
        if (_blurredMap[index.x, index.y] == 1)
            blurredTileMap.RefreshTile(Vector3Int.FloorToInt(position));
    }

    public void UnSeeTiles(Vector2[] positions)
    {
        foreach (Vector2 position in positions)
        {
            Vector2Int tileIndex = World.Instance.getTileIndex(position.x, position.y);
            if (_blurredMap[tileIndex.x, tileIndex.y] <= 1)
            {
                _blurredMap[tileIndex.x, tileIndex.y] = 0;
                if (blurredTileMap)
                    blurredTileMap.RefreshTile(Vector3Int.FloorToInt(position));
                continue;
            }
            _blurredMap[tileIndex.x, tileIndex.y] -= 1;
        }
    }
    
    public void SeeTiles(Vector2[] positions)
    {
        foreach (var position in positions)
        {
            Vector2Int tileIndex = World.Instance.getTileIndex(position.x, position.y);
            _blurredMap[tileIndex.x, tileIndex.y] += 1;
            if (_blurredMap[tileIndex.x, tileIndex.y] == 1)
                blurredTileMap.RefreshTile(Vector3Int.FloorToInt(position));
        }
    }

    public Vector2Int getTileIndex(float x, float y)
         {
             TileIndex.x = Mathf.FloorToInt(x) + 250;
             TileIndex.y = Mathf.FloorToInt(y) + 250;
             
             return TileIndex;
         }

    public bool HasUnit(Vector3Int position)
    {
        Vector2Int tileIndex = getTileIndex(position.x, position.y);
        return _blurredMap[tileIndex.x, tileIndex.y] > 0;
    }

    #endregion
}
