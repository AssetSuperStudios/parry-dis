using UnityEngine;

[CreateAssetMenu(fileName = "Score", menuName = "Scriptable Objects/Score")]
public class Score : ScriptableObject
{
    public int playerScore;
    public int perfectCount;
    public int greatCount;
    public int safeCount;
    public int missCount;
    public int failCount;
}
