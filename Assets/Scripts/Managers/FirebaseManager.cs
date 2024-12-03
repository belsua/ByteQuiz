using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    public MenuManager menuManager;
    public static FirebaseManager instance;
    public DatabaseReference database;
    public FirebaseAuth auth;
    FirebaseUser user;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        transform.SetParent(null, false);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        database = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void PlayLocally()
    {
        PlayerPrefs.SetString("LoginMethod", "Local");
    }

    public async void SignInAnonymously()
    {
        try
        {
            PlayerPrefs.SetString("LoginMethod", "Anon");

            menuManager.ShowLoadingPanel();

            // Ensure Firebase dependencies are available
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"Firebase dependencies are not available: {dependencyStatus}");
                return;
            }

            // Perform anonymous sign-in
            var signInTask = await auth.SignInAnonymouslyAsync();

            if (signInTask != null)
            {
                // Retrieve signed-in user
                user = signInTask.User;
                Debug.Log($"User signed in successfully: {user?.DisplayName ?? "Anonymous"} ({user?.UserId})");
            }
            else
            {
                Debug.LogError("Anonymous sign-in returned null user.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during anonymous sign-in: {ex}");
        }
        finally
        {
            menuManager.HideLoadingPanel();
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
    }
}
