using App.Config.Exceptions.Classes;
using Npgsql;

namespace App.Config.Database;

public static class PostgresExceptionHandler
{
    public static Exception Handle(PostgresException exception)
    {
        return exception.SqlState switch
        {
            // unique violation
            "23505" => new DatabaseConflictException(
                exception.ConstraintName ?? "unique_constraint"),

            // not null violation
            "23502" => new DatabaseValidationException(
                exception.ColumnName ?? "unknown_column"),

            // foreign key violation
            "23503" => new DatabaseConflictException(
                exception.ConstraintName ?? "foreign_key"),

            // check violation
            "23514" => new DatabaseValidationException(
                exception.ConstraintName ?? "check_constraint"),

            _ => exception
        };
    }
}