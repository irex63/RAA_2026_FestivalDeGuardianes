public static class PlayerPrefsNames
{
    // Clave con la que guardamos la preferencia (0 = audio, 1 = texto)
    public const string dialogueKey = "myDialoguePreference";
    public const int audio = 0;
    public const int text = 1;

    // Claves para saber si se jugo un nivel

    // 0 = Sin jugar, 1= Jugado
    public const string easter = "easterPlayed";
    public const string hallowen = "halloweenPlayed";
    public const int played = 1;
    public const int noPlayed = 0;
}