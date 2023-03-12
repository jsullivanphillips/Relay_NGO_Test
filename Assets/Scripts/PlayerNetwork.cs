using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    private int localClientId;
    private NetworkVariable<Vector3> m_playerPosition = new NetworkVariable<Vector3>(new Vector3(0,0,0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private BattleSystem bs;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            localClientId = (int)NetworkManager.Singleton.LocalClientId;
            Debug.Log(localClientId);
        }

        bs = FindObjectOfType<BattleSystem>();

        m_playerPosition.OnValueChanged += (Vector3 previousValue, Vector3 newValue) => {
            transform.position += newValue;
        };
    }

    public void SetSpawnLocation(Vector3 spawnLocation)
    {
        if (IsOwner)
        {
            m_playerPosition.Value = spawnLocation;
        }
    }


}
