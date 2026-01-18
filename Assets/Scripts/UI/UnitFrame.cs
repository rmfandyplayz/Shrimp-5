using Shrimp5.UIContract;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// written by andy
// used to display info about the player or enemy
public class UnitFrame : MonoBehaviour
{
    [Header("references")]
    [SerializeField] Image portraitImage;
    [SerializeField] Image hpBarFill;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] RectTransform statusIconContainer; // horizontal layout group for these icons

    // meant to be called from the manager
    public void Render(HudData hudData)
    {
        nameText.text = hudData.teammateName;

        hpBarFill.fillAmount = (float)hudData.hp / hudData.maxHp;

        hpText.text = $"{hudData.hp} / {hudData.maxHp} HP";

        portraitImage.sprite = SpriteResolver.Get(hudData.portraitIconID);
    }
}
