
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;


namespace Photon.Pun.BPUSH
{


    public class Login : MonoBehaviourPunCallbacks
    {

        #region private variable
        [SerializeField]
        private GameObject popup1;
        [SerializeField]
        private GameObject popup2;
        [SerializeField]
        private GameObject popup3;
        [SerializeField]
        private Text nickname;
        [SerializeField]
        private Text port;

        private int port_num;

        bool isCreate = false;
        string gameVersion = "1";
      
        #endregion

        #region private method
        void Awake()
        {
            popup1.SetActive(true);
            popup2.SetActive(false);
            popup3.SetActive(false);

            //LoadLevel을 자동으로 맞춰줌.
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        
        #endregion

        #region public method
        public void Login_Click()
        {
            if(nickname.text == string.Empty)  { return; }
            PhotonNetwork.NickName = nickname.text;
            popup1.SetActive(false);
            popup2.SetActive(true);
        }

        public void Back_Click()
        {
            popup1.SetActive(true);
            popup2.SetActive(false);
        }

        public void Close_Click()
        {
            popup3.SetActive(false);
        }

        public void Join_Click()
        {
            popup3.SetActive(true);
        }

        public void Connect_Click()
        {
            PlayerPrefs.SetString("port", port.text);
            PhotonNetwork.GameVersion = this.gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }

        public void Create_Click()
        {
            port_num = Random.Range(0, 1000);
            PlayerPrefs.SetString("port", port_num.ToString());
            PhotonNetwork.GameVersion = this.gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            //
            isCreate = true;
        }

        public override void OnConnectedToMaster()
        {
            if (isCreate)
            {
                PlayerPrefs.SetString("port", port_num.ToString());
                PhotonNetwork.CreateRoom(port_num.ToString(), new RoomOptions { MaxPlayers = 4 });
                //PhotonNetwork.JoinRoom(port_num.ToString());
            }
            else
            {
                PlayerPrefs.SetString("port", port.text);
                PhotonNetwork.JoinRoom(port.text);
            }
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            //client가 방에 들어가지 못하면 에러 출력
            port_num = Random.Range(0, 1000);
            PlayerPrefs.SetString("port", port_num.ToString());
            PhotonNetwork.GameVersion = this.gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.CreateRoom(port_num.ToString(), new RoomOptions { MaxPlayers = 4 });
            isCreate = true;

        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            isCreate = false;
        }

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                if(isCreate)
                    PlayerPrefs.SetString("port", port_num.ToString());
                else
                    PlayerPrefs.SetString("port", port.text);
                PhotonNetwork.LoadLevel("LobbyScene");
            }
        }
        #endregion
    }
}
