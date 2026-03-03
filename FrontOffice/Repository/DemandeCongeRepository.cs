using System.Data;
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

    public async Task<(List<DemandeConge> Items, int Total)> GetByUserIdAsync(
        int userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        const string sql = @"
;WITH Base AS (
    SELECT IdDmd, UserId, DateDebut, DebutApresMidi,
           DateFin, FinApresMidi, Motif, NombreJour, decisionYear, Status
    FROM [DemandeConges]
    WHERE UserId = @UserId
)
SELECT *
FROM Base
ORDER BY DateDebut DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

SELECT COUNT(1)
FROM [DemandeConges]
WHERE UserId = @UserId;
";

        using var conn = (SqlConnection)_factory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@UserId", System.Data.SqlDbType.Int).Value = userId;
        cmd.Parameters.Add("@Offset", System.Data.SqlDbType.Int).Value = (page - 1) * pageSize;
        cmd.Parameters.Add("@PageSize", System.Data.SqlDbType.Int).Value = pageSize;

        var items = new List<DemandeConge>();

        using var reader = await cmd.ExecuteReaderAsync(ct);

        // 1) Page
        while (await reader.ReadAsync(ct))
        {
            items.Add(new DemandeConge
            {
                IdDmd = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                DateDebut = reader.GetDateTime(2),
                DebutApresMidi = reader.GetBoolean(3),
                DateFin = reader.GetDateTime(4),
                FinApresMidi = reader.GetBoolean(5),
                Motif = reader.IsDBNull(6) ? null : reader.GetString(6),
                NombreJour = reader.GetDecimal(7),
                decisionYear = reader.GetInt32(8),
                Status = (StatusEnum)reader.GetInt32(9),
            });
        }

        // 2) Total
        int total = 0;
        if (await reader.NextResultAsync(ct) && await reader.ReadAsync(ct))
            total = reader.GetInt32(0);

        return (items, total);
    }
    public async Task<DemandeConge> CreerDemandeAsync(
        DemandeConge demande,
        CancellationToken ct = default)
    {
        if (demande == null) throw new ArgumentNullException(nameof(demande));

        const string sql = @"
        INSERT INTO [DemandeConges]
        (
            UserId,
            DateDebut,
            DebutApresMidi,
            DateFin,
            FinApresMidi,
            Motif,
            NombreJour,
         decisionYear,
            Status
        )
        OUTPUT
            INSERTED.IdDmd,
            INSERTED.UserId,
            INSERTED.DateDebut,
            INSERTED.DebutApresMidi,
            INSERTED.DateFin,
            INSERTED.FinApresMidi,
            INSERTED.Motif,
            INSERTED.NombreJour,
            inserted.decisionYear,
            INSERTED.Status
        VALUES
        (
            @UserId,
            @DateDebut,
            @DebutApresMidi,
            @DateFin,
            @FinApresMidi,
            @Motif,
            @NombreJour,
         @DecisionYear,
            @Status
        );";

        using var conn = (SqlConnection)_factory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = demande.UserId;

        cmd.Parameters.Add("@DateDebut", SqlDbType.DateTime2).Value = demande.DateDebut;
        cmd.Parameters.Add("@DebutApresMidi", SqlDbType.Bit).Value = demande.DebutApresMidi;

        cmd.Parameters.Add("@DateFin", SqlDbType.DateTime2).Value = demande.DateFin;
        cmd.Parameters.Add("@FinApresMidi", SqlDbType.Bit).Value = demande.FinApresMidi;

        cmd.Parameters.Add("@Motif", SqlDbType.NVarChar, 500).Value =
            (object?)demande.Motif ?? DBNull.Value;

        var pNombreJour = cmd.Parameters.Add("@NombreJour", SqlDbType.Decimal);
        pNombreJour.Value = demande.NombreJour;
        pNombreJour.Precision = 10;
        pNombreJour.Scale = 2;

        cmd.Parameters.Add("@DecisionYear", SqlDbType.Int).Value = demande.decisionYear;

        cmd.Parameters.Add("@Status", SqlDbType.Int).Value = (int)demande.Status;

        // Petit plus : CommandBehavior.SingleRow + ExecuteReaderAsync
        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);

        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException("Insertion échouée: aucune ligne retournée.");

        return new DemandeConge
        {
            IdDmd = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            DateDebut = reader.GetDateTime(2),
            DebutApresMidi = reader.GetBoolean(3),
            DateFin = reader.GetDateTime(4),
            FinApresMidi = reader.GetBoolean(5),
            Motif = reader.IsDBNull(6) ? null : reader.GetString(6),
            NombreJour = reader.GetDecimal(7),
            decisionYear = reader.GetInt32(8),
            Status = (StatusEnum)reader.GetInt32(9)
        };
    }
    
    public async Task<decimal?> GetSoldeRestantAsync(int employeId, int decisionYear, CancellationToken ct = default)
    {
        const string sql = @"
        SELECT TOP (1) SoldeRestant
        FROM SoldeConges
        WHERE IdEmploye = @IdEmploye AND [Year] = @Year;";

        using var conn = (SqlConnection)_factory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@IdEmploye", SqlDbType.Int).Value = employeId;
        cmd.Parameters.Add("@Year", SqlDbType.Int).Value = decisionYear;

        var result = await cmd.ExecuteScalarAsync(ct);
        if (result == null || result == DBNull.Value) return null;

        return Convert.ToDecimal(result);
    }
    
    public async Task<List<DemandeConge>> GetAllByUserIdAsync(int userId, CancellationToken ct = default)
    {
        const string sql = @"
SELECT IdDmd, UserId, DateDebut, DebutApresMidi,
       DateFin, FinApresMidi, Motif, NombreJour, decisionYear, Status
FROM [DemandeConges]
WHERE UserId = @UserId
ORDER BY DateDebut DESC;";

        using var conn = (SqlConnection)_factory.CreateConnection();
        await conn.OpenAsync(ct);

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        var items = new List<DemandeConge>();

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new DemandeConge
            {
                IdDmd = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                DateDebut = reader.GetDateTime(2),
                DebutApresMidi = reader.GetBoolean(3),
                DateFin = reader.GetDateTime(4),
                FinApresMidi = reader.GetBoolean(5),
                Motif = reader.IsDBNull(6) ? null : reader.GetString(6),
                NombreJour = reader.GetDecimal(7),
                decisionYear = reader.GetInt32(8),
                Status = (StatusEnum)reader.GetInt32(9),
            });
        }

        return items;
    }
}