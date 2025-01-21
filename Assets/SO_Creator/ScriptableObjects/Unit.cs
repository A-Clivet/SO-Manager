using CustomAttributes;
using UnityEngine;

[IncludeInSOCreator]
public class Unit : ScriptableObject
{
    [RequiredField] public string Name;
    [RequiredField] public Sprite Image;
    [RequiredField] [SliderField(1,100)] public int PV;
    [RequiredField] [SliderField(0,100)] public int ATK;

    public bool HasCapacity;

    [ShowIf("HasCapacity")]
    [RequiredField]
    public string CapacityName;

    [ShowIf("HasCapacity")]
    [RequiredField]
    public string CapacityDescription;

    [ShowIf("HasCapacity")]
    [RequiredField]
    public Sprite CapacityImage;
}