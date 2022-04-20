using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string path;
    public void Awake()
    {
        path = Application.streamingAssetsPath + "/PyExe/"+ "program.exe";
        OpenWithStartInfo();
    }
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);      
    }

    public void QuittGame()
    {
        UnityEngine.Debug.Log("quit");
        Application.Quit();
    } 
    void OpenWithStartInfo()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(path);
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;
        Process.Start(startInfo);
    }
}
