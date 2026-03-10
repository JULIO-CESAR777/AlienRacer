using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerStats", menuName = "PlayerStats", order = 0)]
public class PlayerStats : ScriptableObject
{
    public int hp;
    public int str;

    public int def;
    public float speed;

    public string characterName;

    public bool tutorialCompleted;

    [SerializeField] public List<string> openedChests = new List<string>();
 



    public void SaveData()
    {

        StringBuilder sb= new StringBuilder();

        sb.Append(hp);
        sb.Append("/");
        sb.Append(str);
        sb.Append("/");
        sb.Append(def);
        sb.Append("/");
        sb.Append(speed);
        sb.Append("/");
        sb.Append(characterName);
        sb.Append("/");
        sb.Append(tutorialCompleted? 1:0);
        sb.Append("/");

        foreach (string c in openedChests)
        {  
            sb.Append(c);
            sb.Append("/");
        }



        PlayerPrefs.SetString("playerStats", sb.ToString());
#if UNITY_EDITOR
        Debug.Log(sb.ToString());
#endif


    }

    public void LoadData()
    {

        string data= PlayerPrefs.GetString("playerStats", "");
        string[] dataDivide = data.Split('/');
        /*for (int i = 0; i < dataDivide.Length; i++)
        {

            Debug.LogFormat("DATA {0} : {1}",i, dataDivide[i]);
        }*/

        hp = int.Parse(dataDivide[0]);
        str = int.Parse(dataDivide[1]);
        def = int.Parse(dataDivide[2]);
        speed = float.Parse(dataDivide[3]);
        characterName = dataDivide[4];

        tutorialCompleted = int.Parse(dataDivide[5]) > 0;

        for (int i = 0; i < openedChests.Count; i++)
        {
            openedChests[i] = dataDivide[i + 6];
        }


    }

    void resetData()
    {
        hp= 0;
        str = 0;

        def = 0;
        speed = 0;

        characterName = "DAWD";

        tutorialCompleted = false;



    }
}
