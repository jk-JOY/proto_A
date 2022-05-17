﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using static UIManager;
using UnityEngine.Experimental.Rendering.Universal;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	public static NetworkManager NM;
	void Awake() => NM = this;

	public GameObject DisconnectPanel, WaitingPanel, InfoPanel, GamePanel, ReportPanel, 
		 KickPanel, NoOneKickPanel, CrewWinPanel, ImposterWinPanel;

	public List<PlayerScript> Players = new List<PlayerScript>();
	public PlayerScript MyPlayer;
	
	public GameObject CrewInfoText, ImposterInfoText, WaitingBackground, Background;
	public bool isGameStart;
	public Transform SpawnPoint;
	public Light2D PointLight2D;
	public GameObject[] Interactions;
	public GameObject[] Doors;
	public GameObject[] Lights;
	PhotonView PV;
	public bool isTest;
	public enum ImpoType { OnlyMaster, Rand1, Rand2 }
	public ImpoType impoType;

	void Start()
	{
		if (isTest) return;

		Screen.SetResolution(720, 405, false);
		PV = photonView;
		ShowPanel(DisconnectPanel);
		ShowBackground(WaitingBackground);

		//채팅시스템. 

	}

	public void Connect(InputField NickInput)
    {
        if (string.IsNullOrWhiteSpace(NickInput.text)) return;
        PhotonNetwork.LocalPlayer.NickName = NickInput.text;
        PhotonNetwork.ConnectUsingSettings();
    }

	public override void OnConnectedToMaster()
	{
        PhotonNetwork.JoinLobby();
	}

	public override void OnJoinedLobby()
	{
		PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 10 }, null);
	}

	public override void OnJoinedRoom()
	{
		ShowPanel(WaitingPanel);
		MyPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity)
			.GetComponent<PlayerScript>();

		SetRandColor();
	}

	public void ShowPanel(GameObject CurPanel) 
	{
		DisconnectPanel.SetActive(false);
		WaitingPanel.SetActive(false);
		InfoPanel.SetActive(false);
		GamePanel.SetActive(false);
		ReportPanel.SetActive(false);
		//EmergencyPanel.SetActive(false);
		//VotePanel.SetActive(false);
		KickPanel.SetActive(false);
		NoOneKickPanel.SetActive(false);
		CrewWinPanel.SetActive(false);
		ImposterWinPanel.SetActive(false);

		CurPanel.SetActive(true);
	}

	void ShowBackground(GameObject CurBackground) 
	{
		WaitingBackground.SetActive(false);
		Background.SetActive(false);

		CurBackground.SetActive(true);
	}

	void SetRandColor() 
	{
		List<int> PlayerColors = new List<int>();
		for (int i = 0; i < Players.Count; i++)
			PlayerColors.Add(Players[i].colorIndex);

		while (true) 
		{
			int rand = Random.Range(1, 13);
			if (!PlayerColors.Contains(rand)) 
			{
				MyPlayer.GetComponent<PhotonView>().RPC("SetColor", RpcTarget.AllBuffered, rand);
				break;
			} 
		}
	}

	public void SortPlayers() => Players.Sort((p1, p2) => p1.actor.CompareTo(p2.actor));

	public Color GetColor(int colorIndex) 
	{
		return UM.colors[colorIndex];
	}

	public void GameStart() 
	{
		// 방장이 게임시작
		SetImpoCrew();
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;
		//ChatManager.CM.photonView.RPC("ChatClearRPC", RpcTarget.AllViaServer, false);

		PV.RPC("GameStartRPC", RpcTarget.AllViaServer);
	}

	//
	void SetImpoCrew() 
	{
		List<PlayerScript> GachaList = new List<PlayerScript>(Players);

		if (impoType == ImpoType.OnlyMaster)
		{
			Players[0].GetComponent<PhotonView>().RPC("SetImpoCrew", RpcTarget.AllViaServer, true);// 테스트 : 방장만 임포스터
		}

		else if (impoType == ImpoType.Rand1)
		{
			for (int i = 0; i < 1; i++) // 임포스터 1명 (테스트)
			{
				int rand = Random.Range(0, GachaList.Count); // 랜덤
				Players[rand].GetComponent<PhotonView>().RPC("SetImpoCrew", RpcTarget.AllViaServer, true);
				GachaList.RemoveAt(rand);
			}
		}

		else if (impoType == ImpoType.Rand2) 
		{
			for (int i = 0; i < 2; i++) // 임포스터 2명 (기본)
			{
				int rand = Random.Range(0, GachaList.Count); // 랜덤
				Players[rand].GetComponent<PhotonView>().RPC("SetImpoCrew", RpcTarget.AllViaServer, true);
				GachaList.RemoveAt(rand);
			}
		}
	}

	[PunRPC]
	void GameStartRPC() 
	{
		StartCoroutine(GameStartCo());
	}

	IEnumerator GameStartCo() 
	{
		ShowPanel(InfoPanel);
		ShowBackground(Background);
		if (MyPlayer.isImposter) ImposterInfoText.SetActive(true);
		else CrewInfoText.SetActive(true);

		yield return new WaitForSeconds(3);
		isGameStart = true;
		MyPlayer.SetPos(SpawnPoint.position);
		MyPlayer.SetNickColor();
		MyPlayer.SetMission();
		UM.GetComponent<PhotonView>().RPC("SetMaxMissionGage", RpcTarget.AllViaServer);
		
		yield return new WaitForSeconds(1);
		ShowPanel(GamePanel);
		ShowGameUI();
		StartCoroutine(UM.KillCo());
		//StartCoroutine(UM.EnergencyCo());
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		UM.GetComponent<PhotonView>().RPC("SetMaxMissionGage", RpcTarget.AllViaServer);
	}

	public int GetCrewCount() 
	{
		int crewCount = 0;
		for (int i = 0; i < Players.Count; i++)
			if (!Players[i].isImposter) ++crewCount;
		return crewCount;
	}


	//게임UI를 보여주는 함수  
	//모든 플레이어에게 똑같은 UI를 넣지만, 그대신
	//imposter일 경우 +추가로 UI의 몇개를 더 넣어줘야겠지??

	

	//서로에게 보여지는 UI의 종류. 
	void ShowGameUI() 
	{
		if (MyPlayer.isImposter)
		{
			UM.SetInteractionBtn0(5, true);
			UM.SetInteractionBtn0(0, true);
		   // UM.SetInteractionBtn1(4, false);
		   //UM.SetInteractionBtn2(6, false);
		}
        else
        {
            UM.SetInteractionBtn0(5, true);
            UM.SetInteractionBtn0(0, true);
          //  UM.SetInteractionBtn1(4, false);
            //UM.SetInteractionBtn2(7, false);
        }
	}

	[PunRPC]
	void ReportRPC(int actor, int targetDeadColorIndex) 
	{
		// actor가 리포트함
		ShowPanel(ReportPanel);
		UM.ReportDeadBodyImage.color = UM.colors[targetDeadColorIndex];
		//StartCoroutine(ShowVotePanelCo(actor));
	}

	//[PunRPC]
	//void EmergencyRPC(int actor)
	//{
	//	// actor가 긴급소집함
	//	ShowPanel(EmergencyPanel);
	//	StartCoroutine(ShowVotePanelCo(actor));
	//}

	//IEnumerator ShowVotePanelCo(int callActor) 
	//{
	//	yield return new WaitForSeconds(4);
	//	ShowPanel(VotePanel);
	//	foreach (GameObject DeadBody in GameObject.FindGameObjectsWithTag("DeadBody"))
	//		PhotonNetwork.Destroy(DeadBody);
	//}


	//[PunRPC]
	//void ShowGhostRPC()
	//{
	//	for (int i = 0; i < Players.Count; i++)
	//	{
	//		if (!MyPlayer.isDie) continue;

	//		if (Players[i].isDie)
	//		{
	//			Players[i].transform.GetChild(1).gameObject.SetActive(true);
	//			Players[i].transform.GetChild(2).gameObject.SetActive(true);
	//		}
	//	}
	//}


	public void WinCheck() 
	{
		int crewCount = 0;
		int impoCount = 0;

		for (int i = 0; i < Players.Count; i++)
		{
			var Player = Players[i];
			if (Players[i].isDie) continue;
			if (Player.isImposter)
				++impoCount;
			else
				++crewCount;
		}

		if (impoCount == 0 && crewCount > 0) // 모든 임포가 죽음
			Winner(true);
		else if(impoCount != 0 && impoCount > crewCount) // 임포가 크루보다 많음
			Winner(false);
	}

	public void Winner(bool isCrewWin) 
	{
		if (!isGameStart) return;

		if (isCrewWin) 
		{ 
			print("크루원 승리");
			ShowPanel(CrewWinPanel);
			Invoke("WinnerDelay", 3);
		}
		else 
		{ 
			print("임포스터 승리");
			ShowPanel(ImposterWinPanel);
			Invoke("WinnerDelay", 3);
		}
	}

	void WinnerDelay() 
	{
		Application.Quit();
	}
}