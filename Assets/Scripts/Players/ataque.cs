using UnityEngine;

public class ataque : MonoBehaviour
{
    [SerializeField] private int dano = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.ReceberDano(dano);
            }
        }
    }
}