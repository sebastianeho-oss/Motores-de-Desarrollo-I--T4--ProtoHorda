using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Propiedad estática pública para consultar la pausa desde cualquier script
    public static bool IsPaused { get; private set; } = false;

    [Header("UI Panels")]
    public GameObject HUD;
    public GameObject gameOverUI;
    public GameObject winUI;
    public GameObject pauseUI; // Panel de Pausa

    [Header("Referencias del Jugador")]
    public GameObject player;

    void Start()
    {
        IsPaused = false;
        Time.timeScale = 1f; 
        LockCursor();

        // Ocultar pantallas al comenzar
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winUI != null) winUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        if (HUD != null) HUD.SetActive(true);
    }

    void Update()
    {        
        // Comprobar si alguna pantalla final está activa
        bool isEndGameUIActive = (winUI != null && winUI.activeInHierarchy) || 
                                (gameOverUI != null && gameOverUI.activeInHierarchy);

        // Abrir/Cerrar pausa con Esc solo si la partida no ha terminado
        if (Input.GetKeyDown(KeyCode.Escape) && !isEndGameUIActive)
        {
            TogglePause();
        }

        // Manejo del cursor si hay cualquier UI activa
        bool isAnyUIActive = isEndGameUIActive || IsPaused;

        if (isAnyUIActive)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseUI != null) pauseUI.SetActive(true);
        if (HUD != null) HUD.SetActive(false);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseUI != null) pauseUI.SetActive(false);
        if (HUD != null) HUD.SetActive(true);
    }

    public void GameOver()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (HUD != null) HUD.SetActive(false);
        Time.timeScale = 0f; 
    }

    public void Win()
    {
        if (winUI != null) winUI.SetActive(true);
        if (HUD != null) HUD.SetActive(false);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void MainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}