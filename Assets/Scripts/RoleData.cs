using UnityEngine;

[CreateAssetMenu(fileName = "New Role", menuName = "Game/New Role")]
public class RoleData : ScriptableObject
{
    public string roleName;
    
    [Header("Görseller")]
    public Sprite preStartSprite; // Oyun Başlamadan Önceki Hali (Örn: Hazırol duruşu)
    public Sprite inGameSprite;   // Oyun Başladıktan Sonraki Hali (Örn: Silahlı duruş)
}