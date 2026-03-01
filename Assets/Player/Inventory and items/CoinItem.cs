using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Items/Coins")]
public class CoinItem : ItemBase
{
 
    public int coins = 0;
    public override void Use(KartController user)
    {
        
        for (int i = 0; i < coins; i++)
        {
            user.AddCoin();
            
        }
    }
}
