using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditAwake : MonoBehaviour
{
    private void Awake()
    {
        Invoke(nameof(TransferScene), 5f);
    }

    private void TransferScene()
    {
        SceneManager.LoadScene("CreditScene");
    }
}
