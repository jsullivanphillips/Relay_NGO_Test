using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, PAUSE }

public class BattleSystem : NetworkBehaviour
{
    private NetworkVariable<BattleState> m_state =
        new NetworkVariable<BattleState>(BattleState.START, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public GameObject enemyPrefab;
    public GameObject playerPrefab;
    public Transform enemyBattleStation;
    public Transform[] playerBattleStations;
    Unit enemyUnit;

    void Start()
    {
        m_state.Value = BattleState.START;
        SetupBattle();
    }

    void SetupBattle()
    {
        if (IsServer)
        {
            GameObject mobGO = Instantiate(enemyPrefab, enemyBattleStation.position, Quaternion.identity);
            mobGO.GetComponent<NetworkObject>().Spawn();
            enemyUnit = mobGO.GetComponent<Unit>();
            NetworkManager.OnClientConnectedCallback += (ulong value) => { Debug.Log("client: " + value + " connected"); };
        }
    }
        
    public void DealDamage()
    {
        if (IsServer)
        {
            enemyUnit.TakeDamage(2);
        }
        if(!IsServer)
        {
            DealDamageServerRpc();
        }
    }

    private void Update()
    {
        // do this once every 3 seconds?
        // or maybe have a function on value changed equal to number of clients connected
    }

    [ServerRpc(RequireOwnership = false)]
    void DealDamageServerRpc()
    {
        DealDamage();
    }

}
