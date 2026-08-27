using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Panel")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Tutorial Content")]
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TMP_Text tutorialText;

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject prevButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("Tutorial Slides")]
    [SerializeField] private Sprite[] tutorialImages;
    [TextArea(2, 5)]
    [SerializeField] private string[] tutorialTexts;

    private int currentSlide = 0;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentSlide = 0;
    }

    // =========================================================
    // OPEN
    // =========================================================

    public void OpenTutorial()
    {
        if (tutorialPanel == null)
            return;

        currentSlide = 0;

        tutorialPanel.SetActive(true);

        UpdateSlide();
    }

    // =========================================================
    // NEXT
    // =========================================================

    public void NextSlide()
    {
        if (tutorialImages == null ||
            tutorialImages.Length == 0)
        {
            CloseTutorial();
            return;
        }

        // Kalau sudah berada di slide terakhir
        if (currentSlide >=
            tutorialImages.Length - 1)
        {
            CloseTutorial();
            return;
        }

        currentSlide++;

        UpdateSlide();
    }

    // =========================================================
    // PREVIOUS
    // =========================================================

    public void PreviousSlide()
    {
        if (currentSlide <= 0)
            return;

        currentSlide--;

        UpdateSlide();
    }

    // =========================================================
    // UPDATE SLIDE
    // =========================================================

    private void UpdateSlide()
    {
        // -----------------------------------------------------
        // IMAGE
        // -----------------------------------------------------

        if (tutorialImage != null &&
            tutorialImages != null &&
            currentSlide < tutorialImages.Length)
        {
            tutorialImage.sprite =
                tutorialImages[currentSlide];
        }

        // -----------------------------------------------------
        // TEXT
        // -----------------------------------------------------

        if (tutorialText != null &&
            tutorialTexts != null &&
            currentSlide < tutorialTexts.Length)
        {
            tutorialText.text =
                tutorialTexts[currentSlide];
        }

        // -----------------------------------------------------
        // PREVIOUS BUTTON
        // -----------------------------------------------------

        if (prevButton != null)
        {
            // Slide pertama = tidak ada Prev
            prevButton.SetActive(
                currentSlide > 0
            );
        }

        // -----------------------------------------------------
        // NEXT / FINISH
        // -----------------------------------------------------

        if (nextButtonText != null)
        {
            bool isLastSlide =
                currentSlide >=
                tutorialImages.Length - 1;

            if (isLastSlide)
            {
                nextButtonText.text =
                    "Finish";
            }
            else
            {
                nextButtonText.text =
                    "Next";
            }
        }
    }

    // =========================================================
    // CLOSE
    // =========================================================

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    // =========================================================
    // GET CURRENT SLIDE
    // =========================================================

    public int GetCurrentSlide()
    {
        return currentSlide;
    }
}