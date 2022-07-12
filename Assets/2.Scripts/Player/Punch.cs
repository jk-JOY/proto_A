using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class Punch : MonoBehaviourPunCallbacks
{
    public float attackDamage;
    public PhotonView PV;
    public GameObject playerHPCanvas;

    public float camShakeIntencity;
    public float camShakeTime;

    int dir;

    bool stopping;
    public float stopTime;
    public float slowTime;

    Vector2 camPosition_original;
    public float shake;

    private void Start()
    {
        Destroy(gameObject, 0.3f);
    }

    void Update() => transform.Translate(Vector3.right * (2f * Time.deltaTime * dir));

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!PV.IsMine && col.CompareTag("Player") && col.GetComponent<PhotonView>().IsMine) // 느린쪽 판정
        {
            CinemachineShake.Instance.ShakeCamera(camShakeIntencity, camShakeTime);
            // kkh : 
            PlayerScript player = col.GetComponent<PlayerScript>();
            player.HP_Cur -= attackDamage;

            col.GetComponentInChildren<HealthBar>().hp -= 10;
            
            Debug.Log("때렸다");


        }
    }

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;

    [PunRPC]
    void DestoryRPC() => Destroy(gameObject);
}