using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RoleData roleToGive; 
    
    private RoleSlot mySlot; 
    private Transform originalParent;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    // YENİ: Başlangıçtaki yerel pozisyonu tutmak için değişken
    private Vector2 originalAnchoredPosition; 

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mySlot = GetComponent<RoleSlot>(); 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mySlot.isUsed) return; 

        originalParent = transform.parent;
        
        // YENİ: Parent değişmeden önce, şu anki slot içindeki konumunu kaydet
        originalAnchoredPosition = rectTransform.anchoredPosition;

        transform.SetParent(transform.root); 
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mySlot.isUsed) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        // bool success değişkenine gerek kalmayabilir, mantığa göre düzenledim:

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        
        bool placedSuccessfully = false; // Başarılı olup olmadığını kontrol edelim

        if (hit.collider != null)
        {
            ShadowController shadow = hit.collider.GetComponent<ShadowController>();
            
            if (shadow != null && !shadow.HasRole())
            {
                shadow.AssignRole(roleToGive, mySlot);
                mySlot.MarkAsUsed();
                placedSuccessfully = true; // Başarıyla yerleşti
            }
        }

        // Görsel olarak yerine dön
        transform.SetParent(originalParent);

        if (!placedSuccessfully)
        {
            // EĞER YERLEŞMEDİYSE: Eski kaydettiğimiz konuma dön
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
        else
        {
            // EĞER YERLEŞTİYSE: (Opsiyonel) Slot içinde ortalayabilirsin veya yine eski konumda kalabilir
            // mySlot.MarkAsUsed() zaten objeyi siliyorsa veya gizliyorsa burası önemli olmayabilir.
            rectTransform.anchoredPosition = Vector2.zero; 
        }
    }
}