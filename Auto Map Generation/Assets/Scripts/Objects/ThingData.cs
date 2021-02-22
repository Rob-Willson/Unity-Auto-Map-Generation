using UnityEngine;

public enum ThingType { none, necklace, helmet, backpack, chest, gloves, leggings, cash, ring, boots, torch, weapon, shield, mask, secondaryWeapon, tool, tile, border, terrain };

[System.Serializable]
public class ThingData : MonoBehaviour
{
    public string id;
    [Tooltip("The color identifying a tile, used when generating the map from an image file")] public Color color;
    [Tooltip("The name of this object as displayed to the player")] public string title;
    public string titlePlural;
    [TextArea]
    [Tooltip("The description of this object as displayed to the player")]
    public string description;
    public ThingType thingType;
    public bool blockMovement;
    [Space]
    public int count;
    [Space]
    public int weight;
    public int value;
    public bool stackable;
    public bool twoHanded;
    public float moveSpeedModifier;
    public float capacityModifier;
    public int armour;
    public float autoAttackSpeed;
    public int physicalAttack;
    public int physicalDefence;
    public int magicalAttack;
    public int magicalDefence;

}
