using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InstatiateSummons : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;
     

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            InstantiateServerRpc(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            InstantiateServerRpc(false);
        }

    }

    [ServerRpc]
    private void InstantiateServerRpc(bool instantiate)
    {

        if (instantiate)
        {
            if (spawnedObjectTransform == null)
            {

                spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
                spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            }
        }
        else
        {
            if (spawnedObjectTransform != null)
            {

                spawnedObjectTransform.GetComponent<NetworkObject>().Despawn(true);
                Destroy(spawnedObjectTransform.gameObject);
                spawnedObjectTransform = null;
            }
        }


    }
}
