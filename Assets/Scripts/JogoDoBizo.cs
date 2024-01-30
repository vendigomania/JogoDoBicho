using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JogoDoBizo : MonoBehaviour
{
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject playScreen;
    [SerializeField] private List<CardItem> cards = new List<CardItem>();
    [SerializeField] private Button confirmBtn;

    [SerializeField] private GameObject endScreen;
    [SerializeField] private Image[] yourChoice;
    [SerializeField] private Image[] aiChoice;

    private Sprite[] sprites;
    private int activeRemain = 3;

    void Start()
    {
        for(var i = 1; i < 25; i++)
        {
            cards.Add(Instantiate(cards[0], cards[0].transform.parent));
        }

        sprites = Resources.LoadAll<Sprite>("");

        for(var i = 0; i < sprites.Length; i++)
        {
            cards[i].Init(sprites[i], OnCardClick);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            startScreen.SetActive(true);
            playScreen.SetActive(false);
            endScreen.SetActive(false);
        }
    }

    public void Play()
    {
        startScreen.SetActive(false);
        playScreen.SetActive(true);
        endScreen.SetActive(false);
    }

    public void Privacy()
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
            webView.OnShouldClose += (view) => { return true; };
            webView.Load(AppStartup.PrivacyUrl);
        }
        catch (System.Exception ex)
        {
            statusText.text += $"\n {ex}";
        }
    }

    IEnumerator ResizeWebview(UniWebView view)
    {
        yield return new WaitForSeconds(Time.deltaTime * 2);
        view.Frame = new Rect(0, 0, Screen.width, Screen.height);
    }

    public void Confirm()
    {
        int choice = 0;
        for(var i = 0; i < cards.Count; i++)
        {
            if (cards[i].IsOn)
            {
                yourChoice[choice].sprite = sprites[i];
                choice++;
            }
        }

        List<int> randomList = new List<int>();
        for (var i = 0; i < 25; i++) randomList.Add(i);

        for(var j = 0; j < 3; j++)
        {
            var randomId = Random.Range(0, randomList.Count);
            aiChoice[j].sprite = sprites[randomList[randomId]];
            randomList.RemoveAt(randomId);
        }

        endScreen.SetActive(true);
        playScreen.SetActive(false);
    }

    private void OnCardClick(CardItem card, bool isOn)
    {
        if(isOn)
        {
            if (activeRemain > 0)
            {
                activeRemain--;
            }
            else
            {
                card.IsOn = false;
            }
        }
        else
        {
            activeRemain++;
        }

        confirmBtn.interactable = activeRemain == 0;
    }
}
