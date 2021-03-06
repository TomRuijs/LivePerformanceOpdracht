﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LivePerformanceApplication.Classes.SharedModels;
using LivePerformanceApplication.Exceptions;
using Oracle.DataAccess.Client;

namespace LivePerformanceApplication.Classes
{
    public static class DatabaseManager
    {
        //private static readonly string _strConnection =
        //"Data Source=//fhictora01.fhict.local:1521/fhictora;Persist Security Info=True;User Id=dbi278048;Password=password";

        /// <summary>
        /// Database connectie
        /// </summary>
        private static OracleConnection _Connection = new OracleConnection(_ConnectionString);

        /// <summary>
        /// Bouwestenen van de connectie string
        /// </summary>
        private const string _ConnectionId = "SYSTEM",

            _ConnectionPassword = "password",

            _ConnectionAddress = "//localhost/:1521/xe";

        /// <summary>
        /// Connectiestring van de database 
        /// </summary>
        private static string _ConnectionString
        {

            get
            {
                return string.Format("Data Source={0};Persist Security Info=True;User Id={1};Password={2}", _ConnectionAddress, _ConnectionId, _ConnectionPassword);
            }
        }

        /// <summary>
        /// Maakt het commando en returnt de informatie
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static OracleCommand CreateOracleCommand(string sql)
        {
            OracleCommand command = new OracleCommand(sql, _Connection);
            command.CommandType = System.Data.CommandType.Text;

            return command;
        }

        /// <summary>
        ///  Opent de connectie en geeft de reader terug
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static OracleDataReader ExecuteQuery(OracleCommand command)
        {
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                {
                    try
                    {
                        _Connection.Open();
                    }
                    catch (OracleException exc)
                    {
                        Debug.WriteLine("Database connection failed!\n" + exc.Message);
                        throw;
                    }
                }

                OracleDataReader reader = command.ExecuteReader();

                return reader;
            }
            catch (OracleException)
            {
                return null;
            }
        }

        /// <summary>
        /// Opens the connection with the database and inserts the given information, returns true if insert worked
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static bool ExecuteNonQuery(OracleCommand command)
        {
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                {
                    try
                    {
                        _Connection.Open();
                    }
                    catch (OracleException exc)
                    {
                        Debug.WriteLine("Database connection failed!\n" + exc.Message);
                        throw;
                    }
                }

                command.ExecuteNonQuery();
                return true;
            }
            catch (OracleException)
            {
                return false;
            }
        }

        /// <summary>
        /// Test de connectie met de database
        /// </summary>
        public static void TestConnection()
        {
            try
            {

                _Connection.Open();

            }
            catch (Exception exc)
            {
                throw new DatabaseNotConnectedException(exc.Message);
            }
            finally
            {
                _Connection.Close();
            }
        }

        /// <summary>
        /// Haalt de gebruiker op gebasseerd op inlog gegevens
        /// </summary>
        /// <param name="email"></param>
        /// <param name="wachtwoord"></param>
        /// <returns></returns>
        public static Gebruiker GetGebruiker(string email, string wachtwoord)
        {
            try
            {

                var cmd = CreateOracleCommand("SELECT GebruikerId, Naam, Email, Wachtwoord FROM GEBRUIKER WHERE Email = :Email AND Wachtwoord = :Wachtwoord");
                cmd.Parameters.Add("Email", email);
                cmd.Parameters.Add("Wachtwoord", wachtwoord);
                var reader = ExecuteQuery(cmd);
                while (reader.Read())
                {
                    var mail = reader["Email"].ToString();
                    var naam = reader["Naam"].ToString();
                    int gebruikerId = -1;
                    if (!string.IsNullOrEmpty(reader["GebruikerId"].ToString()))
                        gebruikerId = Convert.ToInt32(reader["GebruikerId"]);
                    var password = reader["Wachtwoord"].ToString();
                    if (!string.IsNullOrEmpty(password))
                        return new Gebruiker(gebruikerId, mail, naam, true);
                    return null;
                }
                return null;

            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _Connection.Close();
            }
        }

        /// <summary>
        /// Voegt een meer toe aan de database
        /// </summary>
        /// <param name="naam"></param>
        /// <param name="prijs"></param>
        /// <param name="boten"></param>
        /// <returns></returns>
        public static bool AddMeer(string naam, double prijs, List<IBoot> boten)
        {
            try
            {
                bool succes = true;

                var cmd = CreateOracleCommand("INSERT INTO MEER (Naam, Prijs) VALUES (:Naam, :Prijs");
                cmd.Parameters.Add("Naam", naam);
                cmd.Parameters.Add("Prijs", prijs);
                succes = ExecuteNonQuery(cmd);
                cmd = CreateOracleCommand("SELECT MAX(MeerId) FROM MEER");
                int meerId = -1;
                var reader = ExecuteQuery(cmd);
                {
                    if (!string.IsNullOrEmpty(reader["MeerId"].ToString()))
                        meerId = Convert.ToInt32(reader["MeerId"].ToString());
                }

                foreach (var boot in boten)
                {
                    cmd = CreateOracleCommand("INSERT INTO BOOTVERBOD (BootId, MeerId) VALUES (:BootId, :MeerId)");
                    cmd.Parameters.Add("MeerId", meerId);
                    cmd.Parameters.Add("BootId", boot.Id);
                    succes = ExecuteNonQuery(cmd);
                }
                return succes;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _Connection.Close();
            }
        }

        /// <summary>
        /// Haalt alle boten op
        /// </summary>
        /// <returns></returns>
        public static List<IBoot> GetBoten()
        {
            try
            {
                var boten = new List<IBoot>();
                var cmd = CreateOracleCommand("SELECT BOOT.BootId, Naam, Prijs, BOOT.SoortBoot, Tankinhoud " +
                                              "FROM BOOT INNER JOIN MOTORBOOT ON BOOT.BootId = MOTORBOOT.BootId");
                var reader = ExecuteQuery(cmd);
                while (reader.Read())
                {
                    int bootId = -1;
                    if (!string.IsNullOrEmpty(reader["BootId"].ToString()))
                        bootId = Convert.ToInt32(reader["BootId"].ToString());
                    var naam = reader["Naam"].ToString();
                    double prijs = -1.00;
                    if (!string.IsNullOrEmpty(reader["Prijs"].ToString()))
                        prijs = Convert.ToDouble(reader["Prijs"].ToString());
                    var soortBoot = reader["SoortBoot"].ToString();
                    double tankInhoud = -1.00;
                    if (!string.IsNullOrEmpty(reader["Tankinhoud"].ToString()))
                        tankInhoud = Convert.ToDouble(reader["Tankinhoud"].ToString());
                    boten.Add(new Motorboot(bootId, naam, prijs, soortBoot, tankInhoud));
                }

                cmd = CreateOracleCommand("SELECT BOOT.BootId, Naam, Prijs, BOOT.SoortBoot FROM BOOT WHERE BOOT.SoortBoot != 'kruiser' AND Boot.SoortBoot != 'motorboot'");
                reader = ExecuteQuery(cmd);
                while (reader.Read())
                {
                    int bootId = -1;
                    if (!string.IsNullOrEmpty(reader["BootId"].ToString()))
                        bootId = Convert.ToInt32(reader["BootId"].ToString());
                    var naam = reader["Naam"].ToString();
                    double prijs = -1.00;
                    if (!string.IsNullOrEmpty(reader["Prijs"].ToString()))
                        prijs = Convert.ToDouble(reader["Prijs"].ToString());
                    var soortBoot = reader["SoortBoot"].ToString();
                    boten.Add(new Spierboot(bootId, naam, prijs, soortBoot));
                }
                return boten;

            }
            catch
            {
                return new List<IBoot>();
            }
            finally
            {
                _Connection.Close();
            }
        }

        /// <summary>
        /// Voegt een nieuwe gebruiker toe, voor het maken van een nieuwe huurder
        /// </summary>
        /// <param name="naam"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool AddGebruiker(string naam, string email)
        {
            try
            {

                var cmd = CreateOracleCommand("INSERT INTO Gebruiker (Naam, Email) VALUES (:Naam, :Email)");
                cmd.Parameters.Add("Naam", naam);
                cmd.Parameters.Add("Email", email);
                return ExecuteNonQuery(cmd);

            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _Connection.Close();
            }
        }

        public static Gebruiker GetHuurder(string email, string naam)
        {
            try
            {
                var commmand = CreateOracleCommand("SELECT GebruikerId, Naam, Email, Wachtwoord FROM GEBRUIKER WHERE Email = :Email AND Naam = :Naam");
                commmand.Parameters.Add("Email", email);
                commmand.Parameters.Add("Naam", naam);
                var reader = ExecuteQuery(commmand);
                while (reader.Read())
                {
                    var mail = reader["Email"].ToString();
                    var klantnaam = reader["Naam"].ToString();
                    int gebruikerId = -1;
                    if (!string.IsNullOrEmpty(reader["GebruikerId"].ToString()))
                        gebruikerId = Convert.ToInt32(reader["GebruikerId"]);
                    var password = reader["Wachtwoord"].ToString();
                    if (!string.IsNullOrEmpty(password))
                        return new Gebruiker(gebruikerId, mail, klantnaam, true);
                    return null;
                }

                return null;


            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                _Connection.Close();
            }
        }

        /// <summary>
        /// Voegt een nieuw huurcontract toe aan de database
        /// </summary>
        /// <param name="email"></param>
        /// <param name="naam"></param>
        /// <param name="huurcontract"></param>
        /// <returns></returns>
        public static bool AddHuurcontract(string email, string naam, Huurcontract huurcontract)
        {
            try
            {
                bool succes = true;



                var huurder = new Gebruiker();
                if (GetHuurder(email, naam) != null)
                {
                    huurder = GetHuurder(email, naam);
                }
                else if (AddGebruiker(naam, email))
                {
                    huurder = GetHuurder(email, naam);
                }
                else
                {
                    return false;
                }


                var cmd =
                    CreateOracleCommand(
                        "INSERT INTO HUURCONTRACT (DatumVan, DatumTot, HuurderId) VALUES (:DatumVan, :DatumTot, :HuurderId)");
                cmd.Parameters.Add("DatumVan", huurcontract.DatumVan);
                cmd.Parameters.Add("DatumTot", huurcontract.DatumTot);
                cmd.Parameters.Add("HuurderId", 2);
                succes = ExecuteNonQuery(cmd);

                int huurcontractId = -1;
                cmd = CreateOracleCommand("SELECT MAX(HuurcontractId) FROM HUURCONTRACT");
                var reader = ExecuteQuery(cmd);
                while (reader.Read())
                {
                    Convert.ToInt32(reader["HuurcontractId"].ToString());
                }

                //MEERHUUR
                foreach (var meer in huurcontract.Meren)
                {
                    cmd = CreateOracleCommand("INSERT INTO MEERHUUR (MeerId, HuurcontractId) VALUES (:MeerId, :HuurcontractId)");
                    cmd.Parameters.Add("MeerId", meer.Id);
                    cmd.Parameters.Add("HuurcontractId", huurcontractId);
                    succes = ExecuteNonQuery(cmd);
                }

                //ARTIKELHUUR
                foreach (var artikel in huurcontract.Artikelen)
                {
                    cmd = CreateOracleCommand("INSERT INTO ARTIKELHUUR (ArtikelId, HuurcontractId) VALUES (:ArtikelId, :HuurcontractId)");
                    cmd.Parameters.Add("ArtikelId", artikel.Id);
                    cmd.Parameters.Add("HuurcontractId", huurcontractId);
                    succes = ExecuteNonQuery(cmd);
                }

                //BOOTHUUR
                foreach (var boot in huurcontract.Boten)
                {
                    cmd = CreateOracleCommand("INSERT INTO BOOTHUUR (BootId, HuurcontractId) VALUES (:BootId, :HuurcontractId)");
                    cmd.Parameters.Add("BootId", boot.Id);
                    cmd.Parameters.Add("HuurcontractId", huurcontractId);
                    succes = ExecuteNonQuery(cmd);
                }

                return succes;


            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                _Connection.Close();
            }
        }

    }

}
