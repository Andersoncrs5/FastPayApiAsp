namespace App.Config.Exceptions.Classes;

public class DatabaseValidationException(string message)
    : Exception(message);