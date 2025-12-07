using UnityEngine;

[CreateAssetMenu(fileName = "New Role", menuName = "Game/New Role")]
public class RoleData : ScriptableObject
{
    public string roleName;
    
    [Header("Görseller")]
    public Sprite preStartSprite; 
    public Sprite inGameSprite;
    
    [Header("Savaş Ayarları")]
    public GameObject projectilePrefab; 
    public float fireRate = 1f;
    
    // --- YENİ EKLENEN KISIM ---
    [Range(-180f, 180f)] // Editörde kaydırma çubuğu çıkarır
    public float shootAngle = 0f; // 0 = Düz, pozitif = Yukarı, negatif = Aşağı
    [Header("Hareket Tipi")]
    public bool isRunner = false; // True ise: Durmaz, dümdüz gider (Motorcu)
    public float runSpeed = 8f;   // Ne kadar hızlı gitsin?
}