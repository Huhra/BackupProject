
namespace Wpf.Backup.Models
{
    public class ConfigurationTest
    {
        public bool Success { get; }
        public string OkMessage { get; }
        public string ErrorMessage { get; }

        public ConfigurationTest(bool success, string okMessage, string errorMessage)
        {
            Success = success;
            OkMessage = okMessage;
            ErrorMessage = errorMessage;
        }

        public ConfigurationTest(string errorMessage)
        {
            Success = false;
            OkMessage = string.Empty;
            ErrorMessage = errorMessage;
        }
    }
}
