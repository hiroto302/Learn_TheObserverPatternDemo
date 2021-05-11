using System.Collections;
using UnityEngine;
using System;           // C# Actions are types in the System namespace

public class PlayerController : MonoBehaviour
{
    #region Field Declarations

    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private GameObject availableBullet;
    [SerializeField] private GameObject shield;
    [SerializeField] private GameObject expolsion;
    //Set by GameSceneController
    [HideInInspector] public float shieldDuration;
    [HideInInspector] public float speed;

    private bool projectileEnabled = true;
    private WaitForSeconds shieldTimeOut;
    private GameSceneController gameSceneController;
    private ProjectileController lastProjectile;

    public event Action HitByEnemy;     // Delegate Type Action : ダメージを受けた時に発火させる

    #endregion

    #region Startup

    private void Start()
    {
        gameSceneController = FindObjectOfType<GameSceneController>();
        // event の追加 : ここでは ScoreUpdateOnKill に追加してるが, EnemyDestroyedHandlerクラスの EnemyDestroyed に追加した方が合理的であると思う
        gameSceneController.ScoreUpdateOnKill += GameSceneController_ScoreUpdateOnKill;

        shieldTimeOut = new WaitForSeconds(shieldDuration);
        EnableShield();
    }

    // gameSceneController.ScoreUpdateOnKill の delegate に 追加するためにmethod引数があるだけ
    private void GameSceneController_ScoreUpdateOnKill(int pointValue)
    {
        EnableProjectile();
    }

    #endregion

    #region Movement & Control

    // Update is called once per frame
    void Update()
    {
        MovePlayer();

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (projectileEnabled)
            {
                FireProjectile();
            }
        }
    }

    private void MovePlayer()
    {
        float horizontalMovement = Input.GetAxis("Horizontal");

        if(Mathf.Abs(horizontalMovement) > Mathf.Epsilon)
        {
            horizontalMovement = horizontalMovement * Time.deltaTime * speed;
            horizontalMovement += transform.position.x;

            float limit =
                Mathf.Clamp(horizontalMovement, ScreenBounds.left, ScreenBounds.right);

            transform.position = new Vector2(limit, transform.position.y);
        }
    }

    #endregion

    #region Projectile Management

    // 投射物の有効化
    public void EnableProjectile()
    // private void EnableProjectile() : event を活用することで private にできる
    {
        projectileEnabled = true;
        availableBullet.SetActive(projectileEnabled);
    }
    // 投射物の無効化
    public void DisableProjectile()
    {
        projectileEnabled = false;
        availableBullet.SetActive(projectileEnabled);
    }

    // 投射物の発射
    private void FireProjectile()
    {
        if (projectileEnabled)
        {
            Vector2 spawnPosition = availableBullet.transform.position;

            ProjectileController projectile =
                Instantiate(projectilePrefab, spawnPosition, Quaternion.AngleAxis(90, Vector3.forward));

            projectile.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            projectile.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
            projectile.isPlayers = true;
            projectile.projectileSpeed = 4;
            projectile.projectileDirection = Vector2.up;

            lastProjectile = projectile;

            // Player が GameOver になった時、Player が Destroy され, 発射されたものがまだ画面外へ到達した時、参照するはずの Player が存在しないので error が起きないよに対処する
            projectile.ProjectileOutOfBounds += EnableProjectile;    // event の 追加

            DisableProjectile();
        }
    }

    #endregion


    #region Damage



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<ProjectileController>())
            TakeHit();
    }

    private void TakeHit()
    {
        GameObject xp = Instantiate(expolsion, transform.position, Quaternion.identity);
        xp.transform.localScale = new Vector2(2, 2);

        if(HitByEnemy != null)
            HitByEnemy();

        lastProjectile.ProjectileOutOfBounds -= EnableProjectile;  // remove
        gameSceneController.ScoreUpdateOnKill -= GameSceneController_ScoreUpdateOnKill;

        Destroy(gameObject);
    }

    #endregion

    #region Shield Management

    public void EnableShield()
    {
        shield.SetActive(true);
        StartCoroutine(DisableShield());
    }

    private IEnumerator DisableShield()
    {
        yield return shieldTimeOut;
        shield.SetActive(false);
        
    }

    #endregion
}
