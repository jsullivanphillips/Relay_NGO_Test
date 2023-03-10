using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlayerController : NetworkBehaviour
{

    NetworkVariable<Vector3> unitPosition = new NetworkVariable<Vector3>(new Vector3(0, 0, 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] GameObject sceneLoader;

    public override void OnNetworkSpawn()
    {
        

        // This is where the position is being changed
        unitPosition.OnValueChanged += (Vector3 previousValue, Vector3 newValue) =>
        {
            transform.position += newValue;
        };
        
    }

    public void LoadLevel1()
    {
        Instantiate(sceneLoader);
    }

    void Update()
    {
        if (!IsSpawned || !IsOwner)
        {
            return;
        }

        Vector3 moveDir = new Vector3(0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.y = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
        unitPosition.Value = moveDir * moveSpeed * Time.deltaTime;

    }
}
