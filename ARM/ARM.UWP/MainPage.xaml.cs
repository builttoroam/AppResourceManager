using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory; //ADAL client library for getting the access token
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using XLabs.Cryptography;

namespace AppResourceManager.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new AppResourceManager.App());

            AppResourceManager.MainPage.ParameterFunc = NativeParameters;
        }


        public IPlatformParameters NativeParameters()
        {
            return new PlatformParameters(PromptBehavior.Auto, false);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get the access token.
            string accessToken = await GetUserOAuthToken();


            // Use the access token to create the storage credentials.
            TokenCredential tokenCredential = new TokenCredential(accessToken);
            tokenCredential.Token = accessToken;
            StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);

            // Create a block blob using those credentials
            CloudBlockBlob blob = new CloudBlockBlob(new Uri("https://marssampleapp.blob.core.windows.net/test/Blob1.txt"), storageCredentials);

            var stream = await blob.OpenWriteAsync();
            var writer = new StreamWriter(stream);
                await writer.WriteLineAsync("Hello world!");
                await writer.FlushAsync();
            await stream.CommitAsync();

            var hash = MD5.GetMd5String("Hello world!");

            blob = new CloudBlockBlob(new Uri("https://marssampleapp.blob.core.windows.net/test/Blob1.hash"), storageCredentials);

            stream = await blob.OpenWriteAsync();
            writer = new StreamWriter(stream);
            await writer.WriteLineAsync(hash);
            await writer.FlushAsync();
            await stream.CommitAsync();

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
                                                                        new PlatformParameters(PromptBehavior.Auto, false));

            return result.AccessToken;
        }
    }
}
