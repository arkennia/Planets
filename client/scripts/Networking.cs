using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Planets;
using Planets.SystemGenerator;
using Planets.SystemGenerator.Terrain;

public partial class Networking : Node
{
    public static Player Player { get; set; }
    public static bool Connected { get; private set; } = false;
    public static Networking Instance { get; private set; }

    public static Guid Uuid { get; private set; }
    private Dictionary<long, PlayerPeer> _players = [];
    // private Dictionary<string, string> _playerInfo = new()
    // {
    //     // {"Name", "PlayerName"}
    // };

    private int _numPlanets = 0;

    [Signal]
    public delegate void PlayerConnectedEventHandler(int peerId, Dictionary<string, string> playerInfo);
    [Signal]
    public delegate void PlayerDisconnectedEventHandler(int peerId);
    [Signal]
    public delegate void ServerDisconnectedEventHandler();
    [Signal]
    public delegate void PlanetLoadedEventHandler(PlanetNode planet);
    [Signal]
    public delegate void SyncingFinishedEventHandler();

    private int _numConnections = 0;

    // private bool _isSyncing = false;

    private Networking()
    {
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance ??= this;
        if (Multiplayer is SceneMultiplayer mp)
        {
            mp.AllowObjectDecoding = true;
        }
        GD.Print("Client Multiplayer instance ID: " + Multiplayer.GetInstanceId());
        using FileAccess file = FileAccess.Open("user://uuid", FileAccess.ModeFlags.Read);
        string uuidString = file.GetLine().Trim();
        Uuid = Guid.Parse(uuidString);
    }

    public Error ConnectToServer()
    {
        ENetMultiplayerPeer peer = new();
        Error e = peer.CreateClient("127.0.0.1", 7000);

        PlanetLoaded += (planet) => GD.Print("Planet loaded! " + planet.ToString());
        // SyncingFinished += GameManager.Instance.WorldLoaded;
        Multiplayer.MultiplayerPeer = peer;
        Multiplayer.PeerConnected += OnPlayerConnected;
        Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectOk;
        Multiplayer.ConnectionFailed += OnConnectionFail;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
        return e;
    }

    public void BeginPlanetSync()
    {
        Error e = RpcId(1, MethodName.NumPlanets, -1);
        if (e != Error.Ok)
            GD.Print($"Error getting number of planets: {e}");
        e = RpcId(1, MethodName.GetPlanets, 0, 0, new Array<float>());
        if (e != Error.Ok)
            GD.Print($"Error getting planets: {e}");
    }

    public void BeginPlayerDataSync()
    {
        Error e = RpcId(1, MethodName.SendPlayerData, null);
        GD.Print("Requesting player data. Code: " + e.ToString());
    }

    public void RequestPlayerSpawn()
    {
        Error e = RpcId(1, MethodName.RequestSpawn);
        GD.Print("Requesting player spawn. Code: " + e.ToString());
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void PlayerLoaded()
    {

    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer(long id)
    {
        if (id != Multiplayer.GetUniqueId())
        {
            PlayerPeer p = new(id);
            _players[id] = p;
            GD.Print($"Connected Players: {_players}");
            RpcId(1, MethodName.SendPeerData, id, new byte[1]);
            EmitSignal(SignalName.PlayerConnected, id);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendPeerData(long id, byte[] peerBytes)
    {
        GD.Print("SendPeerData called.");
        _players[id].PlayerData = new(PlayerDataProto.Parser.ParseFrom(peerBytes));
        GetTree().CurrentScene.GetNode<Node>("Game").AddChild(_players[id]);
        _players[id].Spawn();
        GD.Print($"Peer data received for {id}. Added to tree and calling spawn.");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void GetPlanets(byte[] id, int seed, Array<float> heights)
    {
        GD.Print("Method called by: " + Multiplayer.GetRemoteSenderId());
        if (Multiplayer.GetRemoteSenderId() != 1) return;
        // GD.Print("Data Received: " + planetBytes.Length + " ");
        if (heights.Count > 0)
        {
            // Guid uid = Guid.Parse(id);
            GD.Print(heights[0..10]);
            GD.Print("Seed: " + seed);
            Guid guid = new Guid(id);
            GD.Print("Received GUID: " + guid.ToString());
            PlanetNode planet = PlanetGenerator.GeneratePlanet(scale: 250, heights: heights, seed: seed, guid: guid);
            GetTree().Root.GetNode<Node>("/root/Main/Game/World").AddChild(planet);
            // Planets[planet.Planet.Guid.ToString()] = planet;
            GameManager.AddPlanet(planet);
            EmitSignal(SignalName.PlanetLoaded, planet);
        }
        else
        {
            EmitSignal(SignalName.SyncingFinished);
            GD.Print($"Number of Planets received:  {GameManager.Planets.Count}");
            GameManager.Instance.WorldLoaded();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void NumPlanets(int numPlanets)
    {
        if (Multiplayer.GetRemoteSenderId() != 1) return;
        _numPlanets = numPlanets;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendPlayerData(byte[] playerData)
    {

        GetTree().CurrentScene.GetNode<Player>("Game/Player").PlayerData
            = new(PlayerDataProto.Parser.ParseFrom(playerData));
        GD.Print("PlayerData received");




    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestMovement(byte[] movementBytes)
    {

    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendMovement(long id, byte[] movementBytes)
    {
        if (Multiplayer.GetRemoteSenderId() == MultiplayerPeer.TargetPeerServer)
        {
            PlayerMovement movement = new(PlayerMovementProto.Parser.ParseFrom(movementBytes));
            if (id == Multiplayer.GetUniqueId())
            {
                //float distance = Mathf.Abs(movement.CurrentGlobalPosition.DistanceTo(Player.GlobalPosition));
                Player.GlobalPosition = movement.CurrentGlobalPosition;
            }
            else if (_players.TryGetValue(id, out PlayerPeer p))
            {
                p.GlobalPosition = movement.CurrentGlobalPosition;
                //p.Velocity = movement.Velocity;
                p.Rotation = movement.Rotation;
                // p.UpDirection = movement.Up;
                // p.GlobalPosition = movement.CurrentGlobalPosition;
            }
            else
            {
                GD.PushError($"Error: SendMovement : {id} not found in connected players.\n{System.Environment.StackTrace}");
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendUUID(byte[] uuidBytes)
    {
        GD.Print("Sending UUID to server: " + Uuid.ToString());
        RpcId(1, MethodName.SendUUID, Uuid.ToByteArray());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RequestSpawn()
    {
        Player.Spawn();
    }

    private void OnPlayerDisconnected(long id)
    {
        _players.Remove(id);
        EmitSignal(SignalName.PlayerDisconnected, id);
    }

    private void OnPlayerConnected(long id)
    {
        //EmitSignal(SignalName.PlayerConnected);
    }

    private void OnConnectOk()
    {
        Connected = true;
        int peerId = Multiplayer.GetUniqueId();
        GD.Print("My ID: " + peerId);
        GD.Print(OS.GetUserDataDir());
        //_players[peerId] = Player;
        RpcId(1, MethodName.SendUUID, Uuid.ToByteArray());
        RpcId(1, MethodName.GetPlanets, peerId, 0, new());
        EmitSignal(SignalName.PlayerConnected, peerId);
    }

    private void OnConnectionFail()
    {
        Multiplayer.MultiplayerPeer = null;
    }

    private void OnServerDisconnected()
    {
        Connected = false;
        Multiplayer.MultiplayerPeer = null;
        _players.Clear();
        EmitSignal(SignalName.ServerDisconnected);
    }
}
