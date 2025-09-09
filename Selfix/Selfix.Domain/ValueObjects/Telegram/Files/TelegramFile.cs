using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Telegram.Files;

public sealed class TelegramFile
{
    private const uint MIN_FILE_ID_LENGTH = 4;
    private const uint MAX_FILE_ID_LENGTH = 128;
    private const uint MIN_EXT_LENGTH = 2;
    private const uint MAX_EXT_LENGTH = 4;

    private readonly LimitedString _identifier;
    private readonly LimitedString _extension;
    
    public string Identifier => _identifier;
    public string Extension => _extension;

    private TelegramFile(LimitedString identifier, LimitedString extension)
    {
        _identifier = identifier;
        _extension = extension;
    }

    public static Fin<TelegramFile> From(string identifier, string extension)
    {
        return
            from valueLimited in LimitedString.From(identifier, MIN_FILE_ID_LENGTH, MAX_FILE_ID_LENGTH)
            from extensionLimited in LimitedString.From(extension, MIN_EXT_LENGTH, MAX_EXT_LENGTH)
            select new TelegramFile(valueLimited, extensionLimited);
    }

    public static Fin<TelegramFile> From(string identifierAndExtension)
    {
        var parts = identifierAndExtension.Split('.');
        if (parts.Length != 2) return Error.New("Invalid telegram file id format");
        
        return
            from valueLimited in LimitedString.From(parts[0], MIN_FILE_ID_LENGTH, MAX_FILE_ID_LENGTH)
            from extensionLimited in LimitedString.From(parts[1], MIN_EXT_LENGTH, MAX_EXT_LENGTH)
            select new TelegramFile(valueLimited, extensionLimited);
    }

    public static implicit operator string(TelegramFile telegramFile)
    {
        return $"{(string)telegramFile._identifier}.{(string)telegramFile._extension}";
    }

    public override string ToString()
    {
        return this;
    }
}