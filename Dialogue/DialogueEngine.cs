using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#region Command Classes

// Base class for all dialogue commands.
public abstract class DialogueCommand
{
}

// Represents an entire conversation, with a title and a list of commands.
public class ConversationCommand : DialogueCommand
{
    public string Title;
    public List<DialogueCommand> Commands = new List<DialogueCommand>();

    public override string ToString()
    {
        string result = $"Conversation: {Title}\n";
        foreach (var cmd in Commands)
            result += "  " + cmd.ToString() + "\n";
        return result;
    }
}

// Represents a block condition (a line that starts with <...>)
public class ConditionCommand : DialogueCommand
{
    public string Condition;

    public override string ToString()
    {
        return $"Condition: {Condition}";
    }
}

// Represents an emote command (a line that starts with (...))
public class EmoteCommand : DialogueCommand
{
    public string Emote;

    public override string ToString()
    {
        return $"Emote: {Emote}";
    }
}

// Represents a dialogue line.
public class DialogueLineCommand : DialogueCommand
{
    public string Text;
    // Inline conditions found within angle brackets that are part of the dialogue line.
    public List<string> InlineConditions = new List<string>();
    // Conditional tags (e.g. [Wizard]) that modify the dialogue option.
    public List<string> ConditionalTags = new List<string>();

    public override string ToString()
    {
        string result = $"Dialogue: {Text}";
        if (ConditionalTags.Count > 0)
            result += $" [Tags: {string.Join(", ", ConditionalTags)}]";
        if (InlineConditions.Count > 0)
            result += $" <Inline: {string.Join(", ", InlineConditions)}>";
        return result;
    }
}

#endregion

#region Parser

public static class DialogueParser
{
    /// <summary>
    /// Parses the custom dialogue text into a ConversationCommand object.
    /// </summary>
    public static ConversationCommand Parse(string input)
    {
        ConversationCommand conversation = null;
        // Split input into lines (preserving empty lines might be useful for breaks).
        string[] lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        
        // We'll use a simple state machine. In this example we assume one conversation block.
        foreach (var rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            // Conversation title or boundary (entire conversation is wrapped in {}).
            if (line.StartsWith("{") && line.EndsWith("}"))
            {
                // If no conversation exists, create one with the title.
                if (conversation == null)
                {
                    conversation = new ConversationCommand();
                    conversation.Title = line.Trim('{', '}');
                }
                else
                {
                    // In a full implementation, a new {} would signal the end of the current conversation
                    // and possibly the start of another.
                    // For simplicity, we assume one conversation here.
                }
            }
            // Block-level condition: a line with <...> on its own.
            else if (line.StartsWith("<") && line.EndsWith(">"))
            {
                var condition = line.Trim('<', '>');
                conversation?.Commands.Add(new ConditionCommand { Condition = condition });
            }
            // Emote command: a line starting with "(" and ending with ")".
            else if (line.StartsWith("(") && line.EndsWith(")"))
            {
                var emote = line.Trim('(', ')');
                conversation?.Commands.Add(new EmoteCommand { Emote = emote });
            }
            else
            {
                // Otherwise, treat it as a dialogue line.
                // Create a new dialogue line command.
                var dialogue = new DialogueLineCommand();

                // Extract any inline conditions (e.g. <Str 15>) that are within the line.
                // They may appear anywhere in the line.
                var inlineMatches = Regex.Matches(line, @"<([^>]+)>");
                foreach (Match match in inlineMatches)
                {
                    dialogue.InlineConditions.Add(match.Groups[1].Value);
                }
                // Remove inline conditions from the line text.
                line = Regex.Replace(line, @"<([^>]+)>", "").Trim();

                // Extract conditional tags from within square brackets (e.g. [conditional]).
                var tagMatches = Regex.Matches(line, @"\[(.*?)\]");
                foreach (Match match in tagMatches)
                {
                    dialogue.ConditionalTags.Add(match.Groups[1].Value);
                }
                // Remove those tags from the text.
                line = Regex.Replace(line, @"\[(.*?)\]", "").Trim();

                dialogue.Text = line;
                conversation?.Commands.Add(dialogue);
            }
        }
        return conversation;
    }
}

#endregion

#region Test Program

// This is a test harness for the parser. You can run this as a Console Application.
public class Program
{
    public static void Main()
    {
        string input = @"{conversation_01}
<happy>
okay here is my text.
	-well now you do it.
	-what? why?

<sad>
(smile) here's my next line.
	okay.
	[conditional] here's my condition.
	<Str 15> *try to punch the door down*";

        ConversationCommand conversation = DialogueParser.Parse(input);
        Console.WriteLine(conversation);
    }
}

#endregion
