using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingObject : MonoBehaviour
{
    [Header("Safe Zone")]
    public EdgeCollider2D safeZoneCollider;
    public Vector3 safeZoneOffset = new Vector3 (0, -2.93f, 0);

    void Awake()
    {
        if (safeZoneCollider == null)
        {
            safeZoneCollider = GetComponent<EdgeCollider2D>();
        }

        if (safeZoneCollider != null)
        {
            //safeZoneCollider.edgeRadius = 1f;
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
        
        foreach (var limbRenderer in limbRenderers)
        {
            if (limbRenderer == null || limbRenderer.sprite == null) continue;
            
            if (!CheckSpritePixels(limbRenderer))
            {
                return false;
            }
        }
        
        return true;
    }

    private bool CheckSpritePixels(SpriteRenderer spriteRenderer)
    {
        Sprite sprite = spriteRenderer.sprite;
        Texture2D texture = sprite.texture;
        
        if (!texture.isReadable)
        {
            Debug.LogError($"Texture for {spriteRenderer.name} is not readable! Enable Read/Write in import settings.");
            return false;
        }
        
        Rect textureRect = sprite.textureRect;
        int startX = (int)textureRect.x;
        int startY = (int)textureRect.y;
        int width = (int)textureRect.width;
        int height = (int)textureRect.height;
        
        Color[] pixels = texture.GetPixels(startX, startY, width, height);
        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 pivot = sprite.pivot;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[y * width + x];
                
                if (pixel.a > 0.01f)
                {
                    Vector2 localPixelPos = new Vector2(
                        (x - pivot.x) / pixelsPerUnit,
                        (y - pivot.y) / pixelsPerUnit
                    );
                    
                    Vector2 worldPixelPos = spriteRenderer.transform.TransformPoint(localPixelPos);
                    
                    if (Physics2D.OverlapCircle(worldPixelPos, 0.05f, LayerMask.GetMask("Object")))
                    {
                        Debug.Log($"{spriteRenderer.name} has pixel near edge at {worldPixelPos}");
                        return false;
                    }
                }
            }
        }
        
        return true;
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