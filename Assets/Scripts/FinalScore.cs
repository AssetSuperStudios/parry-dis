using UnityEngine;
using TMPro;

public class FinalScore : MonoBehaviour
{
    [SerializeField] 
    private TMP_Text scoreText;
    [SerializeField] 
    private TMP_Text breakdownText;
    
    [SerializeField] 
    private Score score;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{starScore()}";
        } 

        breakdownText.text = 
        $"Perfect: {score.perfectCount}\n"+
        $"Great: {score.greatCount}\n"+
        $"Safe: {score.safeCount}\n"+
        $"Miss: {score.missCount}\n"+
        $"Fail: {score.failCount}";

        Debug.Log($"{score.playerScore}");
    }

    string starScore()
    {
        string starString = "⭐";
        int points = score.playerScore;
        if (points >= 500)
        {
            starString += "⭐";
        }
        if (points >= 1000)
        {
            starString += "⭐";
        }
        return starString;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
