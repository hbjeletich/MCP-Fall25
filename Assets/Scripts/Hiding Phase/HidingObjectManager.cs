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
            SpriteRenderer spriteRenderer = spawnedObj.GetComponent<SpriteRenderer>();
            if(spriteRenderer != null) spriteRenderer.enabled = false;
            spawnedObj.name = $"HidingObject_{i}";
            
            HidingObject hidingObj = spawnedObj.GetComponent<HidingObject>();
            if (hidingObj == null)
            {
                hidingObj = spawnedObj.AddComponent<HidingObject>();
            }
            
            selectedObjects[i] = hidingObj;
            
            Debug.Log($"Generated object {i}: {objectPrefabs[prefabIndex].name}");
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
}