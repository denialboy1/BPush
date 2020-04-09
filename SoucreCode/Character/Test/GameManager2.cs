using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Photon.Pun.BPush
{
    public class GameManager2 : MonoBehaviourPunCallbacks
    {
        #region Public Fields
        public int ballNum = 6;
        public GameObject[] ball; // 공의 프리팹을 받는 배열
        public GameObject[] insBall; // 생성된 공 오브젝트 배열
        public GameObject[] PlayerPrefabs;
        public static GameManager2 instance;
        public GameObject ending;
        public bool testTrigger = false;
        #endregion

        #region Private Fields
        private int size;
        private int playerNum = 0;
        #endregion

        #region Private Methods
        // Use this for initialization

        private void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        private void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene("LoginScene");
                return;
            }
            if(PAmanager.instance == null)
            {
                makePlayer();
            }
            if (PlayerMove.LocalPlayerInstance == null)
            {
                //makePlayer();
            }
            if (PhotonNetwork.IsMasterClient)
            {
                ballNum = Random.Range(4, 8);
                size = (int)GameObject.Find("BackGround/Floor").transform.localScale.x / 2;
                insBall = new GameObject[ballNum];
                MakeBall();
            }
            DontDestroyOnLoad(this.gameObject);
        }

        private void MakeBall(int method = -1)
        {
            if (method == -1)
            {
                for (int i = 0; i < ballNum; i++)
                {
                    int num = Random.Range(0, ball.Length);
                    insBall[i] = PhotonNetwork.Instantiate(ball[num].name, ball[num].transform.position, Quaternion.identity, 0);
                    insBall[i].transform.position = new Vector3(Random.Range(3f - size, size - 3f), 7f, Random.Range(3f - size, size - 3f));
                }
            }
            else
            {
                int num = Random.Range(0, ball.Length);
                Destroy(insBall[method]);
                insBall[method] = PhotonNetwork.Instantiate(ball[num].name, ball[num].transform.position, Quaternion.identity, 0);
                insBall[method].transform.position = new Vector3(Random.Range(3f - size, size - 3f), 3f, Random.Range(3f - size, size - 3f));
            }
        }

        private void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                /*
                for (int i = 0; i < ballNum; i++)
                {
                    if (insBall[i])
                        if (insBall[i].transform.position.y <= -5f)
                        {
                            MakeBall(i);
                        }
                }*/
            }

            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                ending.SetActive(true);
            }
        }

        public void makePlayer()
        {
            Vector3 randomPos = new Vector3(Random.Range(3f - size, size - 3f), 3f, Random.Range(3f - size, size - 3f));
            int num = (int)PhotonNetwork.LocalPlayer.CustomProperties["character_num"] + 1;
            string name = "Player" + num.ToString();
            PhotonNetwork.Instantiate(name, randomPos + Vector3.up * 5f, Quaternion.identity, 0);
        }
        #endregion
    }
}