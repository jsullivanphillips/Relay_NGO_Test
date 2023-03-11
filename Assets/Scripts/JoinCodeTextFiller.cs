using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoinCodeTextFiller : MonoBehaviour
{
    [SerializeField] TMP_Text joinCodeText;
    JoinCodeHolder joinCodeHolder;

    private void Start()
    {
        joinCodeHolder = FindObjectOfType<JoinCodeHolder>();
        joinCodeText.text = joinCodeHolder.joinCode;
    }
}
