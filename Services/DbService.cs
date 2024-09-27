using System.Data.SqlClient;
using System.Text.Json;
using CRRService.Data;
using Dapper;
using Npgsql;

namespace CRRService.Services;
public class DbService(IConfiguration configuration)
{
    private readonly string connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    public async Task<IEnumerable<Crr>> GetAllNewCustomerRiskRatingsAsync(string alreadySelectedCustomerIds = null!)
    {
        IEnumerable<Crr> crrDatas;
        try
        {
            using var connection = new SqlConnection(connectionString);
           // using var connection = new NpgsqlConnection(connectionString);

           if(string.IsNullOrEmpty(alreadySelectedCustomerIds))
                alreadySelectedCustomerIds="";

            await connection.OpenAsync();
            var query=@"SELECT Customer_ID__c AS CIF,
                            CASE 
                                WHEN Risk_Rate = 'HIGH RISK' THEN 'High-High'
                                WHEN Risk_Rate = 'MEDIUM RISK' THEN 'High-Medium'
                                WHEN Risk_Rate = 'LOW RISK' THEN 'High-Low' 
                            END AS RiskClassification
                        FROM crr_table WHERE Customer_ID__c NOT IN (@alreadySelectedCustomerIds)
                        AND NOT EXISTS (SELECT 1 FROM crr_table_api WHERE CIF = crr_table.Customer_ID__c 
                                AND RiskClassification = CASE 
                                    WHEN crr_table.Risk_Rate = 'HIGH RISK' THEN 'High-High'
                                    WHEN Risk_Rate = 'MEDIUM RISK' THEN 'High-Medium'
                                    WHEN Risk_Rate = 'LOW RISK' THEN 'High-Low' 
                                END
                        );";
            // Setting buffered to false is useful for processing large data streams without holding everything in memory
            crrDatas = await connection.QueryAsync<Crr>(query, new { alreadySelectedCustomerIds});// connection.QueryAsync<string>(query, buffered: false);
            return crrDatas ?? [];
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("An error occurred selecting CRRs: " + ex.Message);
            return new List<Crr>();
        }
    }
    public async Task<int> UpdateCustomerRiskRatingsStatusAsync(Crr crrData)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            //using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            string query = @"
                IF NOT EXISTS (SELECT 1 FROM crr_table_api WHERE CIF = @CIF)
                    INSERT INTO crr_table_api 
                        (CIF, RiskClassification, PepCategory, ReasonForClassification, IsSent, DateSent) 
                        VALUES (@CIF, @RiskClassification, @PepCategory, @ReasonForClassification, @IsSent, @DateSent);
                ELSE
                    UPDATE crr_table_api set 
                        RiskClassification=@RiskClassification,
                        PepCategory=@PepCategory,
                        ReasonForClassification=@ReasonForClassification,
                        IsSent=@IsSent, 
                        DateSent=@DateSent
                        WHERE CIF=@CIF;";
            var result = await connection.ExecuteAsync(query, crrData);
            if (result <= 0)//no record updated
            {
                //log 
                Console.WriteLine($"Commit CIF {crrData.CIF} data to db not successful, status: {result}");
                return 0;
            }
            Console.WriteLine($"Commit CIF {crrData.CIF} data to db, status: {result}");

            return result;
        }
        catch (System.Exception ex)
        {
             Console.WriteLine("An error occurred when updating CRR: " + ex.Message + " - "+ ex.StackTrace);
             return 0;
        }
    }
}
