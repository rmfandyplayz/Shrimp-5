using UnityEngine;
using UnityEngine.UI;

public class TooltipBehavior : MonoBehaviour
{
    [SerializeField] Vector2 mouseOffset = new Vector2(5f, -5f);

    [Header("references")]
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Canvas parentCanvas; // main canvas

    private float screenWidth;
    private float screenHeight;

    private void Start()
    {
        if(rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>(); 
        }
    }

    

    private void Update()
    {
        // move the tooltip to mouse position
        Vector2 mousePos = Input.mousePosition;
        transform.position = mousePos + mouseOffset;

        // restrict to screen (clamp to screen)
        // if too far right, flip pivot to right side
        // if too low, flip pivot to bottom
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        // 0 = left/bottom, 1 = right/top
        float newPivotX = 0;
        float newPivotY = 1; // assume top-left pivot

        // check right edge
        if(transform.position.x + rectTransform.rect.width > screenWidth)
        {
            newPivotX = 1;
        }

        if(transform.position.y - rectTransform.rect.height < 0)
        {
            newPivotY = 0;
        }

        rectTransform.pivot = new Vector2(newPivotX, newPivotY);
    }

}
