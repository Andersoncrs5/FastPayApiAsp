using App.Utils.Result;
using Npgsql;

namespace App.Config.Database;

public static class PostgresResultHandler
{
    public static Result<T> Handle<T>(PostgresException exception)
    {
        return exception.SqlState switch
        {
            "23505" => Result<T>.Failure(
                exception.ConstraintName ?? "unique_constraint",
                409),

            "23502" => Result<T>.Failure(
                exception.ColumnName ?? "unknown_column",
                400),

            "23503" => Result<T>.Failure(
                exception.ConstraintName ?? "foreign_key",
                409),

            "23514" => Result<T>.Failure(
                exception.ConstraintName ?? "check_constraint",
                400),
            
            "40001" => Result<T>.Failure(
                "Transaction conflict, please retry",
                409),

            "40P01" => Result<T>.Failure(
                "Deadlock detected, please retry",
                409),
            
            "42501" => Result<T>.Failure(
                "Insufficient database privileges",
                403),
            
            "22P02" => Result<T>.Failure(
                "Invalid data format",
                400),

            "22001" => Result<T>.Failure(
                "Value too long",
                400),

            "22003" => Result<T>.Failure(
                "Numeric value out of range",
                400),

            _ => throw exception
        };
    }
}