using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    private int localClientId;
    private NetworkVariable<Vector3> m_playerPosition = new NetworkVariable<Vector3>(new Vector3(0,0,0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Vector3[] spawnLocations = {
        new Vector3(-5f, 1f, 0f),
        new Vector3(-5f, -1f, 0f),
        new Vector3(-5f, 2.5f, 0f),
        new Vector3(-5f, -2.5f, 0f)
    };

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            localClientId = (int)NetworkManager.Singleton.LocalClientId;
            Debug.Log(localClientId);
            m_playerPosition.Value = spawnLocations[localClientId];
            
        }
        transform.position = m_playerPosition.Value;
    }

    private void Update()
    {
        transform.position = m_playerPosition.Value;
    }
}
