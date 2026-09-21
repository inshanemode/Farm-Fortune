using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles player sensory feedback: floating coin popups, procedural chimes,
/// harvest effects, upgrade alerts, and worker notifications.
public class FeedbackManager : MonoBehaviour
{
    private static FeedbackManager instance;
    public static FeedbackManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FeedbackManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("FeedbackManager");
                    instance = go.AddComponent<FeedbackManager>();
                }
            }
            return instance;
        }
    }

    [Header("Recent Profit")]
    public int lastProfitAmount = 0;
    public string lastProfitSource = "";

    public bool HasRecentProfit()
    {
        return lastProfitAmount > 0;
    }

    private AudioSource audioSource;
    private AudioClip purchaseChime;
    private AudioClip coinDing;
    private AudioClip harvestPop;

    private int lastCheckedMoney = 0;
    private bool upgradeAlertShown = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.6f;

        GenerateProceduralClips();
    }

    private void OnEnable()
    {
        EventManager.StartListening<OnPlayerSellItem>(OnPlayerSellItem);
        EventManager.StartListening<OnHarvestFarmEntity>(OnHarvestFarmEntity);
        EventManager.StartListening<OnPlayerStatUpdate>(OnPlayerStatUpdate);
    }

    private void OnDisable()
    {
        EventManager.StopListening<OnPlayerSellItem>(OnPlayerSellItem);
        EventManager.StopListening<OnHarvestFarmEntity>(OnHarvestFarmEntity);
        EventManager.StopListening<OnPlayerStatUpdate>(OnPlayerStatUpdate);
    }

    private void OnPlayerSellItem(EventParam param)
    {
        OnPlayerSellItem sellEvent = param as OnPlayerSellItem;
        if (sellEvent == null) return;

        int earned = sellEvent.item.sellPrice * sellEvent.amount;
        lastProfitAmount = earned;
        lastProfitSource = $"{sellEvent.amount}x {sellEvent.item.itemName}";

        PlayCoinSound();
        ShowFloatingText($"+{earned:N0} coins", FarmUITheme.GoldReward);
        NotificationUI.Show("Item Sold!", $"+{earned:N0} coins from {lastProfitSource}", 2.5f);
    }

    private void OnHarvestFarmEntity(EventParam param)
    {
        OnHarvestFarmEntity harvestEvent = param as OnHarvestFarmEntity;
        if (harvestEvent == null) return;

        PlayHarvestSound();
        ShowFloatingText($"+{harvestEvent.data.harvestItemAmount} {harvestEvent.data.harvestItem.itemName}", FarmUITheme.GreenActive);
    }

    private void OnPlayerStatUpdate(EventParam param)
    {
        OnPlayerStatUpdate statEvent = param as OnPlayerStatUpdate;
        if (statEvent == null || statEvent.player == null) return;

        int money = statEvent.player.totalMoney;
        int toolCost = statEvent.player.toolLevel * 500; // estimated upgrade threshold

        if (money >= toolCost && lastCheckedMoney < toolCost && !upgradeAlertShown)
        {
            upgradeAlertShown = true;
            NotificationUI.Show("Upgrade Ready!", $"You have enough coins ({money:N0}) to upgrade your farm!", 3f);
        }
        lastCheckedMoney = money;
    }

    public void NotifyWorkerCompleted(WorkerEntity worker)
    {
        if (worker == null) return;
        NotificationUI.Show("Worker Finished", $"Worker completed task: {worker.CurrentTask}", 2f);
    }

    public void PlayPurchaseSound()
    {
        if (audioSource != null && purchaseChime != null)
        {
            audioSource.PlayOneShot(purchaseChime);
        }
    }

    public void PlayClickSound()
    {
        if (audioSource != null && purchaseChime != null)
        {
            audioSource.PlayOneShot(purchaseChime);
        }
    }

    public void PlayCoinSound()
    {
        if (audioSource != null && coinDing != null)
        {
            audioSource.PlayOneShot(coinDing);
        }
    }

    public void PlayHarvestSound()
    {
        if (audioSource != null && harvestPop != null)
        {
            audioSource.PlayOneShot(harvestPop);
        }
    }

    public void SpawnCoinText(int amount)
    {
        ShowFloatingText($"+{amount:N0} xu", FarmUITheme.GoldReward);
    }

    public void ShowFloatingText(string text, Color color)
    {
        StartCoroutine(FloatingTextRoutine(text, color));
    }

    private string currentFloatingText = "";
    private float floatingTextAlpha = 0f;
    private Color floatingTextColor = Color.white;
    private Vector2 floatingTextPos = Vector2.zero;

    private IEnumerator FloatingTextRoutine(string text, Color color)
    {
        currentFloatingText = text;
        floatingTextColor = color;
        floatingTextPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);
        floatingTextAlpha = 1f;

        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            floatingTextPos.y += 40f * Time.deltaTime; // Rise upwards
            floatingTextAlpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        currentFloatingText = "";
    }

    private void OnGUI()
    {
        if (!string.IsNullOrEmpty(currentFloatingText) && floatingTextAlpha > 0.05f)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 22;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            Color c = floatingTextColor;
            c.a = floatingTextAlpha;
            style.normal.textColor = c;

            // Shadow
            GUIStyle shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = new Color(0, 0, 0, floatingTextAlpha * 0.8f);
            GUI.Label(new Rect(floatingTextPos.x - 150 + 2, Screen.height - floatingTextPos.y - 20 + 2, 300, 40), currentFloatingText, shadowStyle);

            GUI.Label(new Rect(floatingTextPos.x - 150, Screen.height - floatingTextPos.y - 20, 300, 40), currentFloatingText, style);
        }
    }

    /// <summary>
    /// Generates pleasant procedural 16-bit cozy farm sounds without external files.
    /// </summary>
    private void GenerateProceduralClips()
    {
        int sampleRate = 44100;

        // 1. Coin Ding (pleasant double chime 587Hz -> 880Hz)
        int dingSamples = (int)(sampleRate * 0.25f);
        float[] dingData = new float[dingSamples];
        for (int i = 0; i < dingSamples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = t < 0.08f ? 587.33f : 880.0f; // D5 to A5
            float envelope = Mathf.Exp(-t * 12f);
            dingData[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }
        coinDing = AudioClip.Create("CoinDing", dingSamples, 1, sampleRate, false);
        coinDing.SetData(dingData, 0);

        // 2. Purchase Chime (ascending triad C5 -> E5 -> G5)
        int chimeSamples = (int)(sampleRate * 0.35f);
        float[] chimeData = new float[chimeSamples];
        for (int i = 0; i < chimeSamples; i++)
        {
            float t = (float)i / sampleRate;
            float freq;
            if (t < 0.10f) freq = 523.25f; // C5
            else if (t < 0.20f) freq = 659.25f; // E5
            else freq = 783.99f; // G5
            float envelope = Mathf.Exp(-t * 8f);
            chimeData[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.45f;
        }
        purchaseChime = AudioClip.Create("PurchaseChime", chimeSamples, 1, sampleRate, false);
        purchaseChime.SetData(chimeData, 0);

        // 3. Harvest Pop (satisfying soft pop 220Hz -> 440Hz)
        int popSamples = (int)(sampleRate * 0.18f);
        float[] popData = new float[popSamples];
        for (int i = 0; i < popSamples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = Mathf.Lerp(220f, 440f, t / 0.18f);
            float envelope = Mathf.Exp(-t * 18f);
            popData[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.6f;
        }
        harvestPop = AudioClip.Create("HarvestPop", popSamples, 1, sampleRate, false);
        harvestPop.SetData(popData, 0);
    }
}
