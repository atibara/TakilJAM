using UnityEngine;
using System.Collections;

public class ShadowController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public Transform[] waypoints;
    public float speed = 2f;
    public Sprite defaultSprite;
    
    [Header("Referanslar")]
    [SerializeField] private AimManager aimManager; // AimScript'inin olduğu objeyi buraya at

    // State
    private RoleData currentRole;
    private RoleSlot sourceSlot;
    private bool hasRole = false;
    private bool isStunned = false; 
    private float nextFireTime = 0f;
    
    // Bileşenler
    private SpriteRenderer myRenderer;
    private Rigidbody2D myRb; 
    private int wpIndex = 0;

    // Mouse Çakışma Çözümü İçin
    private Vector3 clickStartPos;
    private bool isDragging = false;

    void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myRb = GetComponent<Rigidbody2D>();
        
        if(defaultSprite == null) defaultSprite = myRenderer.sprite;

        if (aimManager == null)
        {
            aimManager = GetComponentInChildren<AimManager>();
        }
    }

    void Update()
    {
        // GameManager kontrolü (Eğer GameManager sahne yoksa hata vermemesi için ? kullandık)
        if (GameManager.Instance == null || !GameManager.Instance.isLevelStarted) return;
        if (isStunned) return; 

        Patrol();

        // --- ATEŞ ETME ---
        if (hasRole && currentRole != null && currentRole.projectilePrefab != null)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + currentRole.fireRate;
            }
        }
    }

    // --- MOUSE INPUTLARI ---

    void OnMouseDown()
    {
        if (GameManager.Instance.isLevelStarted || !hasRole) return;

        clickStartPos = Input.mousePosition;
        isDragging = false;
    }

    void OnMouseDrag()
    {
        if (GameManager.Instance.isLevelStarted || !hasRole) return;

        // 10 pikselden fazla oynattıysa sürükleme moduna geç
        if (Vector3.Distance(Input.mousePosition, clickStartPos) > 10f)
        {
            isDragging = true;

            if (aimManager != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;

                Vector3 pivotPos = aimManager.transform.position;
                Vector3 direction = mousePos - pivotPos;
                
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                aimManager.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    void OnMouseUp()
    {
        if (GameManager.Instance.isLevelStarted || !hasRole) return;

        // Sürüklemediyse, sadece tıkladıysa -> ROLÜ SİL
        if (!isDragging)
        {
            RemoveRole();
        }
    }

    // --- FONKSİYONLAR ---

    void Shoot()
    {
        if (aimManager == null) return;

        Transform fp = aimManager.GetFirePoint();

        if (fp != null)
        {
            Instantiate(currentRole.projectilePrefab, fp.position, fp.rotation);
        }
    }

    void RemoveRole()
    {
        if (sourceSlot != null) sourceSlot.MarkAsReturned();
        
        hasRole = false;
        currentRole = null;
        sourceSlot = null;
        myRenderer.sprite = defaultSprite;

        if (aimManager != null)
        {
            aimManager.HideAim();
        }
    }

    public void AssignRole(RoleData newRole, RoleSlot slot) 
    {
        currentRole = newRole;
        sourceSlot = slot;
        hasRole = true;
        if (newRole.preStartSprite != null) myRenderer.sprite = newRole.preStartSprite;
        
        if (aimManager != null)
        {
            aimManager.ShowAim();
        }
    }

    public void ActivateBattleMode()
    {
        if (hasRole && currentRole != null && currentRole.inGameSprite != null) 
            myRenderer.sprite = currentRole.inGameSprite;

        if (aimManager != null)
        {
            aimManager.LockAndHideAim();
        }
    }

    // DÜZELTME: velocity -> linearVelocity (Unity 6)
    IEnumerator StunRoutine()
    {
        isStunned = true; 
        yield return new WaitForSeconds(1.5f); 
        
        isStunned = false;
        myRb.linearVelocity = Vector2.zero; // Unity 6 için linearVelocity
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;
        Transform target = waypoints[wpIndex];
        
        Vector2 newPos = Vector2.MoveTowards(myRb.position, target.position, speed * Time.deltaTime);
        myRb.MovePosition(newPos);
        
        if (target.position.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0); 
        else
            transform.rotation = Quaternion.Euler(0, 180, 0); 

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            wpIndex = (wpIndex + 1) % waypoints.Length;
    }

    // Diğer scriptler çağırıyorsa diye Die fonksiyonu
    public void Die()
    {
        Destroy(gameObject);
    }
    
    public void GetStunned()
    {
        if(!isStunned) StartCoroutine(StunRoutine());
    }

    public bool HasRole() => hasRole;
}