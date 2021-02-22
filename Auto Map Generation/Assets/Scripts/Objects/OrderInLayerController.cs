using System.Collections;
using UnityEngine;

public enum LayerType { defaultLayer, parentLayer, groundLayer, groundBorderLayer, floorLayer, groundPlantLayer, shrubLayer, treeLayer, propLayer, characterLayer, spellOverlayLayer, wallLayer, belowParentLayer, offsetLayer };

public class OrderInLayerController : MonoBehaviour
{
    public LayerType objectLayerType;
    public bool doesNotMove; // Objects which don't move around the world don't need their rendering order updating
    int sortingOrderInLayer;
    SpriteRenderer thisSprite;
    [HideInInspector] public SpriteRenderer parentSpriteRenderer;

    public bool resetSpriteColourationOnAwake;

    void Awake ()
    {
        thisSprite = GetComponent<SpriteRenderer>();

        if(objectLayerType == LayerType.parentLayer || objectLayerType == LayerType.belowParentLayer)
        {
            parentSpriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
        }
        if(resetSpriteColourationOnAwake)
        {
            thisSprite.color = Color.white;
        }
    }

    void OnEnable ()
    {
        // Only update position if object moves
        if(!doesNotMove)
        {
            StartCoroutine(UpdateCoroutine());
        }
        else
        {
            OrderObjectsInLayer();
        }
    }

    private void OnDisable ()
    {
        StopCoroutine(UpdateCoroutine());
    }

    void Start ()
    {
        OrderObjectsInLayer();
    }

    IEnumerator UpdateCoroutine ()
    {
        while(true)
        {
            OrderObjectsInLayer();
            yield return null;
        }
    }

    public void OrderObjectsInLayer ()
    {
        // Just order it based on parent layer, skipping any further calculations
        if(objectLayerType == LayerType.belowParentLayer)
        {
            sortingOrderInLayer = parentSpriteRenderer.sortingOrder - 1;
        }
        else if(objectLayerType == LayerType.parentLayer)
        {
            sortingOrderInLayer = parentSpriteRenderer.sortingOrder + 1;
        }

        // Otherwise, order it based on position in map
        else
        {
            sortingOrderInLayer = Mathf.RoundToInt(transform.position.y * -20);
            switch(objectLayerType)
            {
                case LayerType.defaultLayer:    // If no layer type assigned give it the default
                    break;
                case LayerType.groundLayer:     // ground is a non-selectable/interactable layer
                    sortingOrderInLayer -= 20;
                    break;
                case LayerType.groundBorderLayer:     // ground borders should be placed above ground
                    sortingOrderInLayer -= 19;
                    break;
                case LayerType.groundPlantLayer:
                    sortingOrderInLayer -= 15;
                    break;
                case LayerType.shrubLayer:
                    sortingOrderInLayer -= 2;
                    break;
                case LayerType.treeLayer:
                    sortingOrderInLayer += 2;
                    break;
                case LayerType.floorLayer:
                    //Nothing
                    break;
                case LayerType.characterLayer:
                    sortingOrderInLayer += 10;
                    break;
                case LayerType.spellOverlayLayer:
                    sortingOrderInLayer += 12;
                    break;
                case LayerType.propLayer:
                    sortingOrderInLayer += 2;
                    break;
                case LayerType.wallLayer:
                    break;
                case LayerType.offsetLayer:
                    sortingOrderInLayer += 25;
                    break;
                default:
                    break;
            }
        }

        if(thisSprite)
        {
            thisSprite.sortingOrder = sortingOrderInLayer;
        }
    }

}
