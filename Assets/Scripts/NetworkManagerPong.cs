using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerPong : NetworkManager
{
    public Transform leftRacketSpawn;
    public Transform rightRacketSpawn;
    public GameObject ballPrefab;
    public Ball ball;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPosition = numPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;

        GameObject player = Instantiate(playerPrefab, startPosition.position, Quaternion.identity);

        NetworkServer.AddPlayerForConnection(conn, player);

        if(numPlayers == 2)
        {
            var temBall = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.Spawn(temBall);
            ball = temBall.GetComponent<Ball>();
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if(ball != null)
        {
            ball.RpcUpdateTextScore(0, 0);
            ball.textScore.text = "0 - 0";
            NetworkServer.Destroy(ball.gameObject);
        }
        base.OnServerDisconnect(conn);
    }
}
