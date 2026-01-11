using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace YourNamespace
{
    /// <summary>
    /// Tests connectivity to the game server by sending a test request.
    /// Used for development and debugging server connections.
    /// </summary>
    public class TestServer : MonoBehaviour
    {
        /// <summary>
        /// Unity lifecycle method called before the first frame update.
        /// Initiates the server connectivity test.
        /// </summary>
        void Start()
        {
            StartCoroutine(CheckServer());
        }

        /// <summary>
        /// Coroutine that sends a test request to the server and logs the response.
        /// </summary>
        /// <returns>IEnumerator for coroutine execution.</returns>
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
}
