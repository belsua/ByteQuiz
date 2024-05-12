using UnityEngine;

public class GameSceneManager : MainMenu
{
    public GameObject OnScreenPanel;

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Escape))
        ToggleGameObject(OnScreenPanel);
    }
}
