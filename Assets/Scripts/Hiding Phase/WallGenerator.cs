using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int numberOfWalls = 10;
    
    [Header("Limb References")]
    public PlayerLimbController leftArmController;
    public PlayerLimbController rightArmController;
    public PlayerLimbController leftLegController;
    public PlayerLimbController rightLegController;
    public PlayerLimbController headController;
    
    [Header("Visualization")]
    public bool showTargetAngles = true;
    
    private WallHole[] generatedWalls;

    public WallHole[] GenerateWalls()
    {
        if (leftArmController == null || rightArmController == null || 
            leftLegController == null || rightLegController == null || headController == null)
        {
            Debug.LogError("WallGenerator: Not all limb controllers are assigned!");
            return null;
        }
        
        generatedWalls = new WallHole[numberOfWalls];
        
        for (int i = 0; i < numberOfWalls; i++)
        {
            GameObject wallObj = new GameObject($"Wall_{i + 1}");
            wallObj.transform.parent = transform;
            wallObj.transform.position = Vector3.zero;
            
            WallHole wall = wallObj.AddComponent<WallHole>();
            wall.targetLeftArm = Random.Range(leftArmController.minAngle, leftArmController.maxAngle);
            wall.targetRightArm = Random.Range(rightArmController.minAngle, rightArmController.maxAngle);
            wall.targetLeftLeg = Random.Range(leftLegController.minAngle, leftLegController.maxAngle);
            wall.targetRightLeg = Random.Range(rightLegController.minAngle, rightLegController.maxAngle);
            wall.targetHead = Random.Range(headController.minAngle, headController.maxAngle);
            
            if (showTargetAngles)
            {
                CreateTargetVisuals(wallObj, wall);
            }
            
            generatedWalls[i] = wall;
            
            Debug.Log($"Generated Wall {i + 1}: LA={wall.targetLeftArm:F1}° RA={wall.targetRightArm:F1}° " +
                     $"LL={wall.targetLeftLeg:F1}° RL={wall.targetRightLeg:F1}° H={wall.targetHead:F1}°");
        }
        
        return generatedWalls;
    }

    private void CreateTargetVisuals(GameObject wallParent, WallHole wall)
    {
        CreateTargetLine(wallParent.transform, new Vector3(-2.07f, 1.63f, 0), wall.targetLeftArm, "LA", Color.cyan);
        CreateTargetLine(wallParent.transform, new Vector3(2.07f, 1.63f, 0), wall.targetRightArm, "RA", Color.cyan);
        CreateTargetLine(wallParent.transform, new Vector3(-1.06f, -2.52f, 0), wall.targetLeftLeg, "LL", Color.magenta);
        CreateTargetLine(wallParent.transform, new Vector3(1.06f, -2.52f, 0), wall.targetRightLeg, "RL", Color.magenta);
        CreateTargetLine(wallParent.transform, new Vector3(0f, 3.23f, 0), wall.targetHead, "H", Color.yellow);
    }

    private void CreateTargetLine(Transform parent, Vector3 localPos, float angle, string label, Color color)
    {
        GameObject indicator = new GameObject($"Target_{label}");
        indicator.transform.parent = parent;
        indicator.transform.localPosition = localPos;
        indicator.transform.localRotation = Quaternion.Euler(0, 0, angle);
        
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        
        float length = label == "H" ? 2f : 3f;
        int pixelHeight = (int)(length * 100);
        Texture2D lineTexture = new Texture2D(20, pixelHeight);
        Color[] linePixels = new Color[20 * pixelHeight];
        for (int i = 0; i < linePixels.Length; i++)
        {
            linePixels[i] = color;
        }
        lineTexture.SetPixels(linePixels);
        lineTexture.Apply();
        
        Sprite lineSprite = Sprite.Create(lineTexture, new Rect(0, 0, 20, pixelHeight), new Vector2(0.5f, 0.5f), 100);
        
        sr.sprite = lineSprite;
        sr.sortingOrder = 2;
        
        Debug.Log($"Created target line for {label} at local position {localPos} with angle {angle}");
    }

    public WallHole[] GetGeneratedWalls()
    {
        return generatedWalls;
    }
}