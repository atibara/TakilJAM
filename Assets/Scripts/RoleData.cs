using UnityEngine;

[CreateAssetMenu(fileName = "New Role", menuName = "Game/New Role")]
public class RoleData : ScriptableObject
{
    public string roleName;
    
    [Header("Görseller")]
    public Sprite preStartSprite; 
    public Sprite inGameSprite;
    
    [Header("Savaş Ayarları")]
    public GameObject projectilePrefab; // Mermi Prefab'ı buraya gelecek
    public float fireRate = 1f; // Ne sıklıkla ateş etsin?
}