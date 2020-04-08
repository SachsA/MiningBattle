using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiniMapTile : Tile
{

    [SerializeField]
    private Sprite[] blockSprites = null;

    private Vector3 _position = new Vector3();
    
    private World.BlockType BlockType(ITilemap tilemap, Vector3 position)
    {
        return World.Instance.GetBlockType(position);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        World.BlockType blockType = BlockType(tilemap, position);
        if (!World.Instance.isTileDiscovered((Vector2Int) position))
            blockType = World.BlockType.ROCK;

            switch (blockType)
        {
            case World.BlockType.VOID:
                tileData.sprite = null;
                break;
            case World.BlockType.ROCK:
                tileData.sprite = blockSprites[1];
                break;
            case World.BlockType.IRON:
                tileData.sprite = blockSprites[2];
                break;
            case World.BlockType.COPPER:
                tileData.sprite = blockSprites[3];
                break;
            case World.BlockType.GOLD:
                tileData.sprite = blockSprites[4];
                break;
            case World.BlockType.DIAMOND:
                tileData.sprite = blockSprites[5];
                break;
            case World.BlockType.DAEGUNIUM:
                tileData.sprite = blockSprites[6];
                break;
        } 
    }

#if UNITY_EDITOR

    [MenuItem("Assets/Create/Tiles/MiniMapTile")]
    public static void CreateMiniMapTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save MiniMapTile", "New MiniMapTile", "asset", "Save MiniMapTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MiniMapTile>(), path);
    }

#endif

}
