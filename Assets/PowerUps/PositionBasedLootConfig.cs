using UnityEngine;

[CreateAssetMenu(menuName = "Kart/Loot/Position Based Config")]
public class PositionBasedLootConfig : ScriptableObject
{
    [Header("Rarity weights by position (1..N)")]
   

    public AnimationCurve common = AnimationCurve.Linear(0, 70, 1, 20);
    public AnimationCurve uncommon = AnimationCurve.Linear(0, 20, 1, 25);
    public AnimationCurve rare = AnimationCurve.Linear(0, 8, 1, 30);
    public AnimationCurve epic = AnimationCurve.Linear(0, 2, 1, 18);
    public AnimationCurve legendary = AnimationCurve.Linear(0, 0, 1, 7);

    public void GetWeights(int position, int total, out float wC, out float wU, out float wR, out float wE, out float wL)
    {
        total = Mathf.Max(1, total);
        position = Mathf.Clamp(position, 1, total);

        // 0..1 (0 = primero, 1 = último)
        float t = (total == 1) ? 0f : (float)(position - 1) / (total - 1);

        wC = Mathf.Max(0f, common.Evaluate(t));
        wU = Mathf.Max(0f, uncommon.Evaluate(t));
        wR = Mathf.Max(0f, rare.Evaluate(t));
        wE = Mathf.Max(0f, epic.Evaluate(t));
        wL = Mathf.Max(0f, legendary.Evaluate(t));
    }
}