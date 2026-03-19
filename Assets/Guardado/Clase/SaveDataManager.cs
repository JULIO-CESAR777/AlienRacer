using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class SaveDataManager : MonoBehaviour
{


    [SerializeField] PlayerStats playerStats;
    [SerializeField] ChestsOpened chestManager;
    [SerializeField] WaveStats waveStats;
    StringBuilder sb = new StringBuilder();
  



    void Update()
    {


        if (Input.GetKeyDown(KeyCode.S))
        {
           sb.Clear();

           sb.Append( playerStats.SaveData());
           sb.Append(".");
           sb.Append(waveStats.SaveData());
           sb.Append(".");
           sb.Append(chestManager.SaveData());

          Debug.Log(sb.ToString());


        }
        else if (Input.GetKeyDown(KeyCode.D))
        {

            string[] dataDivide = sb.ToString().Split('.');


            playerStats.LoadData(dataDivide[0]);
            waveStats.LoadData(dataDivide[1]);
            waveStats.LoadData(dataDivide[2]);
        }
    }





}
