using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BlurredTile : Tile
{

    [SerializeField]
    private Sprite blurredSprite = null;
    
    private bool IsDiscovered(Vector3Int position)
    {
        return FOG.Instance.HasUnit(position);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (IsDiscovered(position))
        {
            tileData.sprite = null;
        }
        else
        {
            tileData.sprite = blurredSprite;
        }
    }

#if UNITY_EDITOR

    [MenuItem("Assets/Create/Tiles/BlurredTile")]
    public static void CreateBlurredTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save BlurredTile", "New BlurredTile", "asset", "Save BlurredTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BlurredTile>(), path);
    }

#endif

}
