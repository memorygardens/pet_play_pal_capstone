using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.DAO
{
    public class ParameterizedSqlArray<arrayType>
    {
        public string Snippet = "";
        private readonly List<SqlParameter> parameters = new List<SqlParameter>();
        public SqlParameter[] Parameters { get { return this.parameters.ToArray(); } }
        public ParameterizedSqlArray(string snippet, List<arrayType> values, string uniquePrefix)
        {
            //this will hold our return value: a parameterized snippit of a SQL command, and a dict of params and values to add to your SqlCommand
            List<string> paramNames = new List<string>();
            int i = 0;
            foreach (arrayType element in values)
            {
                string paramName = $"@{uniquePrefix}{i}";//create a prepared variable param for the SQL command
                this.parameters.Add(new SqlParameter(paramName, element));//add the variable to the  dictionary of param-values
                paramNames.Add(paramName);//...and add the variable name to a list as well
                i++;
            }
            this.Snippet = String.Format(snippet, string.Join(",", paramNames));
        }

    }
}
