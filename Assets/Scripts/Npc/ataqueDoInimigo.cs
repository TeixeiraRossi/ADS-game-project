using UnityEngine;

public class ataqueDoInimigo : MonoBehaviour
{

    [SerializeField] private int dano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!other.CompareTag("Player"))
                return;

            BarraVida playerVida = other.GetComponentInParent<BarraVida>();

            if (playerVida != null)
                return;

            playerVida.ReceberDano(dano);

            Debug.Log("Player recebeu dano: " + dano);
        }
    }
}
