using UnityEngine;
using TMPro;
using System.Collections;

// Enumerador para definir la pasiva equipada (Solo una a la vez)
public enum PassiveType { None, Frenzy, Tank, Greed }

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Economía")]
    [SerializeField] private int coins = 0;

    [Header("Tienda UI (Menús y Paneles)")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject mainShopButtons;
    [SerializeField] private GameObject attributesSectionPanel;
    [SerializeField] private GameObject passiveSectionPanel;

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

    [Header("--- SISTEMA DE PASIVAS ---")]
    public PassiveType currentPassive = PassiveType.None;

    [Header("Precios de Pasivas (Monedas)")]
    [SerializeField] private int frenzyPrice = 300;
    [SerializeField] private int tankPrice = 400;
    [SerializeField] private int greedPrice = 500;

    [Header("Pasiva 1: Frenesí")]
    public float frenzyDuration = 5f;
    public bool frenzyActive = false;
    public float frenzyIncomingDamageMultiplier = 1.5f;
    public float frenzyRecoilMultiplier = 1.6f;
    private Coroutine frenzyCoroutine;

    [Header("Pasiva 2: Tanque")]
    public int tankKillStacks = 0;
    public int maxTankStacks = 10;
    public float tankStackDuration = 20f; // Tiempo de duración de las acumulaciones (20 segundos)
    private Coroutine tankTimerCoroutine;

    [Header("Pasiva 3: Codicia")]
    public float greedIncomingDamageMultiplier = 1.8f;

    private bool isShopOpen = false;

    public int Coins => coins;
    public bool IsShopOpen => isShopOpen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentPassive = PassiveType.None;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
        if (mainShopButtons != null) mainShopButtons.SetActive(true);

        UpdateShopUI();
    }

    public void AddCoins(int amount)
    {
        // PASIVA 3: Duplica las monedas obtenidas (+100%)
        if (currentPassive == PassiveType.Greed)
        {
            amount *= 2;
        }

        coins += amount;
        UpdateShopUI();

        if (coinUI != null)
        {
            coinUI.PlayCoinGainedEffect(amount);
        }

        Debug.Log("Monedas obtenidas: +" + amount + ". Total: " + coins);
    }

    #region Lógica de Pasivas

    public void EquipPassive1_Frenzy() => BuyAndEquipPassive(PassiveType.Frenzy, frenzyPrice);
    public void EquipPassive2_Tank() => BuyAndEquipPassive(PassiveType.Tank, tankPrice);
    public void EquipPassive3_Greed() => BuyAndEquipPassive(PassiveType.Greed, greedPrice);

    public void BuyAndEquipPassive(PassiveType newPassive, int price)
    {
        if (currentPassive == newPassive)
        {
            Debug.Log("Ya tienes esta pasiva equipada.");
            return;
        }

        if (coins < price)
        {
            Debug.Log("No tienes suficientes monedas para equipar: " + newPassive + ". Costo: " + price);
            return;
        }

        coins -= price;

        // Limpiar temporadores y acumulaciones anteriores
        ResetPassiveStates();

        currentPassive = newPassive;

        Debug.Log($"¡Pasiva {currentPassive} equipada por {price} monedas! Total restante: {coins}");
        UpdateShopUI();
    }

    private void ResetPassiveStates()
    {
        frenzyActive = false;
        if (frenzyCoroutine != null) StopCoroutine(frenzyCoroutine);

        tankKillStacks = 0;
        if (tankTimerCoroutine != null) StopCoroutine(tankTimerCoroutine);
    }

    // Se invoca cuando muere cualquier enemigo
    public void OnEnemyKilled()
    {
        if (currentPassive == PassiveType.Frenzy)
        {
            if (frenzyCoroutine != null) StopCoroutine(frenzyCoroutine);
            frenzyCoroutine = StartCoroutine(FrenzyRoutine());
        }
        else if (currentPassive == PassiveType.Tank)
        {
            if (tankKillStacks < maxTankStacks)
            {
                tankKillStacks++;
            }

            // Reiniciar el temporizador de 20 segundos cada vez que mata un enemigo
            if (tankTimerCoroutine != null) StopCoroutine(tankTimerCoroutine);
            tankTimerCoroutine = StartCoroutine(TankStackTimerRoutine());

            Debug.Log($"Pasiva Tanque: Acumulación {tankKillStacks}/{maxTankStacks} (+{tankKillStacks * 10}% de daño). Temporizador de 20s reiniciado.");
        }
    }

    private IEnumerator FrenzyRoutine()
    {
        frenzyActive = true;
        Debug.Log("¡Pasiva Frenesí ACTIVADA!");
        yield return new WaitForSeconds(frenzyDuration);
        frenzyActive = false;
        Debug.Log("Pasiva Frenesí DESACTIVADA.");
    }

    private IEnumerator TankStackTimerRoutine()
    {
        yield return new WaitForSeconds(tankStackDuration);
        tankKillStacks = 0;
        Debug.Log("Pasiva Tanque: Expiraron los 20 segundos sin matar enemigos. Las acumulaciones de daño volvieron a 0.");
    }

    public float GetPlayerDamageMultiplier()
    {
        if (currentPassive == PassiveType.Tank)
        {
            // Cada acumulación otorga +10% de daño extra (0.10f)
            return 1f + (tankKillStacks * 0.10f);
        }
        return 1f;
    }

    public float GetReloadTimeMultiplier()
    {
        if (currentPassive == PassiveType.Frenzy && frenzyActive)
        {
            return 0.70f;
        }
        return 1f;
    }

    #endregion

    public void OpenShop()
    {
        if (shopPanel == null) return;

        isShopOpen = true;
        shopPanel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

    #region Navegación de Paneles

    public void ShowMainShopMenu()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(true);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
    }

    public void OpenAttributesSection()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(false);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(true);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
    }

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
        if (speedLevel >= maxSpeedLevel) return;

        int cost = 100 * (speedLevel + 1);
        if (coins < cost) return;

        coins -= cost;
        speedLevel++;

        if (playerMovement != null)
        {
            // Incrementa la velocidad de caminata y sprint un 30% (+30%) en cada compra
            playerMovement.walkSpeed *= 1.30f;
            playerMovement.sprintSpeed *= 1.30f;
        }

        UpdateShopUI();
    }

    public void BuyHealthUpgrade()
    {
        if (healthLevel >= maxHealthLevels) return;

        int cost = 75 * (healthLevel + 1);
        if (coins < cost) return;

        coins -= cost;
        healthLevel++;

        if (playerHealth != null)
        {
            playerHealth.maxHealth += 10f;
        }

        UpdateShopUI();
    }

    public void BuyShieldUpgrade()
    {
        if (shieldLevel >= maxShieldLevels) return;

        int cost = 75 * (shieldLevel + 1);
        if (coins < cost) return;

        coins -= cost;
        shieldLevel++;

        if (playerShield != null)
        {
            playerShield.maxShield += 10f;
        }

        UpdateShopUI();
    }

    #endregion

    private void UpdateShopUI()
    {
        if (coinsText != null)
            coinsText.text = "Monedas: " + coins;

        if (speedLevelText != null)
            speedLevelText.text = "Nivel: " + speedLevel + " / " + maxSpeedLevel;

        if (speedPriceText != null)
            speedPriceText.text = (speedLevel >= maxSpeedLevel) ? "MÁXIMO" : "Costo: " + (100 * (speedLevel + 1));

        if (healthLevelText != null)
            healthLevelText.text = "Nivel: " + healthLevel + " / " + maxHealthLevels;

        if (healthPriceText != null)
            healthPriceText.text = (healthLevel >= maxHealthLevels) ? "MÁXIMO" : "Costo: " + (75 * (healthLevel + 1));

        if (shieldLevelText != null)
            shieldLevelText.text = "Nivel: " + shieldLevel + " / " + maxShieldLevels;

        if (shieldPriceText != null)
            shieldPriceText.text = (shieldLevel >= maxShieldLevels) ? "MÁXIMO" : "Costo: " + (75 * (shieldLevel + 1));
    }
}