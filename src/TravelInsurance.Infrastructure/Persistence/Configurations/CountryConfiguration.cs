using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelInsurance.Domain.Entities;

namespace TravelInsurance.Infrastructure.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> entity)
    {
        entity.ToTable("countries");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(2).IsRequired();
        entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        entity.HasIndex(x => x.Code).IsUnique();
        entity.HasIndex(x => x.Name).IsUnique();

        entity.HasData(Seed());
    }

    private static IEnumerable<Country> Seed()
    {
        (string Code, string Name)[] data =
        [
            ("IQ", "Iraq"), ("US", "United States"), ("GB", "United Kingdom"), ("AE", "United Arab Emirates"),
            ("SA", "Saudi Arabia"), ("TR", "Turkey"), ("EG", "Egypt"), ("FR", "France"),
            ("DE", "Germany"), ("IT", "Italy"), ("ES", "Spain"), ("NL", "Netherlands"),
            ("CH", "Switzerland"), ("SE", "Sweden"), ("NO", "Norway"), ("GR", "Greece"),
            ("AT", "Austria"), ("BE", "Belgium"), ("PT", "Portugal"), ("IE", "Ireland"),
            ("CA", "Canada"), ("AU", "Australia"), ("NZ", "New Zealand"), ("JP", "Japan"),
            ("CN", "China"), ("IN", "India"), ("RU", "Russia"), ("BR", "Brazil"),
            ("ZA", "South Africa"), ("MY", "Malaysia"), ("TH", "Thailand"), ("SG", "Singapore"),
            ("ID", "Indonesia"), ("PH", "Philippines"), ("KR", "South Korea"), ("QA", "Qatar"),
            ("KW", "Kuwait"), ("BH", "Bahrain"), ("OM", "Oman"), ("JO", "Jordan"),
            ("LB", "Lebanon"), ("MA", "Morocco"), ("TN", "Tunisia"), ("PK", "Pakistan"),
            ("DK", "Denmark"),
        ];

        for (var i = 0; i < data.Length; i++)
        {
            yield return new Country
            {
                Id = new Guid($"c0000000-0000-0000-0000-{(i + 1):D12}"),
                Code = data[i].Code,
                Name = data[i].Name
            };
        }
    }
}
