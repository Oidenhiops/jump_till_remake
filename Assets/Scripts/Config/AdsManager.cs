using System;
using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
#endif

/// <summary>
/// Manager central de AdMob: inicializacion, banner y rewarded.
/// Ponlo en un GameObject de la primera escena; sobrevive entre escenas.
///
/// Uso:
///   AdsManager.Instance.ShowBanner();
///   AdsManager.Instance.HideBanner();
///   AdsManager.Instance.ShowRewarded(onRewarded: () => { /* dar premio */ }, onClosed: () => { /* reanudar */ });
/// </summary>
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("Config")]
    [Tooltip("Si esta activo se usan los IDs de prueba de Google en lugar de los reales.")]
    public bool useTestIds = true;

    [Header("Ad Unit IDs - Android")]
    public string androidBannerId = "ca-app-pub-xxxxxxxxxxxxxxxx/yyyyyyyyyy";
    public string androidRewardedId = "ca-app-pub-xxxxxxxxxxxxxxxx/yyyyyyyyyy";

    [Header("Ad Unit IDs - iOS")]
    public string iosBannerId = "ca-app-pub-xxxxxxxxxxxxxxxx/yyyyyyyyyy";
    public string iosRewardedId = "ca-app-pub-xxxxxxxxxxxxxxxx/yyyyyyyyyy";

    [Header("Banner")]
    public bool showBannerOnInit = false;
    [Tooltip("Posicion del banner (arriba o abajo de la pantalla).")]
    public AdPositionSimple bannerPosition = AdPositionSimple.Bottom;

    // IDs de prueba oficiales de Google (no generan ingresos, no te banean).
    private const string TEST_BANNER_ANDROID = "ca-app-pub-3940256099942544/6300978111";
    private const string TEST_REWARDED_ANDROID = "ca-app-pub-3940256099942544/5224354917";
    private const string TEST_BANNER_IOS = "ca-app-pub-3940256099942544/2934735716";
    private const string TEST_REWARDED_IOS = "ca-app-pub-3940256099942544/1712485313";

    public bool IsInitialized { get; private set; }
    public bool IsRewardedReady
    {
        get
        {
#if UNITY_ANDROID || UNITY_IOS
            return rewardedAd != null && rewardedAd.CanShowAd();
#else
            return false;
#endif
        }
    }

    public event Action OnInitialized;

#if UNITY_ANDROID || UNITY_IOS
    private BannerView bannerView;
    private AdPositionSimple currentBannerPosition;
    private RewardedAd rewardedAd;
#endif

    // Callbacks del rewarded que esta en pantalla.
    private Action pendingOnRewarded;
    private Action pendingOnClosed;
    private Action pendingOnFailed;

    // Reintento de carga del rewarded con backoff.
    private int rewardedRetryAttempt = 0;
    private const int MAX_RETRY_DELAY_SECONDS = 64;

    // --------------------------------------------------------------------
    // Ciclo de vida
    // --------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Si este es un duplicado (la escena se recargo y ya existe un AdsManager persistente),
        // Awake ya lo marco para destruir, pero Start igual se ejecuta en este frame.
        // Sin este guard, el duplicado inicializa AdMob otra vez y crea un banner huerfano
        // que nadie destruye (se acumulan banners en pantalla).
        if (Instance != this) return;
        Initialize();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        DestroyBanner();
#if UNITY_ANDROID || UNITY_IOS
        DestroyRewarded();
#endif
    }

    // --------------------------------------------------------------------
    // Inicializacion
    // --------------------------------------------------------------------

    public void Initialize()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (IsInitialized) return;

        // Los callbacks de AdMob pueden llegar en otro hilo; esto los lleva al hilo principal.
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        MobileAds.Initialize(initStatus =>
        {
            IsInitialized = true;
            Debug.Log("[AdsManager] AdMob inicializado.");

            LoadRewarded();
            if (showBannerOnInit) ShowBanner();

            OnInitialized?.Invoke();
        });
#else
        Debug.Log("[AdsManager] Plataforma sin AdMob (Editor/WebGL/Standalone). Los anuncios se omiten.");
#endif
    }

    // --------------------------------------------------------------------
    // IDs
    // --------------------------------------------------------------------

    private string BannerId
    {
        get
        {
#if UNITY_ANDROID
            return useTestIds ? TEST_BANNER_ANDROID : androidBannerId;
#elif UNITY_IOS
            return useTestIds ? TEST_BANNER_IOS : iosBannerId;
#else
            return "unused";
#endif
        }
    }

    private string RewardedId
    {
        get
        {
#if UNITY_ANDROID
            return useTestIds ? TEST_REWARDED_ANDROID : androidRewardedId;
#elif UNITY_IOS
            return useTestIds ? TEST_REWARDED_IOS : iosRewardedId;
#else
            return "unused";
#endif
        }
    }

    // --------------------------------------------------------------------
    // Banner
    // --------------------------------------------------------------------

    /// <summary>Crea (si hace falta) y muestra el banner en la posicion configurada en el Inspector.</summary>
    public void ShowBanner()
    {
        ShowBanner(bannerPosition);
    }

    /// <summary>
    /// Crea (si hace falta) y muestra el banner en la posicion indicada.
    /// Si el banner ya existe en otra posicion, se destruye y se vuelve a crear.
    /// </summary>
    public void ShowBanner(AdPositionSimple position)
    {
        bannerPosition = position;
#if UNITY_ANDROID || UNITY_IOS
        if (!IsInitialized)
        {
            // Si aun no inicializo, lo mostramos cuando termine.
            showBannerOnInit = true;
            return;
        }

        if (bannerView != null && currentBannerPosition != position)
        {
            DestroyBanner();
        }

        if (bannerView == null)
        {
            currentBannerPosition = position;
            AdPosition pos = position == AdPositionSimple.Top ? AdPosition.Top : AdPosition.Bottom;

            if (Application.isEditor)
            {
                // Editor: banner falso del plugin, se deja el comportamiento por defecto.
                AdSize editorSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                bannerView = new BannerView(BannerId, editorSize, pos);
            }
            else
            {
                // Dispositivo real. El plugin de Android centra el banner en TODA la pantalla y luego
                // le aplica el inset del notch como margen, asi que con un banner de ancho completo
                // queda corrido hacia un lado y cortado. Solucion: ancho simetrico (pantalla menos el
                // inset mas grande a cada lado) y posicion custom (x,y en dp) desde la esquina superior.
                ComputeBannerLayout(out int widthDp, out int xDp);
                Debug.Log($"[AdsManager] Banner widthDp={widthDp} xDp={xDp} (screen={Screen.width}x{Screen.height}, safe={Screen.safeArea}, dpi={Screen.dpi})");
                AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(widthDp);

                // y provisional (se corrige al cargar, cuando ya se conoce la altura real del banner).
                int yDp = position == AdPositionSimple.Top ? 0 : Mathf.Max(0, PixelsToDp(Screen.height) - 90);
                bannerView = new BannerView(BannerId, adaptiveSize, xDp, yDp);
            }

            RegisterBannerEvents(bannerView);
            bannerView.LoadAd(new AdRequest());
        }
        else
        {
            bannerView.Show();
        }
#endif
    }

    /// <summary>Oculta el banner sin destruirlo (se puede volver a mostrar sin recargar).</summary>
    public void HideBanner()
    {
#if UNITY_ANDROID || UNITY_IOS
        bannerView?.Hide();
#endif
    }

    /// <summary>Destruye el banner. Llamalo si no lo vas a usar en un buen rato.</summary>
    public void DestroyBanner()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
#endif
    }

#if UNITY_ANDROID || UNITY_IOS
    private static float Density => (Screen.dpi > 0 ? Screen.dpi : 160f) / 160f;

    private static int PixelsToDp(float px) => Mathf.RoundToInt(px / Density);

    /// <summary>
    /// Calcula el ancho del banner (dp) y su offset horizontal (dp) para que quede centrado y fuera
    /// del notch sin importar de que lado este. Usa el inset mayor a ambos lados (simetrico).
    /// </summary>
    private static void ComputeBannerLayout(out int widthDp, out int xDp)
    {
        Rect safe = Screen.safeArea;
        float insetLeft = Mathf.Max(0f, safe.xMin);
        float insetRight = Mathf.Max(0f, Screen.width - safe.xMax);
        float inset = Mathf.Max(insetLeft, insetRight);

        float widthPx = Screen.width - 2f * inset;
        widthDp = Mathf.FloorToInt(widthPx / Density);
        xDp = Mathf.CeilToInt(inset / Density);

        if (widthDp < 320) { widthDp = 320; xDp = Mathf.Max(0, PixelsToDp((Screen.width - 320f * Density) / 2f)); }
    }

    private void RegisterBannerEvents(BannerView banner)
    {
        banner.OnBannerAdLoaded += () =>
        {
            Debug.Log("[AdsManager] Banner cargado.");
            // Recalcula la posicion con el ancho real del anuncio ya cargado
            // (evita que quede desplazado a la derecha en Android).
            if (!Application.isEditor)
            {
                // Reposiciona con la altura real del anuncio ya cargado.
                ComputeBannerLayout(out _, out int xDp);
                int yDp = 0;
                if (currentBannerPosition == AdPositionSimple.Bottom)
                {
                    float heightPx = banner.GetHeightInPixels();
                    yDp = Mathf.Max(0, PixelsToDp(Screen.height - heightPx));
                }
                banner.SetPosition(xDp, yDp);
            }
        };
        banner.OnBannerAdLoadFailed += error => Debug.LogWarning("[AdsManager] Banner fallo al cargar: " + error);
        banner.OnAdPaid += value => Debug.Log($"[AdsManager] Banner pagado: {value.Value} {value.CurrencyCode}");
        banner.OnAdFullScreenContentOpened += () => Debug.Log("[AdsManager] Banner abrio overlay.");
        banner.OnAdFullScreenContentClosed += () => Debug.Log("[AdsManager] Banner cerro overlay.");
    }
#endif

    // --------------------------------------------------------------------
    // Rewarded
    // --------------------------------------------------------------------

    /// <summary>Precarga un rewarded. Se llama solo al iniciar y despues de cada uso.</summary>
    public void LoadRewarded()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!IsInitialized) return;

        DestroyRewarded();

        Debug.Log("[AdsManager] Cargando rewarded...");
        RewardedAd.Load(RewardedId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogWarning("[AdsManager] Rewarded fallo al cargar: " + error);
                ScheduleRewardedRetry();
                return;
            }

            rewardedRetryAttempt = 0;
            rewardedAd = ad;
            RegisterRewardedEvents(ad);
            Debug.Log("[AdsManager] Rewarded listo.");
        });
#endif
    }

    /// <summary>
    /// Muestra el rewarded si esta listo.
    /// onRewarded: se llama solo si el usuario completo el video (aqui das el premio).
    /// onClosed: se llama siempre que el anuncio se cierre (con o sin premio).
    /// onFailed: se llama si no habia anuncio listo o fallo al mostrarse.
    /// </summary>
    public void ShowRewarded(Action onRewarded, Action onClosed = null, Action onFailed = null)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!IsRewardedReady)
        {
            Debug.LogWarning("[AdsManager] Rewarded no esta listo.");
            onFailed?.Invoke();
            LoadRewarded();
            return;
        }

        pendingOnRewarded = onRewarded;
        pendingOnClosed = onClosed;
        pendingOnFailed = onFailed;

        rewardedAd.Show((Reward reward) =>
        {
            Debug.Log($"[AdsManager] Recompensa otorgada: {reward.Amount} {reward.Type}");
            pendingOnRewarded?.Invoke();
            pendingOnRewarded = null;
        });
#else
        // Fuera de movil (Editor, WebGL...) simulamos que el usuario vio el video,
        // para poder probar el flujo del juego sin anuncios reales.
        Debug.Log("[AdsManager] Rewarded simulado (plataforma sin AdMob).");
        onRewarded?.Invoke();
        onClosed?.Invoke();
#endif
    }

#if UNITY_ANDROID || UNITY_IOS
    private void RegisterRewardedEvents(RewardedAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("[AdsManager] Rewarded abierto.");
            // Si quieres pausar el juego: GameManager.Instance.isPause = true;
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("[AdsManager] Rewarded cerrado.");
            pendingOnClosed?.Invoke();
            ClearPendingCallbacks();
            LoadRewarded(); // Precargar el siguiente.
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogWarning("[AdsManager] Rewarded fallo al mostrarse: " + error);
            pendingOnFailed?.Invoke();
            ClearPendingCallbacks();
            LoadRewarded();
        };

        ad.OnAdPaid += value => Debug.Log($"[AdsManager] Rewarded pagado: {value.Value} {value.CurrencyCode}");
    }

    private void DestroyRewarded()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }

    private void ScheduleRewardedRetry()
    {
        rewardedRetryAttempt++;
        float delay = Mathf.Min(Mathf.Pow(2, rewardedRetryAttempt), MAX_RETRY_DELAY_SECONDS);
        Debug.Log($"[AdsManager] Reintentando cargar rewarded en {delay}s.");
        Invoke(nameof(LoadRewarded), delay);
    }
#endif

    private void ClearPendingCallbacks()
    {
        pendingOnRewarded = null;
        pendingOnClosed = null;
        pendingOnFailed = null;
    }

    public enum AdPositionSimple { Top, Bottom }
}
