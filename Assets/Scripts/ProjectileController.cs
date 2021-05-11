using UnityEngine;

public delegate void OutOfBoundsHandler();  // delegate

public class ProjectileController : MonoBehaviour
{
    #region Field Declarations

    public Vector2 projectileDirection;
    public float projectileSpeed;
    public bool isPlayers;

    #endregion
    public event OutOfBoundsHandler ProjectileOutOfBounds;  // event

    #region Movement

    void Update()
    {
        MoveProjectile();
    }

    private void MoveProjectile()
    {
        transform.Translate(projectileDirection * Time.deltaTime * projectileSpeed, Space.World);

        if (ScreenBounds.OutOfBounds(transform.position))
        {
            if(isPlayers == true)
            {
                // 下記のような記述では, TightCoupling になるので、拡張性、Debug、unitTest、maintain しにくいので event を活用する

                // PlayerController playerShip = FindObjectOfType<PlayerController>();
                // playerShip.EnableProjectile();

                if (ProjectileOutOfBounds != null) // このように 参照されてるものがあるかチェックすることで拡張性を持たすことができる
                {
                    ProjectileOutOfBounds();

                    // event に格納されている メソッドの確認
                    // var observers = ProjectileOutOfBounds.GetInvocationList();
                    // foreach( var observer in observers)
                    // {
                    //     print(observer.Method);
                    // }
                }
            }
            Destroy(gameObject);
        }
    }

    #endregion
}
