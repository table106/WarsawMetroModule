using System.Text.RegularExpressions;

public partial class WarsawMetroModule
{
#pragma warning disable 414, IDE0051
    private readonly string TwitchHelpMessage = @"Use '!{0} board <top|bottom>' to board the top/bottom train | Use '!{0} leave' to leave the train";

    KMSelectable[] ProcessTwitchCommand(string command) {
        command = command.Trim().ToUpperInvariant();

        if (Match(command, @"^\s*board top\s*$")) {
            if (Stage2.activeSelf) throw new System.FormatException("Stage 2 is active.");
            return new KMSelectable[] { ButtonTop };
        }
        else if (Match(command, @"^\s*board bottom\s*$"))
        {
            if (Stage2.activeSelf) throw new System.FormatException("Stage 2 is active.");
            return new KMSelectable[] { ButtonBottom };
        }
        else if (Match(command, @"^\s*leave\s*$"))
        {
            if (Stage1.activeSelf) throw new System.FormatException("Stage 1 is active.");
            return new KMSelectable[] { LeaveTrainButton };
        }

        throw new System.FormatException("Invalid command.");
    }
#pragma warning restore IDE0051

    private bool Match(string command, string pattern) => Regex.Match(command, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Success;
}