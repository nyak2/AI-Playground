using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class PathFinder_Ucs
{

    public static bool TryFindPath(Vector3Int startCoordinate, Vector3Int goalCoordinate, ISquareGrid grid, out List<Vector3Int> path, out Dictionary<Vector3Int, Vector3Int?> numberOfCells, out Stopwatch time, bool noDiagonal, PathFinderTest.NaviType nav)
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
                int overrideCost = 0;
                if (neighbour.Cost < 0 || (!noDiagonal && Mathf.Abs(neighbour.Coordinate.x - coordinate.x) > 0 && Mathf.Abs(neighbour.Coordinate.y - coordinate.y) > 0))
                {
                    continue;
                }

                switch (nav)
                {
                    case PathFinderTest.NaviType.Elf:
                        {
                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                            {
                                continue;
                            }
                        }
                        break;
                    case PathFinderTest.NaviType.Golem:
                        {
                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Mountain)
                            {
                                overrideCost = -(int)nav;
                            }
                            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Swamp || neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean || neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                            {
                                overrideCost = (int)nav + 1;
                            }
                        }
                        break;
                    case PathFinderTest.NaviType.SandGoblin:
                        {
                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                            {
                                continue;
                            }

                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Desert || neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                            {
                                overrideCost = -(int)nav;
                            }
                        }
                        break;
                    case PathFinderTest.NaviType.Viking:
                        {
                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava || neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                            {
                                continue;
                            }

                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.SnowPlains)
                            {
                                overrideCost = -(int)nav;
                            }
                            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Desert || neighbour.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                            {
                                overrideCost = (int)nav + 1;
                            }
                        }
                        break;
                    case PathFinderTest.NaviType.Ogre:
                        {
                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                            {
                                continue;
                            }

                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                            {
                                overrideCost = -(int)nav;
                            }
                            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.SnowPlains || neighbour.Cost == (int)TerrainPicker.typesTerrain.Mountain)
                            {
                                overrideCost = (int)nav + 1;
                            }
                        }
                        break;
                    case PathFinderTest.NaviType.LadyOfTheLake:
                        {
                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                            {
                                continue;
                            }

                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                            {
                                overrideCost = -(int)nav;
                            }
                            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Swamp)
                            {
                                overrideCost = -(int)nav + 1;
                            }
                            else if (neighbour.Cost == (int)TerrainPicker.typesTerrain.SnowPlains)
                            {
                                overrideCost = (int)nav;
                            }
                        }
                        break;
                    case PathFinderTest.NaviType.Demon:
                        {
                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                            {
                                continue;
                            }

                            if (neighbour.Cost == (int)TerrainPicker.typesTerrain.Lava)
                            {
                                overrideCost = -(int)nav;
                            }
                            else
                            {
                                overrideCost = 3;
                            }

                        }
                        break;
                }
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
                    if (skip)
                    {
                        continue;
                    }
                }

                if (!costSoFar.ContainsKey(neighbour.Coordinate) || calcCost < costSoFar[neighbour.Coordinate])
                {
                    for (int i = 0; i < connections.Count - 4; i++)
                    {
                        if (connections[i].Cost < neighbour.Cost)
                        {
                            calcCost = DiagonalCheck(coordinate, neighbour.Coordinate, calcCost);
                            break;
                        }
                    }
                    costSoFar[neighbour.Coordinate] = calcCost;
                    cameFrom[neighbour.Coordinate] = coordinate;
                    float priority = calcCost;

                    frontier.Enqueue(neighbour.Coordinate, priority);
                }
            }
        }

        time = watch;

        path = new List<Vector3Int>();
        numberOfCells = cameFrom;
        return true;//PathProcessor.TryGetPath(cameFrom, startCoordinate, goalCoordinate, ref path); 

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
