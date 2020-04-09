using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    #region Public Fields
    public int ballNum = 6;
    public GameObject[] ball; // 공의 프리팹을 받는 배열
    public GameObject[] insBall; // 생성된 공 오브젝트 배열
    #endregion

    #region Private Fields
    private int size;

    #endregion

    #region Private Methods
    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(this.gameObject);
        ballNum = Random.Range(4, 8);
        size = (int)GameObject.Find("BackGround/Floor").transform.localScale.x / 2;
        insBall = new GameObject[ballNum];
        MakeBall();
    }

    void MakeBall(int method = -1)
    {
        if (method == -1)
        {
            for (int i = 0; i < ballNum; i++)
            {
                int num = Random.Range(0, ball.Length - 1);
                insBall[i] = Instantiate(ball[num]);
                insBall[i].transform.position = new Vector3(Random.Range(3f - size, size - 3f), 7f, Random.Range(3f - size, size - 3f));
            }
        }
        else
        {
            int num = Random.Range(0, ball.Length - 1);
            Destroy(insBall[method]);
            insBall[method] = Instantiate(ball[num]);
            insBall[method].transform.position = new Vector3(Random.Range(3f - size, size - 3f), 3f, Random.Range(3f - size, size - 3f));
        }
    }

    private void Update()
    {
        for(int i=0; i<ballNum; i++)
        {
            if(insBall[i].transform.position.y <= -5f)
            {
                MakeBall(i);
            }
        }
    }
    #endregion
}
