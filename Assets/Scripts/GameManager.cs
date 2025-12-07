using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isLevelStarted = false;

    void Awake()
    {
        // Singleton yapısı
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // --- YENİ EKLENEN KISIM: UPDATE ---
    void Update()
    {
        // Klavyeden 'F' tuşuna basıldığı anı yakala
        if (Input.GetKeyDown(KeyCode.F))
        {
            RestartLevel();
        }
    }

    public void StartLevel()
    {
        if (isLevelStarted) return;

        isLevelStarted = true;
        Debug.Log("Oyun Başladı! Görünümler güncelleniyor...");

        ShadowController[] allShadows = FindObjectsByType<ShadowController>(FindObjectsSortMode.None);

        foreach (ShadowController shadow in allShadows)
        {
            shadow.ActivateBattleMode();
        }
    }

    public void RestartLevel()
    {
        Debug.Log("Bölüm Yeniden Yükleniyor...");

        // Aktif sahneyi baştan yükle
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}