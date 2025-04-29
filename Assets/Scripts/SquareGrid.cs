using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public interface ISquareGrid
{
    public CellData GetCell(Vector3Int coordinate);
    public List<CellData> GetCellLinks(Vector3Int coordinate);
}
public class SquareGrid : ISquareGrid
{
    public static readonly List<Vector3Int> DIRECTION = new()
    {
        new Vector3Int(1,0,0), // Right
        new Vector3Int (0,-1,0), // Bottom
        new Vector3Int(-1,0,0), // Left
        new Vector3Int(0,1,0), //Up

        new Vector3Int(1,1,0), // top right
        new Vector3Int(1,-1,0), // bottom right
        new Vector3Int(-1,1,0), // top left
        new Vector3Int(-1,-1,0), // bottom left
    };

    private Dictionary<Vector3Int, CellData> cellStorage;
    private Dictionary<Vector3Int, List<CellData>> cellLinks;
    public Dictionary<Vector3Int, CellData> Cells
    {
        get => cellStorage;
    }
    public SquareGrid(List<int> mapData, int columnsPerRow, bool flipRow)
    {
        Generate(mapData, columnsPerRow, flipRow);
        Link();
    }

    private void Generate(List<int> mapData, int columnsPerRow, bool flipRow)
    {
        cellStorage = new();

        int rows = mapData.Count / columnsPerRow;
        int rows_0b = rows - 1;

        for (int i = rows_0b; i >= 0; i--)
        {
            int revRow = flipRow ? rows_0b - i : i;
            for (int j = 0; j < columnsPerRow; j++)
            {
                Vector3Int coord = new Vector3Int(j, revRow, 0);
                int cost = mapData[(i * rows) + j];
                CellData c = new CellData(coord, cost);
                cellStorage[coord] = c;
            }
        }
    }


    private void Link()
    {
        cellLinks = new();

        foreach (var cell in cellStorage)
        {
            List<CellData> cd = new();
            Vector3Int cellCoord = cell.Value.Coordinate;

            for (int i = 0; i < DIRECTION.Count; i++)
            {
                Vector3Int neighbourCoord = cellCoord + DIRECTION[i];
                cellStorage.TryGetValue(neighbourCoord, out CellData ncell);
                if (ncell != null)
                {
                    cd.Add(ncell);
                }
            }

            cellLinks.Add(cellCoord, cd);
                cell.Value.UpdateNeighbours(cd);
        }
    }

    public bool TryGetCellData(Vector3Int coord, out ICellData cellData)
    {
        cellStorage.TryGetValue(coord, out CellData cell);
        if (cell != null)
        {
            cellData = cell;
            return true;
        }
        else
        {
            cellData = null;
            return false;
        }

    }

    public bool TryChangeCellData(Vector3Int coord, int cost)
    {
        cellStorage.TryGetValue(coord, out CellData cell);
        if (cell != null)
        {
            cell.UpdateCost(cost);
            return true;
        }
        else
        {
            return false;
        }
    }

    public CellData GetCell(Vector3Int coordinate)
    {
        cellStorage.TryGetValue(coordinate, out CellData cell);
        return cell;
    }

    public List<CellData> GetCellLinks(Vector3Int coordinate)
    {
        cellLinks.TryGetValue(coordinate, out List<CellData> list);
        return list;
    }
}

