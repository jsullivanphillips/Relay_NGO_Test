using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Unit : NetworkBehaviour
{
    public NetworkVariable<int> hpMax = new NetworkVariable<int>(10);
    public NetworkVariable<int> hp = new NetworkVariable<int>(8);
    [SerializeField] int localHpMax;
    [SerializeField] int localHp;

    [SerializeField] Slider healthBar;
    [SerializeField] int damage = 4;
    public GameObject targetingIcon;
    bool isDead = false;

    public override void OnNetworkSpawn()
    {
        SetupHealthbar();
    }

    void SetupHealthbar()
    {
        if (IsServer)
        {
            hpMax.Value = localHpMax;
            hp.Value = localHp;

            if (hp.Value > hpMax.Value)
            {
                hpMax.Value = hp.Value;
            }

        }
        hp.OnValueChanged += (int previousValue, int newValue) =>
        {
            healthBar.value = hp.Value;
        };

        healthBar.maxValue = hpMax.Value;
        healthBar.value = hp.Value;
    }

    public virtual bool TakeDamage(int amount)
    {
        if(!isDead)
        {
            hp.Value -= amount;

            if (hp.Value <= 0)
            {
                isDead = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }


    public int DealDamage()
    {
        if(!isDead)
        return damage;

        return 0;
    }


}
