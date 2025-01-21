using UnityEngine;

[IncludeInSOCreator]
public class Ability : ScriptableObject
{
    [RequiredField] public string Name;
    [RequiredField] public string Description;
    [RequiredField] public Sprite Image;
}
