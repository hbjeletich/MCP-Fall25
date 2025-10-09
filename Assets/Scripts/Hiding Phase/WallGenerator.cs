using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    public int numberOfWalls = 10;
    public PlayerLimbController headController;
    public PlayerLimbController leftArmController;
    public PlayerLimbController rightArmController;
    public PlayerLimbController leftLegController;
    public PlayerLimbController rightLegController;
    public bool showTargetAngles = true;
    
    private WallHole[] generatedWalls;

    public WallHole[] GenerateWalls()
    {
        generatedWalls = new WallHole[numberOfWalls];
        
        for (int i = 0; i < numberOfWalls; i++)
        {
            // create wall game object
            GameObject wallObj = new GameObject($"Wall_{i + 1}");
            wallObj.transform.parent = transform;
            wallObj.transform.position = Vector3.zero;

            // fade effect "wall" that doens't really do much now, starts transparent
            SpriteRenderer sr = wallObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSquareSprite();
            sr.color = new Color(0.8f, 0.8f, 0.8f, 0f);
            sr.sortingOrder = -1; // behind character
            
            // add wall hole component and randomize angles based limb range
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
        }
        
        return generatedWalls;
    }

    private void CreateTargetVisuals(GameObject wallParent, WallHole wall)
    {
        // create simple line renderers showing where each limb should be
        // position match character limb attachment points
        // hardcoded in there which i know is not good!
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
        
        // sprite instead of line renderer because i dont like line renderer
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();

        // setting lengths and things (used up a lot of brain power here)
        float length = label == "H" ? 2f : 3f; // if head, 2, else 3
        int pixelHeight = (int)(length * 100);
        Texture2D lineTexture = new Texture2D(20, pixelHeight);
        Color[] linePixels = new Color[20 * pixelHeight];
        for (int i = 0; i < linePixels.Length; i++)
        {
            linePixels[i] = color;
        }
        lineTexture.SetPixels(linePixels);
        lineTexture.Apply();
        
        // create sprite from texture, centered pivot
        Sprite lineSprite = Sprite.Create(lineTexture, new Rect(0, 0, 20, pixelHeight), new Vector2(0.5f, 0.5f), 100);
        
        sr.sprite = lineSprite;
        sr.sortingOrder = 2; // in front of character
    }

    private Sprite CreateSimpleSquareSprite()
    {
        // create bigger wall in front of character
        Texture2D texture = new Texture2D(800, 1000);
        Color[] pixels = new Color[800 * 1000];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 800, 1000), new Vector2(0.5f, 0.5f), 100);
    }

    public WallHole[] GetGeneratedWalls()
    {
        return generatedWalls;
    }
}
