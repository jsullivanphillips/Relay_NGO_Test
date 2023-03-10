using System;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class RelayManager : MonoBehaviour
{ 
    public TMP_Text PlayerIdText;

    public TMP_Text JoinCodeText;

    public TMP_InputField InputJoinCodeText;

    [SerializeField] GameObject NetworkUI;

    JoinCodeHolder joinCodeHolder;
    Guid hostAllocationId;
    Guid playerAllocationId;
    string allocationRegion = "";
    string joinCode = "N/A";
    string playerId = "Not signed in";

    async void Start()
    {
        await UnityServices.InitializeAsync();
        OnSignIn();
        joinCodeHolder = FindObjectOfType<JoinCodeHolder>();
        UpdateUI();
    }

    
    void UpdateUI()
    {
        PlayerIdText.text = "PlayerId: " + playerId;
        JoinCodeText.text = "Join code: " + joinCode;
    }

    /// <summary>
    /// Event handler for when the Sign In button is clicked.
    /// </summary>
    public async void OnSignIn()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerId = AuthenticationService.Instance.PlayerId;

        Debug.Log($"Signed in. Player ID: {playerId}");
        UpdateUI();
    }

    public async void OnHostGame()
    {
        // Important: Once the allocation is created, you have ten seconds to BIND
        Allocation allocation;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(4);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        hostAllocationId = allocation.AllocationId;
        allocationRegion = allocation.Region;

        Debug.Log($"Host Allocation ID: {hostAllocationId}, region: {allocationRegion}");

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            joinCodeHolder.joinCode = joinCode;
            
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();

        UpdateUI();
        NetworkUI.SetActive(false);
    }

    public void OnSubmitJoinCodeText()
    {
        joinCode = InputJoinCodeText.text;
        Debug.Log("joinCode: " + joinCode);
        OnJoin();
    }

    public async void OnJoin()
    {
        Debug.Log("Player - Joining host allocation using join code: |" + joinCode + "|");

        JoinAllocation allocation;
        try
        {//getting stuck right here rn. Faulty join code? cant find relay server? not sure...
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();
        NetworkUI.SetActive(false);
    }




}
