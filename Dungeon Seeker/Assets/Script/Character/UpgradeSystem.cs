using UnityEngine;
using UnityEngine.UI;

public class UpgradeSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text totalPointsText;

    [Header("Upgrade Buttons (+)")]
    [SerializeField] private Button dodgeUpgradeButton;
    [SerializeField] private Button throwUpgradeButton;
    [SerializeField] private Button attackUpgradeButton;

    [Header("Downgrade Buttons (-)")]
    [SerializeField] private Button dodgeMinusButton;
    [SerializeField] private Button throwMinusButton;
    [SerializeField] private Button attackMinusButton;

    [Header("Text Reduce Display")]
    [SerializeField] private Text dodgeReduceText;
    [SerializeField] private Text throwReduceText;
    [SerializeField] private Text attackReduceText;

    [Header("Upgrade Settings")]
    [SerializeField] private int dodgeCost = 10;
    [SerializeField] private float dodgeReduceValue = 0.1f;

    [SerializeField] private int throwCost = 15;
    [SerializeField] private float throwReduceValue = 0.2f;

    [SerializeField] private int attackCost = 20;
    [SerializeField] private float attackReduceValue = 0.15f;

    // ✅ Global poin untuk antar-scene (bisa ditambah dari portal)
    public static int poinGlobal = 500;

    private float baseCooldownDodge = 2f;
    private float baseCooldownThrow = 2f;
    private float baseCooldownAttack = 1f;

    private float totalDodgeReduce = 0f;
    private float totalThrowReduce = 0f;
    private float totalAttackReduce = 0f;

    private void Start()
    {
        dodgeUpgradeButton.onClick.AddListener(UpgradeDodge);
        throwUpgradeButton.onClick.AddListener(UpgradeThrow);
        attackUpgradeButton.onClick.AddListener(UpgradeAttack);

        dodgeMinusButton.onClick.AddListener(DowngradeDodge);
        throwMinusButton.onClick.AddListener(DowngradeThrow);
        attackMinusButton.onClick.AddListener(DowngradeAttack);

        UpdateUI();
    }

    private void UpdateUI()
    {
        totalPointsText.text = "Poin: " + poinGlobal;

        dodgeUpgradeButton.interactable = poinGlobal >= dodgeCost;
        throwUpgradeButton.interactable = poinGlobal >= throwCost;
        attackUpgradeButton.interactable = poinGlobal >= attackCost;

        dodgeMinusButton.interactable = totalDodgeReduce > 0f;
        throwMinusButton.interactable = totalThrowReduce > 0f;
        attackMinusButton.interactable = totalAttackReduce > 0f;

        dodgeReduceText.text = $" {(baseCooldownDodge - totalDodgeReduce):F2}s";
        throwReduceText.text = $" {(baseCooldownThrow - totalThrowReduce):F2}s";
        attackReduceText.text = $" {(baseCooldownAttack - totalAttackReduce):F2}s";
    }

    private void UpgradeDodge()
    {
        if (poinGlobal >= dodgeCost)
        {
            poinGlobal -= dodgeCost;
            totalDodgeReduce += dodgeReduceValue;
            Debug.Log("✅ Upgrade Dodge");
            UpdateUI();
        }
    }

    private void UpgradeThrow()
    {
        if (poinGlobal >= throwCost)
        {
            poinGlobal -= throwCost;
            totalThrowReduce += throwReduceValue;
            Debug.Log("✅ Upgrade Throw");
            UpdateUI();
        }
    }

    private void UpgradeAttack()
    {
        if (poinGlobal >= attackCost)
        {
            poinGlobal -= attackCost;
            totalAttackReduce += attackReduceValue;
            Debug.Log("✅ Upgrade Attack");
            UpdateUI();
        }
    }

    private void DowngradeDodge()
    {
        if (totalDodgeReduce >= dodgeReduceValue)
        {
            poinGlobal += dodgeCost;
            totalDodgeReduce -= dodgeReduceValue;
            Debug.Log("⬅️ Downgrade Dodge");
            UpdateUI();
        }
    }

    private void DowngradeThrow()
    {
        if (totalThrowReduce >= throwReduceValue)
        {
            poinGlobal += throwCost;
            totalThrowReduce -= throwReduceValue;
            Debug.Log("⬅️ Downgrade Throw");
            UpdateUI();
        }
    }

    private void DowngradeAttack()
    {
        if (totalAttackReduce >= attackReduceValue)
        {
            poinGlobal += attackCost;
            totalAttackReduce -= attackReduceValue;
            Debug.Log("⬅️ Downgrade Attack");
            UpdateUI();
        }
    }
}
