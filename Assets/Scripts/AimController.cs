using UnityEngine;

public class AimManager : MonoBehaviour
{
    [Header("Atamalar")]
    [SerializeField] private Transform arrowVisual; 
    [SerializeField] private Transform firePoint;   

    private bool isGameStarted = false;
    private bool isAimingActive = false; // Sadece rol atandığında true olacak
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        // Başlangıçta oku gizle (Çünkü daha rol atanmadı)
        if (arrowVisual != null) arrowVisual.gameObject.SetActive(false);
    }

    void OnMouseDrag()
    {
        // Oyun başladıysa veya rol atanmadıysa (ok kapalıysa) çevirme yapma
        if (isGameStarted || !isAimingActive) return;

        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // --- DIŞARIDAN ÇAĞIRILACAK FONKSİYONLAR ---

    // 1. Rol atandığında çağır: Oku göster ve çevirmeye izin ver
    public void ShowAim()
    {
        isAimingActive = true;
        if (arrowVisual != null) arrowVisual.gameObject.SetActive(true);
        
        // Oku varsayılan (sağ) yönüne sıfırla
        transform.rotation = Quaternion.identity; 
    }

    // 2. Rol geri alındığında çağır: Oku gizle
    public void HideAim()
    {
        isAimingActive = false;
        if (arrowVisual != null) arrowVisual.gameObject.SetActive(false);
    }

    // 3. Oyun başladığında çağır: Oku gizle ve kilitle
    public void LockAndHideAim()
    {
        isGameStarted = true;
        HideAim(); // Görseli kapat
    }

    public Transform GetFirePoint()
    {
        return firePoint;
    }
}