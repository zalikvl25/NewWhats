using Grpc.Core;
using SimpleChatApp.CommonTypes;
using SimpleChatApp.GrpcService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static SimpleChatApp.GrpcService.ChatService;

namespace NewWhats
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        private ChatServiceClient chatServiceClient;
        private ObservableCollection<SimpleChatApp.CommonTypes.MessageData> chat = new ObservableCollection<SimpleChatApp.CommonTypes.MessageData>();
        public RegisterPage()
        {
            InitializeComponent();

            chatServiceClient = new ChatServiceClient(new Channel("localhost: 30051", ChannelCredentials.Insecure));
        }

        private async Task<RegistrationAnswer> RegisterNewUser(string login, string password)
        {
            var userData = new UserData()
            {
                Login = login,
                PasswordHash = SHA256.GetStringHash(password)
            };
            var ans = await chatServiceClient.RegisterNewUserAsync(userData);
            return ans;
        }

        private void Clearner()
        {
            LoginEntry.Text = "";
            PasswordEntry.Text = "";
            PasswordAgain.Text = "";
        }

        private async void Register_Pressed(object sender, EventArgs e)
        {
            if (LoginEntry.Text != null && LoginEntry.Text != "" && PasswordEntry.Text != null && PasswordEntry.Text != "" && PasswordAgain.Text != null && PasswordAgain.Text != "")
            {
                if (PasswordEntry.Text == PasswordAgain.Text)
                { 
                    var result = await RegisterNewUser(LoginEntry.Text, PasswordEntry.Text);
                    if (result.Status == SimpleChatApp.GrpcService.RegistrationStatus.RegistrationSuccessfull)
                    {
                        Clearner();
                        await DisplayAlert("Registration success", "User created", "Ok");
                        await Navigation.PopModalAsync();
                    }
                    else 
                    {
                        await DisplayAlert("Registration error", "Error", "Ok");
                    }
                }
                else { await DisplayAlert("Registration error", "Passwords are not the same", "Ok"); }
            }
            else { await DisplayAlert("Registration error", "Incorrect input", "Ok"); }
        }

        private async void Back_Pressed(object sender, EventArgs e)
        {
            Clearner();
            await Navigation.PopModalAsync();
        }

    }
}