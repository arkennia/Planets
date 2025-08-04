using System;
using Godot;
using Stateless;

namespace Planets;

public partial class GameManager : RefCounted
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
        Enter,
        Connected,
        ConnectionFailed,
        WorldLoaded,
        PlayerSetupComplete,
        Gameplay,
        Pause,
        Resume,
        Exit,
    }

    private readonly StateMachine<State, Triggers> _machine;
    private readonly StateMachine<State, Triggers>.TriggerWithParameters<string> changedParameters;

    public GameManager()
    {
        _machine = new StateMachine<State, Triggers>(State.Starting);

        _machine.Configure(State.Starting)
            .Permit(Triggers.Enter, State.Connecting);

        _machine.Configure(State.Connecting)
            .PermitReentry(Triggers.ConnectionFailed)
            .Permit(Triggers.Connected, State.LoadingWorld);

        _machine.Configure(State.LoadingWorld)
            .Permit(Triggers.WorldLoaded, State.PlayerSetup);

        _machine.Configure(State.PlayerSetup)
            .Permit(Triggers.PlayerSetupComplete, State.Gameplay);

        _machine.Configure(State.Gameplay)
            .Permit(Triggers.Pause, State.Paused)
            .PermitReentry(Triggers.Resume);

        _machine.Configure(State.Paused)
            .Permit(Triggers.Resume, State.Gameplay)
            .Permit(Triggers.Exit, State.Exiting);
    }
}
