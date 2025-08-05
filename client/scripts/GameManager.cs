using System;
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
    public static GameManager Instance { get; private set; } = new();
    public State CurrentState => _machine.State;

    private readonly StateMachine<State, Triggers> _machine;
    // private readonly StateMachine<State, Triggers>.TriggerWithParameters<string> changedParameters;

    private GameManager()
    {
        _machine = new StateMachine<State, Triggers>(State.Starting);

        _machine.Configure(State.Starting)
            .Permit(Triggers.Connect, State.Connecting)
            .OnEntry(_OnStartingEntry);

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
            .Permit(Triggers.PlayerSetupComplete, State.Gameplay);

        _machine.Configure(State.Gameplay)
            .Permit(Triggers.Pause, State.Paused)
            .PermitReentry(Triggers.Resume);

        _machine.Configure(State.Paused)
            .Permit(Triggers.Resume, State.Gameplay)
            .Permit(Triggers.Exit, State.Exiting);

        _machine.Configure(State.Exiting)
            .OnEntry(_OnExitingEntry);

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

    }

    private void _OnConnectingEntry()
    {
        Error e = Networking.Instance.ConnectToServer();
        if (e == Error.Ok)
        {
            ConnectedToServer();
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
        if (GetTree().Root.GetNode("/Main") is Client c)
        {
            c.ShowLoadingScreen();
        }
        RpcId(1, Networking.MethodName.NumPlanets, -1);
        RpcId(1, Networking.MethodName.GetPlanets, 0, 0, new Array<float>());
    }

    private void _OnLoadingWorldExit()
    {

    }

    private void _OnPlayerSetupEntry()
    {

    }

    private void _OnPlayerSetupExit()
    {
        if (GetTree().Root.GetNode("/Main") is Client c)
        {
            c.RemoveLoadingScreen();
        }
    }

    private void _OnExitingEntry()
    {

    }
}
