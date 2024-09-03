using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections;

public class TerritoryConquest : Minigame
{
    [Header("Minigame Variables")]
    public TileClaim tileClaim;
    public Vector2[] spawnPoints = new Vector2[4];
    public TMP_Text timeText, markText;
    public int timer = 60;
    internal int playerIndex;

    public override void SpawnPlayers()
    {
        playerPrefab.GetComponent<SpriteRenderer>().sortingOrder = playerSpriteOrder;
        playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[playerIndex], Quaternion.identity);
    }

    public override void StartMinigame()
    {
        AudioSource.Play();

        score = 0;
        ReceiveSelectedTopic();
        GenerateQuestions();
        StartCoroutine(StartTimerCoroutine());

        //InitializePlayerData();
    }

    IEnumerator StartTimerCoroutine()
    {
        while (timer > 0)
        {
            timeText.text = $"Time left: {timer}";
            yield return new WaitForSeconds(1);
            timer--;
        }
    }

    public override void AnswerCorrect()
    {
        score++;
        tileClaim.ClaimTile();

        AudioManager.PlaySound(correctClip);
        markText.text = "Correct!";
        StartCoroutine(ClearMarkText());
        UnfreezePlayer();
        quizPanel.SetActive(false);
        // update all player current tiles

        RemoveQuestion(currentQuestionIndex);
        GenerateQuestions();
    }

    public override void AnswerWrong()
    {
        AudioManager.PlaySound(wrongClip);
        markText.text = "Wrong!";
        StartCoroutine(ClearMarkText());
        UnfreezePlayer();
        quizPanel.SetActive(false);
        // update all player current tiles

        RemoveQuestion(currentQuestionIndex);
        GenerateQuestions();
    }
    IEnumerator ClearMarkText()
    {
        yield return new WaitForSeconds(2.5f);
        markText.text = string.Empty;
    }

    public override void EndMinigame()
    {
        
    }
}
