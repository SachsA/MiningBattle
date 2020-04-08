#if UNITY_4 || UNITY_3 || UNITY_2 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define PRE_UNITY_5_3
#endif

using UnityEngine;
using System.IO;
#if !PRE_UNITY_5_3
using UnityEngine.SceneManagement;

#endif

namespace FlowPathfinding
{
    public class SaveLoad : MonoBehaviour
    {
        #region PublicVariables

        public Pathfinder pathfinder;

        #endregion

        #region PublicMethods

        public void LoadLevel()
        {
            if (File.Exists(GetLevelPath()))
            {
                byte[] levelBytes = File.ReadAllBytes(GetLevelPath());
                if (pathfinder.worldData != null)
                    pathfinder.worldData = WorldData.Load(levelBytes);
            }
        }

        public void LoadOnlyCostField()
        {
            if (File.Exists(GetLevelPath()))
            {
                byte[] levelBytes = File.ReadAllBytes(GetLevelPath());
                if (pathfinder.worldData != null)
                    pathfinder.worldData.CostFields = WorldData.Load(levelBytes).CostFields;
            }
            else
                Debug.Log("Load new level");
        }

        public void SaveLevel()
        {
            Debug.Log("Save");
            if (pathfinder != null)
                File.WriteAllBytes(GetLevelPath(), pathfinder.worldData.Save());
        }

        #endregion

        #region PrivateMethods

        private static string GetLevelPath()
        {
            string name;

#if !PRE_UNITY_5_3
            name = SceneManager.GetActiveScene().name + "Level.bytes";
#endif

#if PRE_UNITY_5_3
            name = Application.loadedLevelName + "Level.bytes";
#endif

            if (!Directory.Exists(Application.dataPath + "FlowFieldPathfinder/SaveFolder/Levels"))
                Directory.CreateDirectory(Application.dataPath + "FlowFieldPathfinder/SaveFolder/Levels");
            return Application.dataPath + "FlowFieldPathfinder/SaveFolder/Levels/" + name;
        }

        #endregion
    }
}