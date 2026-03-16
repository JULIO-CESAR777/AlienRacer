using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


[CreateAssetMenu(menuName = "Guardados/Chests")]
public class ChestsOpened : ScriptableObject
{
   

    [SerializeField] public List<string> openedChests = new List<string>();







    public string SaveData(  )
    {

        StringBuilder sb = new StringBuilder();

        foreach (string c in openedChests)
        {
            sb.Append(c);
            sb.Append("/");
        }


        return sb.ToString();



    }


    public void LoadData(string cadena)
    {

        
        string[] dataDivide = cadena.Split('/');
        /*for (int i = 0; i < dataDivide.Length; i++)
        {

            Debug.LogFormat("DATA {0} : {1}",i, dataDivide[i]);
        }*/

        for (int i = 0; i <openedChests.Count; i++)
        {
            openedChests[i] = dataDivide[i ];
        }


    }

}
