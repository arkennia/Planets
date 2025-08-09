using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Stateless;

namespace Planets;

public partial class GameManager : Node
{
    public enum State
    {
        Starting,
        Connecting,
        LoadingWorld,
        PlayerSetup,
        Gameplay,
        Paused,
        Exiting,
    }

    public enum Triggers
    {
        Start,
        Connect,
        Connected,
        ConnectionFailed,
        WorldLoaded,
        PlayerSetupComplete,
        Gameplay,
        Pause,
        Resume,
        Exit,
    }
    public static GameManager Instance { get; private set; }

    public State CurrentState => _machine.State;

    private readonly StateMachine<State, Triggers> _machine;
    // private readonly StateMachine<State, Triggers>.TriggerWithParameters<string> changedParameters;

    private GameManager()
    {
        Instance ??= this;
        _machine = new StateMachine<State, Triggers>(State.Starting);

        // _machine.Configure(State.Starting)
        //     .Permit(Triggers.Connect, State.Connecting)
        //     .OnEntry(_OnStartingEntry)
        //     .OnExit(_OnStartingExit);

        // _machine.Configure(State.Connecting)
        //     .PermitReentry(Triggers.ConnectionFailed)
        //     .Permit(Triggers.Connected, State.LoadingWorld)
        //     .OnEntry(_OnConnectingEntry)
        //     .OnExit(_OnConnectingExit);

        // _machine.Configure(State.LoadingWorld)
        //     .Permit(Triggers.WorldLoaded, State.PlayerSetup)
        //     .OnEntry(_OnLoadingWorldEntry)
        //     .OnExit(_OnLoadingWorldExit);

        // _machine.Configure(State.PlayerSetup)
        //     .Permit(Triggers.PlayerSetupComplete, State.Gameplay);

        // _machine.Configure(State.Gameplay)
        //     .Permit(Triggers.Pause, State.Paused)
        //     .PermitReentry(Triggers.Resume);

        // _machine.Configure(State.Paused)
        //     .Permit(Triggers.Resume, State.Gameplay)
        //     .Permit(Triggers.Exit, State.Exiting);

        // _machine.Configure(State.Exiting)
        //     .OnEntry(_OnExitingEntry);

        // GD.Print("Current state in constructor: " + _machine.State);

    }

    public override void _Ready()
    {

        _machine.Configure(State.Starting)
            .Permit(Triggers.Connect, State.Connecting)
            .OnEntry(_OnStartingEntry)
            .OnExit(_OnStartingExit);

        _machine.Configure(State.Connecting)
            .PermitReentry(Triggers.ConnectionFailed)
            .Permit(Triggers.Connected, State.LoadingWorld)
            .OnEntry(_OnConnectingEntry)
            .OnExit(_OnConnectingExit);

        _machine.Configure(State.LoadingWorld)
            .Permit(Triggers.WorldLoaded, State.PlayerSetup)
            .OnEntry(_OnLoadingWorldEntry)
            .OnExit(_OnLoadingWorldExit);

        _machine.Configure(State.PlayerSetup)
            .Permit(Triggers.PlayerSetupComplete, State.Gameplay)
            .OnEntry(_OnPlayerSetupEntry)
            .OnExit(_OnPlayerSetupExit);

        _machine.Configure(State.Gameplay)
            .Permit(Triggers.Pause, State.Paused)
            .PermitReentry(Triggers.Resume);

        _machine.Configure(State.Paused)
            .Permit(Triggers.Resume, State.Gameplay)
            .Permit(Triggers.Exit, State.Exiting);

        _machine.Configure(State.Exiting)
            .OnEntry(_OnExitingEntry);

        GD.Print("Current state in constructor: " + _machine.State);
    }


    public void ConnectToServer() => _machine.Fire(Triggers.Connect);
    public void ConnectionFailed() => _machine.Fire(Triggers.ConnectionFailed);
    public void ConnectedToServer() => _machine.Fire(Triggers.Connected);
    public void WorldLoaded() => _machine.Fire(Triggers.WorldLoaded);
    public void PlayerSetupComplete() => _machine.Fire(Triggers.PlayerSetupComplete);
    public void GameplayStarted() => _machine.Fire(Triggers.Gameplay);
    public void Paused() => _machine.Fire(Triggers.Pause);
    public void Resumed() => _machine.Fire(Triggers.Resume);
    public void ExitGame() => _machine.Fire(Triggers.Exit);

    private void _OnStartingEntry()
    {
        GD.Print("Start State entered.");
    }
    private void _OnStartingExit()
    {
        if (GetTree().CurrentScene is Client c)
        {
            c.RemoveMainMenu();
        }
    }

    private void _OnConnectingEntry()
    {
        GD.Print("Connecting State entered.");
        Error e = Networking.Instance.CallDeferred(Networking.MethodName.ConnectToServer).As<Error>();
        if (e == Error.Ok)
        {
            ConnectedToServer();
            GD.Print("Connected to server!");
        }
        else
        {
            GD.Print("Failed to connect to server!: " + e.ToString());
            ConnectionFailed();
        }
    }

    private void _OnConnectingExit()
    {

    }

    private void _OnLoadingWorldEntry()
    {
        if (GetTree().CurrentScene is Client c)
        {
            c.ShowLoadingScreen();
        }
        // Networking.Instance.CallDeferred(Networking.MethodName.BeginPlanetSync);
    }

    private void _OnLoadingWorldExit()
    {
        if (GetTree().CurrentScene is Client c)
        {
            c.RemoveLoadingScreen();
            c.LoadGameUi();
        }
    }

    private void _OnPlayerSetupEntry()
    {
        if (GetTree().CurrentScene.GetNode<Game>("%Game") is Game g)
        {
            g.SetupPlayer();
            Networking.Instance.BeginPlayerDataSync();
        }
        else
        {
            GD.Print("Failed to call SetupPlayer");
        }
    }

    private void _OnPlayerSetupExit()
    {
        // if (GetTree().Root.GetNode("/Main") is Client c)
        // {
        //     c.RemoveLoadingScreen();
        // }
        Networking.Instance.RequestPlayerSpawn();
    }

    private void _OnExitingEntry()
    {

    }
}
