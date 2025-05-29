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

    private static float Euclidean(Vector3Int a, Vector3Int b)
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
        HEURISTICS.Add(Heuristics.EUCLIDEAN, Euclidean);
        HEURISTICS.Add(Heuristics.OCTILE, OCTILE);
    }

    public static bool TryFindPath(Vector3Int startCoordinate, Vector3Int goalCoordinate, ISquareGrid grid, out List<Vector3Int> path, Heuristics heuristicType, out Dictionary<Vector3Int, Vector3Int?> numberOfCells, out Stopwatch time, out Stopwatch timePathLength, bool noDiagonal, PathFinderTest.NaviType nav)
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

            // Get parent of current node
            Vector3Int? parentCoord = cameFrom[coordinate];

            var connections = grid.GetCellLinks(coordinate);

            foreach (CellData neighbour in connections)
            {
                //int overrideCost = 0;
                if (neighbour.Cost < 0 || (!noDiagonal && Mathf.Abs(neighbour.Coordinate.x - coordinate.x) > 0 && Mathf.Abs(neighbour.Coordinate.y - coordinate.y) > 0))
                {
                    continue;
                }

                //switch (nav)
                //{
                //    case PathFinderTest.NaviType.Elf:
                //        {
                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                //            {
                //                continue;
                //            }
                //        }
                //        break;
                //    case PathFinderTest.NaviType.Golem:
                //        {
                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Mountain)
                //            {
                //                overrideCost = -(int)nav;
                //            }
                //            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Swamp || neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean || neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                //            {
                //                overrideCost = (int)nav + 1;
                //            }
                //        }
                //        break;
                //    case PathFinderTest.NaviType.SandGoblin:
                //        {
                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                //            {
                //                continue;
                //            }

                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Desert || neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                //            {
                //                overrideCost = -(int)nav;
                //            }
                //        }
                //        break;
                //    case PathFinderTest.NaviType.Viking:
                //        {
                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava || neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                //            {
                //                continue;
                //            }

                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.SnowPlains)
                //            {
                //                overrideCost = -(int)nav;
                //            }
                //            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Desert || neighbour.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                //            {
                //                overrideCost = (int)nav + 1;
                //            }
                //        }
                //        break;
                //    case PathFinderTest.NaviType.Ogre:
                //        {
                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                //            {
                //                continue;
                //            }

                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                //            {
                //                overrideCost = -(int)nav;
                //            }
                //            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.SnowPlains || neighbour.Cost == (int)TerrainPicker.typesTerrain.Mountain)
                //            {
                //                overrideCost = (int)nav + 1;
                //            }
                //        }
                //        break;
                //    case PathFinderTest.NaviType.LadyOfTheLake:
                //        {
                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                //            {
                //                continue;
                //            }

                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                //            {
                //                overrideCost = -(int)nav;
                //            }
                //            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                //            {
                //                overrideCost = -(int)nav + 1;
                //            }
                //            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.SnowPlains)
                //            {
                //                overrideCost = (int)nav;
                //            }
                //        }
                //        break;
                //    case PathFinderTest.NaviType.Demon:
                //        {
                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                //            {
                //                continue;
                //            }

                //            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                //            {
                //                overrideCost = -(int)nav;
                //            }
                //            else
                //            {
                //                overrideCost = 3;
                //            }

                //        }
                //        break;
                //}


                //LAZY THETA* LOS SIGHT CHECK
                if (parentCoord != null && LineOfSight(parentCoord.Value, neighbour.Coordinate, grid))
                {
                    // If line of sight exists, calculate cost from parent
                    float newCost = costSoFar[parentCoord.Value] +
                        (HEURISTICS[heuristicType](parentCoord.Value, neighbour.Coordinate) -
                         HEURISTICS[heuristicType](parentCoord.Value, coordinate)) +
                        neighbour.Cost /*+ overrideCost*/;

                    if (!costSoFar.ContainsKey(neighbour.Coordinate) || newCost < costSoFar[neighbour.Coordinate])
                    {
                        costSoFar[neighbour.Coordinate] = newCost;
                        cameFrom[neighbour.Coordinate] = parentCoord; // Set parent to grandparent
                        float priority = newCost + HEURISTICS[heuristicType](neighbour.Coordinate, goalCoordinate);
                        frontier.Enqueue(neighbour.Coordinate, priority);
                    }
                    continue; // Skip the normal processing
                }


                float calcCost = costSoFar[coordinate] + neighbour.Cost /*+ overrideCost*/;

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
                    if (skip)
                    {
                        continue;
                    }
                }

                if (!costSoFar.ContainsKey(neighbour.Coordinate) || calcCost < costSoFar[neighbour.Coordinate])
                {
                    //for (int i = 0; i < connections.Count - 4; i++)
                    //{
                    //    if (connections[i].Cost < neighbour.Cost)
                    //    {
                    //        calcCost = DiagonalCheck(coordinate, neighbour.Coordinate, calcCost);
                    //        break;
                    //    }
                    //}
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

        bool PathFound = PathProcessor.TryGetPath(cameFrom, startCoordinate, goalCoordinate, ref path, out Stopwatch pathLengthTimer);

        timePathLength = pathLengthTimer;

        return PathFound;
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

    private static bool LineOfSight(Vector3Int start, Vector3Int end, ISquareGrid grid)
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
            // Check if current cell is blocked (you might need to adjust this based on your grid implementation)
            Vector3Int current = new Vector3Int(x0, y0, 0);
            if (current != start && current != end) // Don't check start/end points
            {
                var cell = grid.GetCell(current);
                if (cell == null || cell.Cost < 0) // Assuming negative cost means blocked
                {
                    return false;
                }
            }

            if (x0 == x1 && y0 == y1) break;

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
