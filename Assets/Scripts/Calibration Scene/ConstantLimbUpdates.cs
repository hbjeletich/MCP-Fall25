using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class ConstantLimbUpdates : MonoBehaviour
{
    [Header("Limb Settings")]
    public InputManager.LimbPlayer limbPlayer;
    private SpriteResolver spriteResolver;

    void Start()
    {
        spriteResolver = GetComponent<SpriteResolver>();

        if (spriteResolver == null)
        {
            Debug.LogError($"{gameObject.name}: No SpriteResolver found on the GameObject. Needed for ConstantLimbUpdates.");
            return;
        }
    }

    void Update()
    {
        LoadSavedSkin();
    }

    void LoadSavedSkin()
    {
        if (SkinManager.Instance == null || spriteResolver == null) return;

        string category = SkinManager.Instance.GetCategoryForLimb(limbPlayer);
        string savedLabel = SkinManager.Instance.LoadSkin(limbPlayer);

        if (!string.IsNullOrEmpty(savedLabel))
        {
            spriteResolver.SetCategoryAndLabel(category, savedLabel);
        }
    }
}
