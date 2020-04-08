using UnityEngine;

namespace FlowPathfinding
{
    // contains easy arrays for debugging
    public class ColorLists
    {
        #region PublicVariables

        public Color[] PathCostColors;

        #endregion

        #region PrivateMethods

        private void SetupPathColors()
        {
            Color[] colors = {
                Color.white, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta, Color.red, Color.grey,
                Color.black
            };

            int length = 450;
            PathCostColors = new Color[length];
            int step = length / (colors.Length - 1);

            for (int i = 0; i < colors.Length; i++)
                PathCostColors[i * step] = colors[i];

            for (int i = 0; i < colors.Length - 1; i++)
            {
                int min = i * step;
                int max = (i + 1) * step;
                int amount = max - min;

                Color start = PathCostColors[min];
                Color end = PathCostColors[max];

                for (int j = min; j < max; j++)
                {
                    Color one = start * (max - j);
                    Color two = end * (j - min);

                    PathCostColors[j] = (one + two) / amount;
                }
            }
        }

        #endregion

        #region PublicMethods

        // Use this for initialization
        public void Setup()
        {
            SetupPathColors();
        }

        #endregion
    }
}