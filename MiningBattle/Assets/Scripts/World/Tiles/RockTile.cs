using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RockTile : Tile
{

    [SerializeField]
    private Sprite[] rockSprites = null;

    private Vector3 _position = new Vector3();

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
        string composition = string.Empty;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                _position.x = position.x + x;
                _position.y = position.y + y;
                _position.z = position.z;
                if (HasRock(tilemap, _position))
                {
                    composition += 'R';
                }
                else
                {
                    composition += 'V';
                }
            }
        }

        tileData.sprite = rockSprites[0];

        if (composition == "RRRRRRRR")
            return;
        else if (composition == "VVVVVVVV")
            tileData.sprite = rockSprites[5];
        if (composition[1] == 'V' && composition[4] == 'V' && composition[6] == 'R' && composition[3] == 'R')
            tileData.sprite = rockSprites[1];
        else if (composition[1] == 'R' && composition[4] == 'V' && composition[6] == 'R' && composition[3] == 'R')
            tileData.sprite = rockSprites[2];
        else if (composition[1] == 'R' && composition[4] == 'V' && composition[6] == 'V' && composition[3] == 'R')
            tileData.sprite = rockSprites[3];
        else if (composition[1] == 'R' && composition[4] == 'R' && composition[6] == 'V' && composition[3] == 'R')
            tileData.sprite = rockSprites[4];
        else if (composition[1] == 'R' && composition[4] == 'R' && composition[6] == 'V' && composition[3] == 'V')
            tileData.sprite = rockSprites[5];
        else if (composition[1] == 'R' && composition[4] == 'R' && composition[6] == 'R' && composition[3] == 'V')
            tileData.sprite = rockSprites[6];
        else if (composition[1] == 'V' && composition[4] == 'R' && composition[6] == 'R' && composition[3] == 'V')
            tileData.sprite = rockSprites[7];
        else if (composition[1] == 'V' && composition[3] == 'R' && composition[6] == 'R' && composition[3] == 'R')
            tileData.sprite = rockSprites[8];
        else if (composition[1] == 'R' && composition[3] == 'R' && composition[6] == 'R' && composition[3] == 'R' && composition[2] == 'V')
            tileData.sprite = rockSprites[9];
        else if (composition[1] == 'R' && composition[3] == 'R' && composition[6] == 'R' && composition[3] == 'R' && composition[7] == 'V')
            tileData.sprite = rockSprites[10];
        else if (composition[1] == 'R' && composition[3] == 'R' && composition[6] == 'R' && composition[3] == 'R' && composition[5] == 'V')
            tileData.sprite = rockSprites[11];
        else if (composition[1] == 'R' && composition[3] == 'R' && composition[6] == 'R' && composition[3] == 'R' && composition[0] == 'V')
            tileData.sprite = rockSprites[12];
        else
            tileData.sprite = rockSprites[13];
    }

#if UNITY_EDITOR

    [MenuItem("Assets/Create/Tiles/RockTile")]
    public static void CreateRockTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save RockTile", "New RockTile", "asset", "Save RockTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RockTile>(), path);
    }

#endif

}
