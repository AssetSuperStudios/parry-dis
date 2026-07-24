using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Coroutine _bulletFiring;
    [Header("Bullet")]
    [SerializeField]
    private GameObject _bulletPrefab;
    [SerializeField] 
    float bulletInterval = 3.0f;
    [SerializeField] 
    private Transform _offset;
    private Animator enemyAnimator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Fetch the Enemy's Animator component
        enemyAnimator = GetComponent<Animator>();

        _bulletFiring = StartCoroutine(BulletFiring());
    }

    private IEnumerator BulletFiring()
    {
        WaitForSeconds delay = new WaitForSeconds(bulletInterval);

        while (true)
        {
            FireBullet();
            yield return delay;
        }
    }

    async Task FireBullet()
    {
        // Start the long range animation
        enemyAnimator.SetTrigger("isLRange");
        await Awaitable.WaitForSecondsAsync(0.2f);

        Instantiate(_bulletPrefab, _offset.position, transform.rotation);
    }

    void MeleeAttack()
    {
        // Start the tp melee animation
        enemyAnimator.SetTrigger("isTPMelee");
    }

    private void DisableBulletFire()
    {
        if (_bulletFiring != null) StopCoroutine(_bulletFiring);
    }
}
