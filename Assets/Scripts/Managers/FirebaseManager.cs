using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
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

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        database = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void PlayLocally()
    {
        PlayerPrefs.SetString("LoginMethod", "Local");
    }

    public void SignInAnonymously()
    {
        PlayerPrefs.SetString("LoginMethod", "Anon");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth.SignInAnonymouslyAsync().ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        user = task.Result.User;
                        Debug.Log($"User signed in successfully: {user.DisplayName} ({user.UserId})");
                    }
                    else
                    {
                        Debug.LogError(task.Exception);
                    }
                }
                );
            }
            else
            {
                Debug.LogError(task.Exception);
            }
        });
    }
}
