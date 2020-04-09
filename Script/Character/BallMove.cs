using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallMove : MonoBehaviourPunCallbacks{

    #region Public Fields

    #endregion

    #region Private Fileds
    private Rigidbody BallRigid;
    private bool isSet = false;
    public  GameObject Parent;
    private Transform BallTransform;
    public Rigidbody rigid;
    public SphereCollider coll;
    public bool isThrowing = false;
    private Vector3 targetPos;
    private Quaternion targetRot;
    #endregion

    // Use this for initialization
    void Start () {
        rigid = gameObject.GetComponent<Rigidbody>();
        coll = gameObject.GetComponent<SphereCollider>();
	}

    // Update is called once per frame
    void Update()
    {
        BallRigid = gameObject.GetComponent<Rigidbody>();
        if (isSet)
        {
            transform.localPosition = BallTransform.transform.position;

            rigid.velocity = Vector3.zero;
        }
        if (isThrowing && gameObject.GetComponent<Rigidbody>().velocity.magnitude <= 0.2f)
        {
            isThrowing = false;
            Parent = null;
            if (GetComponent<PhotonView>().OwnerActorNr != 0)
                GetComponent<PhotonView>().TransferOwnership(0);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isSet)
        {
            if (other.tag == "Player")
            {
                if (Parent != other.gameObject)
                {
                    other.gameObject.GetComponent<PlayerMove>().Death();
                }
            }
            else if(other.tag == "Ball_2")
            {
                hit();
                other.GetComponent<BallMove>().hit();
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isThrowing)
        {
            if (other.collider.tag == "Player")
            {
                if (Parent != other.gameObject)
                {
                    other.gameObject.GetComponent<PlayerMove>().Death();
                }
            }
            else if (other.collider.tag == "Ball_2")
            {
                hit();
                other.collider.GetComponent<BallMove>().hit();
            }
        }
    }

    #region Public Func
    public void setParent(GameObject Parent, Transform BallTransform)
    {
        this.BallTransform = BallTransform;
        this.Parent = Parent;
        isSet = true;
        gameObject.GetComponent<SphereCollider>().isTrigger = true;
        gameObject.tag = "Ball_2";
        transform.rotation = Quaternion.Euler(BallTransform.rotation.eulerAngles);
    }

    public void unsetParent()
    {
            Debug.Log("Ball's unsetParent()");
            isSet = false;
            if(!isThrowing)
                Parent = null;
            BallTransform = null;
            transform.SetParent(null);
            gameObject.GetComponent<SphereCollider>().isTrigger = false;
            gameObject.tag = "Ball";
    }

    public void hit()
    {
        Parent.GetComponent<PlayerMove>().Animator.SetTrigger("Hit");
        PhotonNetwork.Instantiate(Parent.GetComponent<PlayerMove>().ParticleHit.name, Parent.transform.position, Quaternion.identity, 0);
        Parent.GetComponent<PlayerMove>().UnGrab();
        Vector3 vec = new Vector3(Random.Range(-10, 10), Random.Range(0, 20), Random.Range(-10, 5));
        rigid.AddForce(vec, ForceMode.Impulse);
    }
    #endregion
    #region Hit Check
    #endregion
}
