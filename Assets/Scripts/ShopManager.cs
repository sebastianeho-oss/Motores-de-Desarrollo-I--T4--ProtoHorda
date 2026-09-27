using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    // Patrón Singleton para acceso global rápido
    public static ShopManager Instance { get; private set; }

    [Header("Economía")]
    [SerializeField] private int coins = 0;

    [Header("Tienda UI (Menús y Paneles)")]
    [SerializeField] private GameObject shopPanel;             // Menú principal de la tienda
    [SerializeField] private GameObject mainShopButtons;       // Contenedor de los botones principales (Atributos / Pasivas)
    [SerializeField] private GameObject attributesSectionPanel;// Sección que contiene Velocidad, Vida y Escudo
    [SerializeField] private GameObject passiveSectionPanel;   // Sección para Efectos Pasivos (solo interfaz)

    [Header("Textos UI General")]
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("UI Velocidad")]
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private TextMeshProUGUI speedPriceText;

    [Header("UI Vida (+10 pts máx)")]
    [SerializeField] private TextMeshProUGUI healthLevelText;
    [SerializeField] private TextMeshProUGUI healthPriceText;

    [Header("UI Escudo (+10 pts máx)")]
    [SerializeField] private TextMeshProUGUI shieldLevelText;
    [SerializeField] private TextMeshProUGUI shieldPriceText;

    [Header("Efectos de UI en Juego")]
    [SerializeField] private CoinUI coinUI;

    [Header("Referencias de Jugador y Mejoras")]
    [SerializeField] private MovementPlayerController playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerShield playerShield;

    [Header("Límites y Niveles (Máximo 3)")]
    [SerializeField] private int maxSpeedLevel = 3;
    private int speedLevel = 0;

    [SerializeField] private int maxHealthLevels = 3;
    private int healthLevel = 0;

    [SerializeField] private int maxShieldLevels = 3;
    private int shieldLevel = 0;

    private bool isShopOpen = false;

    public int Coins => coins;
    public bool IsShopOpen => isShopOpen;

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

        // Apagar las subsecciones y asegurar estado inicial
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
        if (mainShopButtons != null) mainShopButtons.SetActive(true);

        UpdateShopUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateShopUI();

        if (coinUI != null)
        {
            coinUI.PlayCoinGainedEffect(amount);
        }

        Debug.Log("Monedas obtenidas: +" + amount + ". Total: " + coins);
    }

    public void OpenShop()
    {
        if (shopPanel == null) return;

        isShopOpen = true;
        shopPanel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Al abrir la tienda, aseguramos mostrar el menú principal
        ShowMainShopMenu();
        UpdateShopUI();
    }

    public void CloseShop()
    {
        if (shopPanel == null) return;

        isShopOpen = false;
        shopPanel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ForceCloseShop()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            isShopOpen = false;
            shopPanel.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("El tiempo de la tienda expiró. Cerrando menú automáticamente.");
        }
    }

    #region Navegación de Paneles (Botones de Sección)

    // Vuelve al menú principal de la tienda (Botón "Atrás") y muestra los botones principales
    public void ShowMainShopMenu()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(true);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
    }

    // Despliega la sección conjunta de Atributos y oculta los botones principales
    public void OpenAttributesSection()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(false);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(true);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
    }

    // Despliega la sección de Efectos Pasivos y oculta los botones principales
    public void OpenPassiveSection()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(false);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(true);
    }

    #endregion

    #region Lógica de Mejoras (Atributos)

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

        if (playerMovement != null)
        {
            playerMovement.walkSpeed += 0.5f;
            playerMovement.sprintSpeed += 0.85f;
        }

        UpdateShopUI();
        Debug.Log("Mejora de velocidad comprada. Nivel: " + speedLevel);
    }

    // Aumenta ÚNICAMENTE la vida máxima en 10 puntos (sin curar de golpe)
    public void BuyHealthUpgrade()
    {
        if (healthLevel >= maxHealthLevels)
        {
            Debug.Log("La vida ya está al máximo.");
            return;
        }

        int cost = 75 * (healthLevel + 1);
        if (coins < cost)
        {
            Debug.Log("No tienes suficientes monedas para la vida.");
            return;
        }

        coins -= cost;
        healthLevel++;

        if (playerHealth != null)
        {
            playerHealth.maxHealth += 10f; // Solo incrementa el límite máximo
            // playerHealth.currentHealth se queda exactamente igual
        }

        UpdateShopUI();
        Debug.Log("¡Vida máxima aumentada en +10! Límite actual: " + (playerHealth != null ? playerHealth.maxHealth : 0));
    }

    // Aumenta ÚNICAMENTE el escudo máximo en 10 puntos (sin recargar de golpe)
    public void BuyShieldUpgrade()
    {
        if (shieldLevel >= maxShieldLevels)
        {
            Debug.Log("El escudo ya está al máximo.");
            return;
        }

        int cost = 75 * (shieldLevel + 1);
        if (coins < cost)
        {
            Debug.Log("No tienes suficientes monedas para el escudo.");
            return;
        }

        coins -= cost;
        shieldLevel++;

        if (playerShield != null)
        {
            playerShield.maxShield += 10f; // Solo incrementa el límite máximo del escudo
            // playerShield.currentShield se queda exactamente igual
        }

        UpdateShopUI();
        Debug.Log("¡Escudo máximo aumentado en +10! Límite actual: " + (playerShield != null ? playerShield.maxShield : 0));
    }

    #endregion

    private void UpdateShopUI()
    {
        if (coinsText != null)
            coinsText.text = "Monedas: " + coins;

        // UI Velocidad
        if (speedLevelText != null)
            speedLevelText.text = "Nivel: " + speedLevel + " / " + maxSpeedLevel;

        if (speedPriceText != null)
        {
            speedPriceText.text = (speedLevel >= maxSpeedLevel) ? "MÁXIMO" : "Costo: " + (100 * (speedLevel + 1));
        }

        // UI Vida
        if (healthLevelText != null)
            healthLevelText.text = "Nivel: " + healthLevel + " / " + maxHealthLevels;

        if (healthPriceText != null)
        {
            healthPriceText.text = (healthLevel >= maxHealthLevels) ? "MÁXIMO" : "Costo: " + (75 * (healthLevel + 1));
        }

        // UI Escudo
        if (shieldLevelText != null)
            shieldLevelText.text = "Nivel: " + shieldLevel + " / " + maxShieldLevels;

        if (shieldPriceText != null)
        {
            shieldPriceText.text = (shieldLevel >= maxShieldLevels) ? "MÁXIMO" : "Costo: " + (75 * (shieldLevel + 1));
        }
    }
}