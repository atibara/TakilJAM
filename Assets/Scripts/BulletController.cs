using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;

    void Start()
    {
        // 1. Mermiyi oluşturulduğu yöne (Sağ/Sol) fırlat
        // Transform.right, objenin kırmızı ok yönüdür.
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * speed;
        
        // 3 saniye sonra kimseye çarpmazsa yok olsun (Performans için)
        Destroy(gameObject, lifeTime); 
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Çarptığı şey bir Gölge mi?
        ShadowController enemy = hitInfo.GetComponent<ShadowController>();
        
        if (enemy != null)
        {
            // --- ÖLDÜRME MANTIĞI ---
            enemy.Die(); // Gölge scriptine bu fonksiyonu ekleyeceğiz
            Destroy(gameObject); // Mermiyi yok et
        }
        else if(hitInfo.tag != "Player") // Kendimizi vurmayalım
        {
             Destroy(gameObject); // Duvara çarparsa yok olsun
        }
    }
}