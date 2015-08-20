/// <summary>
/// The different categories of work that Elves can do.
/// </summary>
[System.Flags]
public enum Labors
{
	Dig = 1,
	Farm = 2,
	Water = 4,

	None = 0,
	All = Dig | Farm | Water,
}