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

    // door start location 
    public float doorStartZ;
    public float doorEndZ;

    // cooldown between throws
    public float cooldown;

    // Start is called before the first frame update
    void Start()
    {
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
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // if (cooldown > 0)
        // {
        //     cooldown -= Time.fixedDeltaTime;
        // }
        // else
        // {
        //     cooldown = 5;
        //     ThrowObject();
        // }


        if (door1.transform.position.z < doorEndZ)
        {
            door1.transform.position = new Vector3(door1.transform.position.x, door1.transform.position.y, door1.transform.position.z + 0.13f);
        }
        else
        {
            door1.transform.position = new Vector3(door1.transform.position.x, door1.transform.position.y, doorStartZ);
        }

        if (door2.transform.position.z < doorEndZ)
        {
            door2.transform.position = new Vector3(door2.transform.position.x, door2.transform.position.y, door2.transform.position.z + 0.13f);
        }
        else
        {
            door2.transform.position = new Vector3(door2.transform.position.x, door2.transform.position.y, doorStartZ);
        }

        if (door3.transform.position.z < doorEndZ)
        {
            door3.transform.position = new Vector3(door3.transform.position.x, door3.transform.position.y, door3.transform.position.z + 0.13f);
        }
        else
        {
            door3.transform.position = new Vector3(door3.transform.position.x, door3.transform.position.y, doorStartZ);
        }

        if (door4.transform.position.z < doorEndZ)
        {
            door4.transform.position = new Vector3(door4.transform.position.x, door4.transform.position.y, door4.transform.position.z + 0.13f);
        }
        else
        {
            door4.transform.position = new Vector3(door4.transform.position.x, door4.transform.position.y, doorStartZ);
        }
    }

    public void ThrowObject()
    {
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
        if (index != 3)
        {
            index += 1;
        }
        else if (index == 3)
        {
            index = 0;
        }

        if (index == 0)
        {
            scientist1.SetActive(true);
            bottle.SetActive(true);
            Invoke("HideObject", 1.5f);
        }
        else if (index == 1)
        {
            scientist2.SetActive(true);
            garlic.SetActive(true);
            Invoke("HideObject", 1.5f);
        }
        else if (index == 2)
        {
            scientist3.SetActive(true);
            hammer.SetActive(true);
            Invoke("HideObject", 1.5f);
        }
        else if (index == 3)
        {
            scientist4.SetActive(true);
            orange.SetActive(true);
            Invoke("HideObject", 1.5f);
        }

    }

    public void HideObject()
    {
        scientist1.SetActive(false);
        scientist2.SetActive(false);
        scientist3.SetActive(false);
        scientist4.SetActive(false);

        bottle.SetActive(false);
        garlic.SetActive(false);
        hammer.SetActive(false);
        orange.SetActive(false);
    }
}
