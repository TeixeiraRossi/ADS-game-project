using UnityEngine;

public class ataque : MonoBehaviour
{
    public Player player; // arraste o Player no Inspector ou use GetComponentInParent

    private void Awake()
    {
        if (player == null)
            player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
        }
    }
}

