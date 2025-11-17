using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    [Header("Limb Skin Sprites")]
    public Sprite[] leftArmSkins;
    public Sprite[] rightArmSkins;
    public Sprite[] leftLegSkins;
    public Sprite[] rightLegSkins;
    public Sprite[] headSkins;

    private static SkinManager instance;
    private const string PREF_PREFIX = "LimbSkin_";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static SkinManager Instance
    {
        get { return instance; }
    }

    public Sprite[] GetSkinsForLimb(InputManager.LimbPlayer limb)
    {
        switch (limb)
        {
            case InputManager.LimbPlayer.LeftArm:
                return leftArmSkins;
            case InputManager.LimbPlayer.RightArm:
                return rightArmSkins;
            case InputManager.LimbPlayer.LeftLeg:
                return leftLegSkins;
            case InputManager.LimbPlayer.RightLeg:
                return rightLegSkins;
            case InputManager.LimbPlayer.Head:
                return headSkins;
            default:
                Debug.LogError($"Unknown limb type: {limb}");
                return null;
        }
    }

    public Sprite GetSkinSprite(InputManager.LimbPlayer limb, int skinIndex)
    {
        Sprite[] skins = GetSkinsForLimb(limb);
        
        if (skins == null || skins.Length == 0)
        {
            Debug.LogWarning($"No skins available for {limb}");
            return null;
        }

        skinIndex = Mathf.Clamp(skinIndex, 0, skins.Length - 1);
        return skins[skinIndex];
    }

    public int GetSkinCount(InputManager.LimbPlayer limb)
    {
        Sprite[] skins = GetSkinsForLimb(limb);
        return skins != null ? skins.Length : 0;
    }

    public void SaveSkin(InputManager.LimbPlayer limb, int skinIndex)
    {
        string key = PREF_PREFIX + limb.ToString();
        PlayerPrefs.SetInt(key, skinIndex);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved {limb} skin: {skinIndex}");
    }

    public int LoadSkin(InputManager.LimbPlayer limb)
    {
        string key = PREF_PREFIX + limb.ToString();
        int savedIndex = PlayerPrefs.GetInt(key, 0); // Default to 0
        
        // Validate the saved index is within range
        int maxIndex = GetSkinCount(limb) - 1;
        if (savedIndex > maxIndex)
        {
            savedIndex = 0;
        }
        
        return savedIndex;
    }

    public void ClearAllSkins()
    {
        foreach (InputManager.LimbPlayer limb in System.Enum.GetValues(typeof(InputManager.LimbPlayer)))
        {
            string key = PREF_PREFIX + limb.ToString();
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
        
        Debug.Log("Cleared all saved skins");
    }

    public bool ValidateSetup()
    {
        bool valid = true;

        if (leftArmSkins == null || leftArmSkins.Length == 0)
        {
            Debug.LogError("SkinManager: No LeftArm skins assigned!");
            valid = false;
        }

        if (rightArmSkins == null || rightArmSkins.Length == 0)
        {
            Debug.LogError("SkinManager: No RightArm skins assigned!");
            valid = false;
        }

        if (leftLegSkins == null || leftLegSkins.Length == 0)
        {
            Debug.LogError("SkinManager: No LeftLeg skins assigned!");
            valid = false;
        }

        if (rightLegSkins == null || rightLegSkins.Length == 0)
        {
            Debug.LogError("SkinManager: No RightLeg skins assigned!");
            valid = false;
        }

        if (headSkins == null || headSkins.Length == 0)
        {
            Debug.LogError("SkinManager: No Head skins assigned!");
            valid = false;
        }

        if (valid)
        {
            Debug.Log("SkinManager setup is valid!");
        }

        return valid;
    }
}