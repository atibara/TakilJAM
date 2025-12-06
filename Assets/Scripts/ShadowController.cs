using UnityEngine;

public class ShadowController : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform[] waypoints;
    public float speed = 2f;
    public Sprite defaultSprite; // Gölgenin boş hali

    // State
    private RoleData currentRole;
    private RoleSlot sourceSlot;
    private bool hasRole = false;
    
    private SpriteRenderer myRenderer;
    private int wpIndex = 0;

    void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        if(defaultSprite == null) defaultSprite = myRenderer.sprite;
    }

    void Update()
    {
        Patrol();

        // Aksiyon mantığı (Sadece oyun başladıysa)
        if (GameManager.Instance.isLevelStarted && hasRole)
        {
            // if(currentRole.roleName == "Polis") { AteşEt(); }
        }
    }

    // --- ROL ALMA ---
    public void AssignRole(RoleData newRole, RoleSlot slot)
    {
        currentRole = newRole;
        sourceSlot = slot;
        hasRole = true;

        // Oyun henüz başlamadığı için "Hazırlık" resmini giy
        if (newRole.preStartSprite != null)
            myRenderer.sprite = newRole.preStartSprite;
    }

    // --- OYUN BAŞLAYINCA ÇAĞRILACAK FONKSİYON ---
    public void ActivateBattleMode()
{
    Debug.Log("1. Savaş Modu Çağrısı Geldi!"); // GameManager buraya ulaşıyor mu?

    if (!hasRole)
    {
        Debug.Log("2. HATA: Rol atanmamış görünüyor!");
        return;
    }

    if (currentRole.inGameSprite == null)
    {
        Debug.Log("3. HATA: RoleData dosyasında 'In Game Sprite' resmi BOŞ!");
        return;
    }

    // Her şey tamamsa değiştir
    myRenderer.sprite = currentRole.inGameSprite;
    Debug.Log("4. BAŞARI: Resim değiştirildi.");
    }

    public bool HasRole() => hasRole;

    void Patrol()
    {
        if (waypoints.Length == 0) return;
        Transform target = waypoints[wpIndex];
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            wpIndex = (wpIndex + 1) % waypoints.Length;
    }
}