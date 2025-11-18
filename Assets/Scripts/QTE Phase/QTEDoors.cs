using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEDoors : MonoBehaviour
{
    // DOOR OBJECTS
    public GameObject door1;
    public GameObject door2;
    public GameObject door3;
    public GameObject door4;

    // SCIENTIST OBJECTS & ANIMATORS
    public GameObject scientist1;
    public GameObject scientist2;
    public GameObject scientist3;
    public GameObject scientist4;
    public Animator scientist1animator;
    public Animator scientist2animator;
    public Animator scientist3animator;
    public Animator scientist4animator;

    // THROWABLE OBJECTS & THEIR STARTING POSITIONS
    public GameObject bottle;
    public GameObject garlic;
    public GameObject hammer;
    public GameObject orange;
    public Vector3 bottleStartPos;
    public Vector3 garlicStartPos;
    public Vector3 hammerStartPos;
    public Vector3 orangeStartPos;

    // THROWABLE OBJECT TARGET POSITION
    public Vector3 objectTargetPos;

    // CAMERA OBJECT
    public GameObject myCamera;

    public GameObject bang;

    // FLOATS & STRINGS
    public float doorStartZ; // closest possible door position to camera
    public float doorEndZ; // farthest possible door position to camera
    public float doorSpeed; // speed at which doors move away from camera
    public string objectThrown; // string denoting which object is being thrown
    public float objectMoveAmount; // how fast object moves toward camera

    // PHASE CONTROLLER SCRIPT REFERENCES
    public RunningPhaseController runController;
    public HidingPhaseController hidingController;
    public GameStateManager gameManager;

    // SCROLLING MATERIALS
    public Material scrollingWall;
    public Material scrollingFloor;
    public Material scrollingCeiling;

    // START FUNCTION
    void Start()
    {

        // DISABLES SCIENTISTS & OBJECTS AT START
        scientist1.SetActive(false);
        scientist2.SetActive(false);
        scientist3.SetActive(false);
        scientist4.SetActive(false);
        bottle.SetActive(false);
        garlic.SetActive(false);
        hammer.SetActive(false);
        orange.SetActive(false);

        // INITIAL VARIABLE VALUES
        doorSpeed = 0.13f;
        objectMoveAmount = 0.03f;
        objectThrown = "none";

        // STARTS BACKGROUND SCROLL
        StartScroll();

        // SAVES START POSITION OF THROWABLE OBJECTS
        bottleStartPos = bottle.transform.position;
        garlicStartPos = garlic.transform.position;
        hammerStartPos = hammer.transform.position;
        orangeStartPos = orange.transform.position;

        // SETS THROWABLE OBJECT TARGET POSITION TO SLIGHTLY ABOVE CAMERA
        objectTargetPos = new Vector3(myCamera.transform.position.x, myCamera.transform.position.y + 0.5f, myCamera.transform.position.z);
        
    }

    // FIXED UPDATE FUNCTION
    void FixedUpdate()
    {
        // LOOPS DOORS TO CREATE ENDLESS HALLWAY
        if (door1.transform.position.z < doorEndZ)
        {
            door1.transform.position = new Vector3(door1.transform.position.x, door1.transform.position.y, door1.transform.position.z + doorSpeed);
        }
        else
        {
            door1.transform.position = new Vector3(door1.transform.position.x, door1.transform.position.y, doorStartZ);
        }

        if (door2.transform.position.z < doorEndZ)
        {
            door2.transform.position = new Vector3(door2.transform.position.x, door2.transform.position.y, door2.transform.position.z + doorSpeed);
        }
        else
        {
            door2.transform.position = new Vector3(door2.transform.position.x, door2.transform.position.y, doorStartZ);
        }

        if (door3.transform.position.z < doorEndZ)
        {
            door3.transform.position = new Vector3(door3.transform.position.x, door3.transform.position.y, door3.transform.position.z + doorSpeed);
        }
        else
        {
            door3.transform.position = new Vector3(door3.transform.position.x, door3.transform.position.y, doorStartZ);
        }

        if (door4.transform.position.z < doorEndZ)
        {
            door4.transform.position = new Vector3(door4.transform.position.x, door4.transform.position.y, door4.transform.position.z + doorSpeed);
        }
        else
        {
            door4.transform.position = new Vector3(door4.transform.position.x, door4.transform.position.y, doorStartZ);
        }

        // LERPS THROWN OBJECT TOWARD CAMERA
        if (objectThrown == "garlic" && garlic.activeSelf)
        {
            garlic.transform.parent = null;
            garlic.transform.position = Vector3.Lerp(garlic.transform.position, objectTargetPos, objectMoveAmount);
        }
        else if (objectThrown == "hammer" && hammer.activeSelf)
        {
            hammer.transform.parent = null;
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, objectTargetPos, objectMoveAmount);
        }
        else if (objectThrown == "orange" && orange.activeSelf)
        {
            orange.transform.parent = null;
            orange.transform.position = Vector3.Lerp(orange.transform.position, objectTargetPos, objectMoveAmount);
        }
        else if (objectThrown == "bottle" && bottle.activeSelf)
        {
            bottle.transform.parent = null;
            bottle.transform.position = Vector3.Lerp(bottle.transform.position, objectTargetPos, objectMoveAmount);
        }
    }

    // STOPS WALLS / FLOOR / CEILING / DOORS SCROLL ANIMATION
    public void StopScroll()
    {
        scrollingCeiling.SetVector("_Direction", new Vector2(0, 0));
        scrollingWall.SetVector("_Direction", new Vector2(0, 0));
        scrollingFloor.SetVector("_Direction", new Vector2(0, 0));
        doorSpeed = 0;
    }

    // STARTS WALLS / FLOORS / CEILING / DOORS SCROLL ANIMATION
    public void StartScroll()
    {
        scrollingCeiling.SetVector("_Direction", new Vector2(0.67f, 0));
        scrollingWall.SetVector("_Direction", new Vector2(1, 0));
        scrollingFloor.SetVector("_Direction", new Vector2(0.33f, 0));
        doorSpeed = 0.13f;
    }

    // SCIENTIST SEARCH ANIMATION (HIDING PHASE)
    public void ScientistSearch(int doorDepth, bool searchFailed)
    {
        // CALCULATES WHICH DOOR IS CLOSEST TO PLAYER
        float[] doorZPositions = {door1.transform.position.z, door2.transform.position.z, door3.transform.position.z, door4.transform.position.z};
        float value = float.PositiveInfinity;
        int index = -1;
        for(int i = 0; i < doorZPositions.Length; i++)
        {
            if(doorZPositions[i] < value)
            {
                index = i;
                value = doorZPositions[i];
            }
        }
        // DEPENDING ON INPUTTED INT, SCIENTIST RUNS OUT OF SPECIFIED DOOR
        index += doorDepth;
        if (index >= 4)
        {
            index -= 4;
        }

        // SETS SCIENTIST ACTIVE AT RESPECTIVE DOOR
        if (index == 0)
        {
            // FIRST DOOR
            scientist1.SetActive(true);
            Debug.Log("SCIENTIST IS SEARCHING");
            scientist1animator.SetBool("throwing", false);
            scientist1animator.SetBool("searching", true);
            scientist1animator.SetBool("searchfailed", searchFailed);
            bottle.SetActive(false);
            if (searchFailed)
            {
                Invoke("HideScientist", 3.5f);
            }
            else
            {
                Invoke("HideScientist", 2.6f);
            }
        }
        else if (index == 1)
        {
            // SECOND DOOR
            scientist2.SetActive(true);
            Debug.Log("SCIENTIST IS SEARCHING");
            scientist2animator.SetBool("throwing", false);
            scientist2animator.SetBool("searching", true);
            scientist2animator.SetBool("searchfailed", searchFailed);
            garlic.SetActive(false);
            if (searchFailed)
            {
                Invoke("HideScientist", 3.5f);
            }
            else
            {
                Invoke("HideScientist", 2.6f);
            }
        }
        else if (index == 2)
        {
            // THIRD DOOR
            scientist3.SetActive(true);
            Debug.Log("SCIENTIST IS SEARCHING");
            scientist3animator.SetBool("throwing", false);
            scientist3animator.SetBool("searching", true);
            scientist3animator.SetBool("searchfailed", searchFailed);
            hammer.SetActive(false);
            if (searchFailed)
            {
                Invoke("HideScientist", 3.5f);
            }
            else
            {
                Invoke("HideScientist", 2.6f);
            }
        }
        else if (index == 3)
        {
            // FOURTH DOOR
            scientist4.SetActive(true);
            Debug.Log("SCIENTIST IS SEARCHING");
            scientist4animator.SetBool("throwing", false);
            scientist4animator.SetBool("searching", true);
            scientist4animator.SetBool("searchfailed", searchFailed);
            orange.SetActive(false);
            if (searchFailed)
            {
                Invoke("HideScientist", 3.5f);
            }
            else
            {
                Invoke("HideScientist", 2.6f);
            }
        }
    }

    // SCIENTIST RUN ANIMATION (RUNNING PHASE)
    public void ScientistRun(int doorDepth)
    {
        // CALCULATES WHICH DOOR IS CLOSEST TO PLAYER
        float[] doorZPositions = {door1.transform.position.z, door2.transform.position.z, door3.transform.position.z, door4.transform.position.z};
        float value = float.PositiveInfinity;
        int index = -1;
        for(int i = 0; i < doorZPositions.Length; i++)
        {
            if(doorZPositions[i] < value)
            {
                index = i;
                value = doorZPositions[i];
            }
        }
        // DEPENDING ON INPUTTED INT, SCIENTIST RUNS OUT OF SPECIFIED DOOR
        index += doorDepth;
        if (index >= 4)
        {
            index -= 4;
        }

        // SETS SCIENTIST ACTIVE AT RESPECTIVE DOOR
        if (index == 0)
        {
            // FIRST DOOR
            scientist1.SetActive(true);
            scientist1animator.SetBool("throwing", false);
            scientist1animator.SetBool("searching", false);
            bottle.SetActive(false);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 1)
        {
            // SECOND DOOR
            scientist2.SetActive(true);
            scientist2animator.SetBool("throwing", false);
            scientist2animator.SetBool("searching", false);
            garlic.SetActive(false);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 2)
        {
            // THIRD DOOR
            scientist3.SetActive(true);
            scientist3animator.SetBool("throwing", false);
            scientist3animator.SetBool("searching", false);
            hammer.SetActive(false);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 3)
        {
            // FOURTH DOOR
            scientist4.SetActive(true);
            scientist4animator.SetBool("throwing", false);
            scientist4animator.SetBool("searching", false);
            orange.SetActive(false);
            Invoke("HideScientist", 1.5f);
        }
    }

    // SCIENTIST THROW ANIMATION (QTE SEQUENCE)
    public void ThrowObject(int doorDepth)
    {
        // CALCULATES WHICH DOOR IS CLOSEST TO PLAYER
        float[] doorZPositions = {door1.transform.position.z, door2.transform.position.z, door3.transform.position.z, door4.transform.position.z};
        float value = float.PositiveInfinity;
        int index = -1;
        for(int i = 0; i < doorZPositions.Length; i++)
        {
            if(doorZPositions[i] < value)
            {
                index = i;
                value = doorZPositions[i];
            }
        }

        // DEPENDING ON HOW MANY OBJECTS HAVE HIT PLAYER, SCIENTIST RUNS OUT OF CLOSER DOOR
        index += doorDepth;
        if (index >= 4)
        {
            index -= 4;
        }

        if (index == 0)
        {
            // FIRST DOOR (BOTTLE OBJECT)
            scientist1.SetActive(true);
            scientist1animator.SetBool("throwing", true);
            scientist1animator.SetBool("searching", false);
            objectThrown = "bottle";
            Invoke("SetObjectThrownActive", 0.5f);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 1)
        {
            // SECOND DOOR (GARLIC OBJECT)
            scientist2.SetActive(true);
            scientist2animator.SetBool("throwing", true);
            scientist2animator.SetBool("searching", false);
            objectThrown = "garlic";
            Invoke("SetObjectThrownActive", 0.5f);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 2)
        {
            // THIRD DOOR (HAMMER OBJECT)
            scientist3.SetActive(true);
            scientist3animator.SetBool("throwing", true);
            scientist3animator.SetBool("searching", false);
            objectThrown = "hammer";
            Invoke("SetObjectThrownActive", 0.5f);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 3)
        {
            // FOURTH DOOR (ORANGE OBJECT)
            scientist4.SetActive(true);
            scientist4animator.SetBool("throwing", true);
            scientist4animator.SetBool("searching", false);
            objectThrown = "orange";
            Invoke("SetObjectThrownActive", 0.5f);
            Invoke("HideScientist", 1.5f);
        }

        gameManager.ThrowAtPlayer(objectThrown);

    }

    // HIDES SCIENTISTS
    public void HideScientist()
    {
        scientist1.SetActive(false);
        scientist2.SetActive(false);
        scientist3.SetActive(false);
        scientist4.SetActive(false);
    }

    // HIDES OBJECTS
    public void DodgeObject()
    {
        bottle.SetActive(false);
        garlic.SetActive(false);
        hammer.SetActive(false);
        orange.SetActive(false);
    }

    // HIT OBJECTS
    public void HitObject()
    {
        bottle.SetActive(false);
        garlic.SetActive(false);
        hammer.SetActive(false);
        orange.SetActive(false);

        bang.SetActive(true);
        Invoke("HideBang", 0.35f);
    }

    public void HideBang()
    {
        bang.SetActive(false);
    }

    // SETS THROWN OBJECT ACTIVE IN HIERARCHY
    public void SetObjectThrownActive()
    {
        if (objectThrown == "bottle")
        {
            bottle.SetActive(true);
            garlic.transform.parent = door1.transform;
            bottle.transform.position = bottleStartPos;
        }
        else if (objectThrown == "garlic")
        {
           garlic.SetActive(true);
           garlic.transform.parent = door2.transform;
           garlic.transform.position = garlicStartPos;
        }
        else if (objectThrown == "hammer")
        {
            hammer.SetActive(true);
            hammer.transform.parent = door3.transform;
            garlic.transform.position = garlicStartPos;
        }
        else if (objectThrown == "orange")
        {
            orange.SetActive(true);
            garlic.transform.parent = door4.transform;
            garlic.transform.position = garlicStartPos;
        }
    }
}
