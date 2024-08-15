using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviourPunCallbacks
{
    static public GameManager Instance;

    [Header("Bonus Score")]

    [SerializeField]
    private int bonusScore = 5;
    [FormerlySerializedAs("teamBounsText")] [SerializeField]
    private TextMeshProUGUI teamScoreText;
    private const string TeamScoreKey = "teamScore";
    private int teamScore = 0;

    [Header("Object Spawn")]
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField] private Transform spawnedObjectsParent;
    [SerializeField] private TextMeshProUGUI nextSpawnPlaceText;
    [SerializeField] private TextMeshProUGUI roomManagerText;
    private int nextSpawnIndex;

    public void AddTeamScore()
    {
        teamScore += bonusScore;
        teamScoreText.text = $"Team Score: {teamScore}";

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
        }
        OnMasterClientSwitched(PhotonNetwork.MasterClient);
    }


    private void UpdateNextSpawnIndexIfMasterClient()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        nextSpawnPlaceText.text = $"[Manager Only] Next spawn at index: {nextSpawnIndex}";
        nextSpawnPlaceText.gameObject.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        SpawnNewPlayer();

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(TeamScoreKey))
        {
            teamScore = (int)PhotonNetwork.CurrentRoom.CustomProperties[TeamScoreKey];
            teamScoreText.text = $"Team Score: {teamScore}";
        }
    }

    private void SpawnNewPlayer()
    {
        GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(0,0,-.1f), Quaternion.identity);
        player?.GetComponent<PlayerSetup>().IsPlayerOwner();
    }

    private IEnumerator SpawnNewObject()
    {

        nextSpawnIndex = Random.Range(0, spawnPoints.Length);

        UpdateNextSpawnIndexIfMasterClient();

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
        Debug.Log($"Player #{newMasterClient.ActorNumber} is the MasterClient");
        UpdateMasterClientUI(newMasterClient);
        if (PhotonNetwork.IsMasterClient)
        {
            UpdateNextSpawnIndexIfMasterClient();
            StartCoroutine(SpawnNewObject());
        }
    }
    private void UpdateMasterClientUI(Photon.Realtime.Player newMasterClient)
    {
        roomManagerText.text = $"Player #{newMasterClient.NickName} is the Room Manager";
        roomManagerText.gameObject.SetActive(true);
    }
}
