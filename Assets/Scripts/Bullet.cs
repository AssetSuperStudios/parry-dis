using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] public float movementSpeed = 24f;
    [SerializeField] private float lifeTime = 10.0f;
    
    private Rigidbody2D rb;
    private bool velocityInitialized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();    
        
        if (!velocityInitialized && rb != null)
        {
            ApplyPhysicsVelocity();
        }

        Destroy(gameObject, lifeTime);
    }

    public void SetBulletSpeed(float newSpeed)
    {
        movementSpeed = newSpeed;
        
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            ApplyPhysicsVelocity();
        }
    }

    private void ApplyPhysicsVelocity()
    {
        rb.linearVelocity = transform.right * movementSpeed;
        velocityInitialized = true;
    }
}
