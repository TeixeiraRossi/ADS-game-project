using UnityEngine;

public class Buff : MonoBehaviour
{
    public enum TipoBuff
    {
        vida,
        energia,
        xp
    }

    [Header("Configurações do Buff")]
    public TipoBuff tipo;
    public float quantidade = 2f;

    void OnTriggerEnter(Collider colisor)
    {
        if (colisor.CompareTag("Player"))
        {
            Player player = colisor.GetComponent<Player>();
            if (player != null)
            {
                player.ReceberBuff(tipo, quantidade); // Chama o método para aplicar o buff no Player
                Destroy(gameObject); // Destroi o objeto do buff após ser coletado
            }
        }
    }
}
