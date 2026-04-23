using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
public static class PathFinder_AStar
{
    public enum Heuristics
    {
        NONE = 0,
        EUCLIDEAN = 1,
        OCTILE = 2
    }

    private static readonly Dictionary<Heuristics, Func<Vector3Int, Vector3Int, float>> HEURISTICS = new();

    private static float EUCLIDEAN(Vector3Int a, Vector3Int b)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static float OCTILE(Vector3Int a, Vector3Int b)
    {
        float dx = Mathf.Abs(a.x - b.x);
        float dy = Mathf.Abs(a.y - b.y);

        return (dx + dy) + ((Mathf.Sqrt(2.0f) - 2.0f) * Mathf.Min(dx, dy));
    }

    static PathFinder_AStar()
    {
        HEURISTICS.Add(Heuristics.NONE, (a, b) => 0);
        HEURISTICS.Add(Heuristics.EUCLIDEAN, EUCLIDEAN);
        HEURISTICS.Add(Heuristics.OCTILE, OCTILE);
    }

    public static bool TryFindPath(Vector3Int startCoordinate, Vector3Int goalCoordinate, ISquareGrid grid, out List<Vector3Int> path, Heuristics heuristicType, out Dictionary<Vector3Int, Vector3Int?> numberOfCells, out Stopwatch time, bool noDiagonal, PathFinderTest.NaviType nav)
    {
        PriorityQueue<Vector3Int> frontier = new PriorityQueue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int?> cameFrom = new Dictionary<Vector3Int, Vector3Int?>();
        Dictionary<Vector3Int, float> costSoFar = new Dictionary<Vector3Int, float>();

        frontier.Enqueue(startCoordinate, 0);

        cameFrom[startCoordinate] = null;
        costSoFar[startCoordinate] = 0;

        var watch = Stopwatch.StartNew();
        while (frontier.Count > 0)
        {
            Vector3Int coordinate = frontier.Dequeue();

            if (coordinate == goalCoordinate)
            {
                watch.Stop();
                break;
            }

            var connections = grid.GetCellLinks(coordinate);

            foreach (CellData neighbour in connections)
            {
                // Diagonal restriction
                if (neighbour.Cost < 0 ||
                    (!noDiagonal &&
                     Mathf.Abs(neighbour.Coordinate.x - coordinate.x) > 0 &&
                     Mathf.Abs(neighbour.Coordinate.y - coordinate.y) > 0))
                {
                    continue;
                }

                int overrideCost;
                if (!IsTraversable(neighbour, nav, out overrideCost))
                    continue;

                float calcCost = costSoFar[coordinate] + neighbour.Cost + overrideCost;

                if (Mathf.Abs(neighbour.Coordinate.x - coordinate.x) > 0 && Mathf.Abs(neighbour.Coordinate.y - coordinate.y) > 0)
                {
                    var connect = grid.GetCellLinks(neighbour.Coordinate);
                    bool skip = false;

                    for (int i = 0; i < connect.Count; i++)
                    {
                        if (connect[i].Cost < neighbour.Cost)
                        {
                            skip = true;
                            break;
                        }

                    }
                    if(skip)
                    {
                        continue;
                    }
                }

                if (!costSoFar.ContainsKey(neighbour.Coordinate) || calcCost < costSoFar[neighbour.Coordinate])
                {
                    for (int i = 0; i < connections.Count -4; i++)
                    {
                        if (connections[i].Cost < neighbour.Cost)
                        {
                            calcCost = DiagonalCheck(coordinate, neighbour.Coordinate, calcCost);
                            break;
                        }
                    }
                    costSoFar[neighbour.Coordinate] = calcCost;
                    cameFrom[neighbour.Coordinate] = coordinate;
                    float priority = calcCost + (HEURISTICS[heuristicType](neighbour.Coordinate, goalCoordinate));

                    frontier.Enqueue(neighbour.Coordinate, priority);
                }

            }

        }

        time = watch;

        path = new List<Vector3Int>();
        numberOfCells = cameFrom;
        return PathProcessor.TryGetPath(cameFrom, startCoordinate, goalCoordinate, ref path);
    }

    private static bool IsTraversable(CellData cell,PathFinderTest.NaviType nav, out int overrideCost)
    {
        overrideCost = 0;

        if (cell.Cost < 0)
            return false;

        switch (nav)
        {
            case PathFinderTest.NaviType.Elf:
                if (cell.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    return false;
                break;

            case PathFinderTest.NaviType.Golem:
                if (cell.Cost == (int)TerrainPicker.typesTerrain.Mountain)
                    overrideCost = -(int)nav;
                else if (cell.Cost == (int)TerrainPicker.typesTerrain.Swamp ||
                         cell.Cost == (int)TerrainPicker.typesTerrain.Ocean ||
                         cell.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    overrideCost = (int)nav + 1;
                break;

            case PathFinderTest.NaviType.SandGoblin:
                if (cell.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    return false;

                if (cell.Cost == (int)TerrainPicker.typesTerrain.Desert ||
                    cell.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                    overrideCost = -(int)nav;
                break;

            case PathFinderTest.NaviType.Viking:
                if (cell.Cost == (int)TerrainPicker.typesTerrain.Lava ||
                    cell.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                    return false;

                if (cell.Cost == (int)TerrainPicker.typesTerrain.SnowPlains)
                    overrideCost = -(int)nav;
                else if (cell.Cost == (int)TerrainPicker.typesTerrain.Desert ||
                         cell.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                    overrideCost = (int)nav + 1;
                break;

            case PathFinderTest.NaviType.Ogre:
                if (cell.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    return false;

                if (cell.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                    overrideCost = -(int)nav;
                else if (cell.Cost == (int)TerrainPicker.typesTerrain.SnowPlains ||
                         cell.Cost == (int)TerrainPicker.typesTerrain.Mountain)
                    overrideCost = (int)nav + 1;
                break;

            case PathFinderTest.NaviType.LadyOfTheLake:
                if (cell.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    return false;

                if (cell.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                    overrideCost = -(int)nav;
                else if (cell.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                    overrideCost = -(int)nav + 1;
                else if (cell.Cost == (int)TerrainPicker.typesTerrain.SnowPlains)
                    overrideCost = (int)nav;
                break;

            case PathFinderTest.NaviType.Demon:
                if (cell.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                    return false;

                if (cell.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    overrideCost = -(int)nav;
                else
                    overrideCost = 3;
                break;
        }

        return true;
    }

    private static float DiagonalCheck(Vector3Int currentCoordinate, Vector3Int endCoordinate, float prev_Cost)
    {
        float nudge = 0.0f;

        if (Mathf.Abs(endCoordinate.x - currentCoordinate.x) > 0 && Mathf.Abs(endCoordinate.y - currentCoordinate.y) > 0)
        {
            nudge = 1.0f;
        }

        return prev_Cost + nudge;

    }


}
