using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Economía")]
    [SerializeField] private int coins = 0;
    [SerializeField] private int coinsPerWave = 100;

    [Header("Tienda")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private TextMeshProUGUI speedPriceText;

    [Header("Mejora de velocidad")]
    [SerializeField] private MovementPlayerController playerMovement;
    [SerializeField] private int maxSpeedLevel = 3;

    private int speedLevel = 0;

    public int Coins => coins;

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

        Debug.Log($"Monedas obtenidas: +{amount}. Total: {coins}");
    }

    public void OpenShop()
    {
        if (shopPanel == null) return;

        shopPanel.SetActive(true);
        Time.timeScale = 0f;

        UpdateShopUI();
    }

    public void CloseShop()
    {
        if (shopPanel == null) return;

        shopPanel.SetActive(false);
        Time.timeScale = 1f;

        if (waveSpawner != null)
        {
            waveSpawner.StartNextWaveFromShop();
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

        Debug.Log($"Mejora de velocidad comprada. Nivel: {speedLevel}/{maxSpeedLevel}");
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
            coinsText.text = $"Monedas: {coins}";

        if (speedLevelText != null)
            speedLevelText.text = $"Nivel: {speedLevel} / {maxSpeedLevel}";

        if (speedPriceText != null)
        {
            if (speedLevel >= maxSpeedLevel)
            {
                speedPriceText.text = "MÁXIMO";
            }
            else
            {
                int cost = 100 * (speedLevel + 1);
                speedPriceText.text = $"Costo: {cost} 🪙";
            }
        }
    }
}