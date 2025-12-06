using UnityEngine;
using System.Collections; // Coroutine için gerekli

public class ShadowController : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform[] waypoints;
    public float speed = 2f;
    public Sprite defaultSprite;
    public Transform firePoint; // Merminin çıkacağı namlu ucu (Elle oluşturacağız)

    // State
    private RoleData currentRole;
    private RoleSlot sourceSlot;
    private bool hasRole = false;
    private bool isStunned = false; // İtildi mi?
    private float nextFireTime = 0f;
    
    private SpriteRenderer myRenderer;
    private Rigidbody2D myRb; // Fizik için
    private int wpIndex = 0;

    void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myRb = GetComponent<Rigidbody2D>();
        if(defaultSprite == null) defaultSprite = myRenderer.sprite;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isLevelStarted) return;
        if (isStunned) return; // İtildiyse hareket etme/ateş etme

        Patrol();

        // --- ATEŞ ETME MANTIĞI ---
        if (hasRole && currentRole.projectilePrefab != null)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + currentRole.fireRate;
            }
        }
    }

    void Shoot()
    {
        // Mermiyi oluştur (FirePoint yoksa gölgenin merkezinden çıkar)
        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
        
        // Mermiyi yarat ve gölgenin baktığı yöne (rotation) göre ayarla
        Instantiate(currentRole.projectilePrefab, spawnPos, transform.rotation);
    }

    // --- HASAR VE ETKİLEŞİM ---

    public void Die()
    {
        // Polis vurduğunda çalışır
        Debug.Log("Gölge öldü!");
        
        // Patlama efekti vs. eklenebilir
        Destroy(gameObject); 
    }

    public void GetStunned()
    {
        // Su çarptığında çalışır
        if(!isStunned) StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        isStunned = true; // Kontrolü kapat (Fizik motoru savursun)
        Debug.Log("Gölge ıslandı ve savruldu!");
        
        yield return new WaitForSeconds(1.5f); // 1.5 saniye sersemle
        
        // Toparlanma
        isStunned = false;
        myRb.linearVelocity = Vector2.zero; // Kaymayı durdur
    }

    // --- ESKİ FONKSİYONLAR (Aynen Kalıyor) ---
    public void AssignRole(RoleData newRole, RoleSlot slot) 
    {
        currentRole = newRole;
        sourceSlot = slot;
        hasRole = true;
        if (newRole.preStartSprite != null) myRenderer.sprite = newRole.preStartSprite;
    }

    public void ActivateBattleMode()
    {
        if (hasRole && currentRole.inGameSprite != null) myRenderer.sprite = currentRole.inGameSprite;
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.isLevelStarted || !hasRole) return;
        if(sourceSlot != null) sourceSlot.MarkAsReturned();
        hasRole = false;
        currentRole = null;
        sourceSlot = null;
        myRenderer.sprite = defaultSprite;
    }

    public bool HasRole() => hasRole;

    void Patrol()
    {
        if (waypoints.Length == 0) return;
        Transform target = waypoints[wpIndex];
        
        // Hareketi Rigidbody ile yapalım ki itilince sorun çıkmasın
        Vector2 newPos = Vector2.MoveTowards(myRb.position, target.position, speed * Time.deltaTime);
        myRb.MovePosition(newPos);
        
        // Yön Çevirme (Mermi doğru yöne gitsin diye Rotation kullanıyoruz)
        if (target.position.x > transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Sağa bak (0 derece)
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Sola bak (180 derece)
        }

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            wpIndex = (wpIndex + 1) % waypoints.Length;
    }
}