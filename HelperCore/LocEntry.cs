namespace HelperCore;

public class LocEntry
{
    public LocEntry(int identifierId, int type, string primaryName, string[] aliases)
    {
        IdentifierId = identifierId;
        Type = type;
        PrimaryName = primaryName;
        Aliases = aliases;
    }

    public int IdentifierId { get; }
    public int Type { get; }
    public string PrimaryName { get; }
    public string[] Aliases { get; }
}
