using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    static public GameManager Instance;

    [Header("Bonus Score")]

    [SerializeField] int bonusScore = 5;
    [SerializeField] TextMeshProUGUI teamBounsText;
    private const string TeamScoreKey = "teamScore";
    private int teamScore = 0;

    [Header("Object Spawn")]
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] Transform spawnedObjectsParent;
    [SerializeField] TextMeshProUGUI nextSpawnPlaceText;
    [SerializeField] TextMeshProUGUI roomManagerText;
    private int nextSpawnIndex;

    public void AddTeamScore()
    {
        teamScore += bonusScore;
        teamBounsText.text = $"Team Score: {teamScore}";

        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            {TeamScoreKey, teamScore}
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnNewPlayer();
            StartCoroutine(SpawnNewObject());
            DisplayNextSpawnIndex();
        }
    }


    private void DisplayNextSpawnIndex()
    {
        nextSpawnPlaceText.text = $"The gift spawn is: {nextSpawnIndex}";
        nextSpawnPlaceText.gameObject.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        SpawnNewPlayer();

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(TeamScoreKey))
        {
            teamScore = (int)PhotonNetwork.CurrentRoom.CustomProperties[TeamScoreKey];
            teamBounsText.text = $"Team Score: {teamScore}";
        }
    }

    void SpawnNewPlayer()
    {
        GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(0,0,-.1f), Quaternion.identity);
        player?.GetComponent<PlayerSetup>().IsPlayerOwner();
    }

    IEnumerator SpawnNewObject()
    {

        nextSpawnIndex = Random.Range(0, spawnPoints.Length);
        nextSpawnPlaceText.text = $"The gift spawn is: {nextSpawnIndex}";

        UpdateRoomState();

        yield return new WaitForSeconds(10f);

        PhotonNetwork.InstantiateRoomObject("Bonus Score", spawnPoints[nextSpawnIndex].position, Quaternion.identity);

        StartCoroutine(SpawnNewObject());

        yield return null;
    }
    private void UpdateRoomState()
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
    {
        { "NextSpawnIndex", nextSpawnIndex }
    };

        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"{newMasterClient.NickName} is the new MasterClient");
        UpdateMasterClientUI(newMasterClient);
        if (PhotonNetwork.IsMasterClient)
        {
            DisplayNextSpawnIndex();
            StartCoroutine(SpawnNewObject());
        }
    }
    private void UpdateMasterClientUI(Photon.Realtime.Player newMasterClient)
    {
        roomManagerText.text = $"{newMasterClient.NickName} is now the Room Manager!";
        roomManagerText.gameObject.SetActive(true);
    }
}
