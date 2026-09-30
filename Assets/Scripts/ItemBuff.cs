using UnityEngine;

public class ItemBuff : MonoBehaviour
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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("colidiu!");
        if (collider.CompareTag("Player"))
        {
            BarraVida player = collider.GetComponent<BarraVida>();
            if (player != null)
            {
                player.ReceberBuff(tipo, quantidade); // Chama o método para aplicar o buff no Player
                Destroy(gameObject); // Destroi o objeto do buff após ser coletado
            }
        }
    }
}
