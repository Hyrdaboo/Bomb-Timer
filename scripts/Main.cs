using Godot;
using Godot.Collections;
using System.Text;

public partial class Main : Node
{
    [Export] private Window explosionWindow;
    [Export] private Array<AudioStreamPlayer> startSounds;
    [Export] private AudioStreamPlayer bombexpl;
    [Export] private AudioStreamPlayer beep;

    private LineEdit lineEdit;
    private Timer bombTimer;
    private AnimatedSprite2D animatedSprite;

    private int startSoundIndex = 0;

    public override void _Ready()
    {
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Transparent, true);
        GetWindow().Borderless = true;

        int size = PlayerPrefs.GetInt("size", 512);
        int curInd = PlayerPrefs.GetInt("ss_ind");
        GetWindow().Size = Vector2I.One * size;
        GetWindow().MoveToCenter();
        startSoundIndex = curInd;
        PlayerPrefs.SetInt("ss_ind", startSoundIndex);
        PlayerPrefs.SetInt("size", size);

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
    
    public override void _Process(double delta)
    {
        if (!beep.Playing && bombTimer.TimeLeft <= 10 && !bombTimer.IsStopped())
        {
            beep.Play();
        }
        lineEdit.Text = GetTimeLeft();
        
        HandleWindowDrag();

        if (Input.IsActionJustPressed("Minimize"))
        {
            GetWindow().Mode = Window.ModeEnum.Minimized;
        }

        if (Input.IsActionJustPressed("SizeUp"))
        {
            Window window = GetWindow();
            window.Size = Vector2I.One * Mathf.Min(DisplayServer.ScreenGetSize().Y, window.Size.Y + 10);

            PlayerPrefs.SetInt("size", window.Size.Y);
        }

        if (Input.IsActionJustPressed("SizeDown"))
        {
            Window window = GetWindow();
            window.Size = Vector2I.One * Mathf.Max(200, window.Size.Y - 10);

            PlayerPrefs.SetInt("size", window.Size.Y);
        }

        if (Input.IsActionJustPressed("Reset"))
        {
            bombTimer.Stop();
            beep.Stop();
            lineEdit.GrabFocus();
        }

        if (Input.IsActionJustPressed("Quit"))
        {
            GetTree().Quit();
        }
    }

    string time = "";
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey e && e.Pressed && !e.IsEcho() && bombTimer.TimeLeft == 0 && !e.IsAction("ui_accept"))
        {
            string code = e.AsTextKeycode();
            if (code.Contains("Ctrl"))
            {
                if ("01234".Contains(code[^1]))
                {
                    startSoundIndex = int.Parse(code[^1].ToString());
                    PlayerPrefs.SetInt("ss_ind", startSoundIndex);
                }
                return;
            }

            if ("0123456789".Contains(code) && !(time == "" && code == "0"))
            {
                if (time.Length >= 6)
                    time = "";
                time += e.AsTextKeycode();
            }
            else if (e.Keycode == Key.Backspace)
                time = "";
            else
                return;

            StringBuilder sb = new StringBuilder("000000");
            for (int i = 0; i < time.Length; i++)
            {
                sb[^(time.Length-i)] = time[i];
            }
            string time_formatted = sb.ToString();

            int hours = Mathf.Min(24, int.Parse(time_formatted[..2]));
            int minutes = Mathf.Min(60, int.Parse(time_formatted[2..4]));
            int seconds = Mathf.Min(60, int.Parse(time_formatted[4..6]));
            bombTimer.WaitTime = Mathf.Max(hours * 3600 + minutes*60 + seconds, 0.01f);
        }
    }

    private void Start(string text)
    {
        if (bombTimer.WaitTime < 3)
            return;
        lineEdit.ReleaseFocus();
        bombTimer.Start();
        startSounds[startSoundIndex].Play();
    }

    private void OnStop()
    {
        lineEdit.GrabFocus();

        time = "";
        beep.Stop();

        explosionWindow.Show();
        bombexpl.Play();
        animatedSprite.Play();
    }

    private string GetTimeLeft()
    {
        double t = bombTimer.TimeLeft > 0 ? bombTimer.TimeLeft : bombTimer.WaitTime;

        int hours = (int)t / 3600;
        int minutes = (int)(t / 60) % 60;
        int seconds = (int)t % 60;

        return $"{hours.ToString().PadLeft(2, '0')}:{minutes.ToString().PadLeft(2, '0')}:{seconds.ToString().PadLeft(2, '0')}";
    }

    bool pressedLastFrame;
    Vector2I offset;
    private void HandleWindowDrag()
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (!pressedLastFrame)
            {
                offset = DisplayServer.MouseGetPosition() - GetWindow().Position;
                pressedLastFrame = true;
            }

            GetWindow().Position = DisplayServer.MouseGetPosition() - offset;
        }
        else
            pressedLastFrame = false;
    }
}
