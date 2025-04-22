using Microsoft.Data.Sqlite;

public static class SqliteHelper
{
    const string DATABASE_FOLDER = "Database";
    const string DATABASE_PATH = $"{DATABASE_FOLDER}/game.db";
    const string CONNECTION_STRING = $"Data Source={DATABASE_PATH}";

    public static void Initialize()
    {
        bool dbExists = File.Exists(DATABASE_PATH);
        if (!dbExists)
        {
            Console.WriteLine("🆕 DB 파일이 없어서 새로 생성 예정");
        }

        using var connection = new SqliteConnection(CONNECTION_STRING);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY,
            UserName TEXT NOT NULL
        );
        """;

        command.ExecuteNonQuery();
    }

    public static bool IsExistUser(int userId)
    {
        using var connection = new SqliteConnection(CONNECTION_STRING);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE Id = $id";
        command.Parameters.AddWithValue("$id", userId);

        var count = (long)command.ExecuteScalar();
        return count > 0;
    }

    public static void InsertUser(int userId, string userName)
    {
        using var connection = new SqliteConnection(CONNECTION_STRING);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (Id, UserName) VALUES ($id, $name)";
        command.Parameters.AddWithValue("$id", userId);
        command.Parameters.AddWithValue("$name", userName);

        command.ExecuteNonQuery();
    }
}