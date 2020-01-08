using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonWorkforce.Models;
using BangazonWorkforce.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;





namespace BangazonWorkforce.Controllers
{
    public class ComputerController : Controller
    {

        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())

                // SQL Query to select Computers
                {
                    cmd.CommandText = @"
                     SELECT  c.Id,
                     c.PurchaseDate,
                    c.DecomissionDate,
                    c.Make,
                    c.Manufacturer FROM Computer c";
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Create list of type Computer containing Computers

                    List<Computer> computers = new List<Computer>();
                    while (reader.Read())
                    {
                        // Create single Computer
                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        };



                        // Add single computer to the list
                        computers.Add(computer);
                    }

                    reader.Close();

                    return View(computers);
                }
            }
        }




        // GET: Computers/Details/5
        public ActionResult Details(int Id)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT
                    Id, PurchaseDate, DecomissionDate, Make, Manufacturer
                FROM Computer Where Id =@Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", Id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computer = new Computer();

                    if (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))

                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        };
                    }
                    reader.Close();


                    return View(computer);
                }
            }
        }





        ActionResult ViewResult(Computer computer)
        {
            throw new NotImplementedException();
        }

        //GET: Computers/Create
        public ActionResult Create()
        {
            // Create a new instance of a CreateComputerViewModel
            // If we want to get all the ccmputers, we need to use the constructor that's expecting a connection string. 
            // When we create this instance, the constructor will run and get all the computers.
            CreateComputerViewModel ComputerViewModel = new CreateComputerViewModel(_config.GetConnectionString("DefaultConnection"));

            // Once we've created it, we can pass it to the view
            return View(ComputerViewModel);
        }

        //POST: Computers/Create

       [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateComputerViewModel model)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Computer
        ( Id, PurchaseDate, DecomissionsDate, Make, Manufacturer )
        VALUES
        ( @PurchaseDate, @DecomissionDate, @Make, @Manufacturer )";
                    cmd.Parameters.Add(new SqlParameter("@PurchaseDate", model.computer.PurchaseDate));
                    cmd.Parameters.Add(new SqlParameter("@DecomissionDate", model.computer.Make));
                    cmd.Parameters.Add(new SqlParameter("@Make", model.computer.Manufacturer));
                    cmd.Parameters.Add(new SqlParameter("@Manufacturer", model.computer.Manufacturer));
                    cmd.ExecuteNonQuery();

                    return RedirectToAction(nameof(Index));
                }
            }
        }


        // GET: Computers/Edit/5
        public ActionResult Edit(int id)
        {
            // Create a new instance of a ComputerEditViewModel
            // Pass it the computerId and a connection string to the database
            ComputerEditViewModel viewModel = new ComputerEditViewModel(id, _config.GetConnectionString("DefaultConnection"));

            // The view model's constructor will work its magic
            // Pass the new instance of the view model to the view

            return View(viewModel);
        }

        //POST: Computers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ComputerEditViewModel computerViewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        // First, update the computer's information, including their Manufacturer
                        // Wipe out all their previously assigned exercises in the join table
                        string command = @"UPDATE Computer
                                            SET PurchaseDate=@PurchaseDate, 
                                            DecomissionDate=@DecomissionDate, 
                                            Make=@Make, 
                                            Manufacturer=@Manufacturer
                                            WHERE Id = @id
                                            DELETE FROM ComputerExercise WHERE computerId =@id";

                        // Loop over the selected exercises and add a new entry for each exercise
                        computerViewModel.SelectedExercises.ForEach(exerciseId =>
                        {
                            command += $" INSERT INTO ComputerExercise (ComputerId, ExerciseId) VALUES (@id, {exerciseId})";

                        });
                        cmd.CommandText = command;
                        cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computerViewModel.Computer.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@DecomissionDate", computerViewModel.Computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@Make", computerViewModel.Computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@Manufacturer", computerViewModel.Computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                    }

                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(computerViewModel);
            }
        }

        //GET: Computers/Delete/5
        public ActionResult Delete(int id)
{
    using (SqlConnection conn = Connection)
    {
        conn.Open();
        using (SqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                        SELECT
                            Id, PurchaseDate, DecomissionDate, Make, Manufacturer
                        FROM Computer
                        WHERE Id = @id";
            cmd.Parameters.Add(new SqlParameter("@id", id));
            SqlDataReader reader = cmd.ExecuteReader();

            Computer Computer = null;

            if (reader.Read())
            {
                Computer = new Computer
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                    DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate")),
                    Make = reader.GetString(reader.GetOrdinal("Make")),
                    Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))

                };
            }
            reader.Close();

            return View(Computer);
        }
    }
}

// POST: Computers/Delete/5
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Delete(int id, IFormCollection collection)
{
    try
    {

        using (SqlConnection conn = Connection)
        {
            conn.Open();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"DELETE FROM Computer WHERE computerId = @id
                        DELETE FROM Computer WHERE Id = @id";
                cmd.Parameters.Add(new SqlParameter("@id", id));

                int rowsAffected = cmd.ExecuteNonQuery();

            }
        }

        return RedirectToAction(nameof(Index));
    }
    catch
    {
        return View();
    }
}
    }
}