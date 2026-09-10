namespace RaceDay.API.Services;

// Thin wrapper around BCrypt.Net-Next so password hashing/verification
// is called consistently everywhere and never stored in plain text.
public static class PasswordService
{
    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
