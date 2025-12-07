using UnityEngine;

public class WaterballController : MonoBehaviour
{
    [Header("Su Topu Ayarları")]
    [SerializeField] private float speed = 12f;          // Mermiden biraz daha yavaş olabilir
    [SerializeField] private float lifeTime = 4f;        // Menzili biraz daha uzun olabilir
    [SerializeField] private float knockbackForce = 10f; // İtme gücü

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Su topunu fırlat
        rb.linearVelocity = transform.right * speed;

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Düşmana çarparsa
        if (other.CompareTag("Enemy"))
        {
            // Düşmanın Rigidbody'sini al (İtmek için gerekli)
            Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                // Düşmanın şu anki hareketini sıfırla (Daha net bir itiş için)
                enemyRb.linearVelocity = Vector2.zero;

                // Merminin gidiş yönünü al
                Vector2 forceDirection = rb.linearVelocity.normalized;

                // O yöne doğru anlık güç uygula (Impulse = Anlık darbe)
                enemyRb.AddForce(forceDirection * knockbackForce, ForceMode2D.Impulse);
            }

            // Su topunu yok et
            Destroy(gameObject);
        }
        // Duvara veya zemine çarparsa
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}