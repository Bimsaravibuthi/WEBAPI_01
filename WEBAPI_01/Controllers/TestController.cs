using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WEBAPI_01.DBConnection;
using System.Configuration;
using TaskAPI.Services.Models;

namespace WEBAPI_01.Controllers
{
    /*  API in general case
     
     *  GET    -> READ
     *  POST   -> CREATE
     *  PUT    -> UPDATE
     *  DELETE -> DELETE
     */

    /*  API in this case
     
     *  GET    -> READ
     *  POST   -> CREATE, UPDATE, DELETE
     */

    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        readonly SqlConnection Connection = new SqlConnection("Server=DESKTOP-2C62J43\\SQLEXPRESS;Database=TASK_APP;Trusted_Connection=True;MultipleActiveResultSets=True");

        SqlCommand command;
        //SqlDataReader dataReader;
        SqlDataAdapter adapt;

        private bool TokenValidation(string tkn)
        {
            string validTKN = "1";
            if (tkn == validTKN)
            {
                return true;
            }
            else return false;
        }

        //  GET
        [HttpGet("GetDirectSQLStringData")]     // Header
        public IActionResult GetDirectSQLStringData(string tkn, string SQL)
        {
            DataTable dt = new DataTable("GET");
            if (TokenValidation(tkn))
            {
                Connection.Open();
                adapt = new SqlDataAdapter(SQL, Connection);
                adapt.Fill(dt);
                adapt.Dispose();
                Connection.Close();

                if (dt is null)
                {
                    return NotFound();
                }

                return Ok(dt);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("GetStoredProDataNoPara")]    // Header
        public IActionResult GetStoredProDataNoPara(string tkn, string strProcedure)
        {
            /*  Supported procedure names
                    #1. All_Users
                    #2. All_Tasks 
            */

            strProcedure = "[dbo].[" + strProcedure + "]";
            DataTable dt = new DataTable("GET");

            if (TokenValidation(tkn))
            {
                Connection.Open();
                command = new SqlCommand(strProcedure, Connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                adapt = new SqlDataAdapter(command);
                adapt.Fill(dt);
                adapt.Dispose();
                Connection.Close();

                if (dt is null)
                {
                    return NotFound();
                }
                return Ok(dt);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("GetStoredProDataWithParaHeader")]     // Header
        public IActionResult GetStoredProDataWithParaHeader(string tkn, string strProcedure, string strParaNames, string strParaValues)
        {
            strProcedure = "[dbo].[" + strProcedure + "]";
            DataTable dt = new DataTable("GET");

            if (TokenValidation(tkn))
            {
                Connection.Open();

                object[] objPara;
                object[] objVal;

                objPara = strParaNames.Split('|');
                objVal = strParaValues.Split('|');

                SqlCommand command = new SqlCommand(strProcedure, Connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                for (int i = 0; i < objPara.Length; i++)
                {
                    command.Parameters.AddWithValue("@" + objPara[i].ToString(), objVal[i]);
                }

                adapt = new SqlDataAdapter(command);
                adapt.Fill(dt);
                adapt.Dispose();
                Connection.Close();

                if (dt is null)
                {
                    return NotFound();
                }
                return Ok(dt);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("GetStoredProDataWithPara")]    // Body
        public IActionResult GetStoredProDataWithPara([FromBody] ProData data)
        {
            /*  Supported procedure names
                     
            */

            DataTable dt = new DataTable("GET");
            data.strProcedure = "[dbo].[" + data.strProcedure + "]";

            if (TokenValidation(data.tkn))
            {
                object[] objPara;
                object[] objVal;

                objPara = data.strParaNames.Split('|');
                objVal = data.strParaValues.Split('|');

                try
                {
                    Connection.Open();
                    adapt = new SqlDataAdapter();
                    command = new SqlCommand(data.strProcedure, Connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    for (int i = 0; i < objPara.Length; i++)
                    {
                        command.Parameters.AddWithValue("@" + objPara[i].ToString(), objVal[i]);
                    }

                    adapt = new SqlDataAdapter(command);
                    adapt.Fill(dt);
                    Connection.Close();
                }
                catch(SqlException ex)
                {
                    throw ex;
                }

                if (dt is null)
                {
                    return NotFound("Data not found.");
                }
                return Ok(dt);
            }
            else
            {
                return NotFound("Invalid Parameter(s).");
            }
        }

        //  POST
        [HttpPost("PostStoredProDataWithPara")]    // Body
        public IActionResult PostStoredProDataWithPara([FromBody] ProData data)
        {
            int strOut = 0;
            data.strProcedure = "[dbo].[" + data.strProcedure + "]";

            if (TokenValidation(data.tkn))
            {
                object[] objPara;
                object[] objVal;

                objPara = data.strParaNames.Split('|');
                objVal = data.strParaValues.Split('|');

                try
                {
                    Connection.Open();
                    adapt = new SqlDataAdapter();
                    command = new SqlCommand(data.strProcedure, Connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    for (int i = 0; i < objPara.Length; i++)
                    {
                        if (objVal[i].ToString().Length < 3000)
                        {
                            command.Parameters.AddWithValue("@" + objPara[i].ToString(), objVal[i]);
                        }
                        else
                        {
                            byte[] imageToSave = Convert.FromBase64String(objVal[i].ToString());
                            SqlParameter sqlParaImage = new SqlParameter(objPara[i].ToString(), SqlDbType.Image);
                            sqlParaImage.Value = imageToSave;
                            command.Parameters.Add(sqlParaImage);
                        }
                    }

                    strOut = command.ExecuteNonQuery();
                    Connection.Close();
            }
                catch (SqlException ex)
            {
                throw ex;
            }

            return Ok(strOut);
            }
            else
            {
                return NotFound("Invalid Parameter(s).");
            }
        }

        [HttpPost("DeleteData")]    // Body
        public IActionResult DeleteData([FromBody] ProData data)
        {
            /*  Supported procedure names
                    #1. Delete_Test 
            */

            data.strProcedure = "[dbo].[" + data.strProcedure + "]";

            if (TokenValidation(data.tkn))
            {
                object[] objPara;
                object[] objVal;

                objPara = data.strParaNames.Split('|');
                objVal = data.strParaValues.Split('|');

                try
                {
                    Connection.Open();
                    adapt = new SqlDataAdapter();
                    command = new SqlCommand(data.strProcedure, Connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    for (int i = 0; i < objPara.Length; i++)
                    {
                        command.Parameters.AddWithValue("@" + objPara[i].ToString(), objVal[i]);
                    }
                    int strOut = command.ExecuteNonQuery();

                    Connection.Close();
                }
                catch (SqlException ex)
                {
                    throw ex;
                }

                return Ok("Record Deleted Successfuly.");
            }
            else
            {
                return NotFound("Invalid Parameter(s).");
            }
        }

        [HttpPost("UpdateData")]    // Body
        public IActionResult UpdateData([FromBody] ProData data)
        {
            /*  Supported procedure names
                    #1. Update_Test 
            */

            data.strProcedure = "[dbo].[" + data.strProcedure + "]";

            if (TokenValidation(data.tkn))
            {
                object[] objPara;
                object[] objVal;

                objPara = data.strParaNames.Split('|');
                objVal = data.strParaValues.Split('|');

                try
                {
                    Connection.Open();
                    adapt = new SqlDataAdapter();
                    command = new SqlCommand(data.strProcedure, Connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    for (int i = 0; i < objPara.Length; i++)
                    {
                        command.Parameters.AddWithValue("@" + objPara[i].ToString(), objVal[i]);
                    }
                    int strOut = command.ExecuteNonQuery();

                    Connection.Close();
                }
                catch (SqlException ex)
                {
                    throw ex;
                }

                return Ok("Record Updated Successfuly.");
            }
            else
            {
                return NotFound("Invalid Parameter(s).");
            }
        }
    }
}
