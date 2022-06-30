using Grpc.Core;
using SimpleChatApp.GrpcService;
using SimpleChatApp.CommonTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static SimpleChatApp.GrpcService.ChatService;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using System.Collections.ObjectModel;

namespace NewWhats
{
    public partial class MainPage : ContentPage
    {
        private ChatServiceClient chatServiceClient;
        private SimpleChatApp.GrpcService.Guid guid;
        private ObservableCollection<MetaDataMessage> chat = new ObservableCollection<MetaDataMessage>();
        private static Dictionary<string, Color> nickNameColor = new Dictionary<string, Color>();
        public MainPage(SimpleChatApp.GrpcService.Guid authguid)
        {
            InitializeComponent();
            chatServiceClient = new ChatServiceClient(new Channel("localhost: 30051", ChannelCredentials.Insecure));
            MessagesList.ItemsSource = chat;
            guid = authguid;
            LoadingChat(guid);
        }
        private class MetaDataMessage
        {
            public string UserLogin
            {
                set;
                get;
            }
            public string Text
            {
                set;
                get;
            }
            public Color Color
            {
                set;
                get;
            }
        }

        private async void LoadingChat(SimpleChatApp.GrpcService.Guid guid)
        {
            var messages = await GetMessages();
            foreach (var message in messages)
            {
                AddMessage(message);
            }
            await Subscribe(AddMessage);
        }

        private async Task<List<SimpleChatApp.GrpcService.MessageData>> GetMessages()
        {
            var now = DateTime.MaxValue;
            var then = DateTime.MinValue;

            var timeIntervalRequest = new TimeIntervalRequest()
            {
                StartTime = Timestamp.FromDateTime(then.ToUniversalTime()),
                EndTime = Timestamp.FromDateTime(now.ToUniversalTime()),
                Sid = guid
            };
            var messages = await chatServiceClient.GetLogsAsync(timeIntervalRequest);
            return messages.Logs.ToList();
        }

        private async Task Subscribe(Action<SimpleChatApp.GrpcService.MessageData> onMessage)
        {
            var streamingCall = chatServiceClient.Subscribe(guid);
            while (await streamingCall.ResponseStream.MoveNext())
            {
                var messages = streamingCall.ResponseStream.Current;
                foreach (var message in messages.Logs)
                {
                    onMessage(message);
                }
            }
        }


        private async Task SendMessage(string message)
        {
            var outgoingMessage = new OutgoingMessage()
            {
                Sid = guid,
                Text = message
            };
            var ans = await chatServiceClient.WriteAsync(outgoingMessage);;
        }


        private async void LogOut_Pressed(object sender, EventArgs e)
        {
            MessageEntry.Text = "";
            await Navigation.PopModalAsync();
        }

        private async void Send_Pressed(object sender, EventArgs e)
        {
            if (MessageEntry.Text != null && MessageEntry.Text != "")
            {
                await SendMessage(MessageEntry.Text);
                MessageEntry.Text = "";
            }
        }

        private void AddMessage(SimpleChatApp.GrpcService.MessageData md)
        {
            MetaDataMessage messageInfo = new MetaDataMessage();

            if (!nickNameColor.Keys.Contains(md.PlayerLogin))
            {
                nickNameColor.Add(md.PlayerLogin, GenerateColor());
            }

            messageInfo.UserLogin = md.PlayerLogin;
            messageInfo.Text = md.Text;
            messageInfo.Color = nickNameColor[md.PlayerLogin];

            chat.Add(messageInfo);
            MessagesList.ScrollTo(messageInfo, ScrollToPosition.End, true);
        }

        private Color GenerateColor()
        {
            Random r = new Random();
            Color newColor = Color.FromRgb(r.Next(255), r.Next(255), r.Next(255));
            if (nickNameColor.ContainsValue(newColor))
            {
                return GenerateColor();
            }
            else 
            { 
                return newColor; 
            }
        }
    }
}
