using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Postgres.Sockets.Database.Migrations
{
    /// <inheritdoc />
    public partial class trigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //create trigger function
            migrationBuilder.Sql(@"
            CREATE FUNCTION public.notify_entity_changes() RETURNS trigger
                LANGUAGE plpgsql
                AS $$
            DECLARE 
                tr_record RECORD;
                trigger_data json;
                notification_data json;
            BEGIN    
                IF (TG_OP = 'DELETE') THEN
                    tr_record = OLD;        
                ELSE
                    tr_record = NEW;        
                END IF;
                
                trigger_data = row_to_json(r)
                    FROM (
                        SELECT 
                            n.*
                        FROM
                            (SELECT tr_record.*) n                            
                    ) r;
                
                notification_data = json_build_object(
                    'table', TG_TABLE_NAME,
                    'operation', TG_OP,
                    'data', trigger_data
	              );

                PERFORM pg_notify(
                    'test_entity_data_changed',
                    notification_data::text
                );

              RETURN tr_record;
            END;
            $$;
            ");
            
            //assign the trigger function to DML operations applied on the testEntity table
            migrationBuilder.Sql(@"
            CREATE TRIGGER 
                tr_testEntity_notifications 
            AFTER INSERT OR DELETE OR UPDATE ON public.""testEntity""
                FOR EACH ROW EXECUTE FUNCTION public.notify_entity_changes();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER tr_notifications ON public.""testEntity"";");
            migrationBuilder.Sql(@"DROP FUNCTION public.notify_entity_changes();");
        }
    }
}
