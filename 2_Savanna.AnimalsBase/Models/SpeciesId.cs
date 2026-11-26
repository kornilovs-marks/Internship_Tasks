namespace Savanna.AnimalsBase.Models;

// Identifies a species by name; replaces the closed AnimalKind enum so plugins can add their own
public readonly record struct SpeciesId(string Value)
{
    public override string ToString() => Value;
}
