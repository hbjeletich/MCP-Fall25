using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingObject : MonoBehaviour
{
    [Header("Safe Zone")]
    public PolygonCollider2D safeZoneCollider;
    public Vector3 safeZoneOffset = new Vector3(0, -2.93f, 0);

    [Header("Detection Settings")]
    [Tooltip("How many pixels to skip when checking (higher = faster but less accurate)")]
    public int pixelCheckStep = 3;
    
    [Tooltip("Percentage of pixels that must be inside the safe zone (0-100)")]
    [Range(0, 100)]
    public float requiredInsidePercentage = 90f;
    
    [Header("Debug")]
    public bool showDetailedDebug = false;

    void Awake()
    {
        if (safeZoneCollider == null)
        {
            safeZoneCollider = GetComponent<PolygonCollider2D>();
        }

        if (safeZoneCollider != null)
        {
            safeZoneCollider.isTrigger = true;
            Debug.Log($"HidingObject {gameObject.name}: SafeZone collider found and set to trigger");
        }
        else
        {
            Debug.LogError($"HidingObject {gameObject.name}: NO POLYGON COLLIDER FOUND!");
        }

        gameObject.layer = LayerMask.NameToLayer("Object");
    }

    public bool CheckMatch(SpriteRenderer[] limbRenderers)
    {
        if (safeZoneCollider == null)
        {
            Debug.LogError("HidingObject: Safe zone collider not assigned!");
            return false;
        }

        Debug.Log("=== STARTING HIDING CHECK ===");
        Debug.Log($"SafeZone Position: {safeZoneCollider.transform.position}");
        Debug.Log($"SafeZone Bounds: {safeZoneCollider.bounds}");

        int totalPixelsChecked = 0;
        int totalPixelsInside = 0;

        foreach (var limbRenderer in limbRenderers)
        {
            if (limbRenderer == null || limbRenderer.sprite == null) 
            {
                Debug.LogWarning($"Skipping null limb renderer");
                continue;
            }

            var limbResult = CheckLimbPixels(limbRenderer);
            totalPixelsChecked += limbResult.totalPixels;
            totalPixelsInside += limbResult.insidePixels;

            Debug.Log($"{limbRenderer.name}: {limbResult.insidePixels}/{limbResult.totalPixels} pixels inside ({limbResult.percentage:F1}%)");
        }

        if (totalPixelsChecked == 0)
        {
            Debug.LogWarning("No pixels were checked!");
            return false;
        }

        float overallPercentage = (totalPixelsInside / (float)totalPixelsChecked) * 100f;
        bool success = overallPercentage >= requiredInsidePercentage;

        Debug.Log($"=== OVERALL RESULT ===");
        Debug.Log($"Total: {totalPixelsInside}/{totalPixelsChecked} pixels inside ({overallPercentage:F1}%)");
        Debug.Log($"Required: {requiredInsidePercentage}% - Result: {(success ? "SUCCESS ✓" : "FAIL ✗")}");

        return success;
    }

    private struct LimbCheckResult
    {
        public int totalPixels;
        public int insidePixels;
        public float percentage;
    }

    private LimbCheckResult CheckLimbPixels(SpriteRenderer spriteRenderer)
    {
        LimbCheckResult result = new LimbCheckResult();
        
        Sprite sprite = spriteRenderer.sprite;
        Texture2D texture = sprite.texture;

        if (!texture.isReadable)
        {
            Debug.LogError($"Texture for {spriteRenderer.name} is not readable! Enable Read/Write in import settings.");
            return result;
        }

        Rect textureRect = sprite.textureRect;
        int startX = (int)textureRect.x;
        int startY = (int)textureRect.y;
        int width = (int)textureRect.width;
        int height = (int)textureRect.height;

        Color[] pixels = texture.GetPixels(startX, startY, width, height);
        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 pivot = sprite.pivot;

        int sampleCount = 0;
        for (int y = 0; y < height; y += pixelCheckStep)
        {
            for (int x = 0; x < width; x += pixelCheckStep)
            {
                Color pixel = pixels[y * width + x];

                // only check visible pixels
                if (pixel.a > 0.1f)
                {
                    result.totalPixels++;

                    // convert pixel position to local space
                    Vector2 localPixelPos = new Vector2(
                        (x - pivot.x) / pixelsPerUnit,
                        (y - pivot.y) / pixelsPerUnit
                    );

                    // convert to world space
                    Vector2 worldPixelPos = spriteRenderer.transform.TransformPoint(localPixelPos);

                    // check if pixel is inside the safe zone
                    bool isInside = safeZoneCollider.OverlapPoint(worldPixelPos);
                    
                    if (isInside)
                    {
                        result.insidePixels++;
                    }
                }
            }
        }

        if (result.totalPixels > 0)
        {
            result.percentage = (result.insidePixels / (float)result.totalPixels) * 100f;
        }

        return result;
    }

    public void SetAlpha(float alpha)
    {
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var childRenderer in childRenderers)
        {
            Color color = childRenderer.color;
            color.a = Mathf.Clamp01(alpha);
            childRenderer.color = color;
        }
    }
}