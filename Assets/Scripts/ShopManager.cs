using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    // Patrón Singleton para acceso global rápido desde las armas y otros scripts
    public static ShopManager Instance { get; private set; }

    [Header("Economía")]
    [SerializeField] private int coins = 0;

    [Header("Tienda UI (Menú)")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private TextMeshProUGUI speedPriceText;

    [Header("Efectos de UI en Juego")]
    [SerializeField] private CoinUI coinUI; // Referencia al script que anima el +X en pantalla

    [Header("Mejora de velocidad")]
    [SerializeField] private MovementPlayerController playerMovement;
    [SerializeField] private int maxSpeedLevel = 3;

    private int speedLevel = 0;
    private bool isShopOpen = false;

    public int Coins => coins;
    public bool IsShopOpen => isShopOpen; // Usado por ShootingSystem y RocketLauncher para bloquear disparos

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        UpdateShopUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateShopUI();

        // Notifica al efecto visual flotante en la UI del juego (si está asignado)
        if (coinUI != null)
        {
            coinUI.PlayCoinGainedEffect(amount);
        }

        Debug.Log("Monedas obtenidas: +" + amount + ". Total: " + coins);
    }

    public void OpenShop()
    {
        if (shopPanel == null) return;

        isShopOpen = true; // Activa el bloqueo global para evitar disparos
        shopPanel.SetActive(true);
        Time.timeScale = 0f;

        // Liberar el cursor para interactuar con los botones de la tienda
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateShopUI();
    }

    public void CloseShop()
    {
        if (shopPanel == null) return;

        isShopOpen = false; // Desactiva el bloqueo global
        shopPanel.SetActive(false);
        Time.timeScale = 1f;

        // Volver a bloquear el cursor para el gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ForceCloseShop()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            isShopOpen = false; // Desactiva el bloqueo global
            shopPanel.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("El tiempo de la tienda expiró. Cerrando menú automáticamente.");
        }
    }

    public void BuySpeedUpgrade()
    {
        if (speedLevel >= maxSpeedLevel)
        {
            Debug.Log("La velocidad ya está al máximo.");
            return;
        }

        int cost = 100 * (speedLevel + 1);

        if (coins < cost)
        {
            Debug.Log("No tienes suficientes monedas.");
            return;
        }

        coins -= cost;
        speedLevel++;

        ApplySpeedUpgrade();
        UpdateShopUI();

        Debug.Log("Mejora de velocidad comprada. Nivel: " + speedLevel + "/" + maxSpeedLevel);
    }

    private void ApplySpeedUpgrade()
    {
        if (playerMovement == null) return;

        playerMovement.walkSpeed += 0.5f;
        playerMovement.sprintSpeed += 0.85f;
    }

    private void UpdateShopUI()
    {
        if (coinsText != null)
            coinsText.text = "Monedas: " + coins;

        if (speedLevelText != null)
            speedLevelText.text = "Nivel: " + speedLevel + " / " + maxSpeedLevel;

        if (speedPriceText != null)
        {
            if (speedLevel >= maxSpeedLevel)
            {
                speedPriceText.text = "MÁXIMO";
            }
            else
            {
                int cost = 100 * (speedLevel + 1);
                speedPriceText.text = "Costo: " + cost;
            }
        }
    }
}