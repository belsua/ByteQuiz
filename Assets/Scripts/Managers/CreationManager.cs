using TMPro;
using UnityEngine;

public class CreationManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameField;
    public TMP_InputField usernameField;
    public TMP_InputField ageField;
    public TMP_Dropdown genderDropdown;
    public TMP_Dropdown sectionDropdown;
    public GameObject errorPanel;
    public FadeManager fadeManager;

    [Header("Avatars")]
    public Animator avatarAnimator; // Reference to the avatar's SpriteRenderer
    public TMP_Text avatarName; // Reference to the name of the animator>
    public Sprite[] avatarOptions; // Array to store different full-body avatar sprites
    private int currentAvatarIndex = 0; // Track the selected avatar
    readonly string[] avatarAnimatorNames = { "Adam", "Alex", "Bob", "Amelia" };

    public void NextAvatar()
    {
        // Cycle to the next avatar sprite
        currentAvatarIndex = (currentAvatarIndex + 1) % avatarOptions.Length;
        avatarAnimator.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>($"Controllers/{avatarAnimatorNames[currentAvatarIndex]}");
        avatarName.text = avatarAnimatorNames[currentAvatarIndex]; // Update the name of the animator>
    }

    public void PreviousAvatar()
    {
        // Cycle to the previous avatar sprite
        currentAvatarIndex = (currentAvatarIndex - 1 + avatarOptions.Length) % avatarOptions.Length;
        avatarAnimator.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>($"Controllers/{avatarAnimatorNames[currentAvatarIndex]}");
        avatarName.text = avatarAnimatorNames[currentAvatarIndex]; // Update the name of the animator>

    }

    public void ValidateAndCreate()
    {
        // Validate name (at least 3 characters)
        if (nameField.text.Length < 3)
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<TMP_Text>().text = "Name must be at least 3 characters.";
            Debug.Log("Error: Name must be at least 3 characters.");
            return;
        }

        // Validate username (at least 3 characters)
        if (usernameField.text.Length < 3)
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<TMP_Text>().text = "Username must be at least 3 characters.";
            Debug.Log("Error: Username must be at least 3 characters.");
            return;
        }

        // Validate age (must be a valid integer)
        if (!int.TryParse(ageField.text, out int age) || age < 13)
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<TMP_Text>().text = "Age must be a valid number and at least 13.";
            Debug.Log("Error: Age must be a valid number and at least 13.");
            return;
        }

        // Validate gender selection (ensure it's a valid value)
        if (genderDropdown == null)
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<TMP_Text>().text = "Section dropdown is not initialized.";
            Debug.Log("Error: Section dropdown is not initialized.");
            return;
        }

        // Validate dropdown selection (ensure it's a valid value)
        if (sectionDropdown == null)
        {
            errorPanel.SetActive(true);
            errorPanel.GetComponentInChildren<TMP_Text>().text = "Section dropdown is not initialized.";
            Debug.Log("Error: Section dropdown is not initialized.");
            return;
        }

        // If all validations pass, create the player and transition to the new scene
        SaveManager.instance.CreatePlayer(currentAvatarIndex, nameField.text, usernameField.text, age, genderDropdown.value.ToString(), sectionDropdown.value.ToString());
        fadeManager.FadeToScene("Crib");
    }
}
