using System.Collections;
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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    void FireBullet()
    {
        Instantiate(_bulletPrefab, _offset.position, transform.rotation);
    }

    private void DisableBulletFire()
    {
        if (_bulletFiring != null) StopCoroutine(_bulletFiring);
    }
}
