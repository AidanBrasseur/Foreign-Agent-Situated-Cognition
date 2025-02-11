﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	public Image[] loadingImages;
	public Image loadParent;
	public TextAsset txtFile;
	public TextMeshProUGUI tutorialText;
	public GameObject quizPanel;
	public bool enableQuiz;

    private readonly int[][] loadTextIndex = new int[][] // level index --> possible text
	{
		new int[]{0}, // MainMenu
		new int[]{0 }, //Tutorial2
		new int[]{0, 1, 2, 3, 4, 5, 6}, // Tutorial1
		new int[]{0, 1, 2, 3, 4, 5, 6}, //MacrophageIntro1
		new int[]{0, 1, 2, 3, 4, 5, 6} , //MacrophageIntro2
		new int[]{7, 8, 9, 10, 11, 12, 13},//BCellTutorial
		new int[]{7, 8, 9, 10, 11, 12, 13}, //BCellIntro
		new int[]{7, 8, 9, 10, 11, 12, 13}, //BCellIntro2
		new int[]{14, 15, 16, 17}, //TCellTutorial
		new int[]{14, 15, 16, 17}, //TCellIntro1
		new int[]{14, 15, 16, 17}, //TCellIntro2
		new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17}, //1.2
		new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17}, //1.3
		new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19}, //2.1
		new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19}, //2.2
		new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19}, //2.3
		new int[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19} //Vaccine?
	};
    // Make sure the loading screen shows for at least 1 second:
    private const float MIN_TIME_TO_SHOW = 5f;
    // The reference to the current loading operation running in the background:
    private AsyncOperation currentLoadingOperation;
    // A flag to tell whether a scene is being loaded or not:
    private bool isLoading;
    // The rect transform of the bar fill game object:
    [SerializeField]
    private RectTransform barFillRectTransform;
    // Initialize as the initial local scale of the bar fill game object. Used to cache the Y-value (just in case):
    private Vector3 barFillLocalScale;
    // The text that shows how much has been loaded:
    [SerializeField]
    private Text percentLoadedText;
    // The elapsed time since the new scene started loading:
    private float timeElapsed;
    // Set to true to hide the progress bar:
    [SerializeField]
    private bool hideProgressBar;
    // Set to true to hide the percentage text:
    [SerializeField]
    private bool hidePercentageText;
    // The animator of the loading screen:
    private Animator animator;
    // Flag whether the fade out animation was triggered.
    public bool didTriggerFadeOutAnimation;
	public float alpha;
    public GameObject prevMenu;
	private string[] loadingTexts;
	public Image background;
	private Image loadImg;
    public bool hideImage = false;
	private void Awake()
    {
        // Singleton logic:

        DontDestroyOnLoad(gameObject);
        Configure();
        Hide();
		LoadText();
		quizPanel.SetActive(false);
    }
	private void LoadText()
	{
		string[] texts = txtFile.text.Split('\n');
		
		int y = SceneManager.GetActiveScene().buildIndex;
		loadingTexts = new string[loadTextIndex[y].Length];
		for (int i=0; i<loadTextIndex[y].Length; i++)
		{
			loadingTexts[i] = texts[loadTextIndex[y][i]];
		}
		
		int index = (int)Random.Range(0, loadingTexts.Length);
		Debug.Log(loadingTexts[index]);
		tutorialText.text = loadingTexts[index];
        if (!hideImage)
        {
            loadImg = Instantiate(loadingImages[loadTextIndex[y][index]]);

            Rect spriteRect = loadImg.sprite.rect;
            float spriteWidth = spriteRect.width;
            float spriteHeight = spriteRect.height;
            loadImg.enabled = true;
            loadImg.transform.SetParent(loadParent.transform);

            loadImg.rectTransform.sizeDelta = new Vector2(500.0f / spriteRect.height * spriteWidth, 500);
            loadImg.rectTransform.localPosition = new Vector3(0, 250, 0);

            loadImg.transform.gameObject.AddComponent<fadeWithBackground>();
        }
	}
	private void Configure()
    {
        // Save the bar fill's initial local scale:
        barFillLocalScale = barFillRectTransform.localScale;
        // Enable/disable the progress bar based on configuration:1
        barFillRectTransform.transform.parent.gameObject.SetActive(!hideProgressBar);
        // Enable/disable the percentage text based on configuration:
        percentLoadedText.gameObject.SetActive(!hidePercentageText);
        // Cache the animator:
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (isLoading)
        {
			alpha = background.GetComponent<Graphic>().color.a;
            // Get the progress and update the UI. Goes from 0 (start) to 1 (end):
            SetProgress(currentLoadingOperation.progress);
			// If the loading is complete and the fade out animation has not been triggered yet, trigger it:
            if (currentLoadingOperation.isDone && !didTriggerFadeOutAnimation)
            {
				animator.SetTrigger("Hide");
                didTriggerFadeOutAnimation = true;
                quizPanel.SetActive(false);
                this.enabled = false;

				Destroy(gameObject, 5f);
            }
            else
            {
                timeElapsed += Time.deltaTime;
                if (timeElapsed >= MIN_TIME_TO_SHOW)
                {
					// The loading screen has been showing for the minimum time required.
					// Allow the loading operation to formally finish:
					//Debug.Log("ending loading screen");
					if (enableQuiz)
					{
						if (!quizPanel.activeSelf)
						{
							ActivateQuiz();
						}
						
						if (quizPanel.GetComponent<Quiz>().correct)
						{
							currentLoadingOperation.allowSceneActivation = true;

						}
					}
					else {
						currentLoadingOperation.allowSceneActivation = true;
					}
                    
                }
            }
        }
    }

	private void ActivateQuiz()
	{
		quizPanel.SetActive(true);
		quizPanel.GetComponent<Quiz>().GenerateNewQuestion();

	}

    // Updates the UI based on the progress:
    private void SetProgress(float progress)
    {
        // Update the fill's scale based on how far the game has loaded:
        barFillLocalScale.x = progress;
        // Update the rect transform:
        barFillRectTransform.localScale = barFillLocalScale;
        // Set the percent loaded text:
        percentLoadedText.text = Mathf.CeilToInt(progress * 100).ToString() + "%";
    }
    // Call this to show the loading screen.
    // We can determine the loading's progress when needed from the AsyncOperation param:
    public void Show(AsyncOperation loadingOperation)
    {

        // Enable the loading screen:
        gameObject.SetActive(true);
        prevMenu.SetActive(false);
        // Store the reference:
        currentLoadingOperation = loadingOperation;
        // Stop the loading operation from finishing, even if it technically did:
        currentLoadingOperation.allowSceneActivation = false;
        // Reset the UI:
        SetProgress(0f);
        // Reset the time elapsed:
        timeElapsed = 0f;
        // Play the fade in animation:
        animator.SetTrigger("Show");
        // Reset the fade out animation flag:
        didTriggerFadeOutAnimation = false;
        isLoading = true;

		//TODO: Reset information / display stuff
    }
    // Call this to hide it:
    public void Hide()
    {
        // Disable the loading screen:
        gameObject.SetActive(false);
        currentLoadingOperation = null;
        isLoading = false;
    }
}