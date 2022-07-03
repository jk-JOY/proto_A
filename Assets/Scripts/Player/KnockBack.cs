using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class KnockBack : MonoBehaviourPunCallbacks
{
    public float knockBackStrength;
    public PhotonView PV;
    //카메라
    public float camShakeIntencity;
    public float camShakeTime;

    int dir;

    bool stopping;
    public float stopTime;
    public float slowTime;

    Vector2 camPosition_original;
    public float shake;


    // private void Start() => Destroy(gameObject, 0.4f);
    private void Start()
    {
        Destroy(gameObject, 0.4f);
    }

    void Update() => transform.Translate(Vector3.right * 4f * Time.deltaTime * dir);

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!PV.IsMine && col.CompareTag("Player") && col.GetComponent<PhotonView>().IsMine)
        {
            Rigidbody2D RB = col.gameObject.GetComponent<Rigidbody2D>();
            Debug.Log("닿는다");

            if (RB != null)
            {
                // TimeStop();
                Debug.Log("타임스탑");
                CinemachineShake.Instance.ShakeCamera(camShakeIntencity, camShakeTime);
                col.GetComponent<PlayerScript>();
                //Vector2 input = col.transform.position - transform.position;
                //input.y = 0;
                //RB.AddForce(input.normalized * knockBackStrength, ForceMode2D.Impulse);
                ////PlayerScript.PS.TimeStop();
                //Debug.Log("OK");
                //StartCoroutine(CamAction());
            }
        }
    }

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;
}


//    private void Start()
//    {
//        hitAudio = GetComponent<AudioSource>();
//        hitEffect.gameObject.transform.position = hitEffectLocation.position;
//    }

//    [PunRPC]
//    private void OnTriggerEnter2D(Collider2D col)
//    {
//        if (col.gameObject.CompareTag("Player"))
//        {
//            hitEffect?.Play();
//            hitAudio?.PlayOneShot(hitSound);
//            Rigidbody2D player = col.gameObject.GetComponent<Rigidbody2D>();
//            Debug.Log("들어옴");

//            if (player != null)
//            {
//                Vector2 difference = player.transform.position - transform.position;
//                player.AddForce(difference.normalized * knockBackStrength, ForceMode2D.Impulse);
//                Debug.Log("나감");
//                difference = difference.normalized * 4;
//                player.AddForce(difference, ForceMode2D.Impulse);
//                // player.isKinematic = true;
//                StartCoroutine(KnockBackCo(player));
//            }
//            //넉백에서 날라가는 위치 자체도 동기화가 필요하다.
//            //   StartCoroutine(PlayerScript.PS.KnockBack(knockDuration, knockBackPower, this.transform));
//        }
//    }
