using UnityEngine;

public class buff : MonoBehaviour
{
    public enum TipoBuff
    {
        Vida,
        Energia,
        XP
    }

    public TipoBuff tipoBuff;
    public float valorBuff = 10f;
    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if(collision.CompareTag("Player"))
        {
            // Apply the buff effect to the player
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.AplicaBuff(tipoBuff, valorBuff);
                Destroy(gameObject); // Destroy the buff object after applying
            }
        }
    }
}
