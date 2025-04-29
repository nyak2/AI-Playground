using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class PathFinderTest : MonoBehaviour
{
    [SerializeField] private SquareGridController gridController;

    [Header("Pathfinding variables")]
    [SerializeField] private Vector3Int startCoordinate;
    [SerializeField] private Vector3Int goalCoordinate;
    [SerializeField] private GameObject goalSprite;


    [Header("Path visualization")]
    [SerializeField] private PathRenderer pathRenderer;

    [SerializeField] private GameObject seeker; 
    
    [SerializeField] private TextMeshProUGUI cellNumberText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI PathFindingText;
    [SerializeField] private TextMeshProUGUI HeuristicText;
    [SerializeField] private TextMeshProUGUI DiagonalText;
    [SerializeField] private TextMeshProUGUI PathStatusText;

    [Header("Navigator type")]
    [SerializeField] private TextMeshProUGUI navitext;
    [SerializeField] private List<Sprite> naviSprites;
    [SerializeField] private GameObject navigatorSelection;
    [SerializeField] private GameObject UCSSettings;
    [SerializeField] private GameObject AStarSettings;

    public enum NaviType
    {
        Elf = 1,
        Golem = 2,
        SandGoblin = 3,
        Viking = 4,
        Ogre = 5,
        LadyOfTheLake = 6,
        Demon = 7
    }

    private NaviType navigators;

    private enum AiTypes
    {
        UCS = 0,
        AStar = 1,
        LazyTheta = 2
    }

    private AiTypes type;
    private PathFinder_AStar.Heuristics HeuristicsType;
    private PathFinder_LazyTheta.Heuristics HeuristicsTypeLazy;

    [SerializeField] Grid grid;
    [SerializeField] SquareGridController squareGridController;
    [SerializeField] SeekTarget seek;

    private bool enableDiagonal;

    private void Start()
    {
        SetDiagonal();
        setAstar();
        navigators = NaviType.Elf;

        HeuristicsType = PathFinder_AStar.Heuristics.OCTILE;
        HeuristicText.text = "Octile";

        seeker.transform.position = startCoordinate;
        startCoordinate = Vector3Int.FloorToInt(seeker.transform.position);
    }
    void Update()
    {
        if (GameManagerStates.playState && !GameManagerStates.editState)
        {
            if (seek.lr.positionCount > 1)
            {
                navigatorSelection.SetActive(false);
            }
            else
            {
                navigatorSelection.SetActive(true);
            }

            PathFindingText.text = type.ToString();
            navitext.text = navigators.ToString();

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                cursorWorldPos.z = 0.0f;
                var gridCoord = grid.WorldToCell(cursorWorldPos);
                if (squareGridController.TryGetCellData(gridCoord, out ICellData cellData))
                {
                    var cellView = squareGridController.GetCellView(gridCoord);
                    if (cellView.BackingData.Cost > 0)
                    {
                        if (checkNaviType(cellView))
                        {
                            SetEndCoordinate(gridCoord);
                        }
                    }
                }
            }

        }
    }
    void SetEndCoordinate(Vector3Int coord)
    {
        seek.pathCoord.Clear();
        seek.counter = 0;
        goalCoordinate = coord;
        goalSprite.transform.position = coord;
        FindPath();
    }

    public void StartPathFinding()
    {
        seek.counter = 0;
        seek.pathCoord.Clear();
        FindPath();
    }

    public void FindPath()
    {
        startCoordinate = Vector3Int.FloorToInt(seeker.transform.position);
        switch (type)
        {
            case AiTypes.UCS:
                {
                    if (PathFinder_Ucs.TryFindPath(startCoordinate, goalCoordinate, gridController.GetSquareGrid(), out var pf, out var NumOfCells, out Stopwatch time, enableDiagonal, navigators))
                    {
                        pathRenderer.enabled = true;
                        pathRenderer.RenderPath(pf);

                        cellNumberText.text = NumOfCells.Count.ToString();
                        timeText.text = convertToMs(time).ToString() + " ms";
                        PathStatusText.color = Color.green;
                        PathStatusText.text = "Valid";
                    }
                    else
                    {
                        pathRenderer.enabled = false;
                        seek.counter = 0;
                        seek.pathCoord.Clear();
                        seek.lr.positionCount = 0;
                        PathStatusText.color = Color.red;
                        PathStatusText.text = "Invalid";
                    }
                }
                break;

            case AiTypes.AStar:
                {
                    if (PathFinder_AStar.TryFindPath(startCoordinate, goalCoordinate, gridController.GetSquareGrid(), out var pf, HeuristicsType , out var NumOfCells, out Stopwatch time, enableDiagonal, navigators))
                    {
                        pathRenderer.enabled = true;
                        pathRenderer.RenderPath(pf);

                        cellNumberText.text = NumOfCells.Count.ToString();
                        timeText.text = convertToMs(time).ToString() + " ms";
                        PathStatusText.color = Color.green;
                        PathStatusText.text = "Valid";
                    }
                    else
                    {
                        pathRenderer.enabled = false;
                        seek.counter = 0;
                        seek.pathCoord.Clear();
                        seek.lr.positionCount = 0;

                        PathStatusText.color = Color.red;
                        PathStatusText.text = "Invalid";
                    }
                }
                break;

            case AiTypes.LazyTheta:
                {
                    if (PathFinder_LazyTheta.TryFindPath(startCoordinate, goalCoordinate, gridController.GetSquareGrid(), out var pf, HeuristicsTypeLazy, out var NumOfCells, out Stopwatch time, enableDiagonal, navigators))
                    {
                        pathRenderer.enabled = true;
                        pathRenderer.RenderPath(pf);

                        cellNumberText.text = NumOfCells.Count.ToString();
                        timeText.text = convertToMs(time).ToString() + " ms";
                        PathStatusText.color = Color.green;
                        PathStatusText.text = "Valid";
                    }
                    else
                    {
                        pathRenderer.enabled = false;
                        seek.counter = 0;
                        seek.pathCoord.Clear();
                        seek.lr.positionCount = 0;

                        PathStatusText.color = Color.red;
                        PathStatusText.text = "Invalid";
                    }
                }
                break;
        }
    }

    private double convertToMs(Stopwatch time)
    {
        return Math.Round(1e3 * (time.ElapsedTicks / (double)Stopwatch.Frequency), 2);
    }

    private bool checkNaviType(CellView cv)
    {
        switch (navigators)
        {
            case NaviType.Elf:
                {
                    if (cv.BackingData.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    {
                        return false;
                    }
                }
                break;
            case NaviType.Golem:
                {
                    break;
                }
            case NaviType.SandGoblin:
                {
                    if (cv.BackingData.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    {
                        return false;
                    }
                }
                break;
            case NaviType.Viking:
                {
                    if (cv.BackingData.Cost == (int)TerrainPicker.typesTerrain.Lava || cv.BackingData.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                    {
                        return false;  
                    }
                }
                break;
            case NaviType.Ogre:
                {
                    if (cv.BackingData.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    {
                        return false;
                    }
                }
                break;
            case NaviType.LadyOfTheLake:
                {
                    if (cv.BackingData.Cost == (int)TerrainPicker.typesTerrain.Lava)
                    {
                        return false;
                    }
                }
                break;
            case NaviType.Demon:
                {
                    if (cv.BackingData.Cost == (int)TerrainPicker.typesTerrain.Ocean)
                    {
                        return false;
                    }
                }
                break;
        }
        return true;
    }

    #region Configuration
    public void setUCS()
    {
        type = AiTypes.UCS;
        UCSSettings.SetActive(true);
        AStarSettings.SetActive(false);
        HeuristicText.text = "-";
    }

    public void setAstar()
    {
        type = AiTypes.AStar;
        UCSSettings.SetActive(false);
        AStarSettings.SetActive(true);
        switch (HeuristicsType)
        {
            case PathFinder_AStar.Heuristics.MANHATTAN:
                {
                    HeuristicText.text = "Manhattan";
                    break;
                }
            case PathFinder_AStar.Heuristics.OCTILE:
                {
                    HeuristicText.text = "Octile";
                    break;
                }
        }

    }

    public void setLazyTheta()
    {
        type = AiTypes.LazyTheta;
        UCSSettings.SetActive(false);
        AStarSettings.SetActive(true);
        switch (HeuristicsType)
        {
            case PathFinder_AStar.Heuristics.MANHATTAN:
                {
                    HeuristicText.text = "Manhattan";
                    break;
                }
            case PathFinder_AStar.Heuristics.OCTILE:
                {
                    HeuristicText.text = "Octile";
                    break;
                }
        }

    }

    public void setHeuristic()
    {
        switch(HeuristicsType)
        {
            case PathFinder_AStar.Heuristics.MANHATTAN:
                {
                    HeuristicsType = PathFinder_AStar.Heuristics.OCTILE;
                    HeuristicText.text = "Octile";
                    break;
                }
            case PathFinder_AStar.Heuristics.OCTILE:
                {
                    HeuristicsType = PathFinder_AStar.Heuristics.MANHATTAN;
                    HeuristicText.text = "Manhattan";
                    break;
                }
        }
    }

    public void SetDiagonal()
    {
        if(enableDiagonal) 
        { 
            enableDiagonal = false;
            DiagonalText.color = Color.red;
        }
        else
        {
            enableDiagonal = true;
            DiagonalText.color = Color.green;

        }
        DiagonalText.text = enableDiagonal.ToString();
    }
    #endregion


    #region navigator types

    public void SetNavitype(TMP_Dropdown dropdown)
    {
        switch(dropdown.value + 1)
        {
            case 1:
                {
                    navigators = NaviType.Elf;
                    seeker.GetComponent<SpriteRenderer>().sprite = naviSprites[0];
                    break;
                }
            case 2:
                {
                    navigators = NaviType.Golem;
                    seeker.GetComponent<SpriteRenderer>().sprite = naviSprites[1];

                    break;
                }
            case 3:
                {
                    navigators = NaviType.SandGoblin;
                    seeker.GetComponent<SpriteRenderer>().sprite = naviSprites[2];

                    break;
                }
            case 4:
                {
                    navigators = NaviType.Viking;
                    seeker.GetComponent<SpriteRenderer>().sprite = naviSprites[3];

                    break;

                }
            case 5:
                {
                    navigators = NaviType.Ogre;
                    seeker.GetComponent<SpriteRenderer>().sprite = naviSprites[4];

                    break;

                }
            case 6:
                {
                     navigators = NaviType.LadyOfTheLake;
                    seeker.GetComponent<SpriteRenderer>().sprite = naviSprites[5];

                    break;

                }
            case 7:
                {
                    navigators = NaviType.Demon;
                    seeker.GetComponent<SpriteRenderer>().sprite = naviSprites[6];

                    break;

                }
        }
    }

    #endregion


}
