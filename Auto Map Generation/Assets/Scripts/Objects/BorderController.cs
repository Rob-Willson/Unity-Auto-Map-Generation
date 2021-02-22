using UnityEngine;

public class BorderController : MonoBehaviour
{
    public SpriteRenderer thisSpriteRenderer;
    LayerMask layerMaskGround = 1 << 8;

    Vector3[] borderingVectors = { new Vector3(0, 1), new Vector3(1, 1), new Vector3(1, 0), new Vector3(1, -1), new Vector3(0, -1), new Vector3(-1, -1), new Vector3(-1, 0), new Vector3(-1, 1) };
    Vector3[] raycastLocations = new Vector3[8];
    RaycastHit2D[] raycastHits = new RaycastHit2D[8];

    [Tooltip("The ID of the tile under this border, assigned by LevelGenerator")] public string tileUnderID;
    public string[] adjacentTileIDs = new string[8];

    public bool[] isBorderRequired = new bool[8];

    void OnEnable ()
    {
        thisSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnDisable ()
    {
        StopAllCoroutines();
        thisSpriteRenderer.sprite = null;
    }

    public void Initialize ()
    {
        GetBorderSprite();
    }

    void GetBorderSprite ()
    {
        for(int i = 0; i < raycastLocations.Length; i++)
        {
            raycastLocations[i].x = transform.position.x + borderingVectors[i].x;
            raycastLocations[i].y = transform.position.y + borderingVectors[i].y;
            raycastHits[i] = Physics2D.Raycast(raycastLocations[i], Vector2.zero, 10f, layerMaskGround);
            if(raycastHits[i])
            {
                ThingData raycastThingData = raycastHits[i].collider.GetComponent<ThingData>();
                if(raycastThingData != null /* && raycastThingData.id == borderID*/)
                {
                    adjacentTileIDs[i] = raycastThingData.id;
                }
            }
        }
        thisSpriteRenderer.sprite = BorderGeneratorOverlord.instance.CalculateBorderRequirements(adjacentTileIDs, this);
    }

}
