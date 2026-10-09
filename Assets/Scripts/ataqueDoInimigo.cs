using UnityEngine;

public class ataqueDoInimigo : MonoBehaviour
{

    [SerializeField] private int dano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
               BarraVida player = other.GetComponent<BarraVida>();
                if (player != null)
                {
                    player.ReceberDano(dano);
                    Debug.Log("Player recebeu dano: " + dano);
                }
            }
        }
    }
}
