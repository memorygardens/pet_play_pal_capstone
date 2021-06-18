
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.IO;


namespace Capstone_Tests.DAO
{
    /// <summary>
    /// A base class to be extended by <see cref="TestClassAttribute">testing</see> classes that test SqlDAO
    /// </summary>
    public class DAOTests
    {
        /// <summary>
        /// The connection string to connect to the SqlDatabase
        /// </summary>
        protected string connectionString = "";


        /// <summary>
        /// Sets up this instance of <see cref="DAOTests"/>.
        /// This should have the <see cref="TestInitializeAttribute">TestInitialize</see> attribute on all subclass implementations <br></br>
        /// also, the <see cref="ResetDB"/> method should be called each time this method is called.
        /// </summary>
        virtual public void Setup()
        {
            ResetDB();
        }
        /// <summary>
        /// Resets the database after each test.
        /// </summary>
        private void ResetDB()
        {
            // Get the connection string from the appsettings.json file
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            connectionString = configuration.GetConnectionString("Testing");

            //read our SQL file
            string path = "PetPlayPals_Test_Setup.sql";
            string setupScript = File.ReadAllText(path);

            //create SQL command from the sql file to reset the database to a known state
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(setupScript, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                throw;
            }
        }
    }
}
