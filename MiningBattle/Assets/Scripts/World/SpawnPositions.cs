using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnPositions
{
    public static List<Vector2> Generate(int nbPlayers, float spawnRadius, float minAngle)
    {
        if (minAngle > 45 || minAngle < 0)
        {
            Debug.LogWarning("minAngle must between 0 and 45 degrees");
            minAngle = 45;
        }

        List<Vector2> spawnPositions = new List<Vector2>();
        List<int> freePositions = new List<int>();

        for (int i = 0; i < 360; i++)
            freePositions.Add(i);

        for (int idPlayer = 0; idPlayer < nbPlayers; idPlayer++)
        {
            int angle = freePositions[Random.Range(0, freePositions.Count - 1)];

            Vector2 position = new Vector2(
                spawnRadius * Mathf.Cos(angle * Mathf.Deg2Rad),
                spawnRadius * Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            spawnPositions.Add(position - new Vector2(0.5f, 0.5f));

            for (int idDelete = 0; idDelete < minAngle; idDelete++)
            {
                if (freePositions.Contains((angle - idDelete) % 360))
                    freePositions.Remove((angle - idDelete) % 360);
                if (freePositions.Contains((angle + idDelete) % 360))
                    freePositions.Remove((angle + idDelete) % 360);
            }
        }

        return spawnPositions;
    }
}