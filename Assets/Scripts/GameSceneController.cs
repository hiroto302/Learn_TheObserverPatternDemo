using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameSceneController : MonoBehaviour
{
    public event EnemyDestroyedHandler ScoreUpdateOnKill;
    public event Action<int> LifeLost;

    #region Field Declarations

    [Header("Enemy & Power Prefabs")]
    [Space]
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private PlayerController playerShip;
    [SerializeField] private PowerupController[] powerUpPrefabs;

	[Header("Level Definitions")]
    [Space]
    public List<LevelDefinition> levels;
    [HideInInspector] public LevelDefinition currentLevel;

    [Header("Player ship settings")][Space]
    [Range(3, 8)]
    public float playerSpeed = 5;
    [Range(1, 10)]
    public float shieldDuration = 3;

    private int totalPoints;
    private int lives = 3;  // プレイヤーのライフ

    private int currentLevelIndex = 0;
    private WaitForSeconds shipSpawnDelay = new WaitForSeconds(2);

    #endregion

    #region Subject Implementation

    private List<IEndGameObserver> endGameObservers;
    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    private void NotifyObservers()
    {
        foreach (IEndGameObserver observer in endGameObservers)
        {
            observer.Notify();
        }
    }

    #endregion

    #region Startup

    void Awake()
    {
        endGameObservers = new List<IEndGameObserver>();
    }

    void Start()
    {
        StartLevel(currentLevelIndex);
    }

    #endregion

    #region Level Management

    private void StartLevel(int levelIndex)
	{
     	currentLevel = levels[levelIndex];

        StartCoroutine(SpawnShip(false));
        StartCoroutine(SpawnEnemies());

        if (currentLevel.hasPowerUps)
            StartCoroutine(SpawnPowerUp());
    }

    private void EndLevel()
    {
        currentLevelIndex++;
        StopAllCoroutines();

        //If last level the game over
        if (currentLevelIndex < levels.Count)
        {
            //TODO: Clean up
            StartLevel(currentLevelIndex);
        }
    }

    #endregion

    #region Spawning

    // Player の 復活
    private IEnumerator SpawnShip(bool delayed)
    {
        if(delayed)
            yield return shipSpawnDelay;

        PlayerController ship = Instantiate(playerShip, new Vector2(0, -4.67f), Quaternion.identity);
        ship.speed = playerSpeed;
        ship.shieldDuration = shieldDuration;

        ship.HitByEnemy += Ship_HitByEnemy;  // event の追加

        yield return null;
    }

    // PlayerController クラスの HitByEnemy が発火した時、行う処理
    private void Ship_HitByEnemy()
    {
        lives--;

        if (LifeLost != null)
        {
            LifeLost(lives);
        }

        if (lives > 0)
        {
            StartCoroutine(SpawnShip(true));
        }
        else
        {
            StopAllCoroutines();
            NotifyObservers();
        }
    }

    // 敵を生み出すメソッド
    private IEnumerator SpawnEnemies()
    {
        WaitForSeconds wait = new WaitForSeconds(currentLevel.enemySpawnDelay);
        yield return wait;

        for (int i = 0; i < currentLevel.numberOfEnemies; i++)
        {
            Vector2 spawnPosition = ScreenBounds.RandomTopPosition();

            EnemyController enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.gameObject.layer = LayerMask.NameToLayer("Enemy");
            enemy.shotSpeed = currentLevel.enemyShotSpeed;
            enemy.speed = currentLevel.enemySpeed;
            enemy.shotdelayTime = currentLevel.enemyShotDelay;
            enemy.angerdelayTime = currentLevel.enemyAngerDelay;

            enemy.EnemyDestroyed += Enemy_EnemyDestroyed;  // enemy が破壊された時の event 追加

            yield return wait;
        }
    }

    private void Enemy_EnemyDestroyed(int pointValue)
    {
        // enemy が破壊された時に行う処理
        totalPoints += pointValue;

        if(ScoreUpdateOnKill != null)
        {
            ScoreUpdateOnKill(totalPoints); // event 発生
        }
    }

    private IEnumerator SpawnPowerUp()
    {
        while (true)
        {
            int index = UnityEngine.Random.Range(0, powerUpPrefabs.Length);
            Vector2 spawnPosition = ScreenBounds.RandomTopPosition();
            Instantiate(powerUpPrefabs[index], spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(UnityEngine.Random.Range(currentLevel.powerUpMinimumWait,currentLevel.powerUpMaximumWait));
        }
    }

    #endregion
}
