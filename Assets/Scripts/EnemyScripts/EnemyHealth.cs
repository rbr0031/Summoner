using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>();
    private EnemyAi _enemyAI;

    // Health Handler
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.OnValueChanged += OnHealthChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            health.OnValueChanged -= OnHealthChanged;
        }
    }

    public void TakeDamage(int damage, NetworkObjectReference attackerId)
    {

        Debug.Log($"TakeDamageServerRpc called with damage: {damage} and attackerId: {attackerId}");
        health.Value -= damage;
        Debug.Log($"Health after damage: {health.Value}, Attacker ID: {attackerId}");

        if (health.Value <= 0)
        {
            Die();
        }
        else
        {
            var enemyAI = GetComponent<EnemyAi>();
            if (enemyAI != null)
            {
                Debug.Log($"Setting current target to attackerId: {attackerId}");
                enemyAI.SetCurrentTarget(attackerId);
            }
            else
            {
                Debug.LogError("EnemyAi component not found on the enemy object.");
            }
        }
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (newHealth <= 0)
        {
            DieClientRpc();
        }
    }

    private void Die()
    {
        DieClientRpc();
        Destroy(gameObject);
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        // Handle client-side death logic here, like playing animations or effects
        if (!IsOwner)
        {
            Destroy(gameObject);
        }
    }
}