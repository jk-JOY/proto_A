﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using static NetworkManager;

public class UIManager : MonoBehaviourPun
{
    public static UIManager UM;
    void Awake() => UM = this;

    // 0 : use, 1 customize, 2 cancel, 3 start, 4 report, 5 kill, 6 sabotage, 7 null, 8 emergency
    public Sprite[] sprites;
    int curBtn0, curBtn1, curBtn2;
    bool active0, active1, active2;

    //이미지 인터렉션 버튼 0 use  
    public Image WaitingInteractionBtn0, InteractionBtn0, InteractionBtn1, InteractionBtn2;
    public Text Interaction0Text;

    public Image PreviewImage;
    public Color[] colors;
    public GameObject CustomizePanel, DiePanel;
    public GameObject[] ColorCancel;
    public Button[] ColorBtn;
    public Button StartBtn;
    public Transform LeftBottom, RightTop, LeftBottomMap, RightTopMap, PlayerMap;
    public GameObject[] MissionMaps;
    public Image KillerImage, DeadbodyImage;
    public Text LogText;
    public GameObject[] Minigames;
    public GameObject MissionClearText;
    public int curInteractionNum;
    public Slider MissionGageSlider;
   // public Transform[] VotePanels;
  //  public GameObject SabotagePanel;
    public Button[] DoorMaps;
    public Image ReportDeadBodyImage;
 //   public Toggle[] VoteToggles;
 //   public Toggle SkipVoteToggle, CancelVoteToggle;
 //   public GameObject SkipVoteResultGrid;
    //public Image KickPanelImage;
    //public Text KickPanelText;
    PhotonView PV;
    //public GameObject VoteResultImage;
    public InputField ChatInput;
    public Text ChatText;
    public Scrollbar ChatScroll;
    public RectTransform ChatContent;
    public GameObject[] ChatPanels;
    public int killCooltime; //emergencyCooltime;
    //public Text VoteTimerText;

    void Start()
    {
        PV = photonView;
    }

    public void SetInteractionBtn0(int index, bool _active)
    {
        curBtn0 = index;
        active0 = _active;

        // 대기실
        if (!NM.isGameStart)
        {
            WaitingInteractionBtn0.sprite = sprites[index];
            WaitingInteractionBtn0.GetComponent<Button>().interactable = active0;
        }
        else
        {
            InteractionBtn0.sprite = sprites[index];
            InteractionBtn0.GetComponent<Button>().interactable = active0;
        }
    }

    //전체 리포트 버튼 UI 및 버튼 사진 
    public void SetInteractionBtn1(int index, bool _active)
    {
        curBtn1 = index;
        active1 = _active;
        InteractionBtn1.sprite = sprites[index];
        InteractionBtn1.GetComponent<Button>().interactable = active1;
    }

    //임포스터에게만 보이는 사보타지 UI 및 버튼 사진
    public void SetInteractionBtn2(int index, bool _active)
    {
        curBtn2 = index;
        active2 = _active;
        InteractionBtn2.sprite = sprites[index];
        InteractionBtn2.GetComponent<Button>().interactable = active2;
    }

    public void ColorChange(int _colorIndex)
    {
        PreviewImage.color = colors[_colorIndex];
        NM.MyPlayer.GetComponent<PhotonView>().RPC("SetColor", RpcTarget.AllBuffered, _colorIndex);
    }

    public void ClickInteractionBtn0()
    {
        // 커스터마이즈
        if (curBtn0 == 1)
        {
            CustomizePanel.SetActive(true);
            SetIsCustomize(false);
            PreviewImage.color = colors[NM.MyPlayer.colorIndex];
        }

        // 킬   ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ 해당Kill은 UI에 달려있어서 그것을 모든 Player에서 할당되도록. 
        else if (curBtn0 == 5)
        {
            if (NM.MyPlayer.isDie) return;
            NM.MyPlayer.Kill();
        }
    }


    public void ClickInteractionBtn1()
    {
        // 사용
        if (curBtn0 == 0)
        {
            // 크루원 작업
            GameObject CurMinigame = Minigames[Random.Range(0, Minigames.Length)];
            CurMinigame.GetComponent<MinigameManager>().StartMission();
        }
    }

    public void SetIsCustomize(bool b)
    {
        NM.MyPlayer.isMove = b;
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) return;
        SetActiveColors();
        SetMap();
        if (!PhotonNetwork.IsMasterClient) return;
        ShowStartBtn();
    }

    void ShowStartBtn()
    {
        StartBtn.gameObject.SetActive(true);
        //StartBtn.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 7; // 기본값\

        //1보다 클경우 변경하는 ..?
        StartBtn.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 1; // 2
    }

    public void SetActiveColors()
    {
        List<int> colorList = new List<int>();
        for (int i = 0; i < NM.Players.Count; i++)
            colorList.Add(NM.Players[i].colorIndex);

        for (int i = 0; i < ColorCancel.Length; i++)
        {
            bool contain = colorList.Contains(i + 1);
            ColorCancel[i].SetActive(contain);
            ColorBtn[i].interactable = !contain;
        }
    }


    public void SetMap()
    {
        // 실제 맵
        float width = RightTop.position.x - LeftBottom.position.x;
        float height = RightTop.position.y - LeftBottom.position.y;

        Vector3 MyPlayerPos = NM.MyPlayer.transform.position;
        float playerWidth = MyPlayerPos.x - LeftBottom.position.x;
        float playerHeight = MyPlayerPos.y - LeftBottom.position.y;

        // 지도
        float widthMap = RightTopMap.position.x - LeftBottomMap.position.x;
        float heightMap = RightTopMap.position.y - LeftBottomMap.position.y;

        float playerMapX = LeftBottomMap.position.x + (playerWidth / width) * widthMap;
        float playerMapY = LeftBottomMap.position.y + (playerHeight / height) * heightMap;

        PlayerMap.position = new Vector3(playerMapX, playerMapY, 0);
    }



    //UI에서 죽여야함. 
    public IEnumerator KillCo()
    {
       // if (!NM.MyPlayer.isImposter) yield break;

        SetInteractionBtn0(5, false);
        NM.MyPlayer.isKillable = false;
        for (int i = 5; i > 0; i--) // 기본 15초 킬대기  //킬 후 20 초 대기. 
        //for (int i = 5; i > 0; i--)
        {
            killCooltime = i;

            if (UM.curBtn0 == 5) 
                Interaction0Text.text = killCooltime.ToString();
            else
                Interaction0Text.text = "";

            yield return new WaitForSeconds(1);
        }
        killCooltime = 0;
        Interaction0Text.text = "";

        NM.MyPlayer.isKillable = true;
    }

    //public IEnumerator EnergencyCo()  //이멀전시도 기본 20초.
    //{
    //    for (int i = 20; i > 0; i--)
    //    {
    //        emergencyCooltime = i;
    //        if (UM.curBtn0 == 8)
    //            Interaction0Text.text = emergencyCooltime.ToString();
    //        else
    //            Interaction0Text.text = "";
    //        yield return new WaitForSeconds(1);
    //    }
    //    emergencyCooltime = 0;
    //    Interaction0Text.text = "";
    //}


    public IEnumerator DieCo(int killerColorIndex, int deadBodyColorIndex)
    {
        DiePanel.SetActive(true);
        KillerImage.color = UM.colors[killerColorIndex];
        DeadbodyImage.color = UM.colors[deadBodyColorIndex];

        yield return new WaitForSeconds(4);
        DiePanel.SetActive(false);
    }

    public void ShowLog(string log)
    {
        LogText.text = log;
    }


    [PunRPC]
    public void SetMaxMissionGage()
    {
        MissionGageSlider.maxValue = NM.GetCrewCount();
    }

    [PunRPC]
    public void AddMissionGage()
    {
        MissionGageSlider.value += 0.25f;

        if (MissionGageSlider.value == MissionGageSlider.maxValue) 
        {
            // 크루원 승리
            NM.Winner(true);
        }
    }

    public IEnumerator MissionClearCo(GameObject MissionPanel) 
    {

        MissionPanel.SetActive(false);
        MissionClearText.SetActive(true);
        yield return new WaitForSeconds(2);
        MissionClearText.SetActive(false);
    }

    public void MissionClear(GameObject MissionPanel) 
    {
        StartCoroutine(MissionClearCo(MissionPanel));
        PV.RPC("AddMissionGage", RpcTarget.AllViaServer);
    }

    public void DoorMapClick(int doorIndex) 
    {
        PV.RPC("DoorMapClickRPC", RpcTarget.AllViaServer, doorIndex);
    }

    [PunRPC]
    void DoorMapClickRPC(int doorIndex) 
    {
        StartCoroutine(DoorCo(doorIndex));
        StartCoroutine(DoorCoolCo(doorIndex));
    }

    IEnumerator DoorCo(int doorIndex) 
    {
        NM.Doors[doorIndex].SetActive(true);
        yield return new WaitForSeconds(7);
        NM.Doors[doorIndex].SetActive(false);
    }

    IEnumerator DoorCoolCo(int doorIndex) 
    {
        if (!NM.MyPlayer.isImposter) yield break;

        DoorMaps[doorIndex].interactable = false;
        yield return new WaitForSeconds(18);
        DoorMaps[doorIndex].interactable = true;
    }
}
