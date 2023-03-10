using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class JoinCodeHolder : MonoBehaviour
{
    public string joinCode;
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

}
