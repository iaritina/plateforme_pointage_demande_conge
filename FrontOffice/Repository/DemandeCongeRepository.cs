using FrontOffice.Data;
using Microsoft.Data.SqlClient;
using Shared.models;

namespace FrontOffice.Repository;

public class DemandeCongeRepository
{
    private readonly DbConnectionFactory _factory;

    public DemandeCongeRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }
    
    public async Task<List<DemandeConge>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        const string sql = @"
        SELECT IdDmd, UserId, DateDebut, DebutApresMidi,
               DateFin, FinApresMidi, Motif, NombreJour, Status
        FROM DemandeConge
        WHERE UserId = @UserId
        ORDER BY DateDebut DESC;";

        using var conn = (SqlConnection)_factory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        var result = new List<DemandeConge>();

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new DemandeConge
            {
                IdDmd = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                DateDebut = reader.GetDateTime(2),
                DebutApresMidi = reader.GetBoolean(3),
                DateFin = reader.GetDateTime(4),
                FinApresMidi = reader.GetBoolean(5),
                Motif = reader.IsDBNull(6) ? null : reader.GetString(6),
                NombreJour = reader.GetDecimal(7),
                Status = (StatusEnum)reader.GetInt32(8)
            });
        }

        return result;
    }
}