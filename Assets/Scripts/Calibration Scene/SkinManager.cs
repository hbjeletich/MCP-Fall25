using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class SkinManager : MonoBehaviour
{
    [Header("Sprite Library")]
    public SpriteLibraryAsset spriteLibraryAsset;
    
    [Header("Category Names")]
    public string headCategory = "Head";
    public string leftArmCategory = "LArms";
    public string rightArmCategory = "RArms";
    public string leftLegCategory = "LLegs";
    public string rightLegCategory = "RLegs";

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

    public string GetCategoryForLimb(InputManager.LimbPlayer limb)
    {
        switch (limb)
        {
            case InputManager.LimbPlayer.LeftArm:
                return leftArmCategory;
            case InputManager.LimbPlayer.RightArm:
                return rightArmCategory;
            case InputManager.LimbPlayer.LeftLeg:
                return leftLegCategory;
            case InputManager.LimbPlayer.RightLeg:
                return rightLegCategory;
            case InputManager.LimbPlayer.Head:
                return headCategory;
            default:
                Debug.LogError($"Unknown limb type: {limb}");
                return "";
        }
    }

    public List<string> GetLabelsForLimb(InputManager.LimbPlayer limb)
    {
        if (spriteLibraryAsset == null)
        {
            Debug.LogError("SkinManager: No Sprite Library Asset assigned!");
            return new List<string>();
        }

        string category = GetCategoryForLimb(limb);
        List<string> labels = new List<string>();

        var categoryLabels = spriteLibraryAsset.GetCategoryLabelNames(category);
        
        if (categoryLabels != null)
        {
            foreach (var label in categoryLabels)
            {
                labels.Add(label);
            }
        }

        return labels;
    }

    public int GetSkinCount(InputManager.LimbPlayer limb)
    {
        return GetLabelsForLimb(limb).Count;
    }

    public string GetLabelByIndex(InputManager.LimbPlayer limb, int index)
    {
        List<string> labels = GetLabelsForLimb(limb);
        
        if (labels.Count == 0)
        {
            Debug.LogWarning($"No labels found for {limb}");
            return "";
        }

        index = Mathf.Clamp(index, 0, labels.Count - 1);
        return labels[index];
    }

    public int GetIndexOfLabel(InputManager.LimbPlayer limb, string labelName)
    {
        List<string> labels = GetLabelsForLimb(limb);
        int index = labels.IndexOf(labelName);
        return index >= 0 ? index : 0; // Return 0 if not found
    }

    public void SaveSkin(InputManager.LimbPlayer limb, string labelName)
    {
        string key = PREF_PREFIX + limb.ToString();
        PlayerPrefs.SetString(key, labelName);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved {limb} skin: {labelName}");
    }

    public void SaveSkinByIndex(InputManager.LimbPlayer limb, int index)
    {
        string labelName = GetLabelByIndex(limb, index);
        SaveSkin(limb, labelName);
    }

    public string LoadSkin(InputManager.LimbPlayer limb)
    {
        string key = PREF_PREFIX + limb.ToString();
        
        string savedLabel = PlayerPrefs.GetString(key, "");
        
        List<string> labels = GetLabelsForLimb(limb);
        if (labels.Count == 0)
        {
            Debug.LogWarning($"No labels available for {limb}");
            return "";
        }

        if (string.IsNullOrEmpty(savedLabel) || !labels.Contains(savedLabel))
        {
            return labels[0];
        }

        return savedLabel;
    }

    public int LoadSkinAsIndex(InputManager.LimbPlayer limb)
    {
        string labelName = LoadSkin(limb);
        return GetIndexOfLabel(limb, labelName);
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
        if (spriteLibraryAsset == null)
        {
            Debug.LogError("SkinManager: No Sprite Library Asset assigned!");
            return false;
        }

        bool valid = true;
        string[] categories = { headCategory, leftArmCategory, rightArmCategory, leftLegCategory, rightLegCategory };
        string[] names = { "Head", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };

        for (int i = 0; i < categories.Length; i++)
        {
            var labels = spriteLibraryAsset.GetCategoryLabelNames(categories[i]);
            
            if (labels == null || !labels.GetEnumerator().MoveNext())
            {
                Debug.LogError($"SkinManager: No labels found for category '{categories[i]}' ({names[i]})");
                valid = false;
            }
            else
            {
                int count = 0;
                foreach (var label in labels)
                {
                    count++;
                }
                Debug.Log($"✓ {names[i]} ({categories[i]}): {count} skins");
            }
        }

        if (valid)
        {
            Debug.Log("SkinManager setup is valid!");
        }

        return valid;
    }
}