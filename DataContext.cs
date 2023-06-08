using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public interface IDataContext
{
    DatabaseFacade Database { get; }
    DbSet<Claim> Claims { get; set; }
    DbSet<ClaimDebtor> ClaimsDebtors { get; set; }
    DbSet<Debtor> Debtors { get; set; }
    DbSet<DebtorNaturalPerson> DebtorNaturalPersons { get; set; }
    DbSet<DebtorLegalPerson> DebtorLegalPersons { get; set; }

    // AddRangeAsync
    Task AddRangeAsync(params object[] entities);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class DataContext : DbContext, IDataContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;

    public DbSet<Claim> Claims { get; set; } = null!;
    public DbSet<ClaimDebtor> ClaimsDebtors { get; set; } = null!;
    public DbSet<Debtor> Debtors { get; set; } = null!;
    public DbSet<DebtorNaturalPerson> DebtorNaturalPersons { get; set; }
    public DbSet<DebtorLegalPerson> DebtorLegalPersons { get; set; }

    public DataContext(
        DbContextOptions<DataContext> options,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory
    )
        : base(options)
    {
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
        // apply configuration from assembly
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    public Task AddRangeAsync(params object[] entities)
    {
        return base.AddRangeAsync(entities);
    }
}

internal class DbInit
{
    public async Task Initialize(IDataContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        if (context.Claims.Any())
        {
            return; // DB has been seeded
        }
        await CreateClaimAndDebtor(context);
        await CreateClaimAndLegalDebtor(context);
        await CreateClaimsAndDebtor(context);
    }

    private async Task CreateClaimAndDebtor(IDataContext context)
    {
        var claim = new Claim { ClaimId = Guid.NewGuid(), ClaimReferenceNumber = "1234567890" };

        var debtor = new DebtorNaturalPerson
        {
            DebtorId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            DateOfBirth = new DateTime(1980, 1, 1)
        };
        claim.Debtors.Add(
            new ClaimDebtor
            {
                Claim = claim,
                Debtor = debtor,
                ClaimDebtorType = ClaimDebtorType.Primary
            }
        );
        await context.AddRangeAsync(claim);
        await context.SaveChangesAsync();
    }

    private async Task CreateClaimAndLegalDebtor(IDataContext context)
    {
        var claim = new Claim { ClaimId = Guid.NewGuid(), ClaimReferenceNumber = "1234567890" };
        var debtorLegal = new DebtorLegalPerson
        {
            DebtorId = Guid.NewGuid(),
            Name = "John Doe Inc.",
            OrganizationNumber = "1234567890"
        };
        var debtorNatural = new DebtorNaturalPerson
        {
            DebtorId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1980, 1, 1)
        };
        claim.Debtors.Add(
            new ClaimDebtor
            {
                Claim = claim,
                Debtor = debtorLegal,
                ClaimDebtorType = ClaimDebtorType.Primary
            }
        );
        claim.Debtors.Add(
            new ClaimDebtor
            {
                Claim = claim,
                Debtor = debtorNatural,
                ClaimDebtorType = ClaimDebtorType.Secondary
            }
        );
        await context.AddRangeAsync(claim);
        await context.SaveChangesAsync();
    }

    private async Task CreateClaimsAndDebtor(IDataContext context)
    {
        var debtorNatural = new DebtorNaturalPerson
        {
            DebtorId = Guid.NewGuid(),
            FirstName = "Joe",
            LastName = "Schmoe",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        var claim1 = new Claim { ClaimId = Guid.NewGuid(), ClaimReferenceNumber = "987654" };

        debtorNatural.Claims.Add(
            new ClaimDebtor
            {
                Claim = claim1,
                Debtor = debtorNatural,
                ClaimDebtorType = ClaimDebtorType.Primary
            }
        );
        var claim2 = new Claim { ClaimId = Guid.NewGuid(), ClaimReferenceNumber = "ASBCD" };
        debtorNatural.Claims.Add(
            new ClaimDebtor
            {
                Claim = claim2,
                Debtor = debtorNatural,
                ClaimDebtorType = ClaimDebtorType.Primary
            }
        );

        await context.AddRangeAsync(debtorNatural);
        await context.SaveChangesAsync();
    }
}
