using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TestServer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CheckServer());
    }

    IEnumerator CheckServer()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:5100/test");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Odpowiedź serwera: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Błąd połączenia: " + request.error);
        }
    }
}
