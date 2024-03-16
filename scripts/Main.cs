using Godot;

public partial class Main : Node
{
    [Export] private AudioStreamPlayer startSound;
    [Export] private AudioStreamPlayer bombexpl;
    [Export] private AudioStreamPlayer beep;

    private LineEdit lineEdit;
    private Timer bombTimer;
    private Window explosionWindow;
    private AnimatedSprite2D animatedSprite;

    public override void _Ready()
    {
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Transparent, true);
        GetViewport().GetWindow().Borderless = true;

        explosionWindow = this.FindChildOfType<Window>() as Window;
        explosionWindow.Borderless = true;
        explosionWindow.Size = new Vector2I(DisplayServer.ScreenGetSize().Y, DisplayServer.ScreenGetSize().Y);
        explosionWindow.Mode = Window.ModeEnum.Maximized;
        explosionWindow.Hide();

        animatedSprite = this.FindChildOfType<AnimatedSprite2D>() as AnimatedSprite2D;
        animatedSprite.Scale = explosionWindow.Size / 64;
        animatedSprite.AnimationFinished += () => explosionWindow.Hide();

        bombTimer = new Timer();
        bombTimer.WaitTime = 3;
        bombTimer.Autostart = false;
        bombTimer.OneShot = true;
        bombTimer.Timeout += OnStop;
        AddChild(bombTimer);

        lineEdit = this.FindChildOfType<LineEdit>() as LineEdit;
        lineEdit.GrabFocus();
        lineEdit.TextSubmitted += Start;
    }

    float timerLast = float.MaxValue;
    public override void _Process(double delta)
    {
        if (bombTimer.TimeLeft > 0 && bombTimer.TimeLeft <= 10 && timerLast > 10)
        {
            beep.Play();
            timerLast = (float)bombTimer.TimeLeft;
        }
        lineEdit.Text = GetTimeLeft();
    }

    string time = "";
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey e && e.Pressed && !e.IsEcho() && bombTimer.TimeLeft == 0 && !e.IsAction("ui_accept"))
        {
            if (e.Keycode == Key.Backspace && !string.IsNullOrEmpty(time))
                time = time.Remove(0, 1);

            string code = e.AsTextKeycode();
            if ("0123456789".Contains(code) && !(time == "" && code == "0"))
            {
                if (time.Length >= 4)
                    time = "";
                time += e.AsTextKeycode();
            }

            if (string.IsNullOrEmpty(time))
                bombTimer.WaitTime = 0.01f;
            else
                bombTimer.WaitTime = Mathf.Max(0.01f, time.Length <= 2 ? int.Parse(time) :
                int.Parse(time[..^2]) * 60 + int.Parse(time.Substring(time.Length - 2, 2)));
        }

        if (@event is InputEventMouseMotion mm && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            GetViewport().GetWindow().Position += (Vector2I)mm.Relative;
        }
    }

    private void Start(string text)
    {
        if (bombTimer.WaitTime < 3)
            return;
        lineEdit.ReleaseFocus();
        bombTimer.Start();
        startSound.Play();
    }

    private void OnStop()
    {
        lineEdit.GrabFocus();
        explosionWindow.Show();
        animatedSprite.Play();

        time = "";
        timerLast = float.MaxValue;
        beep.Stop();
        bombexpl.Play();
    }

    private string GetTimeLeft()
    {
        double t = bombTimer.TimeLeft > 0 ? bombTimer.TimeLeft : bombTimer.WaitTime;

        int minutes = (int)t / 60;
        int seconds = (int)t % 60;

        return $"{minutes.ToString().PadLeft(2, '0')}:{seconds.ToString().PadLeft(2, '0')}";
    }
}
