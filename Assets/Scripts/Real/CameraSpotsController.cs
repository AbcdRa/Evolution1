using System.Collections.Generic;
using UnityEngine;

public class CameraSpotsController : MonoBehaviour
{
    [SerializeField] private CameraSpot[] cameraSpots;
    [SerializeField] private CameraSpot mainCameraSpot;
    [SerializeField] private Transform mainCamera;
    private IPlayerMananger playerMananger;

    /// <summary>
    /// -3 - MyHand
    /// -2 - MyAnimals
    /// -1 - Table
    /// 0, 1, 2, 3 - PlayerId Animals
    /// </summary>
    private int currentPos;
    private Dictionary<int, CameraSpot> cameraSpotsPlayerHands;
    private Dictionary<int, CameraSpot> cameraSpotsPlayerAnimals;

    private void Start()
    {
        playerMananger = GameMananger.instance.playerMananger;
        cameraSpotsPlayerAnimals = new();
        cameraSpotsPlayerHands = new();
        for (int i = 0; i < cameraSpots.Length; i++)
        {
            if (cameraSpots[i].IsPrivate())
            {
                cameraSpotsPlayerHands.Add(cameraSpots[i].GetPlayerId(), cameraSpots[i]);
            }
            else
            {
                cameraSpotsPlayerAnimals.Add(cameraSpots[i].GetPlayerId(), cameraSpots[i]);
            }
        }
        UpdateCurrentPos();
    }

    private void Update()
    {
        int currentPos = this.currentPos;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentPos < -1)
            {
                currentPos++;
            }
            else if (currentPos == -1)
            {
                currentPos = (playerMananger.GetInteractablePlayer().id + 2) % 4;
            }
            else if (currentPos > -1)
            {
                currentPos = -3;
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentPos = currentPos > -2 ? -2 : -3;
        }
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && currentPos == -1)
        {
            currentPos = (playerMananger.GetInteractablePlayer().id + 1) % 4;
        }
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && currentPos == -1)
        {
            currentPos = (playerMananger.GetInteractablePlayer().id + 3) % 4;
        }
        UpdateCurrentPos(currentPos);

    }

    public void UpdateCurrentPos(int currentPos = -3)
    {
        if (currentPos != this.currentPos)
        {
            this.currentPos = currentPos;
            CameraSpot nextCameraSpot = GetNextCamerSpot(currentPos);
            mainCamera.parent = nextCameraSpot.transform;
            mainCamera.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
        }
    }


    public void ResetUpdateCameraPos()
    {
        this.currentPos = -3;
        CameraSpot nextCameraSpot = GetNextCamerSpot(currentPos);
        mainCamera.parent = nextCameraSpot.transform;
        mainCamera.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
    }

    public CameraSpot GetNextCamerSpot(int currentPos)
    {
        if (currentPos == -3)
        {
            return cameraSpotsPlayerHands[playerMananger.GetInteractablePlayer().id];
        }
        if (currentPos == -2) { return cameraSpotsPlayerAnimals[playerMananger.GetInteractablePlayer().id]; };
        return currentPos == -1 ? mainCameraSpot : cameraSpotsPlayerAnimals[currentPos];
    }
}
