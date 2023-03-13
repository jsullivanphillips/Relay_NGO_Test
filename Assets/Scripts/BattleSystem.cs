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
    List<PlayerNetwork> players = new List<PlayerNetwork>();
    List<bool> hasPlayerActed = new List<bool>();
    int clientIdInt;

    void Start()
    {
        m_state.Value = BattleState.START;
        SetupBattle();
    }

    void SetupBattle()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += (ulong clientNum) => NewClientJoin(clientNum);

            NetworkManager.OnClientDisconnectCallback += (ulong clientNum) => ClientDisconnect(clientNum);

            NewClientJoin((ulong)0);

            GameObject mobGO = Instantiate(enemyPrefab, enemyBattleStation.position, Quaternion.identity);
            mobGO.GetComponent<NetworkObject>().Spawn();
            enemyUnit = mobGO.GetComponent<Unit>();

            m_state.Value = BattleState.PLAYERTURN;
            
        }
        PlayerTurn();
    }

    private void NewClientJoin(ulong clientNum)
    {
        clientIdInt = (int)clientNum;
        Debug.Log("Client " + clientIdInt + " has connected.");
        var playerGO = Instantiate(playerPrefab, playerBattleStations[clientIdInt].position, Quaternion.identity);
        playerGO.GetComponent<NetworkObject>().Spawn();
        players.Add(playerGO.GetComponent<PlayerNetwork>());
        hasPlayerActed.Add(false);
    }

    private void ClientDisconnect(ulong clientNum)
    {
        clientIdInt = (int)clientNum;
        NetworkObject clientNetworkObject = players[clientIdInt].GetComponentInParent<NetworkObject>();
        clientNetworkObject.Despawn();
        players.RemoveAt(clientIdInt);
        Debug.Log("Client " + clientIdInt + " has disconnected.");
    }

    bool AllPlayersReady()
    {
        for(int i = 0; i < hasPlayerActed.Count; i++)
        {
            if (hasPlayerActed[i] == false)
                return false;
        }
        return true;
    }

    void PlayerTurn()
    {
        Debug.Log("Player Turn");
        if(IsServer)
        {
            Debug.Log(hasPlayerActed);
        }
    }

    public void DealDamage(int damage, int clientId)
    {
        if (IsServer)
        {
            Debug.Log("client clicking attack button: " + clientId);
            Debug.Log("hasPlayerActed lenght: " + hasPlayerActed.Count);
            if (!hasPlayerActed[clientId])
            {
                enemyUnit.TakeDamage(damage);
                hasPlayerActed[clientId] = true;
            }
        }
    }

    void EnemyTurn()
    {
        Debug.Log("Enemy turn");
        int randomTarget = Random.Range(0, players.Count);
        bool isDead = players[randomTarget].TakeDamage(enemyUnit.DealDamage());
        if(isDead)
        {
            m_state.Value = BattleState.LOST;
        }
        else
        {
            m_state.Value = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    public void OnAttackBtn()
    {
        int damage = 2;
        DealDamageServerRpc(damage, new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    public void DealDamageServerRpc(int damage, ServerRpcParams serverRpcParams)
    {
        int senderClientId = (int)serverRpcParams.Receive.SenderClientId;
        DealDamage(damage, senderClientId);
    }

}
