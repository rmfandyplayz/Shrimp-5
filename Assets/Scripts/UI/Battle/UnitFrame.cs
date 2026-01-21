using Shrimp5.UIContract;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// written by andy
// used to display info about the player or enemy (hp, atk, status effects, pfp)
public class UnitFrame : MonoBehaviour
{
    [Header("references")]
    [SerializeField] Image portraitImage;
    [SerializeField] Image hpBarFill;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI attackSpeedText;

    [Header("status effects")]
    [SerializeField] RectTransform statusIconContainer; // horizontal layout group for these icons
    [SerializeField] StatusIcon statusIconPrefab;

    private List<StatusIcon> spawnedIcons = new();


    // meant to be called from the manager
    public void Render(HudData hudData)
    {
        nameText.text = hudData.teammateName;
        hpText.text = $"{hudData.hp} / {hudData.maxHp}";
        attackSpeedText.text = $"{hudData.attackSpeed}";
        attackText.text = $"{hudData.attack}";

        hpBarFill.fillAmount = (float)hudData.hp / hudData.maxHp;

        portraitImage.sprite = SpriteResolver.Get(hudData.portraitIconID);

        // handle passives/status effects
        List<List<string>> passives = hudData.passives;
        if(passives != null)
        {
            while(spawnedIcons.Count < passives.Count)
            {
                StatusIcon newIcon = Instantiate(statusIconPrefab, statusIconContainer);
                spawnedIcons.Add(newIcon);
            }

            for(int i = 0; i < spawnedIcons.Count; i++)
            {
                if (i < passives.Count)
                {
                    spawnedIcons[i].gameObject.SetActive(true);

                    // index 0 is icon image id, index 1 is icon description
                    string iconID = passives[i][0];
                    string description = (passives[i].Count > 1) ? passives[i][1] : ""; // empty if there's nothing there

                    spawnedIcons[i].Configure(iconID, description);
                }
                else
                {
                    spawnedIcons[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach(StatusIcon icon in spawnedIcons)
                icon.gameObject.SetActive(false);
        }
    }
}
