using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.BPush
{
    public class PAmanager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public GameObject joystick;
        private JoyStickTest joyControll;
        public static GameObject instance;
        public GameObject BallPoint;
        public Camera Pcamera;
        private RaycastHit RayHit;
        private Vector3 front;
        public GameObject Ball;
        Animator animator;

        public bool isGrab = false;

        // Use this for initialization
        void Start()
        {
            animator = GetComponent<Animator>();
            GameObject.Find("BackGroundImage");
            joyControll = GameObject.Find("BackGroundImage").GetComponent<JoyStickTest>();
                
        }
        void Awake()
        {
            if (photonView.IsMine)
            {
                CameraWork camerawork = gameObject.GetComponent<CameraWork>();
                instance = gameObject;
                camerawork.OnStartFollowing();
                BallPoint = transform.GetChild(0).gameObject;
            }
            
            DontDestroyOnLoad(gameObject);
            
        }
        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                float h = joyControll.GetHorizontalValue();
                float v = joyControll.GetVerticalValue();
                if (h == 0 && v == 0)
                    animator.SetBool("isRun", false);
                else
                    animator.SetBool("isRun", true);
                instance.transform.localPosition += Vector3.forward * v * 5.0f * Time.deltaTime;
                instance.transform.localPosition += Vector3.right * h * 5.0f * Time.deltaTime;
                Vector3 vec = new Vector3(h, 0, v);
                front = instance.transform.TransformDirection(Vector3.forward);
                if (vec != Vector3.zero)
                    instance.transform.rotation = Quaternion.RotateTowards(instance.transform.rotation, Quaternion.LookRotation(vec), 180.0f);
                if (Input.GetButtonDown("Fire1"))
                    photonView.RPC("Death", RpcTarget.All);
                photonView.RPC("Grab", RpcTarget.All);
            }
        }


        [PunRPC]
        public void Grab()
        {
            if(photonView.IsMine)
            {
                if(Physics.Raycast(instance.transform.position, front, out RayHit, 1.3f))
                {
                    if(RayHit.transform.tag == "Ball" && !isGrab)
                    {
                        StartCoroutine("Cool");
                        Ball = RayHit.collider.gameObject;
                        BallPoint.GetComponent<SphereCollider>().radius = Ball.GetComponent<SphereCollider>().radius * Ball.transform.localScale.x;
                        BallPoint.GetComponent<SphereCollider>().isTrigger = false;
                        Ball.GetComponent<BallMove>().setParent(gameObject, BallPoint.transform);
                        isGrab = true;

                    }
                }
            }
        }

        [PunRPC]
        public void UnGrab()
        {
            if(photonView.IsMine)
            {
                if(isGrab)
                {
                    BallPoint.GetComponent<SphereCollider>().isTrigger = true;
                    isGrab = false;
                    Ball.GetComponent<BallMove>().unsetParent();
                }
            }
        }

        [PunRPC]
        public void Death()
        {
            if(photonView.IsMine)
            {
                gameObject.GetComponent<CharacterController>().enabled = false;
                animator.SetTrigger("Death");
                photonView.RPC("UnGrab", RpcTarget.All);
                PhotonNetwork.LeaveRoom();
            }
        }


        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
            }
            else
            {
                // Network player, receive data
            }
        }

        [PunRPC]
        public void OnTriggerEnter(Collider other)
        {
            if(photonView.IsMine)
            {
                if(other.tag == "Ball_2")
                {
                    if(other.GetComponent<BallMove>().Parent != gameObject)
                    photonView.RPC("Death", RpcTarget.All);
                }
            }
        }

        [PunRPC]
        public void OnCollisionEnter(Collision collision)
        {
            if (photonView.IsMine)
            {
                if (collision.gameObject.tag == "Ball_2")
                {
                    if (collision.gameObject.GetComponent<BallMove>().Parent != gameObject)
                        photonView.RPC("Death", RpcTarget.All);
                }
            }
        }
        #endregion


        IEnumerator Cool()
        {
            if (!isGrab)
            {
                yield return new WaitForSeconds(7.0f);
                photonView.RPC("UnGrab", RpcTarget.All);
            }
        }
    }
}