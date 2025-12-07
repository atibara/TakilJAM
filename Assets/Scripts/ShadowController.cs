using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
        
        // Başlangıç rotasyonu
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.isLevelStarted)
        {
            // --- OYUN BAŞLADI ---
            if (!isStunned)
            {
                // EĞER MOTORCUYSA (Runner) -> Düz Git
                if (hasRole && currentRole.isRunner)
                {
                    MoveStraight();
                    // Motorcu ateş etmez, kendisi çarpar. O yüzden HandleShooting yok.
                }
                // DEĞİLSE (Normal Rol) -> Devriye Gez + Ateş Et
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
            // --- HAZIRLIK ---
            if(directionArrow != null) directionArrow.SetActive(true);
            DetermineFirstTarget();
        }
    }

    // --- YENİ: MOTORCU HAREKETİ ---
    void MoveStraight()
    {
        // Karakter nereye bakıyorsa (Sağ/Sol) o yöne, RoleData'daki hızıyla gitsin
        float moveSpeed = currentRole.runSpeed;
        
        // transform.right = Karakterin baktığı yön (Kırmızı ok)
        myRb.linearVelocity = transform.right * moveSpeed; 
    }
    
    // --- ÇARPIŞMA (MOTORCU İÇİN) ---
    void OnTriggerEnter2D(Collider2D other)
    {
        // Şartları sadeleştirdik:
        // Sadece çarptığım şey "Enemy" ise çalış.
        // (Rolüm var mı yok mu, motorcu muyum bakma.)

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Düşmana çarptım! Sahne değişiyor...");

            // 1. Düşmanı Yok Et
            Destroy(other.gameObject);

            // 2. Polis Ölsün ve Sahne Değişsin
            Die();
        }
    }

    // --- (AŞAĞISI ESKİ KODLARIN AYNISI) ---

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

    void Patrol()
    {
        if (waypoints.Count == 0) return;
        Transform target = waypoints[targetIndex];
        Vector2 newPos = Vector2.MoveTowards(myRb.position, target.position, speed * Time.deltaTime);
        myRb.MovePosition(newPos);
        
        if (target.position.x > transform.position.x) transform.rotation = Quaternion.Euler(0, 0, 0); 
        else transform.rotation = Quaternion.Euler(0, 180, 0); 

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            targetIndex = (targetIndex + 1) % waypoints.Count;
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

    public void Die()
    {
        // 1. Önce görüntüyü ve çarpışmayı kapat (Sanki ölmüş gibi görünsün)
        if (myRenderer != null) myRenderer.enabled = false;

        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol != null) myCol.enabled = false;

        // Hareketi durdur
        if (myRb != null) myRb.linearVelocity = Vector2.zero;

        // 2. Script hala hayatta olduğu için Invoke artık çalışabilir
        Invoke(nameof(LoadSampleScene), 0.5f);

        // Not: Destroy(gameObject) yapmana gerek yok, sahne değişince zaten yok olacak.
    }

    void LoadSampleScene()
    {
        SceneManager.LoadScene("SampleScene 2");  // değiştirmek istediğin sahne adı
    }

    public void GetStunned() { if(!isStunned) StartCoroutine(StunRoutine()); }

    IEnumerator StunRoutine()
    {
        isStunned = true; 
        yield return new WaitForSeconds(1.5f); 
        isStunned = false;
        myRb.linearVelocity = Vector2.zero; 
    }

    public bool HasRole() => hasRole;
}