using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MineableTile : Tile
{
    [SerializeField]
    private Sprite mineableSprite = null;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        //tilemap.RefreshTile(position);
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector3Int nPos = new Vector3Int(position.x + x, position.y + y, position.z);

                if (World.Instance.IsTileMineable(new Vector2(nPos.x, nPos.y)))
                {
                    tilemap.RefreshTile(nPos);
                }
            }
        }
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (World.Instance.IsTileMineable(new Vector2Int(position.x , position.y)))
        {
            tileData.sprite = mineableSprite;
        } else
        {
            tileData.sprite = null;
        }
    }

#if UNITY_EDITOR

    [MenuItem("Assets/Create/Tiles/MineableTile")]
    public static void CreateMineableTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save MineableTile", "New MineableTile", "asset", "Save MineableTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MineableTile>(), path);
    }

#endif

}
