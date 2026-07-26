using UnityEngine;
using UnityEngine.UI;

public class FinalScore : MonoBehaviour
{
    [SerializeField] 
    private Text scoreText; 
    [SerializeField] 
    private Score score;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (scoreText != null) scoreText.text = ""; 

        scoreText.text = $"SCORE: {score.playerScore}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
