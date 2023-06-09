﻿namespace Furesoft.PrattParser.Text;

public sealed class Message
{
    public Message(MessageSeverity severity, string text, SourceRange range)
    {
        Severity = severity;
        Text = text;
        Range = range;
        Document = range.Document;
    }

    public SourceRange Range { get; }

    public SourceDocument Document { get; }

    public MessageSeverity Severity { get; }
    public string Text { get; }

    public static Message Error(string message, SourceRange range)
    {
        return new(MessageSeverity.Error, message, range);
    }

    public static Message Error(string message)
    {
        return new(MessageSeverity.Error, message, SourceRange.Empty);
    }

    public static Message Info(string message, SourceRange range)
    {
        return new(MessageSeverity.Info, message, range);
    }

    public static Message Warning(string message, SourceRange range)
    {
        return new(MessageSeverity.Warning, message, range);
    }

    public override string ToString()
    {
        if (Document == null) return Text;

        return $"{Document.Filename}:{Range.Start.Line}:{Range.Start.Column} {Text}";
    }
}
