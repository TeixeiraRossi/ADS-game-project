using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Transform player;
    private float speed = 3f;
    [SerializeField] private float distanciaDeParada = 1.5f;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            float distanciaAtual = Vector2.Distance(transform.position, player.position);
            if (distanciaAtual > distanciaDeParada)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.gameObject.CompareTag("Player"))
        {
            Destroy(colisao.gameObject);
        }
    }
}

