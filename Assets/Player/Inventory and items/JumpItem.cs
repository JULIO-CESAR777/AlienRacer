using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Items/JumpItem")]
public class JumpItem : ItemBase
{
    public float boostForce = 25f;
    public float duration = 6f;

    public override void Use(KartController user)
    {
        user.GetComponent<KartPowerUpController>().ApplyJump( boostForce, duration);
    }
}
