using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    // Bu static değişken, sahneler arası referansı tutacak
    private static BackgroundMusic instance = null;

    void Awake()
    {
        // Eğer daha önce oluşturulmuş bir müzik çalıcı yoksa...
        if (instance == null)
        {
            instance = this; // Bu objeyi "tek ve asıl" olarak işaretle
            DontDestroyOnLoad(this.gameObject); // Sahne değişince YOK ETME
        }
        else
        {
            // Eğer zaten çalan bir müzik objesi varsa ve yeni bir sahneye girdiğimizde
            // yanlışlıkla ikincisi oluştuysa, bu yeni oluşanı hemen yok et.
            // Böylece müzik üst üste binmez.
            Destroy(this.gameObject);
        }
    }
}