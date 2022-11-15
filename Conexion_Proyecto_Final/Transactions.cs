using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Interfaces.Streaming;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI.Common;
using System.Data;
using System.Collections;
using System.Data.SqlClient;

namespace Conexion_Proyecto_Final
{
    public class Transactions
    {
        //-----------------------------------------------Variables------------------------------------------------------
        //Esta variable guarda la dirección de conexión.
        static string pathConection = "Server=localhost; Database=dbarchivos; uid=root; password=;";

        //-----------------------------------------------Funciones------------------------------------------------------
        //Esta función realiza la búsqueda de un usuario con la información ingresada en el form Login para comprobar que
        //si exista y la contraseña esté correcta, de esta forma inicia sesión. (CRUD-READ)
        public static int Login(string email, string password)
        {
            int result = 0;
            try
            {
                MySqlConnection conn = new MySqlConnection(pathConection);
                conn.Open();
                string query = "SELECT COUNT(*) FROM tblusuario WHERE strEmail = @strEmail AND strPassword = @strPassword";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@strEmail", email);
                cmd.Parameters.AddWithValue("@strPassword", password);
                result = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
            }
            catch { }

            return result;
        }

        //Esta función realiza la inserción/registro de un usuario y la información ingresada en el form Register para
        //que pueda iniciar sesión con esas credenciales. (CRUD-CREATE/UPDATE)
        public static string Register(string name, string lastName, string email, string password)
        {
            string result = "";

            try
            {
                MySqlConnection conn = new MySqlConnection(pathConection);
                conn.Open();
                string query = "INSERT INTO tblusuario VALUES(@strEmail, @strPassword, @strFirstName, @strLastName)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@strEmail", email);
                cmd.Parameters.AddWithValue("@strPassword", password);
                cmd.Parameters.AddWithValue("@strFirstName", name);
                cmd.Parameters.AddWithValue("@strLastName", lastName);
                cmd.ExecuteNonQuery();
                conn.Close();
                return result = "You have successfully registered.";
            }
            catch { return result = "Sorry, the registration failed. Please try again."; }
        }

        //Esta función realiza la validación de que el email que se ingresó a la hora de registrarse no esté registrado ya. (CRUD-READ)
        static public int validationEmail(string email)
        {
            int result = 0;

            try
            {
                MySqlConnection conn = new MySqlConnection(pathConection);
                conn.Open();
                string query = "SELECT COUNT(*) FROM tblusuario WHERE strEmail = @strEmail";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@strEmail", email);
                result = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
            }
            catch { }

            return result;
        }

        //Esta función realiza la creación de la tabla del archivo CSV que se abrió. (CRUD-CREATE)
        public static int CreateTable(List<string> titles, string tableName)
        {
            string fields = "";
            bool types = titles[0].Contains("str");
            string query = "";

            for (int i = 0; i < titles.Count; i++)
            {                
                    fields = fields + titles[i] + " VARCHAR(50),";
            }
            
            try
            {
                MySqlConnection conn = new MySqlConnection(pathConection);
                conn.Open();
                query = "CREATE TABLE IF NOT EXISTS " + tableName + " (" + fields + " CONSTRAINT PK_ID PRIMARY KEY(" + titles[0] + ") );";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                conn.Close();               
                return 1;
            }
            catch { return 0; }
        }

        //Esta función realiza un registro en la tabla tblArchivos en donde plasma qué tablas de archivos hay
        //en la base de datos y qué usuario la insertó (CRUD-CREATE(Insert))
        public static string InsertNewTable(string tableName, string email)
        {
            string result = "";

            try
            {
                MySqlConnection conn = new MySqlConnection(pathConection);
                conn.Open();
                string query = "INSERT INTO tblarchivos VALUES(null," + "'" + tableName + "'," + "'" + email + "')";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                return result = "Archivo registrado correctamente";
            }
            catch { return result = "No se pudo registrar el archivo"; }
        }

        //Esta función realiza la inserción de los datos de la tabla del archivo CSV que se abrió. (CRUD-CREATE(Insert))
        public static string InsertInfo(List<string> titles, string tableName, string[] info, char separator)
        {
            string result = "", infoQuery = "";
            string query = "";

            try
            {
                for (int i = 1; i < info.Length; i++)
                {
                    string[] dataQuery = info[i].Split(separator);
                    infoQuery = "";

                    for (int k = 0; k < titles.Count; k++)
                    {
                        infoQuery = infoQuery + "'" + dataQuery[k] + "'";

                        if (k < titles.Count - 1)
                        {
                            infoQuery = infoQuery + ", ";
                        }
                    }
                    MySqlConnection conn = new MySqlConnection(pathConection);
                    conn.Open();
                    query = "INSERT INTO " + tableName + " VALUES(" + infoQuery + ");";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    //cmd.Parameters.AddWithValue("@tableName", tableName);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                return result = "INFORMACIÓN INGRESADA A LA BASE DE DATOS CORRECTAMENTE";
            }
            catch { return result = "ERROR";  }
        }

        //Esta función realiza la eliminación de la tabla específicada, junto con este se elimina el registro de la misma en la tabla tblArchivos.
        //(CRUD-DELETE)
        public static int DropTable(string tableName)
        {
            string result = "";
            try
            {
                MySqlConnection conn = new MySqlConnection(pathConection);
                conn.Open();
                string query = "DROP TABLE IF EXISTS " + tableName + "; DELETE FROM tblarchivos WHERE strNombreArchivo = '" + tableName + "'";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                //cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.ExecuteNonQuery();
                conn.Close();
                return 1;
            }
            catch { return 0; }
        }

        //Esta función realiza la edición de la tabla específicada con la información nueva. (CRUD-UPDATE)
        public static int EditTable(string tableName, string[] info)
        {
            try
            {
                for (int i = 0; i < info.Length; i++)
                {
                    MySqlConnection conn = new MySqlConnection(pathConection);
                    conn.Open();
                    string query = "INSERT INTO " + tableName + " VALUES(" + info[i] + ");";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                return 1;
            }
            catch { return 0; }
        }
    }
}