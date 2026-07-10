using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelTourManagement.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely add RejectionReason column if it doesn't exist
            migrationBuilder.Sql(@"
                ALTER TABLE travel_documents 
                ADD COLUMN IF NOT EXISTS rejection_reason character varying(500);
            ");

            // Safely add document_under_review to booking_status if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_enum WHERE enumlabel = 'document_under_review' AND enumtypid = 'booking_status'::regtype) THEN
                        ALTER TYPE booking_status ADD VALUE 'document_under_review';
                    END IF;
                END
                $$;
            ");

            // Safely add Completed to package_status if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_enum WHERE enumlabel = 'Completed' AND enumtypid = 'package_status'::regtype) THEN
                        ALTER TYPE package_status ADD VALUE 'Completed';
                    END IF;
                END
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
