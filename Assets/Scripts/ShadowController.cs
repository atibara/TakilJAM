using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShadowController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public List<Transform> waypoints; 
    public float speed = 2f;
    
    [Header("Görseller")]
    public Sprite defaultSprite;        // Oyun başlamadan önceki sabit duruş (Idle)
    public Sprite defaultPatrolSprite;  // YENİ: Oyun başlayınca rolü yoksa yürüyüş hali (Patrol)

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
        
        // Başlangıçta yönü ayarla
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
                Patrol();
                HandleShooting();
                
                if(directionArrow != null) directionArrow.SetActive(false);
            }
        }
        else
        {
            // --- HAZIRLIK EVRESİ ---
            if(directionArrow != null) directionArrow.SetActive(true);
            DetermineFirstTarget();
        }
    }

    // --- TIKLAMA VE YÖN ---

    void OnMouseDown()
    {
        if (GameManager.Instance.isLevelStarted) return;

        if (hasRole)
        {
            RemoveRole();
        }
        else
        {
            FlipDirection();
        }
    }

    void FlipDirection()
    {
        float currentY = transform.rotation.eulerAngles.y;

        if (Mathf.Abs(currentY) < 1f) 
            transform.rotation = Quaternion.Euler(0, 180, 0); 
        else
            transform.rotation = Quaternion.Euler(0, 0, 0); 

        DetermineFirstTarget();
    }

    // --- DEVRİYE ---

    void DetermineFirstTarget()
    {
        if (waypoints.Count < 2) return;

        bool facingRight = (Mathf.Abs(transform.rotation.eulerAngles.y) < 1f);
        Transform bestTarget = waypoints[0];

        foreach (Transform wp in waypoints)
        {
            if (facingRight)
            {
                if (wp.position.x > bestTarget.position.x) bestTarget = wp;
            }
            else
            {
                if (wp.position.x < bestTarget.position.x) bestTarget = wp;
            }
        }
        targetIndex = waypoints.IndexOf(bestTarget);
    }

    void Patrol()
    {
        if (waypoints.Count == 0) return;

        Transform target = waypoints[targetIndex];
        
        Vector2 newPos = Vector2.MoveTowards(myRb.position, target.position, speed * Time.deltaTime);
        myRb.MovePosition(newPos);
        
        if (target.position.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0); 
        else
            transform.rotation = Quaternion.Euler(0, 180, 0); 

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            targetIndex = (targetIndex + 1) % waypoints.Count;
        }
    }

    // --- ATEŞ ETME ---

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
        
        // AÇI HESAPLAMA (RoleData'dan gelen açı)
        Quaternion facingRotation = transform.rotation;
        Quaternion finalRotation = facingRotation * Quaternion.Euler(0, 0, currentRole.shootAngle);

        Instantiate(currentRole.projectilePrefab, spawnPos, finalRotation);
    }

    // --- ROL VE GÖRSEL YÖNETİMİ (GÜNCELLENDİ) ---

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
        
        // Rolü bıraktığında varsayılan (sabit) haline dönsün
        myRenderer.sprite = defaultSprite; 
    }

    public void ActivateBattleMode()
    {
        // 1. Durum: ROLÜ VARSA -> Rolün savaş görselini giy
        if (hasRole && currentRole != null && currentRole.inGameSprite != null) 
        {
            myRenderer.sprite = currentRole.inGameSprite;
        }
        // 2. Durum: ROLÜ YOKSA (Default Haldeyse) -> Yürüyüş görselini giy (YENİ KISIM)
        else if (!hasRole && defaultPatrolSprite != null)
        {
            myRenderer.sprite = defaultPatrolSprite;
        }
    }

    // --- HASAR ---

    public void Die()
    {
        Destroy(gameObject); 
    }

    public void GetStunned()
    {
        if(!isStunned) StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        isStunned = true; 
        yield return new WaitForSeconds(1.5f); 
        isStunned = false;
        myRb.linearVelocity = Vector2.zero; 
    }

    public bool HasRole() => hasRole;
}