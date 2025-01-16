using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    private const int damage = -20;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(this);
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        PlayerNetworkStats playerStats = collider.gameObject.GetComponent<PlayerNetworkStats>();

        if (playerStats != null)
        {
            playerStats.ReceiveDamage(damage);
        }

        Destroy(gameObject);
    }

}