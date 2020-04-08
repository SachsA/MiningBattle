using UnityEngine;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class CostFieldManager : MonoBehaviour
    {
        #region PrivateVariables

        private static readonly int PathMask = Shader.PropertyToID("_PathMask");
        private static readonly int PathTex = Shader.PropertyToID("_PathTex");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int ColorProp = Shader.PropertyToID("_Color");

        private readonly List<GameObject> _costFieldsGameObjects = new List<GameObject>();

        private Material _myMaterial;
        
        private Texture2D _texture;

        #endregion

        #region PublicVariables

        public Pathfinder pathfinder;

        #endregion

        #region PrivateMethods

        private void SetupCostFieldAlpha(GameObject costField, WorldArea area)
        {
            _myMaterial = new Material(Shader.Find("Custom/CostShader"));
            _myMaterial.SetColor(ColorProp, Color.white);
            _myMaterial.SetTexture(MainTex, Resources.Load("Textures/Black") as Texture);
            _myMaterial.SetTexture(PathTex, Resources.Load("Textures/Green") as Texture);

            _texture = new Texture2D(area.GridWidth, area.GridLength, TextureFormat.ARGB32, true)
            {
                wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point
            };

            int y = 0;
            while (y < _texture.height)
            {
                int x = 0;
                while (x < _texture.width)
                {
                    Color color;
                    if (area.TileGrid[x][y] != null && pathfinder.worldData.CostFields[area.Index][x][y] != 1)
                        color = new Color(0, 0, 0, pathfinder.worldData.CostFields[area.Index][x][y] * 0.01f);
                    else
                        color = new Color(0, 0, 0, -1);

                    _texture.SetPixel(x, _texture.height - 1 - y, color);
                    x++;
                }

                y++;
            }

            _texture.Apply();

            _myMaterial.SetTexture(PathMask, _texture);
            costField.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = _myMaterial;

            Resources.UnloadUnusedAssets();
        }

        #endregion

        #region PublicMethods

                public void SetupCostField()
        {
            if (pathfinder.worldData.WorldGenerated)
            {
                _costFieldsGameObjects.Clear();
                int count = pathfinder.transform.GetChild(0).childCount;

                bool createNewCostFields = pathfinder.worldData.CostFields.Count == 0;

                for (int i = 0; i < pathfinder.worldData.WorldAreas.Count; i++)
                {
                    if (createNewCostFields)
                    {
                        int[][] costFieldValues = new int[pathfinder.worldData.WorldAreas[i].GridWidth][];
                        for (int j = 0; j < pathfinder.worldData.WorldAreas[i].GridWidth; j++)
                            costFieldValues[j] = new int[pathfinder.worldData.WorldAreas[i].GridLength];

                        pathfinder.worldData.CostFields.Add(costFieldValues);
                    }

                    // visuals
                    GameObject costField;
                    if (i < count) // re-use earlier made cost field
                    {
                        costField = pathfinder.transform.GetChild(0).GetChild(i).gameObject;
                        SetupCostFieldAlpha(costField, pathfinder.worldData.WorldAreas[i]);
                        costField.transform.localScale = new Vector3(
                            pathfinder.worldData.WorldAreas[i].GridWidth * pathfinder.worldData.Pathfinder.tileSize, 1,
                            pathfinder.worldData.WorldAreas[i].GridLength * pathfinder.worldData.Pathfinder.tileSize);

                        if (pathfinder.worldData.WorldAreas[i].AngleDirectionX)
                            costField.transform.rotation = Quaternion.Euler(0, 0,
                                pathfinder.worldData.WorldAreas[i].Angle *
                                pathfinder.worldData.WorldAreas[i].AnglePositive);
                        else
                            costField.transform.rotation = Quaternion.Euler(
                                pathfinder.worldData.WorldAreas[i].Angle *
                                pathfinder.worldData.WorldAreas[i].AnglePositive, 0, 0);

                        var position = pathfinder.worldData.WorldAreas[i].Origin + new Vector3(
                                               (pathfinder.worldData.WorldAreas[i].GridWidth - 1) *
                                               pathfinder.worldData.Pathfinder.tileSize * 0.5f, 0,
                                               -(pathfinder.worldData.WorldAreas[i].GridLength - 1) *
                                               pathfinder.worldData.Pathfinder.tileSize * 0.5f);
                        position += Vector3.up * 0.15f; //costField.transform.up * 0.25f;
                        costField.transform.position = position;


                        costField.SetActive(true);
                        _costFieldsGameObjects.Add(costField);
                    }
                    else // create new cost field
                    {
                        costField = Instantiate(Resources.Load("Prefab/CostFieldVisualParent"),
                            pathfinder.worldData.WorldAreas[i].Origin + new Vector3(
                                pathfinder.worldData.WorldAreas[i].GridWidth *
                                pathfinder.worldData.Pathfinder.tileSize * 0.5f, 0,
                                -pathfinder.worldData.WorldAreas[i].GridLength *
                                pathfinder.worldData.Pathfinder.tileSize * 0.5f), Quaternion.identity) as GameObject;
                        SetupCostFieldAlpha(costField, pathfinder.worldData.WorldAreas[i]);
                        if (costField != null)
                        {
                            costField.transform.localScale = new Vector3(
                                pathfinder.worldData.WorldAreas[i].GridWidth * pathfinder.worldData.Pathfinder.tileSize,
                                1,
                                pathfinder.worldData.WorldAreas[i].GridLength *
                                pathfinder.worldData.Pathfinder.tileSize);
                            costField.transform.parent = transform.GetChild(0);


                            if (pathfinder.worldData.WorldAreas[i].AngleDirectionX)
                                costField.transform.rotation = Quaternion.Euler(0, 0,
                                    pathfinder.worldData.WorldAreas[i].Angle *
                                    pathfinder.worldData.WorldAreas[i].AnglePositive);
                            else
                                costField.transform.rotation = Quaternion.Euler(
                                    pathfinder.worldData.WorldAreas[i].Angle *
                                    pathfinder.worldData.WorldAreas[i].AnglePositive, 0, 0);

                            costField.transform.position += Vector3.up * 0.15f; // costField.transform.up * 0.25f;
                            _costFieldsGameObjects.Add(costField);
                        }
                    }
                }
            }
        }

        public void RemoveCostField()
        {
            if (pathfinder.worldData.WorldGenerated)
            {
                foreach (GameObject field in _costFieldsGameObjects)
                    if (field != null)
                        field.SetActive(false);
            }
        }
        
        public void EditCostFieldAlpha(List<Tile> tilesChanged)
        {
            if (pathfinder.worldData.CostFields.Count > 0)
            {
                WorldArea area;
                List<int> areaIndexes = new List<int>();
                foreach (Tile tileHit in tilesChanged)
                {
                    area = pathfinder.worldData.WorldAreas[tileHit.WorldAreaIndex];

                    if (tileHit.Cost == 1)
                        pathfinder.worldData.CostFields[area.Index][tileHit.GridPos.X][tileHit.GridPos.Y] = 0;
                    else
                        pathfinder.worldData.CostFields[area.Index][tileHit.GridPos.X][tileHit.GridPos.Y] =
                            tileHit.Cost;

                    if (!areaIndexes.Contains(tileHit.WorldAreaIndex))
                        areaIndexes.Add(tileHit.WorldAreaIndex);
                }

                foreach (int index in areaIndexes)
                {
                    var costField = _costFieldsGameObjects[index];
                    area = pathfinder.worldData.WorldAreas[index];

                    _myMaterial = new Material(Shader.Find("Custom/CostShader"));
                    _myMaterial.SetColor(ColorProp, Color.white);
                    _myMaterial.SetTexture(MainTex, Resources.Load("Textures/Black") as Texture);
                    _myMaterial.SetTexture(PathTex, Resources.Load("Textures/Green") as Texture);

                    _texture = new Texture2D(area.GridWidth, area.GridLength, TextureFormat.ARGB32, true)
                    {
                        wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point
                    };

                    int y = 0;
                    while (y < _texture.height)
                    {
                        int x = 0;
                        while (x < _texture.width)
                        {
                            Color color;
                            if (area.TileGrid[x][y] != null && pathfinder.worldData.CostFields[area.Index][x][y] != 0)
                                color = new Color(0, 0, 0, pathfinder.worldData.CostFields[area.Index][x][y] * 0.01f);
                            else
                                color = new Color(0, 0, 0, -1);

                            _texture.SetPixel(x, _texture.height - 1 - y, color);
                            x++;
                        }

                        y++;
                    }

                    _texture.Apply();

                    _myMaterial.SetTexture(PathMask, _texture);
                    //Debug.Log("DONE");
                    costField.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = _myMaterial;

                    Resources.UnloadUnusedAssets();
                }
            }
        }

        #endregion        
    }
}