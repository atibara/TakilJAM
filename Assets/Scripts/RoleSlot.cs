using UnityEngine;

public class RoleSlot : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public bool isUsed = false; // Bu rol şu an bir gölgede mi?

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Rol gölgeye verilince çalışır
    public void MarkAsUsed()
    {
        isUsed = true;
        canvasGroup.alpha = 0.3f; // Silikleştir
        canvasGroup.blocksRaycasts = false; // Tıklanamaz yap
    }

    // Rol geri alınınca çalışır
    public void MarkAsReturned()
    {
        isUsed = false;
        canvasGroup.alpha = 1f; // Parlat
        canvasGroup.blocksRaycasts = true; // Tıklanabilir yap
    }
}