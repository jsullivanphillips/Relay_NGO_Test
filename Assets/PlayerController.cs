using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlayerController : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (!IsSpawned)
        {
            return;
        }
        if (!IsOwner)
        {
            return;
        }
        Vector3 moveDir = new Vector3(0, 0, 0);
        if(Input.GetKey(KeyCode.W))
        {
            moveDir += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir += new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir += new Vector3(1, 0, 0);
        }
        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
