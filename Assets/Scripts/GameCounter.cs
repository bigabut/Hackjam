using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameCounter : MonoBehaviour
{
    public static GameCounter Instance { get; private set; }

    [Header("Cut Counter")]
    [SerializeField] private int cutCount = 0;
    [SerializeField] private int maxCutCount = 5;
    [SerializeField] private TMP_Text cutCounterText;

    [Header("Head Movement Counter")]
    [SerializeField] private int headMoveCount = 0;
    [SerializeField] private int maxHeadMoveCount = 20;
    [SerializeField] private TMP_Text headMoveCounterText;

    [Header("Scene Reload")]
    [SerializeField] private bool reloadSceneWhenBothEmpty = true;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    // =========================================================
    // CUT
    // =========================================================

    public bool CanCut()
    {
        return cutCount < maxCutCount;
    }

    public void AddCut()
    {
        if (!CanCut())
        {
            Debug.Log(
                "Cut counter sudah habis."
            );

            return;
        }

        cutCount++;

        UpdateUI();
        CheckReload();
    }

    // =========================================================
    // HEAD MOVE
    // =========================================================

    public bool CanMove()
    {
        return headMoveCount < maxHeadMoveCount;
    }

    public void AddHeadMove()
    {
        if (!CanMove())
        {
            Debug.Log(
                "Head movement counter sudah habis."
            );

            return;
        }

        headMoveCount++;

        UpdateUI();
        CheckReload();
    }

    // =========================================================
    // CHECK COUNTER
    // =========================================================

    public bool IsCutLimitReached()
    {
        return cutCount >= maxCutCount;
    }

    public bool IsMoveLimitReached()
    {
        return headMoveCount >= maxHeadMoveCount;
    }

    private void CheckReload()
    {
        if (!reloadSceneWhenBothEmpty)
            return;

        if (IsCutLimitReached() &&
            IsMoveLimitReached())
        {
            Debug.Log(
                "Semua counter habis. " +
                "Reloading scene..."
            );

            ReloadScene();
        }
    }

    // =========================================================
    // UI
    // =========================================================

    private void UpdateUI()
    {
        if (cutCounterText != null)
        {
            cutCounterText.text =
                $"{cutCount} / {maxCutCount}";
        }

        if (headMoveCounterText != null)
        {
            headMoveCounterText.text =
                $"{headMoveCount} / {maxHeadMoveCount}";
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetCounter()
    {
        cutCount = 0;
        headMoveCount = 0;

        UpdateUI();
    }

    // =========================================================
    // RELOAD SCENE
    // =========================================================

    public void ReloadScene()
    {
        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public int CutCount => cutCount;
    public int MaxCutCount => maxCutCount;

    public int HeadMoveCount =>
        headMoveCount;

    public int MaxHeadMoveCount =>
        maxHeadMoveCount;
}