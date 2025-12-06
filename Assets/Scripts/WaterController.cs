using UnityEngine;

public class WaterController : MonoBehaviour
{
    public float throwForce = 8f; // Fırlatma gücü
    public float pushPower = 5f;  // Çarptığında rakibi itme gücü
    
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- EĞİK ATIŞ MANTIĞI (45 Derece) ---
        // Hem ileri (transform.right) hem yukarı (Vector2.up) kuvvet uygularız.
        // Bu ikisinin toplamı 45 derecelik bir vektör oluşturur.
        Vector2 throwDirection = (transform.right + Vector3.up).normalized;
        
        rb.linearVelocity = throwDirection * throwForce;
        
        Destroy(gameObject, 4f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Çarptığı objenin fiziği var mı?
        Rigidbody2D targetRb = collision.gameObject.GetComponent<Rigidbody2D>();
        ShadowController enemy = collision.gameObject.GetComponent<ShadowController>();

        if (targetRb != null && enemy != null)
        {
            // --- İTME (KNOCKBACK) MANTIĞI ---
            // Suyun geliş yönüne göre rakibi geriye it
            Vector2 pushDir = (collision.transform.position - transform.position).normalized;
            
            // Rakibi it (Impulse: Anlık darbe)
            targetRb.AddForce(pushDir * pushPower, ForceMode2D.Impulse);
            
            // Gölgeye "Ben itildim, devriyeyi durdur" de
            enemy.GetStunned(); 
        }

        // Su çarptığı yerde yok olsun (veya partikül çıkarsın)
        Destroy(gameObject);
    }
}