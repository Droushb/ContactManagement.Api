using Microsoft.EntityFrameworkCore;
using ContactManagement.Entities;

namespace ContactManagement.Data;

public class ContactManagementDbContext : DbContext
{
    public ContactManagementDbContext(DbContextOptions<ContactManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<ContactCustomFieldValue> ContactCustomFieldValues => Set<ContactCustomFieldValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.HasMany(e => e.CustomFieldValues)
                .WithOne(e => e.Contact)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.HasMany(e => e.ContactValues)
                .WithOne(e => e.CustomField)
                .HasForeignKey(e => e.CustomFieldId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContactCustomFieldValue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ContactId, e.CustomFieldId }).IsUnique();
        });
    }
}
