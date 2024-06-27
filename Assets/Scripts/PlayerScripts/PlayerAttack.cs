using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    public int attackDamage = 10;
    public Texture2D swordCursor;
    public Vector2 cursorHotspot = Vector2.zero;
    private Transform currentTarget;

    void Update()
    {
        if(!IsClient) return;

        HandleMouseHover();

        if (Input.GetMouseButtonDown(0) && currentTarget != null) // Left mouse button
        {
            Attack();
        }
    }

    private void HandleMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                if (currentTarget != hit.transform)
                {
                    ResetPreviousTarget();
                    currentTarget = hit.transform;
                }
                Cursor.SetCursor(swordCursor, cursorHotspot, CursorMode.Auto); // Change cursor to sword
                return;
            }
        }
        ResetPreviousTarget();
    }

    private void ResetPreviousTarget()
    {
        if (currentTarget != null)
        {
            currentTarget = null;
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Reset cursor to default
    }

    void Attack()
    {
        if (currentTarget != null)
        {
            var enemy = currentTarget.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                var clientId = NetworkManager.Singleton.LocalClientId;
                Debug.Log($"Attacking enemy with ID: {currentTarget.GetComponent<NetworkObject>().NetworkObjectId}");
                AttackRpc(currentTarget.GetComponent<NetworkObject>(), clientId);
            }
        }
    }

    [Rpc(SendTo.Server)]    
    void AttackRpc(NetworkObjectReference currentTarget, ulong clientId)
    {
        var attackerId = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        Debug.Log($"Server received attack on enemy ID: {currentTarget} from attacker ID: {attackerId}");

        if (currentTarget.TryGet(out NetworkObject enemyObject))
        {
            Debug.Log($"Enemy object found with ID: {currentTarget}");
            var enemy = enemyObject.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                Debug.Log($"EnemyHealth component found on object with ID: {currentTarget}");
                enemy.TakeDamage(attackDamage, attackerId);
            }
            else
            {
                Debug.LogError("EnemyHealth component not found on the enemy object.");
            }
        }
        else
        {
            Debug.LogError($"Enemy object not found with ID: {currentTarget}");
        }
    }
}
