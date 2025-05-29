using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStates : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private PathFinderTest finder;
    [SerializeField] private SeekTarget seek;
    [SerializeField] private GameObject playUi;
    [SerializeField] private GameObject editUi;
    [SerializeField] private GameObject statesUi;
    [SerializeField] private GameObject panelUi;
    [SerializeField] private GameObject map1;
    [SerializeField] private GameObject map2;
    [SerializeField] private Color playColor;
    [SerializeField] private Color editColor;

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            playUi.SetActive(false);
            editUi.SetActive(false);
            statesUi.SetActive(false);
            panelUi.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            statesUi.SetActive(true);
            panelUi.SetActive(true);
            if (!GameManagerStates.editState && GameManagerStates.playState)
            {
                playUi.SetActive(true);
                editUi.SetActive(false);
            }
            else if (GameManagerStates.editState && !GameManagerStates.playState)
            {
                playUi.SetActive(false);
                editUi.SetActive(true);
            }
        }
    }

    public void SetPlay()
    {
        cam.backgroundColor = playColor;
        if(GameManagerStates.editState && !GameManagerStates.playState)
        {
            playUi.SetActive(true);
            editUi.SetActive(false);
            finder.StartPathFinding();
        }
        GameManagerStates.playState = true;
        GameManagerStates.editState = false;
    }

    public void SetEdit()
    {
        cam.backgroundColor = editColor;

        if (!GameManagerStates.editState && GameManagerStates.playState)
        {
            playUi.SetActive(false);
            editUi.SetActive(true);
        }
        GameManagerStates.editState = true;
        GameManagerStates.playState = false;
    }

    public void SwitchMaps()
    {
        if(map1.activeSelf)
        {
            map1.SetActive(false);
            map2.SetActive(true);
        }
        else
        {
            map1.SetActive(true);
            map2.SetActive(false);
        }
    }
}
