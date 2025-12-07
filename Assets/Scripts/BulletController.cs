using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    [SerializeField] private float speed = 20f;      // Mermi hızı (Yüksek tut)
    [SerializeField] private float lifeTime = 3f;    // Çarpmazsa kaç saniyede yok olsun?

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Mermiyi fırlatıldığı yöne (Right) doğru hızlandır
        rb.linearVelocity = transform.right * speed;

        // Çarpışma olmazsa belli süre sonra sahneden temizle
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Düşmana çarparsa
        if (other.CompareTag("Enemy"))
        {
            // Düşmanı yok et
            Destroy(other.gameObject);
            
            // Kendini yok et
            Destroy(gameObject);
        }
        // Duvara veya zemine çarparsa
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            // Sadece kendini yok et
            Destroy(gameObject);
        }
    }
}