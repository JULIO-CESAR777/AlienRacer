using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(menuName = "Guardados/Players")]
public class PlayerStats : ScriptableObject
{
    public int hp;
    public int str;

   


    public string SaveData( )
    {

        StringBuilder sb = new StringBuilder();

        sb.Append(hp);
        sb.Append("/");
        sb.Append(str);
       





        return sb.ToString();
    }


    public void LoadData(string cadena)
    {
 
        string[] dataDivide = cadena.Split('/');
        /*for (int i = 0; i < dataDivide.Length; i++)
        {

            Debug.LogFormat("DATA {0} : {1}",i, dataDivide[i]);
        }*/

        hp = int.Parse(dataDivide[0]);
        str = int.Parse(dataDivide[1]);
        
        


    }
}
