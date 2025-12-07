using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public List<Transform> waypoints; 
    public float speed = 2f;
    public Sprite defaultSprite;
    public Sprite defaultPatrolSprite; 
    
    [Header("Referanslar")]
    [SerializeField] private Transform firePoint;     
    [SerializeField] private GameObject directionArrow; 

    // State
    private RoleData currentRole;
    private RoleSlot sourceSlot;
    private bool hasRole = false;
    private bool isStunned = false; 
    private float nextFireTime = 0f;
    private int targetIndex = 0; 

    // Bileşenler
    private SpriteRenderer myRenderer;
    private Rigidbody2D myRb; 

    void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        myRb = GetComponent<Rigidbody2D>();
        
        if(defaultSprite == null) defaultSprite = myRenderer.sprite;
        
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.isLevelStarted)
        {
            // OYUN BAŞLADI
            if (!isStunned) // Sersemlemediyse hareket et
            {
                if (hasRole && currentRole.isRunner)
                {
                    MoveStraight();
                }
                else
                {
                    Patrol();
                    HandleShooting();
                }
                
                if(directionArrow != null) directionArrow.SetActive(false);
            }
        }
        else
        {
            // HAZIRLIK EVRESİ
            if(directionArrow != null) directionArrow.SetActive(true);
            DetermineFirstTarget();
            // Hazırlıkta yerinde dursun ama yerçekimi çalışsın
            myRb.linearVelocity = new Vector2(0, myRb.linearVelocity.y); 
        }
    }

    // --- YENİLENEN HAREKET MANTIĞI (VELOCITY) ---

    void MoveStraight()
    {
        // Y eksenindeki hızı (yerçekimini) koru, sadece X'i değiştir
        float moveSpeed = currentRole.runSpeed;
        
        // Karakter sağa bakıyorsa (+1), sola bakıyorsa (-1)
        float direction = (transform.rotation.eulerAngles.y == 0) ? 1f : -1f;
        
        myRb.linearVelocity = new Vector2(direction * moveSpeed, myRb.linearVelocity.y);
    }

    void Patrol()
    {
        if (waypoints.Count == 0) return;
        Transform target = waypoints[targetIndex];

        // Hedefe olan yatay yönü bul
        float direction = (target.position.x > transform.position.x) ? 1f : -1f;
        
        // Hızı uygula (Yine Y eksenine dokunmadan)
        myRb.linearVelocity = new Vector2(direction * speed, myRb.linearVelocity.y);
        
        // Yüzünü dön
        if (direction > 0) transform.rotation = Quaternion.Euler(0, 0, 0); 
        else transform.rotation = Quaternion.Euler(0, 180, 0); 

        // Hedefe X ekseninde yaklaştı mı? (Y farkını önemsemiyoruz artık)
        if (Mathf.Abs(transform.position.x - target.position.x) < 0.2f)
        {
            targetIndex = (targetIndex + 1) % waypoints.Count;
        }
    }

    // --- STUN (SERSEMLEME) MANTIĞI ---

    public void GetStunned()
    {
        // Zaten sersemlemişse süreyi sıfırla veya tekrar başlatma
        if(!isStunned) StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        isStunned = true; 
        
        // Sersemlediğinde kontrolü bırak, fizik motoru savursun.
        // Hızı sıfırlamıyoruz! Merminin verdiği itme gücü (force) işlesin.
        
        yield return new WaitForSeconds(1.5f); 
        
        isStunned = false;
        
        // Sersemleme bitince sadece X hızını sıfırla, Y kalsın (düşüyorsa düşsün)
        myRb.linearVelocity = new Vector2(0, myRb.linearVelocity.y); 
    }

    // --- DİĞER FONKSİYONLAR (Aynen Kalıyor) ---
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasRole && currentRole.isRunner && other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
            Die();
        }
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.isLevelStarted) return;
        if (hasRole) RemoveRole();
        else FlipDirection();
    }

    void FlipDirection()
    {
        float currentY = transform.rotation.eulerAngles.y;
        if (Mathf.Abs(currentY) < 1f) transform.rotation = Quaternion.Euler(0, 180, 0); 
        else transform.rotation = Quaternion.Euler(0, 0, 0); 
        DetermineFirstTarget();
    }

    void DetermineFirstTarget()
    {
        if (waypoints.Count < 2) return;
        bool facingRight = (Mathf.Abs(transform.rotation.eulerAngles.y) < 1f);
        Transform bestTarget = waypoints[0];
        foreach (Transform wp in waypoints)
        {
            if (facingRight) { if (wp.position.x > bestTarget.position.x) bestTarget = wp; }
            else { if (wp.position.x < bestTarget.position.x) bestTarget = wp; }
        }
        targetIndex = waypoints.IndexOf(bestTarget);
    }

    void HandleShooting()
    {
        if (hasRole && currentRole != null && currentRole.projectilePrefab != null)
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
        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
        Quaternion facingRotation = transform.rotation;
        Quaternion finalRotation = facingRotation * Quaternion.Euler(0, 0, currentRole.shootAngle);
        Instantiate(currentRole.projectilePrefab, spawnPos, finalRotation);
    }

    public void AssignRole(RoleData newRole, RoleSlot slot) 
    {
        currentRole = newRole;
        sourceSlot = slot;
        hasRole = true;
        if (newRole.preStartSprite != null) myRenderer.sprite = newRole.preStartSprite;
    }

    void RemoveRole()
    {
        if (sourceSlot != null) sourceSlot.MarkAsReturned();
        hasRole = false;
        currentRole = null;
        sourceSlot = null;
        myRenderer.sprite = defaultSprite; 
    }

    public void ActivateBattleMode()
    {
        if (hasRole && currentRole != null && currentRole.inGameSprite != null) 
            myRenderer.sprite = currentRole.inGameSprite;
        else if (!hasRole && defaultPatrolSprite != null)
            myRenderer.sprite = defaultPatrolSprite;
    }

    public void Die() {
        Destroy(gameObject); 
    }

    public bool HasRole() => hasRole;
}