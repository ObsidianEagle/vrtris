using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBlockController : MonoBehaviour
{
    public AudioSource BlockDropAudioSource;

    public bool isSettled { get; set; }
    public bool isGrabbed { get; set; }

    private readonly int DEFAULT_FRAMERATE = 72;
    private readonly float BOTTOM_Y = 1.5f;
    private readonly float LEFT_WALL_X = -5.5f;
    private readonly float RIGHT_WALL_X = 5.5f;

    private int counter = 0;
    private Quaternion lastControllerRotation = Quaternion.identity;
    private Vector3 lastLateralUpdateLocation = Vector3.zero;
    private Vector3 lastDownUpdateLocation = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        isSettled = false;
        isGrabbed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSettled)
        {
            // Handle key presses
            if (
                (Input.GetKeyDown(KeyCode.RightArrow) || OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) 
                && !WouldCollideWithWall(1)
            )
            {
                transform.Translate(new Vector3(1, 0, 0), Space.World);
            } 
            else if (
                (Input.GetKeyDown(KeyCode.LeftArrow) || OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                && !WouldCollideWithWall(-1)
            )
            {
                transform.Translate(new Vector3(-1, 0, 0), Space.World);
            } 
            else if (Input.GetKeyDown(KeyCode.DownArrow) || OVRInput.GetDown(OVRInput.RawButton.B)) 
            {
                transform.Translate(new Vector3(0, -1, 0), Space.World);
                BlockDropAudioSource.Play();
            } 
            else if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.RawButton.A))
            {  
                RotateClockwise();
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))
            {
                isGrabbed = true;
                lastControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
                lastLateralUpdateLocation = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                lastDownUpdateLocation = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            }
            else if (OVRInput.GetUp(OVRInput.RawButton.RHandTrigger))
            {
                isGrabbed = false;
                SnapRotation();
            }

            if (isGrabbed)
            {
                Quaternion currentRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
                Vector3 currentLocation = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

                // Rotation
                Vector3 eulers = new Vector3(0, 0, currentRotation.eulerAngles.z - lastControllerRotation.eulerAngles.z);
                transform.Rotate(eulers, Space.World);
                lastControllerRotation = currentRotation;

                // X-translation
                if (currentLocation.x - lastLateralUpdateLocation.x > 0.05 && !WouldCollideWithWall(1))
                {
                    transform.Translate(new Vector3(1, 0, 0), Space.World);
                    lastLateralUpdateLocation = currentLocation;
                } 
                else if (currentLocation.x - lastLateralUpdateLocation.x < -0.05 && !WouldCollideWithWall(-1))
                {
                    transform.Translate(new Vector3(-1, 0, 0), Space.World);
                    lastLateralUpdateLocation = currentLocation;
                }

                // Y-translation
                if (currentLocation.y - lastDownUpdateLocation.y < -0.03)
                {
                    transform.Translate(new Vector3(0, -1, 0), Space.World);
                    lastDownUpdateLocation = currentLocation;
                }
            }

            // Move on timer
            if (counter >= DEFAULT_FRAMERATE) 
            {
                counter = 0;
                transform.Translate(new Vector3(0, -1, 0), Space.World);
                BlockDropAudioSource.Play();
            } 
            else 
            {
                counter++;
            }
        }
    }

    public void TrySettle(List<GameObject> settledBlocks)
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Vector3 subBlockPos = transform.GetChild(i).position;

            if (subBlockPos.y <= BOTTOM_Y)
            {
                isSettled = true;
            }

            for (int j = 0; j < settledBlocks.Count; j++)
            {
                Vector3 settledBlockPos = settledBlocks[j].transform.position;

                if (
                    Mathf.Floor(subBlockPos.x) == Mathf.Floor(settledBlockPos.x) 
                    && Mathf.Floor(subBlockPos.y - 1) == Mathf.Floor(settledBlockPos.y)
                )
                {
                    isSettled = true;
                }
            }
        }
    }

    public void SnapRotation()
    {
        float zAngle = (float) Mathf.RoundToInt(transform.rotation.eulerAngles.z / 90) * 90;
        Quaternion rot = Quaternion.AngleAxis(zAngle, Vector3.forward);
        transform.rotation = rot;
    }

    protected bool WouldCollideWithWall(int xChange)
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            if (xChange < 0 && transform.GetChild(i).position.x - 1 == LEFT_WALL_X)
            {
                return true;
            }
            else if (xChange > 0 && transform.GetChild(i).position.x + 1 == RIGHT_WALL_X)
            {
                return true;
            }
        }

        return false;
    }

    protected void RotateClockwise()
    {
        // TODO: Rotation collision detection
        transform.Rotate(new Vector3(0, 0, 90));
    }
}
