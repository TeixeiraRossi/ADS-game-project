using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Transform player;
    private float speed = 3f;
    [SerializeField] private float distanciaDeParada = 1.5f;
    void Start()
    {
        /*player = GameObject.FindGameObjectWithTag("Player").transform;*/ //identifica e guarda posição de objeto com tag Player
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player != null) //Verifica se encontrou o player        
        {
            float distanciaAtual = Vector2.Distance(transform.position, player.position);//compara posição do enemy com player
            if (distanciaAtual > distanciaDeParada)
            {//move o enemy ate o player, parando na distancia de parada
                transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.gameObject.CompareTag("Player"))//Destroi o player se colidir com ele
        {
            Destroy(colisao.gameObject);
        }
    }
}

