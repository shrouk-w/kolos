using kolos.DTOs;
using kolos.Exceptions;
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

    public async Task<int> CreateNewServiceAsync(AddServiceToDB_DTO serviceToDbDto, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            //--------- updateing fulfill date
            var query1 = @"INSERT INTO [dbo].[Visit] (visit_id,client_id, mechanic_id,date)
                            VALUES (@visit_id, @client_id, @mechanic_id, @date);
                            SELECT SCOPE_IDENTITY();";
            
            await using var checkCommand = new SqlCommand(query1, connection, (SqlTransaction)transaction);
            checkCommand.Parameters.AddWithValue("@date", serviceToDbDto.date);
            checkCommand.Parameters.AddWithValue("@visit_id", serviceToDbDto.visitId);
            checkCommand.Parameters.AddWithValue("@client_id", serviceToDbDto.clientId);
            checkCommand.Parameters.AddWithValue("@mechanic_id", serviceToDbDto.mechnicId);

            var updated = await checkCommand.ExecuteScalarAsync(cancellationToken);
            
            int insertedId = Convert.ToInt32(updated);
            
            
            if(insertedId <= 0)
                throw new Exception("insert to visit failed");

            
            //----------- insert into product-warehouse

            for (int i = 0; i < serviceToDbDto.services.Count; i++)
            {

                var insertQuery =
                    @"INSERT INTO [dbo].[Visit_Service] (visitId,serviceId, serviceFee)
                            VALUES (@visitId, @serviceId, @serviceFee);
                            SELECT SCOPE_IDENTITY();";

                await using var insertCommand = new SqlCommand(insertQuery, connection, (SqlTransaction)transaction);
                insertCommand.Parameters.AddWithValue("@visitId", serviceToDbDto.visitId);
                insertCommand.Parameters.AddWithValue("@serviceId", serviceToDbDto.services[i]);
                insertCommand.Parameters.AddWithValue("@serviceFee", serviceToDbDto.visitServices[i].serviceFee);
                
                var insertedIdObj = await insertCommand.ExecuteScalarAsync(cancellationToken);
                
                await transaction.CommitAsync(cancellationToken);
            }

            return insertedId;
        }
        catch (Exception a)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new Exception($"Update failed: "+a.Message);
        }
    }

    public async Task<bool> DoesVisitExist(int serviceVisitId, CancellationToken cancellationToken)
    {
        await using (var connection = new SqlConnection(_connectionString)){
            
            await connection.OpenAsync(cancellationToken);

            var query = @"SELECT COUNT(1) FROM [dbo].[Visit] WHERE visit_id = @id";
            
            await using (var command = new SqlCommand(query, connection)){
                command.Parameters.AddWithValue("@id", serviceVisitId);
                
                var result = await command.ExecuteScalarAsync(cancellationToken);
                
                return Convert.ToInt32(result) > 0;
            }
        }
    }

    public async Task<bool> DoesClientExist(int serviceClientId, CancellationToken cancellationToken)
    {
        await using (var connection = new SqlConnection(_connectionString)){
            
            await connection.OpenAsync(cancellationToken);

            var query = @"SELECT COUNT(1) FROM [dbo].[Client] WHERE client_id = @id";
            
            await using (var command = new SqlCommand(query, connection)){
                command.Parameters.AddWithValue("@id", serviceClientId);
                
                var result = await command.ExecuteScalarAsync(cancellationToken);
                
                return Convert.ToInt32(result) > 0;
            }
        }
    }

    public async Task<int> GetMechnicId(string serviceMechnicLicenceNumber, CancellationToken cancellationToken)
    {
        await using (var connection = new SqlConnection(_connectionString)){
            
            await connection.OpenAsync(cancellationToken);

            var query = @"
                        SELECT mechanic_id
                        FROM [dbo].[Mechanic] 
                        Where licence_number = @ln";
            
            await using (var command = new SqlCommand(query, connection)){
                command.Parameters.AddWithValue("@ln", serviceMechnicLicenceNumber);
                
                var result = await command.ExecuteScalarAsync(cancellationToken);

                if (result.Equals(DBNull.Value))
                {
                    throw new NotFoundException("cant get mechnic id");
                }
                
                return Convert.ToInt32(result);
            }
        }
    }

    public async Task<int> GetServiceId(string name, CancellationToken cancellationToken)
    {
        await using (var connection = new SqlConnection(_connectionString)){
            
            await connection.OpenAsync(cancellationToken);

            var query = @"
                        SELECT service_id
                        FROM [dbo].[Service] 
                        Where name = @ln";
            
            await using (var command = new SqlCommand(query, connection)){
                command.Parameters.AddWithValue("@ln", name);
                
                var result = await command.ExecuteScalarAsync(cancellationToken);

                if (result.Equals(DBNull.Value))
                {
                    throw new NotFoundException("cant get service id");
                }
                
                return Convert.ToInt32(result);
            }
        }
    }
}
