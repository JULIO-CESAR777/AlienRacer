using UnityEngine;

public class SaveDataManager : MonoBehaviour
{


    [SerializeField] PlayerStats playerStats;

    private void Start()
    {
        
    }

    void Update()
    {


        if (Input.GetKeyDown(KeyCode.S))
        {
            playerStats.SaveData();

        }
        else if (Input.GetKeyDown(KeyCode.D))
        { 
            playerStats.LoadData(); 
        }
    }


}
