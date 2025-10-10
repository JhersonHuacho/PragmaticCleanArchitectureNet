using Bookify.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Configurations
{
	internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
	{
		public void Configure(EntityTypeBuilder<OutboxMessage> builder)
		{
			builder.ToTable("outbox_message");

			builder.HasKey(outboxMessage => outboxMessage.Id);

			builder.Property(outboxMessage => outboxMessage.Content).HasColumnType("jsonb");
		}
	}
}
