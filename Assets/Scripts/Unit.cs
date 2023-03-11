using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Unit : NetworkBehaviour
{
    public NetworkVariable<int> hpMax = new NetworkVariable<int>(10);
    public NetworkVariable<int> hp = new NetworkVariable<int>(8);
    [SerializeField] Slider healthBar;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (hp.Value > hpMax.Value)
            {
                hpMax.Value = hp.Value;
            }
        }
        healthBar.maxValue = hpMax.Value;
        StartCoroutine(UpdateHp());
        Debug.Log("Jerry hp value: " + hp.Value);
        Debug.Log("Jerry hpMax value: " + hpMax.Value);

    }

    public virtual bool TakeDamage(int amount)
    {
        hp.Value -= amount;

        if (hp.Value <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator UpdateHp()
    {
        healthBar.value = hp.Value;
        yield return new WaitForSeconds(1f);
        StartCoroutine(UpdateHp());
    }


}
