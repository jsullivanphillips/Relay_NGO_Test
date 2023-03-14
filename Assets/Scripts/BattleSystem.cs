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
    [SerializeField] Color32 tauntColor;
    [SerializeField] Color32 baseColor;


    Unit enemyUnit;
    List<PlayerNetwork> players = new List<PlayerNetwork>();
    List<bool> hasPlayerActed = new List<bool>();
    int clientIdInt;

    void Start()
    {
        if (IsServer)
        {
            m_state.Value = BattleState.START;
        }
        SetupBattle();
    }

    void SetupBattle()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += (ulong clientNum) => NewClientJoin(clientNum);

            NetworkManager.OnClientDisconnectCallback += (ulong clientNum) => ClientDisconnect(clientNum);

            NewClientJoin((ulong)0);

            SpawnMobs();

            m_state.Value = BattleState.PLAYERTURN;
            
        }
        PlayerTurn();
    }

    private void SpawnMobs()
    {
        GameObject mobGO = Instantiate(enemyPrefab, enemyBattleStation.position, Quaternion.identity);
        mobGO.GetComponent<NetworkObject>().Spawn();
        enemyUnit = mobGO.GetComponent<Unit>();
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

    void SetBoolList(List<bool> lst, bool value)
    {
        for (int i = 0; i < lst.Count; i ++)
        {
            lst[i] = value;
        }    
    }

    int playerWithTaunt()
    {
        List<int> playersWithTaunt = new List<int>();
        for (int i = 0; i < players.Count; i ++)
        {
            if (players[i].hasTaunt)
                playersWithTaunt.Add(i);
        }
        if(playersWithTaunt.Count > 0)
            return playersWithTaunt[Random.Range(0, playersWithTaunt.Count)];
        
        return -1;
    }

    void PlayerTurn()
    {
        Debug.Log("Player Turn");
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy turn");
        int target;
        yield return new WaitForSeconds(1f);

        if (playerWithTaunt() != -1)
        {
            target = playerWithTaunt();
        }
        else
        {
            target = Random.Range(0, players.Count);
        }
        
        bool isDead = players[target].TakeDamage(enemyUnit.DealDamage());

        if(isDead)
        {
            m_state.Value = BattleState.LOST;
        }
        else
        {
            m_state.Value = BattleState.PLAYERTURN;
            SetBoolList(hasPlayerActed, false);
            ClearPlayerTaunts();
            PlayerTurn();
        }
    }

    void DealDamage(int damage, int clientId)
    {
        if (IsServer)
        {
            if (!hasPlayerActed[clientId])
            {
                enemyUnit.TakeDamage(damage);
                hasPlayerActed[clientId] = true;
            }
            ExitPlayerTurn();
        }
    }

    void SetPlayerTaunt(int clientId)
    {
        if(IsServer)
        {
            if(!hasPlayerActed[clientId])
            {
                players[clientId].hasTaunt = true;
                players[clientId].m_playerColor.Value = tauntColor;
                hasPlayerActed[clientId] = true;
            }
        }
        ExitPlayerTurn();
    }

    void ClearPlayerTaunts()
    {
        for (int i = 0; i < players.Count; i ++)
        {
            players[i].hasTaunt = false;
            players[i].m_playerColor.Value = baseColor;
        }
    }

    void ExitPlayerTurn()
    {
        if (AllPlayersReady())
        {
            m_state.Value = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }


    public void OnAttackBtn()
    {
        int damage = 2;
        DealDamageServerRpc(damage, new ServerRpcParams());
    }

    public void OnTauntBtn()
    {
        SetTauntServerRpc(new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTauntServerRpc(ServerRpcParams serverRpcParams)
    {
        int senderClientId = (int)serverRpcParams.Receive.SenderClientId;
        SetPlayerTaunt(senderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DealDamageServerRpc(int damage, ServerRpcParams serverRpcParams)
    {
        int senderClientId = (int)serverRpcParams.Receive.SenderClientId;
        DealDamage(damage, senderClientId);
    }

}
