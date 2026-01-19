using Shrimp5.UIContract;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image icon;

    private string tooltipText;
    private IBattleUIActions controller;

    public void Configure(string iconID, string description)
    {
        icon.sprite = SpriteResolver.Get(iconID);
        tooltipText = description;
    }

    private IBattleUIActions GetController()
    {
        if (controller == null)
        {
            BattleController controllerFind = FindAnyObjectByType<BattleController>();
            if (controllerFind != null)
            {
                controller = controllerFind;
            }
        }

        return controller;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetController()?.SetTooltipTarget(tooltipText);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        GetController()?.SetTooltipTarget("");
    }
}
