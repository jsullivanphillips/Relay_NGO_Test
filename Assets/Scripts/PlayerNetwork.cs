using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;


public class PlayerNetwork : NetworkBehaviour
{
    private int localClientId;
    private NetworkVariable<FixedString128Bytes> playerName = new NetworkVariable<FixedString128Bytes>();

    public NetworkVariable<int> hpMax = new NetworkVariable<int>(10);
    public NetworkVariable<int> hp = new NetworkVariable<int>(10);

    private NetworkVariable<Vector3> m_playerPosition = new NetworkVariable<Vector3>(new Vector3(0,0,0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] Slider healthBar;
    //private BattleSystem bs;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            localClientId = (int)NetworkManager.Singleton.LocalClientId;
            Debug.Log(localClientId);
        }

        //bs = FindObjectOfType<BattleSystem>();

        m_playerPosition.OnValueChanged += (Vector3 previousValue, Vector3 newValue) => {
            transform.position += newValue;
        };

        SetupHealthbar();
    }

    void SetupHealthbar()
    {
        hp.OnValueChanged += (int previousValue, int newValue) =>
        {
            healthBar.value = hp.Value;
        };

        healthBar.maxValue = hpMax.Value;
        healthBar.value = hp.Value;
    }


    public void SetSpawnLocation(Vector3 spawnLocation)
    {
        if (IsOwner)
        {
            m_playerPosition.Value = spawnLocation;
        }
    }

    public bool TakeDamage(int amount)
    {
        hp.Value -= amount;
        Debug.Log("new hp: " + hp.Value );

        if (hp.Value <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}
