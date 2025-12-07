using UnityEngine;

public class WaterballController : MonoBehaviour
{
    [Header("Su Topu Ayarları")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private float knockbackForce = 15f; // Değeri biraz artırdım, daha net olsun

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Mermiyi fırlat
        // Unity 6 kullanıyorsun diye linearVelocity bıraktım
        rb.linearVelocity = transform.right * speed; 

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Düşmana çarparsa
        if (other.CompareTag("Enemy"))
        {
            // 1. Fiziksel İtme Kısmı
            Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                // Düşmanın kendi hızını sıfırla ki itiş net hissedilsin
                enemyRb.linearVelocity = Vector2.zero;

                // Merminin gidiş yönünü al
                Vector2 forceDirection = rb.linearVelocity.normalized;

                // İtme uygula
                enemyRb.AddForce(forceDirection * knockbackForce, ForceMode2D.Impulse);
            }

            // 2. KODSAL DURDURMA KISMI (EKSİK OLAN BUYDU!)
            // Düşmanın ShadowController scriptine ulaş ve onu sersemlet
            ShadowController enemyScript = other.GetComponent<ShadowController>();
            
            if (enemyScript != null)
            {
                // Bu fonksiyon düşmanın 'Patrol' yapmasını 1.5 saniye engeller
                // Böylece fiziksel itme gücü boşa gitmez, düşman savrulur.
                enemyScript.GetStunned();
            }

            // Su topunu yok et
            Destroy(gameObject);
        }
        // Duvara çarparsa
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}