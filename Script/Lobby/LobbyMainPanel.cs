using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Chat;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.BPush
{
    public class LobbyMainPanel : MonoBehaviourPunCallbacks, IChatClientListener
    {
        [Header("Login Panel")]
        public GameObject LoginPanel;

        public InputField PlayerNameInput;

        [Header("Selection Panel")]
        public GameObject SelectionPanel;

        [Header("Join Random Room Panel")]
        public GameObject JoinRandomRoomPanel;

        [Header("Room List Panel")]
        public GameObject RoomListPanel;



        public GameObject RoomListContent;
        public GameObject RoomListEntryPrefab;

        [Header("Inside Room Panel")]
        public Text roomtext;
        public GameObject InsideRoomPanel;
        public Button ReadyGameButton;
        public Button StartGameButton;
        public GameObject PlayerListEntryPrefab;
        public GameObject user;
        public GameObject[] otherUser;




        [Header("Character List")]
        public GameObject[] characterlist;
        int character_num = 0;

        private Dictionary<string, RoomInfo> cachedRoomList;
        private Dictionary<string, GameObject> roomListEntries;


        #region UNITY

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            cachedRoomList = new Dictionary<string, RoomInfo>();
            roomListEntries = new Dictionary<string, GameObject>();

            PlayerNameInput.text = "Player " + Random.Range(1000, 10000);
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnConnectedToMaster()
        {
            this.SetActivePanel(SelectionPanel.name);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ClearRoomListView();

            UpdateCachedRoomList(roomList);
            UpdateRoomListView();
        }

        public override void OnLeftLobby()
        {
            cachedRoomList.Clear();

            ClearRoomListView();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            string roomName = "Room " + Random.Range(1000, 10000);

            RoomOptions options = new RoomOptions { MaxPlayers = 4 };
            options.CustomRoomProperties = new Hashtable() { { "character1", false }, { "character2", false }, { "character3", false }, { "character4", false } };
            PhotonNetwork.CreateRoom(roomName, options, null);
        }
        int value = 0;
        public override void OnJoinedRoom()
        {
            roomtext.text = "Room : " + PhotonNetwork.CurrentRoom.Name;
            ChannelsToJoinOnConnect[0] = roomtext.text;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "ready", false } });

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                Debug.Log(p.NickName);
            }

            if (!((bool)PhotonNetwork.CurrentRoom.CustomProperties["character1"]))
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "character1", true } });
                character_num = 0;
            }
            else if (!((bool)PhotonNetwork.CurrentRoom.CustomProperties["character2"]))
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "character2", true } });
                character_num = 1;
            }
            else if (!((bool)PhotonNetwork.CurrentRoom.CustomProperties["character3"]))
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "character3", true } });
                character_num = 2;
            }
            else if (!((bool)PhotonNetwork.CurrentRoom.CustomProperties["character4"]))
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "character4", true } });
                character_num = 3;
            }
            value = character_num;
            PlayerPrefs.SetInt("character", character_num);
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "character_num", character_num } });
            int character_index = 0, otherUser_index = 0;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                //자기자신
                if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {

                    Vector3 vRotate, vScale;
                    vRotate = new Vector3(0, 180, 0);
                    vScale = new Vector3(300, 300, 300);
                    GameObject temp = null;
                    character_index = int.Parse(p.CustomProperties["Character"].ToString());
                    temp = Instantiate(characterlist[character_index]);
                    if (temp != null)
                    {
                        temp.transform.SetParent(user.transform);
                        temp.transform.Rotate(vRotate);
                        temp.transform.localScale = vScale;
                        temp.transform.localPosition = Vector3.zero;
                        user.SetActive(true);
                        user.GetComponentInChildren<Text>().text = p.NickName;
                    }
                }
                else
                {
                    //다른유저를 만들자.
                    if (p.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                    {

                        
                        Vector3 vRotate, vScale;
                        vRotate = new Vector3(0, 180, 0);
                        vScale = new Vector3(200, 200, 200);
                        GameObject temp = null;
                        character_index = int.Parse(p.CustomProperties["Character"].ToString());
                        temp = Instantiate(characterlist[character_index]);
                        if (temp != null)
                        {
                            temp.transform.SetParent(otherUser[otherUser_index].transform);
                            temp.transform.Rotate(vRotate);
                            temp.transform.localScale = vScale;
                            temp.transform.localPosition = Vector3.zero;
                            otherUser[otherUser_index].SetActive(true);
                            otherUser[otherUser_index].GetComponentInChildren<Text>().text = p.NickName;
                        }
                        otherUser_index++;
                    }
                }
            }

            SetActivePanel(InsideRoomPanel.name);


            StartGameButton.gameObject.SetActive(CheckPlayersReady());
            ReadyGameButton.gameObject.SetActive(!CheckPlayersReady());

            ///////////채팅 구현//////////////

#if PHOTON_UNITY_NETWORKING
            this.chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
#endif

            bool appIdPresent = !string.IsNullOrEmpty(this.chatAppSettings.AppIdChat);


            if (!appIdPresent)
            {
                Debug.LogError("You need to set the chat app ID in the PhotonServerSettings file in order to continue.");
            }
            this.Connect();
        }

        public override void OnLeftRoom()
        {
            SetActivePanel(SelectionPanel.name);
            /*
            switch (character_num)
            {
                case 0:
                    PhotonNetwork.CurrentRoom.CustomProperties["character1"] = false;
                    break;
                case 1:
                    PhotonNetwork.CurrentRoom.CustomProperties["character2"] = false;
                    break;
                case 2:
                    PhotonNetwork.CurrentRoom.CustomProperties["character3"] = false;
                    break;
                case 3:
                    PhotonNetwork.CurrentRoom.CustomProperties["character4"] = false;
                    break;
            }*/
        }


        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(InsideRoomPanel.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

            value++;

            int index = PhotonNetwork.CurrentRoom.PlayerCount - 2;
            Vector3 v1, v2;
            v1 = new Vector3(0, 180, 0);
            v2 = new Vector3(200, 200, 200);
            GameObject temp = null;
            temp = Instantiate(characterlist[value]);
            if (temp != null)
            {
                temp.transform.SetParent(otherUser[index].transform);
                temp.transform.Rotate(v1);
                temp.transform.localScale = v2;
                temp.transform.localPosition = Vector3.zero;
                otherUser[index].SetActive(true);
                otherUser[index].GetComponentInChildren<Text>().text = newPlayer.NickName;
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
            ReadyGameButton.gameObject.SetActive(!CheckPlayersReady());
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
            ReadyGameButton.gameObject.SetActive(!CheckPlayersReady());
        }

        public void OnClickReady()
        {

            if (!((bool)PhotonNetwork.LocalPlayer.CustomProperties["ready"]))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "ready", true } });

            }
            else
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "ready", false } });
            }

        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {

            if (CheckPlayersReady())
            {
                StartGameButton.gameObject.SetActive(true);
                ReadyGameButton.gameObject.SetActive(false);
            }
            else
            {
                StartGameButton.gameObject.SetActive(false);
                ReadyGameButton.gameObject.SetActive(true);
            }

        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
            ReadyGameButton.gameObject.SetActive(!CheckPlayersReady());
        }

        #endregion

        #region UI CALLBACKS

        public void OnBackButtonClicked()
        {
            SetActivePanel(SelectionPanel.name);
        }

        public void OnCreateRoomButtonClicked()
        {
            string roomName = Random.Range(0, 1000).ToString();

            RoomOptions options = new RoomOptions { MaxPlayers = 4 };
            options.CustomRoomProperties = new Hashtable() { { "character1", false },
                                                             { "character2", false },
                                                             { "character3", false },
                                                             { "character4", false } };
            PhotonNetwork.CreateRoom(roomName, options, null);
        }



        public void OnJoinRandomRoomButtonClicked()
        {
            SetActivePanel(JoinRandomRoomPanel.name);

            PhotonNetwork.JoinRandomRoom();
        }

        public void OnLeaveGameButtonClicked()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void OnLoginButtonClicked()
        {
            string playerName = PlayerNameInput.text;

            if (!playerName.Equals(""))
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();

            }
            else
            {
                Debug.LogError("Player Name is invalid.");
            }
        }

        public void OnRoomListButtonClicked()
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            SetActivePanel(RoomListPanel.name);

        }

        public void OnStartGameButtonClicked()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            PhotonNetwork.LoadLevel("Test_Scene");
        }

        #endregion

        private bool CheckPlayersReady()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object isPlayerReady;
                if (p.CustomProperties.TryGetValue("ready", out isPlayerReady))
                {

                    if (!(bool)isPlayerReady)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void ClearRoomListView()
        {
            foreach (GameObject entry in roomListEntries.Values)
            {
                Destroy(entry.gameObject);
            }

            roomListEntries.Clear();
        }

        public void LocalPlayerPropertiesUpdated()
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
            ReadyGameButton.gameObject.SetActive(!CheckPlayersReady());
        }

        public void SetActivePanel(string activePanel)
        {
            LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
            SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));

            JoinRandomRoomPanel.SetActive(activePanel.Equals(JoinRandomRoomPanel.name));
            RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name));    // UI should call OnRoomListButtonClicked() to activate this
            InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
        }

        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }

        private void UpdateRoomListView()
        {
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                GameObject entry = Instantiate(RoomListEntryPrefab);
                entry.transform.SetParent(RoomListContent.transform);
                entry.transform.localScale = Vector3.one;
                Vector3 v = entry.transform.localPosition;
                v.z = 0;
                entry.transform.localPosition = v;
                entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

                roomListEntries.Add(info.Name, entry);
            }
        }

        public void OnSelectionToLoginBackButtonClick()
        {
            SelectionPanel.SetActive(false);
            LoginPanel.SetActive(true);
            PhotonNetwork.Disconnect();
        }
        public void OnSelectionToRoomListBackButtonClick()
        {
            RoomListPanel.SetActive(false);
            SelectionPanel.SetActive(true);
        }


        ///////////////////////////////////////////////////////////////
        #region  Chat Variable
        [Header("Chatting")]
        public string[] ChannelsToJoinOnConnect; // set in inspector. Demo channels to join automatically.


        public int HistoryLengthToFetch; // set in inspector. Up to a certain degree, previously sent messages can be fetched for context

        private string selectedChannelName; // mainly used for GUI/input

        public ChatClient chatClient;

#if !PHOTON_UNITY_NETWORKING
    [SerializeField]
#endif
        protected internal AppSettings chatAppSettings;


        public InputField InputFieldChat;   // set in inspector
        public Text CurrentChannelText;     // set in inspector

        public bool ShowState = true;
        #endregion

        public void Chat_Connect()
        {
            this.chatClient = new ChatClient(this);
            this.chatClient.Connect(this.chatAppSettings.AppIdChat, "1.0", new Photon.Chat.AuthenticationValues(PhotonNetwork.LocalPlayer.NickName));
        }

        /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnDestroy.</summary>
        public void OnDestroy()
        {
            if (this.chatClient != null)
            {
                this.chatClient.Disconnect();
            }
        }

        /// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
        public void OnApplicationQuit()
        {
            if (this.chatClient != null)
            {
                this.chatClient.Disconnect();
            }
        }

        public GameObject userReady;
        public GameObject[] otherReady;
        public GameObject userStar;
        public GameObject[] otherStar;
        public void Update()
        {
            if (this.chatClient != null)
            {
                this.chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!
            }

            int i = 0;
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    if ((bool)p.CustomProperties["ready"])
                    {
                        userReady.SetActive(true);
                    }
                    else
                    {
                        userReady.SetActive(false);
                    }
                }
                else
                {
                    if ((bool)p.CustomProperties["ready"])
                    {
                        otherReady[i].SetActive(true);
                    }
                    else
                    {
                        otherReady[i].SetActive(false);
                    }
                    
                }

                if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    if (p.IsMasterClient)
                    {
                        userStar.SetActive(true);
                    }
                    else
                    {
                        userStar.SetActive(false);
                    }
                }
                else
                {
                    if (p.IsMasterClient)
                    {
                        otherStar[i].SetActive(true);
                    }
                    else
                    {
                        otherStar[i].SetActive(false);
                    }
                    i++;
                }
            }
        }


        public void OnEnterSend()
        {
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
            {
                this.SendChatMessage(this.InputFieldChat.text);
                this.InputFieldChat.text = "";
            }
        }

        public void OnClickSend()
        {
            if (this.InputFieldChat != null)
            {
                this.SendChatMessage(this.InputFieldChat.text);
                this.InputFieldChat.text = "";
            }
        }

        private void SendChatMessage(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine))
            {
                return;
            }

            string privateChatTarget = string.Empty;
        
            this.chatClient.PublishMessage(this.selectedChannelName, inputLine);
        }

        public void PostHelpToCurrentChannel()
        {
        }

        public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
        {
            if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
            {
                Debug.LogError(message);
            }
            else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        public void OnConnected()
        {
            if (this.ChannelsToJoinOnConnect != null && this.ChannelsToJoinOnConnect.Length > 0)
            {
                this.chatClient.Subscribe(this.ChannelsToJoinOnConnect, this.HistoryLengthToFetch);
            }

            this.chatClient.SetOnlineStatus(ChatUserStatus.Online); // You can set your online state (without a mesage).
        }

        public void OnDisconnected()
        {
        }

        public void OnChatStateChange(ChatState state)
        {
            // use OnConnected() and OnDisconnected()
            // this method might become more useful in the future, when more complex states are being used.

        }

        public void OnSubscribed(string[] channels, bool[] results)
        {
            // in this demo, we simply send a message into each channel. This is NOT a must have!
            foreach (string channel in channels)
            {
                this.chatClient.PublishMessage(channel, "님이 입장하였습니다."); // you don't HAVE to send a msg on join but you could.

            }

            Debug.Log("OnSubscribed: " + string.Join(", ", channels));


            // Switch to the first newly created channel
            this.ShowChannel(channels[0]);
        }

        private void InstantiateChannelButton(string channelName)
        {
        }

        private void InstantiateFriendButton(string friendId)
        {
        }


        public void OnUnsubscribed(string[] channels)
        {
        }

        
        public void OnPrivateMessage(string sender, object message, string channelName)
        {
            // as the ChatClient is buffering the messages for you, this GUI doesn't need to do anything here
            // you also get messages that you sent yourself. in that case, the channelName is determinded by the target of your msg
            this.InstantiateChannelButton(channelName);

            byte[] msgBytes = message as byte[];
            if (msgBytes != null)
            {
                Debug.Log("Message with byte[].Length: " + msgBytes.Length);
            }
            if (this.selectedChannelName.Equals(channelName))
            {
                this.ShowChannel(channelName);
            }
        }

        /// <summary>
        /// New status of another user (you get updates for users set in your friends list).
        /// </summary>
        /// <param name="user">Name of the user.</param>
        /// <param name="status">New status of that user.</param>
        /// <param name="gotMessage">True if the status contains a message you should cache locally. False: This status update does not include a
        /// message (keep any you have).</param>
        /// <param name="message">Message that user set.</param>
        public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
        {

            Debug.LogWarning("status: " + string.Format("{0} is {1}. Msg:{2}", user, status, message));

        }

        public void AddMessageToSelectedChannel(string msg)
        {
            ChatChannel channel = null;
            bool found = this.chatClient.TryGetChannel(this.selectedChannelName, out channel);
            if (!found)
            {
                Debug.Log("AddMessageToSelectedChannel failed to find channel: " + this.selectedChannelName);
                return;
            }

            if (channel != null)
            {
                channel.Add("Bot", msg, 0); //TODO: how to use msgID?
            }

        }

        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            //채팅 채널이 같으면
            if (channelName.Equals(this.selectedChannelName))
            {
                //텍스트 업데이트
                this.ShowChannel(this.selectedChannelName);
            }
        }

        public void ShowChannel(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
            {
                return;
            }

            ChatChannel channel = null;
            bool found = this.chatClient.TryGetChannel(channelName, out channel);
            if (!found)
            {
                Debug.Log("현재 채팅채널 찾을수 없음 : " + channelName);
                return;
            }

            this.selectedChannelName = channelName;
            this.CurrentChannelText.text = channel.ToStringMessages();
            Debug.Log("채팅채널 : " + this.selectedChannelName);

        }

        public void OpenDashboard()
        {
            Application.OpenURL("https://dashboard.photonengine.com");
        }
    }
}