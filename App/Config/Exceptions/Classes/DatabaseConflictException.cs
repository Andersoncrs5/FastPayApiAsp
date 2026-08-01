namespace App.Config.Exceptions.Classes;


public class DatabaseConflictException(string message)
    : Exception(message);