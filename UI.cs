using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class UI : Control
{
    const int FontSize = 35;
    const float TypeWriterSpeed = 0.05f;
    [Export]Label Notification = new Label();
    Panel DialogueBox;
    Panel MenuBox;

    Theme UITheme = ResourceLoader.Load<Theme>("res://Fonts/UITheme.tres");


    public override void _Ready()
    {
        DialogueBox = (Panel)GetChild(0);
        MenuBox = (Panel)GetChild(1);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("quit"))
        {
            if (!MenuBox.Visible)
            {
                GetTree().Paused = true;
                MenuBox.Show();
                Input.MouseMode = Input.MouseModeEnum.Confined;
            }
            else
            {
                GetTree().Paused = false;
                MenuBox.Hide();
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }
    public async void Notify(string Message)
    {
        Label NewNotification = (Label)Notification.Duplicate();
        DialogueBox.AddChild(NewNotification);
        DialogueBox.Visible = true;
        NewNotification.AddThemeColorOverride("font_color", new Color(0, 0, 0));
        NewNotification.AddThemeFontSizeOverride("font_size",FontSize);
        NewNotification.Theme = UITheme;
        NewNotification.VisibleCharacters = 0;
        NewNotification.Text = Message;
        NewNotification.Show();
        DialogueBox.MoveChild(NewNotification, 0);
        foreach(char i in NewNotification.Text)
        {
            NewNotification.VisibleCharacters++;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        Timer NotificationTimer = new Timer();
        AddChild(NotificationTimer);
        NotificationTimer.WaitTime = 5;
        NotificationTimer.OneShot = true;
        NotificationTimer.Timeout += () => NotificationTimeout(NewNotification);
        NotificationTimer.Start();
    }
    async void NotificationTimeout(Label NewNotification) {
        foreach (char i in NewNotification.Text)
        {
            NewNotification.VisibleCharacters++;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        DialogueBox.Visible = false;
        NewNotification.QueueFree();
    }

    public void OnExitButtonPressed()
    {
        GetTree().Quit();
    }
}
