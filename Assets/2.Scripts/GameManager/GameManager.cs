using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public partial class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                m_instance = FindObjectOfType<GameManager>();
            }

            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    private static GameManager m_instance; // 싱글톤이 할당될 static 변수

    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
        }
    }


    //주기적으로 자동 실행될 메서드 -현재 유물 진행도 여부
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {


    }


    private AudioSource audioSource;

    public bool isGameEnd
    {
        get;
        private set;
    }
    public bool isGameStart
    {
        get;
        private set;
    }

    // 게임 오버 처리
    public void EndGame()
    {
        // 게임 오버 상태를 참으로 변경
        isGameEnd = true;
        // 게임 오버 UI를 활성화
        //UIManager.instance.SetActiveGameoverUI(true);
    }



    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        //씬 매니저 로비씬. 
    }
}
