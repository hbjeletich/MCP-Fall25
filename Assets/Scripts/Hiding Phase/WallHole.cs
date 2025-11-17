using UnityEngine;

public class WallHole : MonoBehaviour
{
    [Header("Target Limb Angles")]
    public float targetLeftArm = 0f;
    public float targetRightArm = 0f;
    public float targetLeftLeg = 0f;
    public float targetRightLeg = 0f;
    public float targetHead = 0f;
    
    [Header("Tolerance")]
    public float angleTolerance = 10f;

    public bool CheckMatch(PlayerLimbController[] limbs)
    {
        foreach (var limb in limbs)
        {
            float targetAngle = GetTargetAngle(limb.limbName);
            float limbAngle = limb.GetCurrentAngle();
            
            if (Mathf.Abs(limbAngle - targetAngle) > angleTolerance)
            {
                Debug.Log($"{limb.limbName} mismatch! Target: {targetAngle}, Current: {limbAngle}");
                return false;
            }
        }
        
        return true;
    }

    private float GetTargetAngle(string limbName)
    {
        switch (limbName)
        {
            case "LeftArm": return targetLeftArm;
            case "RightArm": return targetRightArm;
            case "LeftLeg": return targetLeftLeg;
            case "RightLeg": return targetRightLeg;
            case "Head": return targetHead;
            default: return 0f;
        }
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
