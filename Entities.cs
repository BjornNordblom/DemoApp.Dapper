using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class Claim
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid ClaimId { get; set; }
    public string ClaimReferenceNumber { get; set; } = null!;
    public List<ClaimDebtor> Debtors { get; } = new();
}

public enum ClaimDebtorType
{
    Primary = 1,
    Secondary = 2
}

public class ClaimDebtor
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid DebtorId { get; set; }
    public Debtor Debtor { get; set; } = null!;
    public ClaimDebtorType ClaimDebtorType { get; set; }
}

public class ClaimDebtorConfiguration : IEntityTypeConfiguration<ClaimDebtor>
{
    public void Configure(EntityTypeBuilder<ClaimDebtor> builder)
    {
        builder.ToTable("ClaimsDebtors");
        builder.HasKey(cd => new { cd.ClaimId, cd.DebtorId });
    }
}

public class Debtor
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid DebtorId { get; set; }
    public List<ClaimDebtor> Claims { get; } = new();
}

public class DebtorNaturalPerson : Debtor
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}

public class DebtorNaturalPersonConfiguration : IEntityTypeConfiguration<DebtorNaturalPerson>
{
    public void Configure(EntityTypeBuilder<DebtorNaturalPerson> builder)
    {
        builder.ToTable("DebtorNaturalPersons");
    }
}

public class DebtorLegalPerson : Debtor
{
    public string Name { get; set; } = null!;
    public string OrganizationNumber { get; set; } = null!;
}

public class DebtorLegalPersonConfiguration : IEntityTypeConfiguration<DebtorLegalPerson>
{
    public void Configure(EntityTypeBuilder<DebtorLegalPerson> builder)
    {
        builder.ToTable("DebtorLegalPersons");
    }
}
