using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "tutorial"; // Gerçek oyun sahnesi adını gir

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Enter tuşu
        {
            PlayGame();
        }
    }

}