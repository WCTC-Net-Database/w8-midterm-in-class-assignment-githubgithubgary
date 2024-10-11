using System;
using W8_assignment_template.Data;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Characters;

namespace W8_assignment_template.Services;

public class GameEngine
{
    private readonly IContext _context;
    private readonly MapManager _mapManager;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private readonly IRoomFactory _roomFactory;
    private ICharacter _player;
    private ICharacter _goblin;
    private ICharacter _ghoul;
    private ICharacter _cat;


    private List<IRoom> _rooms;

    public GameEngine(IContext context, IRoomFactory roomFactory, MenuManager menuManager, MapManager mapManager, OutputManager outputManager)
    {
        _roomFactory = roomFactory;
        _menuManager = menuManager;
        _mapManager = mapManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void AttackCharacter()
    {
        // TODO Update this method to allow for attacking a selected monster in the room.
        // TODO e.g. "Which monster would you like to attack?"
        // TODO Right now it just attacks the first monster in the room.
        // TODO It is ok to leave this functionality if there is only one monster in the room.
        //var target = _player.CurrentRoom.Characters.FirstOrDefault(c => c != _player);
        var targets = _player.CurrentRoom.Characters.FindAll(c => c != _player);
        if (targets != null)
        {
            if (targets.Count() == 1)
            {
                _player.Attack(targets[0]);  //this character will always be zero
                //_player.CurrentRoom.RemoveCharacter(targets[0]);
            }
            else
            {
                _outputManager.WriteLine("Select which character to attack!", ConsoleColor.Red);
                int cnt = 1;
                foreach (CharacterBase c in targets) {

                    _outputManager.WriteLine($"{cnt}. {c.Name} the {c.Type}", ConsoleColor.Green);
                    cnt = cnt + 1;
                }
                _outputManager.Write("Enter a number to attack? "); //Should give them an out but for now....
                _outputManager.Display();

                var input = Console.ReadLine();
                int indx = Int32.Parse(input) - 1; // I know that I should check to make sure that this would work normally but for now...
                _player.Attack(targets[indx]);
                //_player.CurrentRoom.RemoveCharacter(targets[indx]);
            }

        }
        else
        {
            _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
        }
    }

    private void GameLoop()
    {
        while (true)
        {
            _mapManager.DisplayMap();
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Move North");
            _outputManager.WriteLine("2. Move South");
            _outputManager.WriteLine("3. Move East");
            _outputManager.WriteLine("4. Move West");

            // Check if there are characters in the current room to attack
            if (_player.CurrentRoom.Characters.Any(c => c != _player))
            {
                _outputManager.WriteLine("5. Attack");
            }

            _outputManager.WriteLine("6. Exit Game");

            _outputManager.Display();

            var input = Console.ReadLine();

            string? direction = null;
            switch (input)
            {
                case "1":
                    direction = "north";
                    break;
                case "2":
                    direction = "south";
                    break;
                case "3":
                    direction = "east";
                    break;
                case "4":
                    direction = "west";
                    break;
                case "5":
                    if (_player.CurrentRoom.Characters.Any(c => c != _player))
                    {
                        AttackCharacter();
                    }
                    else
                    {
                        _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
                    }

                    break;
                case "6":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose a valid option.", ConsoleColor.Red);
                    break;
            }

            // Update map manager with the current room after movement
            if (!string.IsNullOrEmpty(direction))
            {
                _outputManager.Clear();
                _player.Move(direction);
                _mapManager.UpdateCurrentRoom(_player.CurrentRoom);
            }
        }
    }

    private void LoadMonsters()
    {
        _goblin = _context.Characters.OfType<Goblin>().FirstOrDefault();
        _cat = _context.Characters.OfType<Cat>().FirstOrDefault();
        _ghoul = _context.Characters.OfType<Ghoul>().FirstOrDefault();

        var random = new Random();
        var randomRoom = _rooms[random.Next(_rooms.Count)];
        randomRoom.AddCharacter(_goblin); // Use helper method

        // TODO Load your two new monsters here into the same room
        var randomRoom2 = _rooms[random.Next(_rooms.Count)];
        randomRoom2.AddCharacter(_cat);
        randomRoom2.AddCharacter(_ghoul);
    }

    private void SetupGame()
    {
        var startingRoom = SetupRooms();
        _mapManager.UpdateCurrentRoom(startingRoom);

        _player = _context.Characters.OfType<Player>().FirstOrDefault();
        _player.Move(startingRoom);
        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause for a second before starting the game loop
        Thread.Sleep(1000);
        GameLoop();
    }

    private IRoom SetupRooms()
    {
        // TODO Update this method to create more rooms and connect them together

        var entrance = _roomFactory.CreateRoom("entrance", _outputManager);
        var treasureRoom = _roomFactory.CreateRoom("treasure", _outputManager);
        var dungeonRoom = _roomFactory.CreateRoom("dungeon", _outputManager);
        var library = _roomFactory.CreateRoom("library", _outputManager);
        var armory = _roomFactory.CreateRoom("armory", _outputManager);
        var garden = _roomFactory.CreateRoom("garden", _outputManager);
        var study = _roomFactory.CreateRoom("study", _outputManager);
        var kitchen = _roomFactory.CreateRoom("kitchen", _outputManager);

        entrance.North = treasureRoom;
        entrance.West = library;
        entrance.East = garden;

        treasureRoom.South = entrance;
        treasureRoom.West = dungeonRoom;
        treasureRoom.East = study;

        study.West = treasureRoom;
        study.North = kitchen;
        kitchen.South = study;

        dungeonRoom.East = treasureRoom;


        library.East = entrance;
        library.South = armory;

        armory.North = library;

        garden.West = entrance;

        // Store rooms in a list for later use
        _rooms = new List<IRoom> { entrance, treasureRoom, dungeonRoom, library, armory, garden, study, kitchen };

        return entrance;
    }
}
