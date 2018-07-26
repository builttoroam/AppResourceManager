using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory; //ADAL client library for getting the access token
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace AppResourceManager
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        public static Func<IPlatformParameters> ParameterFunc { get; set; }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            return;

            // Get the access token.
            string accessToken = await GetUserOAuthToken();


            // Use the access token to create the storage credentials.
            TokenCredential tokenCredential = new TokenCredential(accessToken);
            StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);

            // Create a block blob using those credentials
            CloudBlockBlob blob = new CloudBlockBlob(new Uri("https://marssampleapp.blob.core.windows.net/test/Blob1.txt"), storageCredentials);

            using (var stream = await blob.OpenWriteAsync())
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync("Hello world!");
                await writer.FlushAsync();
            }
           
        }

        static async Task<string> GetUserOAuthToken()
        {
            const string ResourceId = "https://storage.azure.com/";
            const string AuthEndpoint = "https://login.microsoftonline.com/{0}/oauth2/token";
            const string TenantId = "9236ecf1-cfae-41f7-829c-af7e3d09a7e6"; // Tenant or directory ID

            // Construct the authority string from the Azure AD OAuth endpoint and the tenant ID. 
            string authority = string.Format(CultureInfo.InvariantCulture, AuthEndpoint, TenantId);
            AuthenticationContext authContext = new AuthenticationContext(authority);

            // Acquire an access token from Azure AD. 
            AuthenticationResult result = await authContext.AcquireTokenAsync(ResourceId,
                                                                        "eb9f7307-b632-4b21-8b1b-723fc7c999cd",
                                                                        new Uri(@"http://MarsSampleApp/"),
                                                                        ParameterFunc(), new UserIdentifier("nick@builttoroam.com",UserIdentifierType.UniqueId));

            return result.AccessToken;
        }
    }
}
