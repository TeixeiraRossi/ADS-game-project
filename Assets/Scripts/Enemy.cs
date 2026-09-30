using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Guarda a referência para o Transform do jogador
    private Transform player;

    [Header("Movimento")]

    // Velocidade com que o inimigo persegue o jogador
    [SerializeField] private float velocidade = 3f;

    // Distância mínima que o inimigo mantém do jogador
    [SerializeField] private float distanciaDeParada = 1.5f;

    [Header("Vida")]

    // Quantidade máxima de vida do inimigo
    [SerializeField] private int vidaMaxima = 3;

    [Header("Knockback")]

    // Força com que o inimigo será empurrado ao receber um ataque
    [SerializeField] private float forcaKnockback = 1.5f;

    // Tempo que o inimigo ficará atordoado depois de receber o ataque
    [SerializeField] private float duracaoStun = 0.2f;


    // Guarda a quantidade atual de vida do inimigo
    private int vidaAtual;

    // Indica se o inimigo está atordoado
    private bool atordoado;


    // Start é executado uma vez quando o objeto é iniciado
    private void Start()
    {
        // Começa o inimigo com a vida máxima configurada
        vidaAtual = vidaMaxima;
    }


    // Update é executado a cada frame
    private void Update()
    {
        // Se o inimigo estiver atordoado,
        // ele não pode se movimentar
        if (atordoado)
            return;


        // Procura o objeto que possui a Tag "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");


        // Se não encontrou nenhum jogador,
        // interrompe a função
        if (playerObject == null)
            return;


        // Guarda o Transform do jogador encontrado
        player = playerObject.transform;


        // Calcula a distância entre o inimigo e o jogador
        float distanciaAtual = Vector2.Distance(
            transform.position,
            player.position
        );


        // Se o inimigo estiver longe o suficiente do jogador,
        // ele começa a persegui-lo
        if (distanciaAtual > distanciaDeParada)
        {
            // Move o inimigo na direção do jogador
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                velocidade * Time.deltaTime
            );
        }
    }

    // Função chamada quando o inimigo colide com Player
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            Player playerScript = collider.GetComponent<Player>();
            if (playerScript != null)
            {
                //playerScript.ReceberDano(1); // Aplica 1 de dano ao jogador
                Debug.Log("Inimigo colidiu com o Player! Dano aplicado: 1");
            }
        }
    }

    // Função chamada quando o inimigo recebe dano
    public void ReceberDano(int dano)
    {
        // Diminui a vida do inimigo
        vidaAtual -= dano;


        // Mostra no Console quanto de vida o inimigo ainda possui
        Debug.Log("Inimigo recebeu dano! Vida: " + vidaAtual);


        // Aplica o knockback e o stun
        AplicarKnockback();


        // Verifica se a vida chegou a zero
        if (vidaAtual <= 0)
        {
            // Se chegou a zero, o inimigo morre
            Morrer();
        }
    }

    /*public void AplicarDano(int dano)
    {
        if(player != null)
        {
            Player playerScript = player.GetComponent<Player>();
            if(playerScript != null)
            {
                playerScript.ReceberDano(dano);
            }
        }
    }*/


    // Função responsável por empurrar o inimigo para trás
    // e deixá-lo atordoado por alguns segundos
    private void AplicarKnockback()
    {
        // Se não temos referência ao jogador,
        // não conseguimos calcular a direção do knockback
        if (player == null)
            return;


        // Ativa o estado de atordoamento
        atordoado = true;


        // Calcula a direção contrária ao jogador
        // Jogador está à esquerda do inimigo
        // O inimigo será empurrado para a direita
        Vector2 direcaoKnockback =
            (transform.position - player.position).normalized;


        // Move o inimigo para trás utilizando
        // a força configurada no Inspector
        transform.position +=
            (Vector3)(direcaoKnockback * forcaKnockback);


        // Depois do tempo configurado,
        // chama a função que encerra o stun
        Invoke(nameof(FinalizarStun), duracaoStun);
    }


    // Função chamada depois que o tempo de stun termina
    private void FinalizarStun()
    {
        // Permite que o inimigo volte a se movimentar
        atordoado = false;
    }


    // Função responsável pela morte do inimigo
    private void Morrer()
    {
        // Mostra uma mensagem no Console
        Debug.Log("Inimigo morreu!");


        // Remove o inimigo da cena
        Destroy(gameObject);
    }
}