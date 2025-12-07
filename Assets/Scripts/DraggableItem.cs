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
        mySlot = GetComponentInParent<RoleSlot>();
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
        // 1. Işınları tekrar aç (Tekrar tıklanabilsin)
        canvasGroup.blocksRaycasts = true;

        bool placedSuccessfully = false;

        // Mouse'un olduğu noktaya dünyada bir ışın at
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            // Çarptığım objede yoksa, ebeveynine (Parent) bak
            ShadowController shadow = hit.collider.GetComponentInParent<ShadowController>();

            // Eğer geçerli bir gölge bulduysak ve henüz rolü yoksa
            if (shadow != null && !shadow.HasRole())
            {
                // Rolü ata
                shadow.AssignRole(roleToGive, mySlot);

                // Slot'u kullanıldı olarak işaretle (UI'da grileşmesi vs için)
                if (mySlot != null)
                {
                    mySlot.MarkAsUsed();
                }

                placedSuccessfully = true;
            }
        }

        // --- KRİTİK KISIM ---

        // 2. Objeyi eski ailesinin (Slotun) içine geri koy
        transform.SetParent(originalParent);

        // 3. Pozisyonu SADECE ve SADECE en başta kaydettiğimiz yere eşitle.
        // Başarılı olsa da, başarısız olsa da buraya gitmeli.
        // Asla Vector2.zero yapmıyoruz.
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
    }