using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMove : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Public Fields

    public float MoveSpeed; // player movespeed
    public float curSpeed;
    public JoyStickTest joystick; // joystick script
    public Stamina stamina;// 플레이어 스테미너
    public GameObject UI;
    public Animator Animator;
    public Camera DeathCamera;
    public GameObject ParticleHit;
    public GameObject ParticleDeath;
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    #endregion

    #region Private Fields
    private Vector3 myPos;
    private Quaternion myRot;
    private Vector3 targetPos;
    private Quaternion targetRot;
    private  GameObject ui;
    //플레이어 이동방향
    private Vector3 _moveVector;

    //플레이어의 Transform
    private Transform _transform;

    //플레이어 회전 방향
    private Vector3 _moveRotate;

    //플레이어 메쉬
    private GameObject Mesh;

    //플레이어의 정면 방향
    public Vector3 front;

    //공을 잡을 레이캐스트 HIT정보 저장
    private RaycastHit RayHit;

    //공을 잡게 될 GameObject
    private GameObject BallCollider;

    public GameObject Ball;

    private bool waitGrab = true;
    private bool CanGrab = true;
    private bool isDeath = false;

    [SerializeField]
    private Camera MainCamera;

    #endregion

    #region Private Methods

    void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }
    }
    void Start()
    {
        curSpeed = MoveSpeed;
        _transform = transform;
        _moveVector = Vector3.zero;
        _moveRotate = Vector3.zero;
        Mesh = transform.GetChild(0).gameObject;
        BallCollider = transform.GetChild(0).GetChild(0).gameObject;
        Animator = transform.GetChild(0).GetComponent<Animator>();
        if (UI != null)
        {
            ui = Instantiate(UI);
            ui.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            joystick = ui.transform.GetChild(0).GetComponent<JoyStickTest>();
            stamina = ui.transform.GetChild(1).GetComponent<Stamina>();
        }
        DeathCamera = Instantiate(DeathCamera);
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (!isDeath)
            {
                _moveVector = PoolInput();
                front = transform.GetChild(0).gameObject.transform.TransformDirection(Vector3.forward);
            }
        }

    }

    private void FixedUpdate()
    {
        if (!isDeath)
        {
            Move();
            if (CanGrab)
                Grab();
            else if (stamina.GetStamina() <= 0f)
            {
                UnGrab();
                stamina.SetStamina(0f);
            }
        }
    }
    #endregion

    #region Public Methods
    // 조이스틱 인풋 ->
    public Vector3 PoolInput()
    {
        float h = joystick.GetHorizontalValue();
        float v = joystick.GetVerticalValue();
        Vector3 moveDir = new Vector3(h, 0, v);
        if (moveDir.magnitude == 0)
            Animator.SetBool("isRun", false);
        else
        {
            Animator.SetBool("isRun", true);
        }
        return moveDir;
    }

    public Vector3 RotateInput()     // <- 조이스틱 인풋
    {
        float h = joystick.GetHorizontalValue();
        float v = joystick.GetVerticalValue();
        Vector3 RotateDir = new Vector3(h, 0, v);
        return RotateDir;
    }
    // 전진 및 회전
    public void Move()
    {
        Vector3 input;
        _moveVector = PoolInput();
        _transform.Translate(_moveVector * curSpeed * Time.deltaTime);
        input = RotateInput();
        if (input != Vector3.zero)
            Mesh.transform.rotation = Quaternion.RotateTowards(_transform.rotation, Quaternion.LookRotation(input), 180.0f);
    }
    // 공 잡기
    public void Grab()
    {
        if(!photonView.IsMine)
        {
            return;
        }
        if (Physics.Raycast(transform.position, front, out RayHit, 1.3f))
        {
            if (RayHit.transform.tag == "Ball")
            {
                RayHit.collider.gameObject.GetComponent<BallMove>().setParent(gameObject, BallCollider.transform);
                BallCollider.GetComponent<SphereCollider>().radius = RayHit.collider.GetComponent<SphereCollider>().radius;
                BallCollider.GetComponent<SphereCollider>().enabled = true;
                Ball = RayHit.collider.gameObject;
                CanGrab = false;
                stamina.isDecrease = true;
                stamina.SetStamina(150f);
                curSpeed -= Ball.GetComponent<Rigidbody>().mass * 1.5f;
            }
        }
    }
    // 죽음
    public void Death()
    {
        if (Ball)
            UnGrab();
        Debug.Log("으악");
        PhotonNetwork.Instantiate(ParticleDeath.name, gameObject.transform.position, Quaternion.identity, 0);
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        Animator.SetTrigger("Death");
        Destroy(ui);
    }
    // 공 놓기
    public void UnGrab()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (Ball)
        {
            curSpeed = MoveSpeed;
            Debug.Log("Player's UnGrab()");
            BallCollider.GetComponent<SphereCollider>().enabled = false;
            Ball.GetComponent<BallMove>().unsetParent();
            stamina.isDecrease = false;
            Ball = null;
            if (waitGrab)
                StartCoroutine("GrabCool");
        }
    }
    // 공 던지기
    public void push()
    {
        if (!CanGrab && Ball)
        {
            Debug.Log("Player's push()");
            Animator.SetTrigger("Push");
            Ball.GetComponent<Rigidbody>().AddForce(front * 5f + Vector3.up, ForceMode.Impulse);
            Ball.GetComponent<BallMove>().isThrowing = true;
            UnGrab();
        }
    }
    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /* 
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isDeath);
        }
        else
        {
            targetPos = (Vector3)stream.ReceiveNext();
            targetRot = (Quaternion)stream.ReceiveNext();
            this.isDeath = (bool)stream.ReceiveNext();
        }
        */
    }
    #endregion
    #region IEnumerator
    IEnumerator GrabCool()
    {
        waitGrab = false;
        CanGrab = false;
        yield return new WaitForSeconds(1.5f);
        waitGrab = true;
        CanGrab = true;
        Debug.Log("Wait finish");
    }
    #endregion
}
