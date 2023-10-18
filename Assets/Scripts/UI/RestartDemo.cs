using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartDemo : MonoBehaviour
{
    public int totalSeconds = 3;
    public GameObject uiObject;
    public TMPro.TextMeshProUGUI text;

    private static RestartDemo _instance;
    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
    }

    public static void RestartAndEnable()
    {
        if(_instance)
            _instance.Restart();
    }

    public void Restart()
    {
        uiObject.SetActive(true);
        StartCoroutine(DoRestart());
    }

    IEnumerator DoRestart()
    {
        for(int i = 0; i < totalSeconds + 1; i++)
        {
            text.text = (totalSeconds - i) + "";
            yield return new WaitForSecondsRealtime(1f);
        }
        text.text = "Loading...";
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
