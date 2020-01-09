using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using BangazonWorkforce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonWorkforce.Models.ViewModels;
using System;

namespace BangazonWorkforce.Controllers
{
    public class ComputersController : Controller
    {
        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Computers
//fetches all computers
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Make, Manufacturer, PurchaseDate, DecomissionDate FROM Computer";

                  
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Computer> computers = new List<Computer>();
                    DateTime? NullDateTime = null;
                    while (reader.Read())
                    {
                        //create individual instance of computer
                        Computer currentComputer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecomissionDate = reader.IsDBNull(reader.GetOrdinal("DecomissionDate")) ? NullDateTime : reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))
                        };
                            computers.Add(currentComputer);

                    }
                    reader.Close();

                    return View(computers);
                }
            }
        }
        //End of the get method

        // GET: Computers/Details/5
        public ActionResult Details(int id)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM Computer  WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computer = null;
                    try
                    {

                        if (reader.Read())
                        {
                            computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))
                            };

                        }
                    }
                    catch (SqlNullValueException)
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                        };
                    }
                    reader.Close();

                    return View(computer);

                }
            }

        }

        // GET: Computers/Create
           //Start of the create method
        public ActionResult Create()
        {
            return View();
        }

        // POST: Computers/Create
        //this posts the new Computer made by the user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Computer (Make, Manufacturer, PurchaseDate)
                                                VALUES (@Make, @Manufacturer, @PurchaseDate)";

                        cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@Manufacturer", computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computer.PurchaseDate));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }

            }
            catch
            {
                return View();
            }
        }
//End of the Create Method

        // GET: Computers/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Computers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Computers/Delete/5
        public ActionResult Delete(int id, ComputerDeleteViewModel viewModel)
        {
            ComputerDeleteViewModel comp = viewModel;
            List<Computer> AssignedComputers = isAssigned();
            foreach (Computer c in AssignedComputers)
            {
                if (c.Id == id)
                {
                    comp.isAssigned = true;
                    break;
                }
                else
                {
                    comp.isAssigned = false;
                }
            }
            using (SqlConnection conn = Connection)
            {
                // open the connection 
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // run the query 
                    cmd.CommandText = $@"SELECT Id,
                                                PurchaseDate,
                                                DecomissionDate,
                                                Make,
                                                Manufacturer
                                        FROM Computer
                                        WHERE Id = @id;";

                    // parameters
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    Computer computer = null;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                            };
                        }
                        else
                        {
                          

                            computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                DecomissionDate = null,
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                            };
                        }
                    }
                    comp.Computer = computer;
                    
                    reader.Close();
                    return View(comp);
                }
            }

        }

        // POST: Computers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, ComputerDeleteViewModel viewModel)
        {



            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                       DELETE FROM Computer WHERE Id = @Id 
                                       AND Id NOT IN (SELECT ComputerId FROM ComputerEmployee)
                                                ";

                        cmd.Parameters.Add(new SqlParameter("@Id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        private Computer GetComputerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT Id,
                                                PurchaseDate,
                                                DecomissionDate,
                                                Make,
                                                Manufacturer
                                        FROM Computer
                                        WHERE Id = @id;";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    Computer computer = null;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                        };
                    }
                    reader.Close();
                    return computer;
                }
            }
        }
        private List<Computer> isAssigned()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = $@"SELECT Id,
                                                Make, 
                                                Manufacturer,
                                                PurchaseDate
                                              
                                                FROM Computer 
                                                WHERE Id IN (SELECT ComputerId FROM ComputerEmployee)
";



                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Computer> AssignedComputers = new List<Computer>();
                    while (reader.Read())
                    {
                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                        };
                        AssignedComputers.Add(computer);
                    }
                    reader.Close();
                    return AssignedComputers;

                }

            }
        }
    }
}