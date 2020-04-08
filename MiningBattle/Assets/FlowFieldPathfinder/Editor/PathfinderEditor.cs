using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FlowPathfinding
{
    [CustomEditor(typeof(Pathfinder))]
    public class PathfinderEditor : Editor
    {
        private Transform _brush;
        private readonly List<Tile> _tilesCostChanged = new List<Tile>();

        private bool _showSettings = true;
        private bool _showVisualDebuggingSettings;
        private bool _showCostDrawing;

        private readonly string[] _brushScaleNames = { "1x1", "3x3", "5x5", "7x7" };
        private readonly int[] _brushScaleValues = { 1, 3, 5, 7 };

        private readonly string[] _sectorLevelNames = { "None", "0", "1" };
        private readonly int[] _sectorLevelValues = { -1, 0, 1 };

        private GUIContent[] _maxLevelNames;
        private readonly int[] _maxLevelValues = { 0, 1, 2 };

        private readonly string[] _levelScalingNames = { "2", "3", "4" };
        private readonly int[] _levelScalingValues = { 2, 3, 4 };

        private readonly GUIContent _content = new GUIContent();

        private void Settings(Pathfinder myTarget)
        {
            if(_maxLevelNames == null)
            {
                _maxLevelNames = new GUIContent[3];
                _maxLevelNames[0] = new GUIContent();
                _maxLevelNames[1] = new GUIContent();
                _maxLevelNames[2] = new GUIContent();

                _maxLevelNames[0].text = "None";
                _maxLevelNames[1].text = "1";
                _maxLevelNames[2].text = "2";
            }


            GUI.backgroundColor = _showSettings ? Color.grey : Color.green;

            if (GUILayout.Button(" Settings "))
                _showSettings = !_showSettings;

            GUI.backgroundColor = Color.white;

            if (_showSettings)
                ShowSettings(myTarget);

        }

        private void ShowSettings(Pathfinder myTarget)
        {
            EditorGUILayout.Space();
            _content.text = "Multi Layered Structure";
            _content.tooltip = "Enable when walkable areas go over each other";
            myTarget.worldIsMultiLayered = EditorGUILayout.Toggle(_content, myTarget.worldIsMultiLayered);
            EditorGUILayout.Space();
            myTarget.worldStart = EditorGUILayout.Vector3Field("World Top-Left ", myTarget.worldStart);
            myTarget.worldWidth = EditorGUILayout.FloatField("World Width ", myTarget.worldWidth);
            myTarget.worldLength = EditorGUILayout.FloatField("World Length ", myTarget.worldLength);
            myTarget.worldHeight = EditorGUILayout.FloatField("World Height ", myTarget.worldHeight);

            _content.text = "Climb Height";
            _content.tooltip = "Defines the allowable height difference between tiles";
            myTarget.generationClimbHeight = EditorGUILayout.FloatField(_content, myTarget.generationClimbHeight);

            myTarget.tileSize = EditorGUILayout.FloatField("Tile Size ", myTarget.tileSize);

            _content.text = "Sector Size";
            _content.tooltip = "sector rectangle size in relation to tiles: 1x1, 2x2, etc, tiles big";
            myTarget.sectorSize = EditorGUILayout.IntField(_content, myTarget.sectorSize);

            if (myTarget.worldIsMultiLayered)
            {
                _content.text = "Character/Layer Height";
                _content.tooltip = "Defines the minimal Y distance between geometry, try to make it match your character plus a little extra";
                myTarget.characterHeight = EditorGUILayout.FloatField(_content, myTarget.characterHeight);
                myTarget.twoDimensionalMode = false;
            }
            else
            {
                _content.text = "2D Mode";
                _content.tooltip = "Base Seeker location on Y position instead of Z. Intended for 2D X/Y axis games only";
                myTarget.twoDimensionalMode = EditorGUILayout.Toggle(_content, myTarget.twoDimensionalMode);
            }

            _content.text = "Levels Of Abstraction";
            _content.tooltip = "Amount of abstraction layers, usually you will want to use 1";
            myTarget.maxLevelAmount = EditorGUILayout.IntPopup(_content, myTarget.maxLevelAmount, _maxLevelNames, _maxLevelValues);
            myTarget.levelScaling = EditorGUILayout.IntPopup("Abstraction Scaling", myTarget.levelScaling, _levelScalingNames, _levelScalingValues);


            EditorGUILayout.Space();
            _content.text = "Maximum Angle of Ramps ";
            _content.tooltip = "This value shows you the maximum angle geometry can have from the ground away. This is determined by the ClimbHeight and TileSize ";
            EditorGUILayout.FloatField("Maximum Angle of Ramps ", Mathf.Atan(myTarget.generationClimbHeight / myTarget.tileSize) * (180 / Mathf.PI));
            EditorGUILayout.Space();

            _content.text = "Ground Layer ";
            _content.tooltip = "Defines which layers are seen as walkable ";
            myTarget.groundLayer = EditorGUILayout.LayerField(_content, myTarget.groundLayer);

            _content.text = "Obstacle Layer ";
            _content.tooltip = "Defines which layers are seen as blocked ";
            myTarget.obstacleLayer = EditorGUILayout.LayerField(_content, myTarget.obstacleLayer);
        }




        private void VisualDebugging(Pathfinder myTarget)
        {
            GUI.backgroundColor = _showVisualDebuggingSettings ? Color.grey : Color.green;

            if (GUILayout.Button(" Visual Debugging "))
                _showVisualDebuggingSettings = !_showVisualDebuggingSettings;

            GUI.backgroundColor = Color.white;

            if (_showVisualDebuggingSettings)
                ShowVisualDebugging(myTarget);

        }

        private void ShowVisualDebugging(Pathfinder myTarget)
        {
            EditorGUILayout.Space();
            myTarget.drawTiles = EditorGUILayout.Toggle("Draw Tiles ", myTarget.drawTiles);
            myTarget.drawSectors = EditorGUILayout.Toggle("Draw Sectors ", myTarget.drawSectors);
            myTarget.drawSectorNetwork = EditorGUILayout.Toggle("Draw Network Graph ", myTarget.drawSectorNetwork);
            myTarget.drawSectorLevel = EditorGUILayout.IntPopup("Draw Level", myTarget.drawSectorLevel, _sectorLevelNames, _sectorLevelValues);
            EditorGUILayout.Space();

            myTarget.drawTree = EditorGUILayout.Toggle("Draw Data Tree  ", myTarget.drawTree);

            EditorGUILayout.Space();
            myTarget.showIntegrationField = EditorGUILayout.Toggle("Draw Integration Fields  ", myTarget.showIntegrationField);
            myTarget.showFlowField = EditorGUILayout.Toggle("Draw Flow Fields ", myTarget.showFlowField);

        }




        private void CostDrawing(Pathfinder myTarget)
        {
            GUI.backgroundColor = _showCostDrawing ? Color.grey : Color.green;

            if (GUILayout.Button(" Edit Cost "))
            {
                if (myTarget.costManager == null)
                    myTarget.costManager = myTarget.GetComponent<CostFieldManager>();

                if (_showCostDrawing) // showing now, will be closed
                {
                    myTarget.worldData.drawCost = false;
                    myTarget.costManager.RemoveCostField();
                }

                _showCostDrawing = !_showCostDrawing;
            }


            GUI.backgroundColor = Color.white;

            if (_showCostDrawing)
                ShowCostDrawing(myTarget);

        }

        private void ShowCostDrawing(Pathfinder myTarget)
        {
            EditorGUILayout.Space();
            if (myTarget.worldData.drawCost)
            {
                if (GUILayout.Button(" Close cost field "))
                {
                    myTarget.worldData.drawCost = false;
                    myTarget.costManager.RemoveCostField();
                }
            }
            else
            {
                if (GUILayout.Button(" Open/Create cost field "))
                {
                    myTarget.worldData.drawCost = true;
                    myTarget.costManager.SetupCostField();
                }
            }


            if (myTarget.worldData.drawCost)
            {
                myTarget.maxCostValue = EditorGUILayout.IntSlider("Maximum Cost Value ", myTarget.maxCostValue, 1, 100);
                myTarget.brushStrength = EditorGUILayout.IntSlider("Brush Strength ", myTarget.brushStrength, 1, 100);


                myTarget.brushSize = EditorGUILayout.IntPopup("Brush Size", myTarget.brushSize, _brushScaleNames, _brushScaleValues);
                myTarget.brushFallOff = EditorGUILayout.IntSlider("Brush Strength FallOff", myTarget.brushFallOff, 0, 10);
            }





            EditorGUILayout.Space();
            if (GUILayout.Button(" Save Cost Field "))
            {
                Debug.Log("save button pressed");
                myTarget.GetComponent<SaveLoad>().SaveLevel();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button(" Load Cost Field "))
            {
                myTarget.GenerateWorld(false, true);
            }
        }



        public override void OnInspectorGUI()
        {
            Pathfinder myTarget = (Pathfinder)target;

            Undo.RecordObject(myTarget, "modify settings on Pathfinder");

            EditorGUILayout.Space();
            Settings(myTarget);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            VisualDebugging(myTarget);


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            CostDrawing(myTarget);



            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();





            GUI.backgroundColor = Color.yellow;
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            _content.text = "Generate Map";
            _content.tooltip = "Gives you a preview of what the map will look like, and allows you to draw cost in the editor.";
            if (GUILayout.Button(_content))
            {
                myTarget.GenerateWorld(false, false);
                EditorUtility.SetDirty(target);
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            _content.text = "Remove Existing CostField";
            _content.tooltip = "Save an empty cost field over the existing one.";
            if (GUILayout.Button(_content))
            {
                myTarget.worldData.CostFields = new List<int[][]>();
                myTarget.GetComponent<SaveLoad>().SaveLevel();
            }



            //SECURITY!! only change if you understand what this changes, read documentation
            if (GUI.changed)
            {
                if (myTarget.generationClimbHeight > myTarget.characterHeight * 0.98f)
                    myTarget.generationClimbHeight = myTarget.characterHeight * 0.98f;

                //myTarget.characterClimbHeight = myTarget.generationClimbHeight;
                // if (myTarget.characterClimbHeight > myTarget.generationClimbHeight * 0.98f)
                //    myTarget.characterClimbHeight = myTarget.generationClimbHeight * 0.98f;



                //myTarget.tileSize = EditorGUILayout.FloatField("Tile Size ", myTarget.tileSize);
                // Undo.RegisterUndo(myTarget, "Tile Size");
                //Undo.RecordObject(myTarget, "Set Tile Size");
                EditorUtility.SetDirty(target);
            }

        }


        void OnSceneGUI()
        {
            Pathfinder myTarget = (Pathfinder)target;

            if (myTarget.worldData.WorldAreas.Count != 0) // no values/ nothing generated
            {
                if (myTarget.worldData.drawCost)
                {
                    if (_brush == null)
                    {
                        _brush = myTarget.transform.GetChild(1).gameObject.transform;
                        _brush.gameObject.SetActive(true);
                    }

                    _brush.localScale = new Vector3(myTarget.brushSize * myTarget.tileSize, 1, myTarget.brushSize * myTarget.tileSize);

                    Event current = Event.current;
                    int controlId = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
                    int layer = (1 << myTarget.groundLayer);

                    switch (current.type)
                    {
                        case EventType.MouseMove:
                            {
                                Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);

                                if (Physics.Raycast(ray, out var hit, 100f, layer))
                                {
                                    WorldArea area = myTarget.worldData.TileManager.GetWorldAreaAtPosition(hit.point);
                                    Tile tileHit = myTarget.worldData.TileManager.GetTileInWorldArea(area, hit.point);

                                    if (tileHit != null)
                                        _brush.position = myTarget.worldData.TileManager.GetTileWorldPosition(tileHit, area);
                                }

                                break;
                            }


                        case EventType.MouseDrag:
                            {
                                if (current.button == 0)
                                {
                                    //Ray ray = Camera.current.ScreenPointToRay(e.mousePosition);
                                    Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);

                                    if (Physics.Raycast(ray, out var hit, 100f, layer))
                                    {
                                        WorldArea area = myTarget.worldData.TileManager.GetWorldAreaAtPosition(hit.point);
                                        Tile tileHit = myTarget.worldData.TileManager.GetTileInWorldArea(area, hit.point);

                                        if (tileHit != null)
                                        {
                                            tileHit.Cost += myTarget.brushStrength;
                                            if (tileHit.Cost > myTarget.maxCostValue)
                                                tileHit.Cost = myTarget.maxCostValue;

                                            _tilesCostChanged.Clear();
                                            _tilesCostChanged.Add(tileHit);
                                            _brush.position = myTarget.worldData.TileManager.GetTileWorldPosition(tileHit, area);



                                            if (myTarget.brushSize != 1)
                                            {
                                                int brushMaxDif = (int)(myTarget.brushSize * 0.5f);

                                                for (int x = -brushMaxDif; x <= brushMaxDif; x++)
                                                {
                                                    for (int y = -brushMaxDif; y <= brushMaxDif; y++)
                                                    {
                                                        if (x != 0 || y != 0)
                                                        {
                                                            if (Physics.Raycast(_brush.position + new Vector3(x * myTarget.tileSize, 0.1f, y * myTarget.tileSize) + (Vector3.up * myTarget.generationClimbHeight), Vector3.down, out hit, myTarget.generationClimbHeight + 0.2f, layer))
                                                            {
                                                                area = myTarget.worldData.TileManager.GetWorldAreaAtPosition(hit.point);
                                                                tileHit = myTarget.worldData.TileManager.GetTileInWorldArea(area, hit.point);

                                                                if (tileHit != null)
                                                                {
                                                                    int fallOff = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) * myTarget.brushFallOff;
                                                                    if (fallOff < myTarget.brushStrength)
                                                                    {
                                                                        tileHit.Cost += myTarget.brushStrength - fallOff;
                                                                        if (tileHit.Cost > myTarget.maxCostValue)
                                                                            tileHit.Cost = myTarget.maxCostValue;

                                                                        _tilesCostChanged.Add(tileHit);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                            myTarget.costManager.EditCostFieldAlpha(_tilesCostChanged);
                                        }
                                    }
                                }
                                current.Use();
                                break;
                            }


                        case EventType.Layout:
                            HandleUtility.AddDefaultControl(controlId);
                            break;



                    }

                    if (GUI.changed)
                        EditorUtility.SetDirty(target);
                }
                else
                {
                    if (_brush != null)
                        _brush.gameObject.SetActive(false);

                    _brush = null;
                }
            }

        }


    }
}