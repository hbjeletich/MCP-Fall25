using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingObjectManager : MonoBehaviour
{
    [Header("Object Prefabs")]
    public GameObject[] objectPrefabs;
    
    [Header("Selection Settings")]
    public int numberOfObjects = 3;
    
    [Header("Spawn Settings")]
    public Vector3 spawnPosition = Vector3.zero;
    
    private HidingObject[] selectedObjects;
    private int[] selectedPrefabIndices; 

    public HidingObject[] GenerateObjects()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0)
        {
            Debug.LogError("HidingObjectManager: No object prefabs assigned!");
            return null;
        }
        
        selectedObjects = new HidingObject[numberOfObjects];
        selectedPrefabIndices = new int[numberOfObjects];
        
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < objectPrefabs.Length; i++)
        {
            availableIndices.Add(i);
        }
        
        for (int i = 0; i < numberOfObjects && i < objectPrefabs.Length; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int prefabIndex = availableIndices[randomIndex];
            availableIndices.RemoveAt(randomIndex);
            
            selectedPrefabIndices[i] = prefabIndex;
            
            GameObject spawnedObj = Instantiate(objectPrefabs[prefabIndex], spawnPosition, Quaternion.identity, transform);
            spawnedObj.name = $"HidingObject_{i}";
            
            spawnedObj.SetActive(false);
            
            HidingObject hidingObj = spawnedObj.GetComponent<HidingObject>();
            if (hidingObj == null)
            {
                hidingObj = spawnedObj.AddComponent<HidingObject>();
            }
            
            selectedObjects[i] = hidingObj;
            
            Debug.Log($"Generated object {i}: {objectPrefabs[prefabIndex].name} (disabled)");
        }
        
        return selectedObjects;
    }

    public HidingObject[] GetSelectedObjects()
    {
        return selectedObjects;
    }
    
    public int[] GetSelectedPrefabIndices()
    {
        return selectedPrefabIndices;
    }
    
    public void ShowSelectedObject(int index)
    {
        if (selectedObjects == null)
        {
            Debug.LogError("HidingObjectManager: No objects have been generated yet!");
            return;
        }
        
        if (index < 0 || index >= selectedObjects.Length)
        {
            Debug.LogError($"HidingObjectManager: Invalid index {index}! Must be between 0 and {selectedObjects.Length - 1}");
            return;
        }
        
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] != null)
            {
                selectedObjects[i].gameObject.SetActive(false);
            }
        }
        
        if (selectedObjects[index] != null)
        {
            selectedObjects[index].gameObject.SetActive(true);
            Debug.Log($"HidingObjectManager: Enabled hiding object {index}");
        }
    }

    public void HideAllObjects()
    {
        if (selectedObjects == null) return;
        
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] != null)
            {
                selectedObjects[i].gameObject.SetActive(false);
            }
        }
        
        Debug.Log("HidingObjectManager: Disabled all hiding objects");
    }

    public HidingObject GetActiveObject()
    {
        if (selectedObjects == null) return null;
        
        foreach (var obj in selectedObjects)
        {
            if (obj != null && obj.gameObject.activeSelf)
            {
                return obj;
            }
        }
        
        return null;
    }
}