using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Affiche l'écran de défaite quand le joueur meurt et fige le jeu.</summary>
public class GameOverMenu : MonoBehaviour
{
    [SerializeField] GameObject gameOverUI;       // le panneau à afficher
    [SerializeField] string menuSceneName = "Menu";
    [Tooltip("Petit délai (temps réel) avant l'affichage, pour voir le dernier coup")]
    [SerializeField] float showDelay = 0.6f;

    PlayerHealth health;

    void Awake()
    {
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    void Start()
    {
        health = FindFirstObjectByType<PlayerHealth>();
        if (health == null) { Debug.LogWarning("GameOverMenu : aucun PlayerHealth trouvé"); return; }
        health.Died += OnPlayerDied;
    }

    void OnDestroy()
    {
        if (health != null) health.Died -= OnPlayerDied;
    }

    void OnPlayerDied()
    {
        // Bloque tout de suite les contrôles (tir, déplacement, pause)
        PauseMenu.GameIsPaused = true;
        PauseMenu.PlayerIsDead = true;
        StartCoroutine(ShowAfterDelay());
    }

    System.Collections.IEnumerator ShowAfterDelay()
    {
        // Temps réel : fonctionne même si timeScale vaut 0
        yield return new WaitForSecondsRealtime(showDelay);
        gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        PauseMenu.GameIsPaused = false;
        PauseMenu.PlayerIsDead = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        PauseMenu.PlayerIsDead = false;
        PauseMenu.GameIsPaused = false;
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}