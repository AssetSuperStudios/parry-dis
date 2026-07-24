using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] 
    float movementSpeed = 3.2f;
    private Rigidbody2D rb;
    [SerializeField] 
    float lifeTime = 10.0f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();    
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * movementSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision.");
        if (collision.GetComponent<Player>())
        {
            Destroy(gameObject);
            Debug.Log("Player hit.");
        }
    }
}
