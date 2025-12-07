using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Veri")]
    public RoleData roleToGive; // Inspector üzerinden atanacak RoleData (Polis/İtfaiye)

    // Bileşenler
    private RoleSlot mySlot;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    // Pozisyon Kayıtları
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mySlot = GetComponent<RoleSlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Eğer slot zaten kullanıldıysa sürüklemeye izin verme
        if (mySlot != null && mySlot.isUsed) return;

        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;

        // Sürüklerken UI'da en öne gelmesi için root'a (en dışa) taşıyoruz
        transform.SetParent(transform.root);
        
        // Işınların (Raycast) içinden geçmesine izin ver ki arkadaki Gölgeyi görebilelim
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mySlot != null && mySlot.isUsed) return;
        
        // Mouse ile hareket et
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Işınları tekrar aç
        canvasGroup.blocksRaycasts = true;
        
        bool placedSuccessfully = false;

        // Mouse'un olduğu noktaya dünyada bir ışın atıyoruz
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            // === KRİTİK DÜZELTME BURADA ===
            // Işın 'AimPivot'a çarpmış olabilir. O yüzden 'GetComponentInParent' kullanıyoruz.
            // Bu komut: "Çarptığım objede yoksa, onun babasına (Parent) bak" der.
            ShadowController shadow = hit.collider.GetComponentInParent<ShadowController>();

            // Eğer geçerli bir gölge bulduysak ve henüz rolü yoksa
            if (shadow != null && !shadow.HasRole())
            {
                shadow.AssignRole(roleToGive, mySlot);
                
                if (mySlot != null)
                {
                    mySlot.MarkAsUsed();
                }
                
                placedSuccessfully = true;
            }
        }

        // Objeyi eski hiyerarşisine (Slotun içine) geri koy
        transform.SetParent(originalParent);

        if (!placedSuccessfully)
        {
            // Başarısızsa eski yerine geri dön
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
        else
        {
            // Başarılıysa (veya kullanıldı olarak işaretlendiyse) tam merkeze oturt
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}