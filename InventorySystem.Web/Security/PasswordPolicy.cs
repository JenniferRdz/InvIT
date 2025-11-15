using System.Text.RegularExpressions;

namespace InventorySystem.Web.Security
{
    public static class PasswordPolicy
    {
        public static bool IsStrong(string password, out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(password) || password.Length < 10) { error = "Mínimo 10 caracteres."; return false; }
            if (!Regex.IsMatch(password, "[A-Z]")) { error = "Falta una mayúscula."; return false; }
            if (!Regex.IsMatch(password, "[a-z]")) { error = "Falta una minúscula."; return false; }
            if (!Regex.IsMatch(password, "[0-9]")) { error = "Falta un número."; return false; }
            if (!Regex.IsMatch(password, @"[\W_]")) { error = "Falta un símbolo."; return false; }
            return true;
        }
    }
}
