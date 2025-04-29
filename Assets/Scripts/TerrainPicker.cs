using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainPicker : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] SquareGridController squareGridController;
    [SerializeField] private TextMeshProUGUI terrainText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI paintingModeText;
    [SerializeField] private Image backImage;
    [SerializeField] private GameObject seeker;
    [SerializeField] private List<Color> bacKImageColors;
    private List<CellData> cellList = new List<CellData>();

    private Stack<List<int>> changesStackCost = new Stack<List<int>>();
    private Stack<List<Vector3Int>> changesStackCoord = new Stack<List<Vector3Int>>();
    private List<Vector3Int> brushtempCoord = new List<Vector3Int>();
    private List<int> brushtempCost = new List<int>();
    bool pushed;
    bool painting;
    public enum typesTerrain
    {
        Barrier = -1,
        GrassLands = 1,
        Mountain = 2,
        Desert = 3,
        SnowPlains = 4,
        Swamp = 5,
        Ocean = 6,
        Lava = 7,
    }

    private enum paintingStyle
    {
        DotStyle,
        BrushStyle,
        FillStyle
    }

    private typesTerrain terrainType;
    private paintingStyle paintstyle;

    private void Start()
    {
        terrainType = typesTerrain.GrassLands;
        backImage.color = bacKImageColors[1];
        paintstyle = paintingStyle.DotStyle;
    }
    void Update()
    {
        terrainText.text = terrainType.ToString();
        terrainText.color = bacKImageColors[Mathf.Clamp((int)terrainType,0,7)];
        int cost = (int)terrainType;
        costText.text = cost.ToString();
        paintingModeText.text = paintstyle.ToString();

        if (GameManagerStates.editState && !GameManagerStates.playState)
        {
            if(!painting)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
                {
                    if (changesStackCoord.Count > 0 && changesStackCost.Count > 0)
                    {
                        List<Vector3Int> coords = changesStackCoord.Pop();
                        List<int> costs = changesStackCost.Pop();
                        Debug.Log("Poped");

                        for (int i = 0; i < coords.Count; i++)
                        {
                            Vector3Int coord = coords[i];
                            int prevCost = costs[i];
                            Debug.Log("Moving");

                            if (squareGridController.TryChangeCellData(coord,prevCost))
                            {
                                var cellView = squareGridController.GetCellView(coord);
                                if (squareGridController.TryGetCellData(coord, out ICellData cellData))
                                {
                                    cellView.SetData(cellData);

                                }

                            }
                        }
                    }

                }
            }

            switch(paintstyle)
            {
                case paintingStyle.DotStyle:
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            cursorWorldPos.z = 0.0f;
                            var gridCoord = grid.WorldToCell(cursorWorldPos);
                            if (squareGridController.TryGetCellData(gridCoord, out ICellData cellData))
                            {
                                if(cellData.Cost != (int)terrainType)
                                {
                                    List<Vector3Int> tempCoord = new List<Vector3Int>();
                                    List<int> tempCost = new List<int>();

                                    tempCoord.Add(cellData.Coordinate);
                                    tempCost.Add(cellData.Cost);

                                    changesStackCoord.Push(tempCoord);
                                    changesStackCost.Push(tempCost);

                                    if (squareGridController.TryChangeCellData(gridCoord, (int)terrainType))
                                    {
                                        var cellView = squareGridController.GetCellView(gridCoord);
                                        cellView.SetData(cellData);
                                    }

                                }
                            }
                        }
                        break;
                    }
                case paintingStyle.BrushStyle:
                    {
                        if (Input.GetMouseButton(0))
                        {
                            Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            cursorWorldPos.z = 0.0f;
                            var gridCoord = grid.WorldToCell(cursorWorldPos);
                            if (squareGridController.TryGetCellData(gridCoord, out ICellData cellData))
                            {
                                if (cellData.Cost != (int)terrainType)
                                {
                                    painting = true;
                                    brushtempCoord.Add(cellData.Coordinate);
                                    brushtempCost.Add(cellData.Cost);
                                    if (squareGridController.TryChangeCellData(gridCoord, (int)terrainType))
                                    {
                                        var cellView = squareGridController.GetCellView(gridCoord);
                                        cellView.SetData(cellData);
                                    }
                                }
                            }
                        }
                        else if(Input.GetMouseButtonUp(0) && brushtempCoord.Count > 0 && brushtempCost.Count > 0)
                        {
                            pushed = false;
                            painting = false;
                        }

                        if (!pushed)
                        {
                            changesStackCoord.Push(brushtempCoord);
                            changesStackCost.Push(brushtempCost);

                            brushtempCoord = new List<Vector3Int>();
                            brushtempCost = new List<int>();
                            pushed = true;
                        }
                        break;
                    }

                case paintingStyle.FillStyle:
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            cursorWorldPos.z = 0.0f;
                            var gridCoord = grid.WorldToCell(cursorWorldPos);


                            if (squareGridController.TryGetCellData(gridCoord, out ICellData cellData))
                            {
                                paintNeighbour(gridCoord, cellData, out bool paintSingle);

                                if (cellData.Cost != (int)terrainType)
                                {
                                    List<Vector3Int> tempCoord = new List<Vector3Int>();
                                    List<int> tempCost = new List<int>();

                                    if(paintSingle) // change the tile clicked
                                    {
                                        tempCoord.Add(cellData.Coordinate);
                                        tempCost.Add(cellData.Cost);

                                        if (squareGridController.TryChangeCellData(gridCoord, (int)terrainType)) 
                                        {
                                            var cellView = squareGridController.GetCellView(gridCoord);
                                            cellView.SetData(cellData);
                                        }
                                    }
                                    else //change all tiles connected to this tile
                                    {
                                        foreach (var neighbour in cellList)
                                        {
                                                if (squareGridController.TryGetCellData(neighbour.Coordinate, out ICellData data))
                                                {
                                                    tempCoord.Add(data.Coordinate);
                                                    tempCost.Add(data.Cost);

                                                    if (squareGridController.TryChangeCellData(neighbour.Coordinate, (int)terrainType))
                                                    {
                                                        var cellView = squareGridController.GetCellView(neighbour.Coordinate);
                                                        cellView.SetData(data);
                                                    }

                                                }
                                        }
                                    }
                                    changesStackCoord.Push(tempCoord);
                                    changesStackCost.Push(tempCost);
                                }

                            }

                        }
                        break;
                    }

            }
        }
    }

    public void setBarrier()
    {
        terrainType = typesTerrain.Barrier;
        backImage.color = bacKImageColors[0];
    }

    public void setGrass()
    {
        terrainType = typesTerrain.GrassLands;
        backImage.color = bacKImageColors[1];
    }

    public void setMountain()
    {
        terrainType = typesTerrain.Mountain;
        backImage.color = bacKImageColors[2];
    }

    public void setDesert()
    {
        terrainType = typesTerrain.Desert;
        backImage.color = bacKImageColors[3];
    }
    public void setSnowPlains()
    {
        terrainType = typesTerrain.SnowPlains;
        backImage.color = bacKImageColors[4];
    }
    public void setSwamp()
    {
        terrainType = typesTerrain.Swamp;
        backImage.color = bacKImageColors[5];
    }

    public void setOcean()
    {
        terrainType = typesTerrain.Ocean;
        backImage.color = bacKImageColors[6];
    }
    public void setLava()
    {
        terrainType = typesTerrain.Lava;
        backImage.color = bacKImageColors[7];
    }

    public void setDotStyle()
    {
        paintstyle = paintingStyle.DotStyle;
    }

    public void setBrushStyle()
    {
        paintstyle = paintingStyle.BrushStyle;
    }

    public void setFillStyle()
    {
        paintstyle = paintingStyle.FillStyle;
    }

    private void paintNeighbour(Vector3Int clickCoordinate, ICellData data, out bool nothing)
    {
        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        frontier.Enqueue(clickCoordinate);
        cellList.Clear();

        while (frontier.Count > 0)
        {
            Vector3Int cellCoordinate = frontier.Dequeue();

            if(squareGridController.TryGetCellData(cellCoordinate, out ICellData celldata))
            {
                  foreach (var neighbour in celldata.Neighbours)
                  {
                    if (neighbour.Cost != data.Cost || Mathf.Abs(neighbour.Coordinate.x - cellCoordinate.x) > 0 && Mathf.Abs(neighbour.Coordinate.y - cellCoordinate.y) > 0)
                    {
                        continue;
                    }

                    if (!cellList.Contains(neighbour))
                      {
                          frontier.Enqueue(neighbour.Coordinate);
                          cellList.Add(neighbour);
                      }
                  }
            }

        }
        
        if(cellList.Count <=0)
        {
            nothing = true;
        }
        else
        {
            nothing = false;
        }
    }
}
