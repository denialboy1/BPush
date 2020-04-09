using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp : MonoBehaviour
{
    #region Public Fields

    public JoyStickTest joystick; // joystick script
    public float MoveSpeed; // player movespeed

    #endregion

    #region Private Fields

    private Vector3 _moveVector;
    private Transform _transform;
    private Vector3 _moveRotate;
    private GameObject Mesh;
    private Vector3 front;
    private RaycastHit RayHit;
    private Transform Ball;
    #endregion

    #region Private Methods

    void Start()
    {
        _transform = transform;
        _moveVector = Vector3.zero;
        _moveRotate = Vector3.zero;
        Mesh = transform.GetChild(0).gameObject;
        Ball = transform.GetChild(0).GetChild(0);
    }

    void Update()
    {
        HandleInput();
        front = transform.TransformDirection(Vector3.forward);
    }

    private void FixedUpdate()
    {
        Move();
        CheckBall();
    }

    #endregion

    #region Public Methods

    public void HandleInput()
    {
        _moveVector = PoolInput();
    }

    public Vector3 PoolInput()
    {
        float h = joystick.GetHorizontalValue();
        float v = joystick.GetVerticalValue();
        Vector3 moveDir = new Vector3(h, 0, v);

        return moveDir;
    }

    public Vector3 RotateInput()
    {
        float h = joystick.GetHorizontalValue();
        float v = joystick.GetVerticalValue();
        Vector3 RotateDir = new Vector3(h, 0, v);
        return RotateDir;
    }
   
    public void Move()
    {
        Quaternion Look;
        _transform.Translate(_moveVector * MoveSpeed * Time.deltaTime);
        Look = Quaternion.LookRotation(RotateInput());
        if(Look != _transform.rotation)
            Mesh.transform.rotation = Quaternion.RotateTowards(_transform.rotation, Look, 180.0f);
    }

    public void CheckBall()
    {
        if (Physics.Raycast(transform.position, front, out RayHit, 1.3f))
        {
            if (RayHit.transform.tag == "Ball")
            {
                RayHit.collider.gameObject.GetComponent<BallMove>().setParent(gameObject, Ball.transform);
                Ball.GetComponent<SphereCollider>().enabled = true;
            }
        }
    }
    #endregion
}
