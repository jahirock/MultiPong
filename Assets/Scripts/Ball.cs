using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class Ball : NetworkBehaviour
{
    public float speed = 30F;
    public Rigidbody2D rb;

    NetworkManagerPong networkManager;

    public int leftScore;
    public int rightScore;

    public TMP_Text textScore, winnerScore;

    public override void OnStartServer()
    {
        StartCoroutine(StartBall(0));
        networkManager = FindObjectOfType<NetworkManagerPong>();
    }

    private void Start()
    {
        textScore = GameObject.Find("ScoreText").GetComponent<TMP_Text>();
        winnerScore = GameObject.Find("WinnerPlayerText").GetComponent<TMP_Text>();
        winnerScore.gameObject.SetActive(false);
    }

    [ServerCallback]
    void Update()
    {
        int delay = 0;
        if(transform.position.x > networkManager.rightRacketSpawn.position.x)
        {
            leftScore++;
            if (leftScore >= 5)
            {
                RpcWinState("Izquierdo");
                delay = 5;
                leftScore = 0;
                rightScore = 0;
            }
            StartCoroutine(StartBall(delay));
            RpcUpdateTextScore(leftScore, rightScore);
        }

        if (transform.position.x < networkManager.leftRacketSpawn.position.x)
        {
            rightScore++;
            if (leftScore >= 5)
            {
                RpcWinState("Derecho");
                delay = 5;
                leftScore = 0;
                rightScore = 0;
            }
            StartCoroutine(StartBall(delay));
            RpcUpdateTextScore(leftScore, rightScore);
        }
    }

    [ServerCallback] //Solo se ejecutan en el servidor, no en el cliente.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.GetComponent<Player>() != null)
        {
            //Se refleja el angulo que llevaba la pelota
            rb.velocity = Vector2.Reflect(rb.velocity, collision.contacts[0].normal).normalized * speed;
            //Se calcula la velocidad en y de la pelota, dependiendo de la posicion con respecto a la raqueta
            float y = HitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.y);
            //Se cambia la direccion en x
            float x = collision.relativeVelocity.x > 0 ? 1 : -1;

            Vector2 dir = new Vector2(x, y).normalized;
            rb.velocity = dir * speed;
        }
    }

    float HitFactor(Vector2 ballPosition, Vector2 racketPosition, float racketHeight)
    {
        return (ballPosition.y - racketPosition.y) / racketHeight;
    }

    [ClientRpc]
    public void RpcUpdateTextScore(int leftScore, int rightScore)
    {
        textScore.text = $"{leftScore} - {rightScore}";
    }

    [ClientRpc]
    public void RpcWinState(string winner)
    {
        winnerScore.gameObject.SetActive(true);
        winnerScore.text = $"Ha ganado el jugador del lado <b>{winner}</b>";
    }

    [ClientRpc]
    public void RpcDisableWinState()
    {
        winnerScore.gameObject.SetActive(false);
    }

    IEnumerator StartBall()
    {
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        transform.position = Vector2.zero;

        yield return new WaitForSeconds(2);

        rb.simulated = true;

        float direction = Random.Range(0f, 1f) > 0.5F? 1: -1;

        rb.velocity = Vector2.right * speed * direction;
    }

    IEnumerator StartBall(float delay)
    {
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        transform.position = Vector2.zero;
        // Nuevo codigo
        yield return new WaitForSeconds(delay);
        RpcDisableWinState();
        yield return new WaitForSeconds(2);
        rb.simulated = true;
        float direction = Random.Range(0f, 1f) > 0.5f ? 1 : -1;
        rb.velocity = Vector2.right * speed * direction;
    }
}
