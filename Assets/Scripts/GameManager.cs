using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GamePhase
{
    Gathering,
    Survival,
    Escape
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    public GameObject playerPrefab;
    GameObject player;
    
    [Header("Abilities & Game Over")]
    public GameObject[] abilityTriggers;
    public Image[] abilityIcons;
    public GameObject gameOverPanel;
    public TextMeshProUGUI hidePromptText;
    
    [Header("Game Flow Mechanics")]
    public TextMeshProUGUI generalPromptText;
    public TextMeshProUGUI survivalTimerText;
    public GameObject liftObject;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Gathering;
    public int ItemsCollected { get; private set; } = 0;
    private const int MaxItems = 3;
    private float survivalTimer = 60f;
    private bool isGameOver;

    Pathfinding pathfinding;
    DirectorAI directorAI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        player = GameObject.Find("Player");
        pathfinding = GameObject.Find("Hunter").GetComponent<Pathfinding>();
        directorAI = FindObjectOfType<DirectorAI>();

        if (hidePromptText != null)
            hidePromptText.gameObject.SetActive(false);

        if (generalPromptText != null)
            generalPromptText.gameObject.SetActive(false);

        if (survivalTimerText != null)
            survivalTimerText.gameObject.SetActive(false);

        if (liftObject != null)
            liftObject.SetActive(false);
    }

    void Update()
    {
        if (!player)
        {
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.transform.position = new Vector3(-17.56f, 2.51f, -4.09f);
            newPlayer.name = "Player";
            player = newPlayer;
        }

        if (CurrentPhase == GamePhase.Survival)
        {
            survivalTimer -= Time.deltaTime;
            
            if (survivalTimerText != null)
            {
                int minutes = Mathf.FloorToInt(Mathf.Max(0, survivalTimer) / 60F);
                int seconds = Mathf.FloorToInt(Mathf.Max(0, survivalTimer) - minutes * 60);
                survivalTimerText.text = string.Format("SURVIVE !!\n{0:00}:{1:00}", minutes, seconds);
            }

            if (survivalTimer <= 0)
            {
                StartEscapePhase();
            }
        }
    }

    public void ItemCollected()
    {
        ItemsCollected++;
        if (ItemsCollected >= MaxItems && CurrentPhase == GamePhase.Gathering)
        {
            StartSurvivalPhase();
        }
    }

    private void StartSurvivalPhase()
    {
        CurrentPhase = GamePhase.Survival;
        survivalTimer = 60f;
        
        if (survivalTimerText != null)
            survivalTimerText.gameObject.SetActive(true);

        if (directorAI != null)
        {
            directorAI.SetMenaceLocked(true, 100f);
        }
    }

    private void StartEscapePhase()
    {
        CurrentPhase = GamePhase.Escape;
        
        if (survivalTimerText != null)
            survivalTimerText.gameObject.SetActive(false);
        
        if (liftObject != null)
            liftObject.SetActive(true);

        ShowGeneralPrompt(true, "keluar");
    }

    public void Abilities(int index)
    {
        pathfinding.moreLikely(index);
        //abilityIcons[index].color = Color.green;
    }

    public void ShowGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over!");
        }

        StartCoroutine(ReturnToMainMenuAfterDelay(5f));
    }

    private IEnumerator ReturnToMainMenuAfterDelay(float delay)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 1f;

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ReturnToMainMenu();
            yield break;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        isGameOver = false;
    }

    public void ShowHidePrompt(bool visible, string message)
    {
        if (hidePromptText == null)
            return;

        hidePromptText.gameObject.SetActive(visible);
        if (visible)
            hidePromptText.text = message;
    }

    public void ShowHidePrompt(bool visible)
    {
        ShowHidePrompt(visible, "Press E to hide");
    }

    public void ShowGeneralPrompt(bool visible, string message = "")
    {
        if (generalPromptText == null) return;
        generalPromptText.gameObject.SetActive(visible);
        if (visible)
            generalPromptText.text = message;
    }
}
