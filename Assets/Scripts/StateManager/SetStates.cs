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
    [SerializeField] private Color playColor;
    [SerializeField] private Color editColor;
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
}
