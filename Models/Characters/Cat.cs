using W8_assignment_template.Interfaces;

namespace W8_assignment_template.Models.Characters;

public class Cat : CharacterBase, ILootable
{
    public string Treasure { get; set; }

    public Cat()
    {
    }

    public Cat(string name, string type, int level, int hp, string treasure, IRoom startingRoom) : base(name, type, level, hp, startingRoom)
    {
        Treasure = treasure;
    }

    public override void UniqueBehavior()
    {
        throw new NotImplementedException();
    }
}
