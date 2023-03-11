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
    public Transform enemyBattleStation;
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

    [ServerRpc(RequireOwnership = false)]
    void DealDamageServerRpc()
    {
        DealDamage();
    }

}
