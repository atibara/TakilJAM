using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isLevelStarted = false;

    void Awake()
    {
        Instance = this;
    }

    // Butonun görmesi için 'public' olması şart
    public void StartLevel()
    {
        isLevelStarted = true;
        Debug.Log("Oyun Başladı! Görünümler güncelleniyor...");

        // --- DEĞİŞEN KISIM BURASI ---
        // Eskisi: FindObjectsOfType<ShadowController>();
        // Yenisi (Daha Hızlı):
        ShadowController[] allShadows = FindObjectsByType<ShadowController>(FindObjectsSortMode.None);

        foreach (ShadowController shadow in allShadows)
        {
            shadow.ActivateBattleMode();
        }
    }
}