using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DaeguniumTile : Tile
{
    [SerializeField] private Sprite[] oreSprites = null;
    
    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector3Int nPos = new Vector3Int(position.x + x, position.y + y, position.z);

                if (HasDaegunium(tilemap, nPos))
                {
                    tilemap.RefreshTile(nPos);
                }
            }

        }
    }
    
    private bool HasDaegunium(ITilemap tilemap, Vector3 position)
    {
        return World.Instance.GetBlockType(position) == World.BlockType.DAEGUNIUM;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        string composition = string.Empty;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                if (HasDaegunium(tilemap, new Vector3(position.x + x, position.y + y, position.z)))
                {
                    composition += 'R';
                }
                else
                {
                    composition += 'V';
                }
            }
        }

        tileData.sprite = null;

        if (composition == "RRRRRRRR")
            tileData.sprite = oreSprites[0];
    }

#if UNITY_EDITOR

    [MenuItem("Assets/Create/Tiles/DaeguniumTile")]
    public static void CreateOreTile()
    {
        string path =
            EditorUtility.SaveFilePanelInProject("Save DaeguniumTile", "New DaeguniumTile", "asset", "Save DaeguniumTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DaeguniumTile>(), path);
    }

#endif
}