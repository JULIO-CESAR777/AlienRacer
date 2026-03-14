using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
[CreateAssetMenu(menuName = "Guardados/Waves")]
public class WaveStats : ScriptableObject
{

    public int waves;

    public int enemiesKilled;

    public string SaveData()
    {

        StringBuilder sb = new StringBuilder();

        sb.Append(waves);
        sb.Append("/");
        sb.Append(enemiesKilled);





        return sb.ToString();
    }


    public void LoadData(string cadena)
    {

        string[] dataDivide = cadena.Split('/');
     

        waves = int.Parse(dataDivide[0]);
        enemiesKilled = int.Parse(dataDivide[1]);




    }
}
