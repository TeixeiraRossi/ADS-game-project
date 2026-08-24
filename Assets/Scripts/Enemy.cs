using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Transform player;

    [Header("Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private float distanciaDeParada = 1.5f;

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;

    private int vidaAtual;

    private void Start()
    {
        vidaAtual = vidaMaxima;
    }

    private void Update()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        player = playerObject.transform;

        float distanciaAtual = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distanciaAtual > distanciaDeParada)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                velocidade * Time.deltaTime
            );
        }
    }

    public void ReceberDano(int dano)
    {
        vidaAtual -= dano;

        Debug.Log("Inimigo recebeu dano! Vida: " + vidaAtual);

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        Debug.Log("Inimigo morreu!");

        Destroy(gameObject);
    }
}