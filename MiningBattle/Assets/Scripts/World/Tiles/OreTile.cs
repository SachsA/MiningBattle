using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OreTile : Tile
{
    [SerializeField] private Sprite[] oreSprites = null;
    
    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector3Int nPos = new Vector3Int(position.x + x, position.y + y, position.z);

                if (HasRock(tilemap, nPos))
                {
                    tilemap.RefreshTile(nPos);
                }
            }

        }
    }
    
    private bool HasRock(ITilemap tilemap, Vector3 position)
    {
        World.BlockType type = World.Instance.GetBlockType(position);
        return type != World.BlockType.VOID && type != World.BlockType.DAEGUNIUM;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (!World.Instance.isTileDiscovered((Vector3)position))
        {
            tileData.sprite = null;
            return;
        }
        
        string composition = string.Empty;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                if (HasRock(tilemap, new Vector3(position.x + x, position.y + y, position.z)))
                {
                    composition += 'R';
                }
                else
                {
                    composition += 'V';
                }
            }
        }

        tileData.sprite = oreSprites[0];

        if (composition == "RRRRRRRR")
            return;
        if (composition[1] == 'V' && composition[3] == 'V' && composition[4] == 'V' && composition[6] == 'v')
            tileData.sprite = oreSprites[1];
        else if (composition[1] == 'R' && composition[3] == 'V' && composition[4] == 'R' && composition[6] == 'V')
            tileData.sprite = oreSprites[2];
        else if (composition[1] == 'R' && composition[3] == 'V' && composition[4] == 'R' && composition[6] == 'R')
            tileData.sprite = oreSprites[3];
        else if (composition[1] == 'V' && composition[3] == 'V' && composition[4] == 'R' && composition[6] == 'R')
            tileData.sprite = oreSprites[4];
        else if (composition[1] == 'V' && composition[3] == 'R' && composition[4] == 'R' && composition[6] == 'R')
            tileData.sprite = oreSprites[5];
        else if (composition[1] == 'V' && composition[3] == 'R' && composition[4] == 'V' && composition[6] == 'R')
            tileData.sprite = oreSprites[6];
        else if (composition[1] == 'R' && composition[3] == 'R' && composition[4] == 'V' && composition[6] == 'R')
            tileData.sprite = oreSprites[7];
        else if (composition[1] == 'R' && composition[3] == 'R' && composition[4] == 'V' && composition[6] == 'V')
            tileData.sprite = oreSprites[8];
        else if (composition[1] == 'R' && composition[3] == 'R' && composition[4] == 'R' && composition[6] == 'V')
            tileData.sprite = oreSprites[9];
    }

#if UNITY_EDITOR

    [MenuItem("Assets/Create/Tiles/OreTile")]
    public static void CreateOreTile()
    {
        string path =
            EditorUtility.SaveFilePanelInProject("Save OreTile", "New OreTile", "asset", "Save OreTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<OreTile>(), path);
    }

#endif
}