using Dapper;

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(Guid ClaimId);
    Task<Claim?> GetWithDebtorsByIdAsync(Guid ClaimId);
    Task<IList<Claim>> GetAllAsync();
}

public class ClaimRepository : IClaimRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public ClaimRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<Claim?> GetByIdAsync(Guid ClaimId)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        var result = await connection.QueryAsync<Claim>(
            "SELECT * FROM Claims WHERE ClaimId = @ClaimId",
            new { ClaimId }
        );
        return result.FirstOrDefault();
    }

    public async Task<Claim?> GetWithDebtorsByIdAsync(Guid ClaimId)
    {
        /*
        SELECT * FROM [Claims]
        INNER JOIN [ClaimsDebtors] ON [ClaimsDebtors].[ClaimId] = [Claims].[ClaimId]
        INNER JOIN [Debtors] ON [Debtors].[DebtorId] = [ClaimsDebtors].[DebtorId] AND [ClaimsDebtors].[ClaimDebtorType] = 1
        WHERE [Claims].[ClaimId] = @ClaimId
        */
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        var claim = await connection.QueryAsync<Claim, ClaimDebtor, Debtor, Claim>(
            "SELECT * FROM [Claims] "
                + "INNER JOIN [ClaimsDebtors] ON [ClaimsDebtors].[ClaimId] = [Claims].[ClaimId] "
                + "INNER JOIN [Debtors] ON [Debtors].[DebtorId] = [ClaimsDebtors].[DebtorId] "
                + "WHERE [Claims].[ClaimId] = @ClaimId",
            (claim, claimDebtor, debtor) =>
            {
                claim.Debtors.Add(claimDebtor);
                //claimDebtor.Debtor = debtor;
                return claim;
            },
            new { ClaimId },
            splitOn: "ClaimId,DebtorId"
        );
        var result = claim
            .GroupBy<Claim, Guid>(c => c.ClaimId)
            .Select(g =>
            {
                var groupedClaim = g.First();
                var debtors = g.Select(c => c.Debtors.Single()).ToList();
                groupedClaim.Debtors.AddRange(
                    debtors.Where(d => !groupedClaim.Debtors.Any(cd => cd.DebtorId == d.DebtorId))
                );
                return groupedClaim;
            });

        return result.FirstOrDefault();
    }

    public async Task<IList<Claim>> GetAllAsync()
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        var result = await connection.QueryAsync<Claim>("SELECT * FROM Claims");
        return result.ToList();
    }
}
