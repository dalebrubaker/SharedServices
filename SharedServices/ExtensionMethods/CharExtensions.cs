namespace BruSoftware.SharedServices.ExtensionMethods;

public static class CharExtensions
{
    public static bool IsVowel(this char ch)
    {
        switch (ch)
        {
            case 'A':
            case 'a':
            case 'E':
            case 'e':
            case 'I':
            case 'i':
            case 'O':
            case 'o':
            case 'U':
            case 'u':
                return true;
            default:
                return false;
        }
    }
}