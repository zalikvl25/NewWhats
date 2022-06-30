using Grpc.Core;
using SimpleChatApp.CommonTypes;
using SimpleChatApp.GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static SimpleChatApp.GrpcService.ChatService;

namespace NewWhats
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogInPage : ContentPage
    {
        private ChatServiceClient chatServiceClient;
        public LogInPage()
        {
            InitializeComponent();
            chatServiceClient = new ChatServiceClient(new Channel("localhost: 30051", ChannelCredentials.Insecure));
        }

        private void RegisterPage_Pressed(object sender, EventArgs e)
        {
            var registrationPage = new RegisterPage();
            Navigation.PushModalAsync(registrationPage);
        }
        private async void Login(string login, string password)
        {
            var userData = new UserData()
            {
                Login = login,
                PasswordHash = SHA256.GetStringHash(password)
            };
            var authorizationData = new AuthorizationData()
            {
                ClearActiveConnection = true,
                UserData = userData
            };
            var ans = await chatServiceClient.LogInAsync(authorizationData);

            if (ans.Status == SimpleChatApp.GrpcService.AuthorizationStatus.AuthorizationSuccessfull)
            {
                var chatPage = new MainPage(ans.Sid);
                await Navigation.PushModalAsync(chatPage);
                LoginEntry.Text = "";
                PasswordEntry.Text = "";
            }
            else
            { await DisplayAlert("LogIn error", "Error", "Ok"); };
        }

        private async void Login_Pressed(object sender, EventArgs e)
        {
            if (LoginEntry.Text != null && LoginEntry.Text != "" && PasswordEntry.Text != null && PasswordEntry.Text != "")
            {
                Login(LoginEntry.Text, PasswordEntry.Text);
            }
            else { await DisplayAlert("LogIn error", "Incorrect input", "Ok"); }
        }
    }
}