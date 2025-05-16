using kolos.DTOs;
using Microsoft.Data.SqlClient;

namespace kolos.Repositories;

public class VisitsRepository : IVisitsRepository
{
    private readonly string _connectionString;

    public VisitsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<VisitResponseDTO> GetVisitsAsync(int id, CancellationToken cancellationToken)
    {
        var visitResponseDto = new VisitResponseDTO();
        visitResponseDto.VisitServices = new List<VisitServiceDTO>();

        await using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);

            var query = @"SELECT 
                        [dbo].[Visit].[date],
                        [dbo].[Client].[first_name],
                        [dbo].[Client].[last_name],
                        [dbo].[Client].[date_of_birth],
                        [dbo].[Mechanic].[mechanic_id],
                        [dbo].[Mechanic].[licence_number],
                        [dbo].[Visit_Service].[service_fee],
                        [dbo].[Service].[name]
                        FROM [dbo].[Client] 
                        Inner Join [dbo].[Visit] On [dbo].[Client].[client_id] = [dbo].[Visit].[client_id]
                        Inner Join [dbo].[Mechanic] On [dbo].[Visit].[mechanic_id] = [dbo].[Mechanic].[mechanic_id]
                        Inner Join [dbo].[Visit_Service] On [dbo].[Visit_Service].[visit_id] = [dbo].[Visit].[visit_id]
                        Inner Join [dbo].[Service] On [Dbo].[Service].[service_id] = [dbo].[Visit_Service].[service_id]
                        WHERE [dbo].[Visit].[visit_id] = @id";

            await using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        visitResponseDto.client = new ClientDTO()
                        {
                            firstName = reader.GetString(reader.GetOrdinal("first_name")),
                            lastName = reader.GetString(reader.GetOrdinal("last_name")),
                            dateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                        };
                        visitResponseDto.mechanic = new MechanicDTO()
                        {
                            licenseNumber = reader.GetString(reader.GetOrdinal("licence_number")),
                            mechanicID = reader.GetInt32(reader.GetOrdinal("mechanic_id")),
                        };
                        var temp = new VisitServiceDTO()
                        {
                            serviceFee = reader.GetFloat(reader.GetOrdinal("service_fee")),
                            name = reader.GetString(reader.GetOrdinal("name")),
                        };
                        visitResponseDto.VisitServices.Add(temp);

                    }
                }
                
            }

        }
        return visitResponseDto;
    }
}
