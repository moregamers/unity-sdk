using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

using UnityEngine;

using MiniJson;

using MG.Data;
using MG.Enums;
using MG.Utils;
using MG.Events;
using MG.EnumsExtensions;

public class MoreGamers : Singleton<MoreGamers>
{
    #region Private constant fields

    private const float REFRESH_RATE = 20f;

    private const string SDK_VERSION_NUMBER = "1.1.1";

    private const string BASE_URL = "https://app.moregamers.com/";

    private const string AMAZON_MODEL_IDENTIFIER = "Amazon";

    private static readonly string[] URL_PARAMETERS =
        new string[3] { "ad?game=", "&sdk=unity&platform=", string.Concat("&sdkVersion=", SDK_VERSION_NUMBER) };

    #endregion

    #region Private static events

    /// <summary>
    /// Occurs when on banner received.
    /// </summary>
    private static event EventHandler<EventBannerArgs> onBannerReceived;

    /// <summary>
    /// Occurs when on ad failed to load.
    /// </summary>
    private static event EventHandler onBannerFailedToLoad;

    #endregion

    #region Public static event properties

    /// <summary>
    /// Occurs when on ad received.
    /// </summary>
    public static event EventHandler<EventBannerArgs> OnBannerReceived
    {
        add
        {
            AddHandler_OnBannerReceived(value);
        }
        remove
        {
            RemoveHandler_OnBannerReceived(value);
        }
    }

    /// <summary>
    /// Occurs when on ad failed to load.
    /// </summary>
    public static event EventHandler OnBannerFailedToLoad
    {
        add
        {
            AddHandler_OnBannerFailedToLoad(value);
        }
        remove
        {
            RemoveHandler_OnBannerFailedToLoad(value);
        }
    }

    #endregion

    /// <summary>
    /// These methods are needed so events can be used on iOS.
    /// </summary>
    #region Private static custom add/remove event methods

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void AddHandler_OnBannerFailedToLoad(EventHandler methodToAdd)
    {
        onBannerFailedToLoad = (EventHandler)Delegate.Combine(
            onBannerFailedToLoad,
            methodToAdd);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void RemoveHandler_OnBannerFailedToLoad(EventHandler methodToRemove)
    {
        onBannerFailedToLoad = (EventHandler)Delegate.Remove(
            onBannerFailedToLoad,
            methodToRemove);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void AddHandler_OnBannerReceived(EventHandler<EventBannerArgs> methodToAdd)
    {
        onBannerReceived = (EventHandler<EventBannerArgs>)Delegate.Combine(
            onBannerReceived,
            methodToAdd);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void RemoveHandler_OnBannerReceived(EventHandler<EventBannerArgs> methodToRemove)
    {
        onBannerReceived = (EventHandler<EventBannerArgs>)Delegate.Remove(
            onBannerReceived,
            methodToRemove);
    }

    #endregion

    #region Private static fields

    private static bool instanceCreated = false;

    #endregion

    #region Private exposed fields

    [SerializeField]
#pragma warning disable 649
    private string m_moreGamersGameID;
#pragma warning restore 649

    #endregion

    #region Private fields

    private bool m_alreadyInUse;

    private Platform m_store;

    private float m_lastRequestTime;

    private string m_lastClickURL;

    private Texture2D m_lastRectangleTexture;

    private Texture2D m_lastSquareTexture;

    private Dictionary<string, Texture2D> m_cachedRectanlges;

    private Dictionary<string, Texture2D> m_cachedSquares;

    #endregion

    #region Private monobehaviour methods

    private void Awake()
    {
        if (instanceCreated)
        {
            GameObject.Destroy(base.gameObject);

            return;
        }

        GameObject.DontDestroyOnLoad(base.gameObject);

        instanceCreated = true;

        this.m_alreadyInUse = false;

        this.m_lastRequestTime = float.NegativeInfinity;

        this.m_cachedRectanlges = new Dictionary<string, Texture2D>();

        this.m_cachedSquares = new Dictionary<string, Texture2D>();

        if (string.IsNullOrEmpty(this.m_moreGamersGameID))
            throw new UnityException("Please provide the moregamers ID for your game");

        if (SystemInfo.deviceModel.Contains(AMAZON_MODEL_IDENTIFIER))
            this.m_store = Platform.Amazon;
        else switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    this.m_store = Platform.GooglePlay;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    this.m_store = Platform.Itunes;
                    break;
                case RuntimePlatform.WP8Player:
                    this.m_store = Platform.Windows;
                    break;
                default:
                    this.m_store = Platform.None;
                    break;
            }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Creates the URL needed for the request.
    /// </summary>
    /// <returns>The URL.</returns>
    private string CreateURL()
    {
        return string.Concat(
            BASE_URL,
            URL_PARAMETERS[0],
            this.m_moreGamersGameID,
            URL_PARAMETERS[1],
            this.m_store.GetDescription(),
            URL_PARAMETERS[2]);
    }

    /// <summary>
    /// Gets the advert.
    /// </summary>
    /// <returns>The advert.</returns>
    private IEnumerator GetBanner(BannerSize bannerSize)
    {
        if (this.m_alreadyInUse)
            yield break;

        this.m_alreadyInUse = true;

        if (string.IsNullOrEmpty(this.m_moreGamersGameID))
        {
            this.m_alreadyInUse = false;
            yield break;
        }

        if (Time.time - this.m_lastRequestTime < REFRESH_RATE && !string.IsNullOrEmpty(this.m_lastClickURL))
        {
            if (bannerSize == BannerSize.Square)
            {
                if (onBannerReceived != null)
                    onBannerReceived(
                        this,
                        new EventBannerArgs(
                            this.m_lastClickURL,
                            this.m_lastSquareTexture));
            }
            else
            {
                if (onBannerReceived != null)
                    onBannerReceived(
                        this,
                        new EventBannerArgs(
                            this.m_lastClickURL,
                            this.m_lastRectangleTexture));
            }

            this.m_alreadyInUse = false;

            yield break;
        }

        Dictionary<string, object> jsonDictionary;

        using (WWW request = new WWW(CreateURL()))
        {
            yield return request;

            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError(string.Concat("An error occured: ", request.error));

                if (onBannerFailedToLoad != null)
                    onBannerFailedToLoad(this, null);

                this.m_alreadyInUse = false;

                yield break;
            }

            // To work around the malformed json
            string json = request.text;
            json = Regex.Replace(json, "(^\")|(\"$)|(\\\\)", "");

            jsonDictionary = json.dictionaryFromJson();

            if (!jsonDictionary["error"].ToString().ToLower().Equals("false"))
            {
                Debug.LogError(string.Concat("An error occured: ", jsonDictionary["error"].ToString()));

                if (onBannerFailedToLoad != null)
                    onBannerFailedToLoad(this, null);

                this.m_alreadyInUse = false;

                yield break;
            }
        }

        Response response = new Response(jsonDictionary);

        string imageUrl = bannerSize == BannerSize.Square ? response.LandscapeImageURL : response.PortraitImageURL;

        if (bannerSize == BannerSize.Square)
        {
            if (this.m_cachedSquares.ContainsKey(imageUrl))
            {
                this.m_lastClickURL = response.ClickURL;

                this.m_lastSquareTexture = this.m_cachedSquares[imageUrl];

                if (onBannerReceived != null)
                    onBannerReceived(
                        this,
                        new EventBannerArgs(
                            response.ClickURL,
                            this.m_cachedSquares[imageUrl]));

                this.m_alreadyInUse = false;

                yield break;
            }
        }
        else
        {
            if (this.m_cachedRectanlges.ContainsKey(imageUrl))
            {
                this.m_lastClickURL = response.ClickURL;

                this.m_lastRectangleTexture = this.m_cachedRectanlges[imageUrl];

                if (onBannerReceived != null)
                    onBannerReceived(
                        this,
                        new EventBannerArgs(
                            response.ClickURL,
                            this.m_cachedRectanlges[imageUrl]));

                this.m_alreadyInUse = false;

                yield break;
            }
        }

        using (WWW imageRequest = new WWW(imageUrl))
        {
            yield return imageRequest;

            if (!string.IsNullOrEmpty(imageRequest.error))
            {
                Debug.LogError(string.Concat(
                    "An error occured: ",
                    imageRequest.error));

                if (onBannerFailedToLoad != null)
                    onBannerFailedToLoad(this, null);

                this.m_alreadyInUse = false;

                yield break;
            }

            base.StartCoroutine(Track(response.TrackingURL));

            this.m_lastRequestTime = Time.time;

            this.m_lastClickURL = response.ClickURL;

            if (bannerSize == BannerSize.Square)
            {
                this.m_lastSquareTexture = imageRequest.texture;
                this.m_cachedSquares.Add(imageRequest.url, imageRequest.texture);
            }
            else
            {
                this.m_lastRectangleTexture = imageRequest.texture;
                this.m_cachedRectanlges.Add(imageRequest.url, imageRequest.texture);
            }

            if (onBannerReceived != null)
                onBannerReceived(
                    this,
                    new EventBannerArgs(
                        response.ClickURL,
                        imageRequest.texture));
        }

        imageUrl = bannerSize == BannerSize.Square ? response.PortraitImageURL : response.LandscapeImageURL;

        using (WWW imageRequest = new WWW(imageUrl))
        {
            yield return imageRequest;

            if (!string.IsNullOrEmpty(imageRequest.error))
            {
                Debug.LogError(string.Concat(
                    "An error occured: ",
                    imageRequest.error));

                if (onBannerFailedToLoad != null)
                    onBannerFailedToLoad(this, null);

                this.m_alreadyInUse = false;

                yield break;
            }

            if (bannerSize == BannerSize.Square)
            {
                this.m_lastRectangleTexture = imageRequest.texture;

                this.m_cachedRectanlges.Add(
                    imageRequest.url,
                    imageRequest.texture);
            }
            else
            {
                this.m_lastSquareTexture = imageRequest.texture;

                this.m_cachedSquares.Add(
                    imageRequest.url,
                    imageRequest.texture);
            }
        }

        this.m_alreadyInUse = false;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Gets the ad.
    /// </summary>
    /// <param name="adSize">Ad size.</param>
    public void banner(BannerSize bannerSize = BannerSize.Square)
    {
        base.StartCoroutine(this.GetBanner(bannerSize));
    }

    #endregion

    #region Private static methods

    /// <summary>
    /// Track the specified tracking URL.
    /// </summary>
    /// <param name="trackingURL">Tracking URL.</param>
    private static IEnumerator Track(string trackingURL)
    {
        using (WWW request = new WWW(trackingURL))
            yield return request;
    }

    #endregion
}
