using System.Collections;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    
    [SerializeField] private Button grantMasterClientButton;
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


    private void UpdateNextSpawnIndexText()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            nextSpawnPlaceText.text = $"[Manager Only] Next spawn at index: {nextSpawnIndex}";
            nextSpawnPlaceText.gameObject.SetActive(true);
        }
        else
        {
            nextSpawnPlaceText.gameObject.SetActive(false);
        }
    }

    public void GrantMasterClientToNextPlayer()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        var isOnlyPlayer = PhotonNetwork.CurrentRoom.PlayerCount == 1;
        if (isOnlyPlayer) return;
        
        var nextPlayer = PhotonNetwork.CurrentRoom.Players
            .Values
            .FirstOrDefault(player => player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber + 1) ?? PhotonNetwork.CurrentRoom.Players
            .Values
            .First(player => player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber);

        PhotonNetwork.SetMasterClient(nextPlayer);
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

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        grantMasterClientButton.gameObject.SetActive(PhotonNetwork.CurrentRoom.PlayerCount > 1);
    }

    private void SpawnNewPlayer()
    {
        GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(0,0,-.1f), Quaternion.identity);
        player?.GetComponent<PlayerSetup>().IsPlayerOwner();
    }

    private IEnumerator SpawnNewObject()
    {

        nextSpawnIndex = Random.Range(0, spawnPoints.Length);

        UpdateNextSpawnIndexText();

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
        UpdateNextSpawnIndexText();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnNewObject());
        }
        else
        {
            grantMasterClientButton.gameObject.SetActive(false);
        }
    }
    private void UpdateMasterClientUI(Photon.Realtime.Player newMasterClient)
    {
        roomManagerText.text = $"Player #{newMasterClient.ActorNumber} is the Room Manager";
        roomManagerText.gameObject.SetActive(true);

        grantMasterClientButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }
}
