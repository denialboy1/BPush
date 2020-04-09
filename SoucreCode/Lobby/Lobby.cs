using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Chat;
using Photon.Realtime;
using Photon.Pun;

namespace Photon.Pun.BPUSH
{
    public class Lobby : MonoBehaviourPunCallbacks, IChatClientListener
    {

        [SerializeField]
        private Text port;

        #region  Chat Variable
        public string[] ChannelsToJoinOnConnect; // set in inspector. Demo channels to join automatically.


        public int HistoryLengthToFetch; // set in inspector. Up to a certain degree, previously sent messages can be fetched for context

        public string UserName { get; set; }

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

        //int playerNumber = PhotonNetwork.PlayerList.Length;
        int id = 0;
        // Use this for initialization
        public GameObject[] characters;
        public Text[] texts;

        void Start()
        {
            
            PhotonNetwork.AutomaticallySyncScene = true;
            port.text = "Port : " + PlayerPrefs.GetString("port");

            ChannelsToJoinOnConnect[0] = PlayerPrefs.GetString("port");
            ///////////////////////////////////////////////////////////////////////


            this.UserName = PhotonNetwork.NickName;
            
#if PHOTON_UNITY_NETWORKING
            this.chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
#endif

            bool appIdPresent = !string.IsNullOrEmpty(this.chatAppSettings.AppIdChat);


            if (!appIdPresent)
            {
                Debug.LogError("You need to set the chat app ID in the PhotonServerSettings file in order to continue.");
            }
            this.Connect();
            ////////////////////////////////////////////////////////////////////////////////////////
            ///

            
            id = PhotonNetwork.PlayerList.Length;

            switch (PhotonNetwork.PlayerList.Length)
            {
                case 4:
                    texts[3].gameObject.SetActive(true);
                    texts[3].text = PhotonNetwork.NickName;
                    break;
                case 3:
                    texts[2].gameObject.SetActive(true);
                    texts[2].text = PhotonNetwork.NickName;
                    break;
                case 2:
                    texts[1].gameObject.SetActive(true);
                    texts[1].text = PhotonNetwork.NickName;
                    break;
                case 1:
                    texts[0].gameObject.SetActive(true);
                    texts[0].text = PhotonNetwork.NickName;
                    break;
            }
        }

        public void Connect()
        {

            this.chatClient = new ChatClient(this);
#if !UNITY_WEBGL
            this.chatClient.UseBackgroundWorkerForSending = true;
#endif

            this.chatClient.Connect(this.chatAppSettings.AppIdChat, "1.0", new Photon.Chat.AuthenticationValues(this.UserName));

            //여기서 
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

        public void Update()
        {
            if (this.chatClient != null)
            {
                this.chatClient.Service(); // make sure to call this regularly! it limits effort internally, so calling often is ok!
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

        public void OnStartGameButtonClicked()
        {
            PhotonNetwork.LoadLevel("Scene1");
        }
        public int TestLength = 2048;
        private byte[] testBytes = new byte[2048];

        private void SendChatMessage(string inputLine)
        {
            if (string.IsNullOrEmpty(inputLine))
            {
                return;
            }

            bool doingPrivateChat = this.chatClient.PrivateChannels.ContainsKey(this.selectedChannelName);
            string privateChatTarget = string.Empty;
            if (doingPrivateChat)
            {
                // the channel name for a private conversation is (on the client!!) always composed of both user's IDs: "this:remote"
                // so the remote ID is simple to figure out

                string[] splitNames = this.selectedChannelName.Split(new char[] { ':' });
                privateChatTarget = splitNames[1];
            }
            //UnityEngine.Debug.Log("selectedChannelName: " + selectedChannelName + " doingPrivateChat: " + doingPrivateChat + " privateChatTarget: " + privateChatTarget);



            if (doingPrivateChat)
            {
                this.chatClient.SendPrivateMessage(privateChatTarget, inputLine);
            }
            else
            {
                this.chatClient.PublishMessage(this.selectedChannelName, inputLine);
            }
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

        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            if (channelName.Equals(this.selectedChannelName))
            {
                // update text
                this.ShowChannel(this.selectedChannelName);
            }
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
                Debug.Log("ShowChannel failed to find channel: " + channelName);
                return;
            }

            this.selectedChannelName = channelName;
            this.CurrentChannelText.text = channel.ToStringMessages();
            Debug.Log("ShowChannel: " + this.selectedChannelName);

        }

        public void OpenDashboard()
        {
            Application.OpenURL("https://dashboard.photonengine.com");
        }

        public void Exit()
        {
            PhotonNetwork.LoadLevel("LoginScene");
        }
    }
}