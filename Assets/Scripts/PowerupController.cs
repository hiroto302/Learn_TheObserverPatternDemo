using UnityEngine;

public class PowerupController :MonoBehaviour, IEndGameObserver
{
    #region Field Declarations

    public GameObject explosion;

    [SerializeField]
    private PowerType powerType;

    #endregion

    #region Movement

    void Update()
    {
       Move();
    }

    private void Move()
    {
        transform.Translate(Vector2.down * Time.deltaTime * 3, Space.World);

        if (ScreenBounds.OutOfBounds(transform.position))
        {
            // Destroy(gameObject);
            RemoveAndDestroy();
        }
    }

    #endregion

    #region Collisons

    private void OnCollisionEnter2D(Collision2D collision)
    {
       //TODO: Apply Power ups
        if (powerType == PowerType.Shield)
        {
            PlayerController playerShip = collision.gameObject.GetComponent<PlayerController>();
            if(playerShip != null)
            {
                playerShip.EnableShield();
            }
        }

        // Destroy(gameObject);
        RemoveAndDestroy();
    }

    // 破壊された時、Observer が知らせた処理を実行する時、追加した処理を知らせる Observerが消えた時,参照するのを避けるために下記の処理を行う
    private void RemoveAndDestroy()
    {
        GameSceneController gameSceneController = FindObjectOfType<GameSceneController>();
        gameSceneController.RemoveObserver(this);
        Destroy(gameObject);
    }

    public void Notify()
    {
        Destroy(gameObject);
    }

    #endregion
}

public enum PowerType
{
    Shield,
    X2
};