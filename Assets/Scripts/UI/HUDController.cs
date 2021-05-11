using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour, IEndGameObserver
{
	#region Field Declarations

	[Header("UI Components")]
    [Space]
	public Text scoreText;
    public StatusText statusText;
    public Button restartButton;

    [Header("Ship Counter")]
    [SerializeField]
    [Space]
    private Image[] shipImages;

    private GameSceneController gameSceneController;

    #endregion

    #region Startup

    private void Awake()
    {
        statusText.gameObject.SetActive(false);
    }

    private void Start()
    {
        gameSceneController = FindObjectOfType<GameSceneController>();
        gameSceneController.ScoreUpdateOnKill += GameSceneController_ScoreUpdateOnKill; // event 追加
        gameSceneController.LifeLost += HideShip;

        gameSceneController.AddObserver(this); // observer が実行する処理を追加
    }

    private void GameSceneController_ScoreUpdateOnKill(int pointValue)
    {
        UpdateScore(pointValue);
    }

    #endregion

    #region Public methods

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString("D5");
    }

    public void ShowStatus(string newStatus)
    {
        statusText.gameObject.SetActive(true);
        StartCoroutine(statusText.ChangeStatus(newStatus));
    }

    public void HideShip(int imageIndex)
    {
        shipImages[imageIndex].gameObject.SetActive(false);
    }

    public void ResetShips()
    {
        foreach (Image ship in shipImages)
            ship.gameObject.SetActive(true);
    }

    public void Notify()
    {
        ShowStatus("Game Over");
    }

    #endregion
}
