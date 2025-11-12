using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEDoors : MonoBehaviour
{
    // door objects
    public GameObject door1;
    public GameObject door2;
    public GameObject door3;
    public GameObject door4;

    // scientist objects
    public GameObject scientist1;
    public GameObject scientist2;
    public GameObject scientist3;
    public GameObject scientist4;

    // throwable objects
    public GameObject bottle;
    public GameObject garlic;
    public GameObject hammer;
    public GameObject orange;

    public GameObject camera;

    // door start location 
    public float doorStartZ;
    public float doorEndZ;

    public float doorSpeed;
    public string objectThrown;

    // cooldown between throws
    public float cooldown;

    public RunningPhaseController runController;
    public Material scrollingWall;
    public Material scrollingFloor;
    public Material scrollingCeiling;

    // Start is called before the first frame update
    void Start()
    {
        objectThrown = "none";
        // disable animations at first
        scientist1.SetActive(false);
        scientist2.SetActive(false);
        scientist3.SetActive(false);
        scientist4.SetActive(false);

        bottle.SetActive(false);
        garlic.SetActive(false);
        hammer.SetActive(false);
        orange.SetActive(false);

        // set cooldown
        cooldown = 0.5f;
        doorSpeed = 0.13f;
        StartScroll();
        
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
        if (objectThrown == "garlic")
        {
            garlic.transform.parent = null;
            garlic.transform.position = Vector3.Lerp(garlic.transform.position, camera.transform.position, 0.02f);
        }
        else if (objectThrown == "hammer")
        {
            hammer.transform.parent = null;
            hammer.transform.position = Vector3.Lerp(hammer.transform.position, camera.transform.position, 0.02f);
        }
        else if (objectThrown == "orange")
        {
            orange.transform.parent = null;
            orange.transform.position = Vector3.Lerp(orange.transform.position, camera.transform.position, 0.02f);
        }
        else if (objectThrown == "bottle")
        {
            bottle.transform.parent = null;
            bottle.transform.position = Vector3.Lerp(bottle.transform.position, camera.transform.position, 0.02f);
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

    // SCIENTIST RUN ANIMATION BETWEEN DOORS DURING RUNNING PHASE
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
        // DEPENDING ON RUN SPEED, SCIENTIST RUNS OUT OF CLOSER DOOR
        index += doorDepth;
        if (index >= 4)
        {
            index -= 4;
        }

        // SETS SCIENTIST ACTIVE AT RESPECTIVE DOOR
        if (index == 0)
        {
            scientist1.SetActive(true);
            bottle.SetActive(false);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 1)
        {
            scientist2.SetActive(true);
            garlic.SetActive(false);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 2)
        {
            scientist3.SetActive(true);
            hammer.SetActive(false);
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 3)
        {
            scientist4.SetActive(true);
            orange.SetActive(false);
            Invoke("HideScientist", 1.5f);
        }
    }

    // SCIENTIST THROW ANIMATION DURING QTE SEQUENCE
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
            scientist1.SetActive(true);
            bottle.SetActive(true);
            objectThrown = "bottle";
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 1)
        {
            scientist2.SetActive(true);
            garlic.SetActive(true);
            objectThrown = "garlic";
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 2)
        {
            scientist3.SetActive(true);
            hammer.SetActive(true);
            objectThrown = "hammer";
            Invoke("HideScientist", 1.5f);
        }
        else if (index == 3)
        {
            scientist4.SetActive(true);
            orange.SetActive(true);
            objectThrown = "orange";
            Invoke("HideScientist", 1.5f);
        }

    }

    public void HideScientist()
    {
        scientist1.SetActive(false);
        scientist2.SetActive(false);
        scientist3.SetActive(false);
        scientist4.SetActive(false);
    }

    public void DodgeObject()
    {
        bottle.SetActive(false);
        garlic.SetActive(false);
        hammer.SetActive(false);
        orange.SetActive(false);
    }

    public void ObjectHit()
    {

    }
}
