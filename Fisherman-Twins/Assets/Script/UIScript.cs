using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

public class UIScript : MonoBehaviour
{
    GameController GC;
    PlayerController PC;

    public Transform scoreUI;

    public Text goldText;
    public Text weightText;
    public Text netText;
    public Text distanceText;

    public GameObject gameOverPanel;
    public Text scoreText;

    public bool isUIOn = true;

    public void GameOver()
    {
        gameOverPanel.SetActive(true);

        var score = PC.gold_total;
        scoreText.text = string.Format("Score: {0}", score);
    }
    private void Start()
    {
        GC = GameController.GetInstance();
        PC = GC.player;
    }

    public void SetUIActive(bool val)
    {
        scoreUI.gameObject.SetActive(val);
    }

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        goldText.text = string.Format("G: {0}", PC.gold_total);
        weightText.text = string.Format("W: {0}/{1}", PC.weight, MAX_NET_WEIGHT);
        netText.text = string.Format("Net: {0}/{1}", PC.net_left, DEFAULT_NET_NUM);
        var distance = PC.distance;
        distanceText.text = string.Format("{0} m", distance.ToString("0.0"));
    }
}
