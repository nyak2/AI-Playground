using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class PathFinder_LazyTheta
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

    static PathFinder_LazyTheta()
    {
        HEURISTICS.Add(Heuristics.NONE, (a, b) => 0);
        HEURISTICS.Add(Heuristics.EUCLIDEAN, EUCLIDEAN);
        HEURISTICS.Add(Heuristics.OCTILE, OCTILE);
    }

    public static bool TryFindPath(
        Vector3Int startCoordinate,
        Vector3Int goalCoordinate,
        ISquareGrid grid,
        out List<Vector3Int> path,
        Heuristics heuristicType,
        out Dictionary<Vector3Int, Vector3Int?> numberOfCells,
        out Stopwatch time,
        bool noDiagonal,
        PathFinderTest.NaviType nav)
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

            Vector3Int? parentCoord = cameFrom[coordinate];
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

                // ===== LAZY THETA* LOS =====
                if (parentCoord != null && LineOfSight(parentCoord.Value, neighbour.Coordinate, grid, nav))
                {
                    float newCost =
                        costSoFar[parentCoord.Value] +
                        (HEURISTICS[heuristicType](parentCoord.Value, neighbour.Coordinate) -
                         HEURISTICS[heuristicType](parentCoord.Value, coordinate)) +
                        neighbour.Cost + overrideCost;

                    if (!costSoFar.ContainsKey(neighbour.Coordinate) ||
                        newCost < costSoFar[neighbour.Coordinate])
                    {
                        costSoFar[neighbour.Coordinate] = newCost;
                        cameFrom[neighbour.Coordinate] = parentCoord;

                        float priority =
                            newCost +
                            HEURISTICS[heuristicType](neighbour.Coordinate, goalCoordinate);

                        frontier.Enqueue(neighbour.Coordinate, priority);
                    }

                    continue;
                }

                // ===== NORMAL A* STEP =====
                float calcCost = costSoFar[coordinate] + neighbour.Cost + overrideCost;

                if (!costSoFar.ContainsKey(neighbour.Coordinate) || calcCost < costSoFar[neighbour.Coordinate])
                {
                    costSoFar[neighbour.Coordinate] = calcCost;
                    cameFrom[neighbour.Coordinate] = coordinate;

                    float priority =
                        calcCost +
                        HEURISTICS[heuristicType](neighbour.Coordinate, goalCoordinate);

                    frontier.Enqueue(neighbour.Coordinate, priority);
                }
            }
        }

        time = watch;
        path = new List<Vector3Int>();
        numberOfCells = cameFrom;

        return PathProcessor.TryGetPath(
            cameFrom,
            startCoordinate,
            goalCoordinate,
            ref path);
    }


    private static bool IsTraversable(
        CellData cell,
        PathFinderTest.NaviType nav,
        out int overrideCost)
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

    // ==============================
    // TERRAIN-AWARE LINE OF SIGHT
    // ==============================
    private static bool LineOfSight(
        Vector3Int start,
        Vector3Int end,
        ISquareGrid grid,
        PathFinderTest.NaviType nav)
    {
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            Vector3Int current = new Vector3Int(x0, y0, 0);

            if (current != start && current != end)
            {
                var cell = grid.GetCell(current);

                if (cell == null)
                    return false;

                int _;
                if (!IsTraversable(cell, nav, out _))
                    return false;
            }

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return true;
    }
}