namespace Mini.Engine.DirectX;

internal static class DebugNameGenerator
{
    public static string GetName(string user, string abbreviation, int? index = null)
    {
        var name = $"{user}_{abbreviation}";
        if (index != null)
        {
            name += $"[{index.Value}]";
        }

        return name;
    }

    public static string GetName(string user)
    {
        return user;
    }

    public static string GetName(string user, string abbreviation)
    {
        return GetName(user, abbreviation, null);
    }

    private static string GetName(string user, string abbreviation, string? contentType, int? index = null)
    {
        var name = GetName(user, abbreviation, index);
        if (contentType != null)
        {
            name += $"<{contentType}>";
        }

        return name;
    }

    public static string GetName<TContent>(string user, string abbreviation, int? index = null)
    {
        return GetName(user, abbreviation, typeof(TContent).Name, index);
    }

    public static string GetName(string user, string abbreviation, Enum? contentEnum = null, int? index = null)
    {
        var contentType = ToContentTypeString(contentEnum);
        return GetName(user, abbreviation, contentType, index);
    }


    public static string GetName(string user, string abbreviation, string? meaning = null, Enum? contentEnum = null, int? index = null)
    {
        var contentType = ToContentTypeString(contentEnum);
        if (!string.IsNullOrEmpty(meaning))
        {
            user += $"::{meaning}";
        }

        return GetName(user, abbreviation, contentType, index);
    }

    private static string? ToContentTypeString(Enum? contentEnum)
    {
        if (contentEnum != null)
        {
            return Enum.GetName(contentEnum.GetType(), contentEnum);
        }

        return null;
    }
}