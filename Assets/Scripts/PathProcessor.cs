using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathProcessor
{
    public static bool TryGetPath(Dictionary<Vector3Int, Vector3Int?> dict, Vector3Int startCoordinate, Vector3Int goalCoordinate, ref List<Vector3Int> path)
    {
        if (dict.ContainsKey(goalCoordinate))
        {
            Vector3Int fromCoordinate = goalCoordinate;
            while (fromCoordinate != startCoordinate)
            {
                path.Add(fromCoordinate);
                fromCoordinate = dict[fromCoordinate].Value;
            }
            path.Add(startCoordinate);

            path.Reverse();
            return true;
        }
        else
        {
            return false;
        }
    }

}
