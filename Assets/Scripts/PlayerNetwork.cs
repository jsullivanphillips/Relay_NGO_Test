using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;


public class PlayerNetwork : NetworkBehaviour
{
    private int localClientId;
    //public NetworkVariable<FixedString128Bytes> playerName = new NetworkVariable<FixedString128Bytes>();
    public NetworkVariable<Color32> m_playerColor = new NetworkVariable<Color32>();
    NetworkVariable<int> hpMax = new NetworkVariable<int>(0);
    NetworkVariable<int> hp = new NetworkVariable<int>(0);
    [SerializeField] int localHpMax;
    [SerializeField] int localHp;
    [SerializeField] int attackDamage;

    public List<Unit> enemyUnits = new List<Unit>();

    int target = 0;

    public bool hasTaunt = false;


    [SerializeField] Color32 tauntColor;
    [SerializeField] Color32 baseColor;
    [SerializeField] SpriteRenderer spriteRenderer;

    private NetworkVariable<Vector3> m_playerPosition = new NetworkVariable<Vector3>(new Vector3(0,0,0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] Slider healthBar;
    private BattleSystem bs;


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            localClientId = (int)NetworkManager.Singleton.LocalClientId;
            Debug.Log(localClientId);
        }
        if(IsServer)
        {
            hpMax.Value = localHpMax;
            hp.Value = localHp;
        }

        m_playerColor.OnValueChanged += (Color32 previousValue, Color32 newValue) =>
        {
            spriteRenderer.color = newValue;
        };

        bs = FindObjectOfType<BattleSystem>();

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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeTarget();
        }
    }

    private void ChangeTarget()
    {
        bs.GetNextTargetServerRpc(target, new ServerRpcParams());
    }

    [ClientRpc]
    public void RecieveNextTargetClientRpc(int newTarget, ClientRpcParams clientRpcParams = default)
    {
        target = newTarget;
    }


    public int GetTarget()
    {
        return target;
    }

    public int GetAttackDamage()
    {
        return attackDamage;
    }

    // This should only be called by server anyway
    public bool TakeDamage(int amount)
    {
        if (IsServer)
        {
            hp.Value -= amount;
            Debug.Log("new hp: " + hp.Value);

            if (hp.Value <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }


}
