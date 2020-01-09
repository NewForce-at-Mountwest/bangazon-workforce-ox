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





namespace BangazonWorkforce.Controllers
{
    public class EmployeeController : Controller
    {

        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
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
        // GET: Employees
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())

                // SQL Query to select Employees
                {
                    cmd.CommandText = @"
                     SELECT e.Id,
                     e.FirstName,
                    e.LastName,
                    e.DepartmentId,
                    d.[Name],
                    e.IsSuperVisor
                    FROM Employee e
                    LEFT JOIN Department d ON e.DepartmentId = d.Id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Create list of type Employee containing Employees

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        // Create single Employee
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),
                            CurrentDepartment = new Department
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };

                        // Add single employee to the list
                        employees.Add(employee);
                    }

                    reader.Close();

                    return View(employees);
                }
            }
        }

        public ActionResult Details(int id)
        {
            //Create our view model
            EmployeeDetailsViewModel employee = GetEmployeeById(id);
            return View(employee);
        }


        private EmployeeDetailsViewModel GetEmployeeById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //giant SQL query to get all of the information we'll need in our view model
                    cmd.CommandText = @"
                                SELECT e.Id AS EmployeeId, 
                                e.FirstName, 
                                e.LastName, 
                                d.[Name] AS DepartmentName, 
                                c.Id AS ComputerId, 
                                c.Make, 
                                c.Manufacturer,
                                ce.Id AS ComputerEmployeeId,
                                ce.AssignDate,
                                ce.UnassignDate,
                                tp.Id AS TrainingProgramId, 
                                tp.Name AS TrainingProgramName
                                FROM Employee e 
                                LEFT JOIN Department d ON e.DepartmentId = d.Id
                                LEFT JOIN ComputerEmployee ce ON ce.EmployeeId = e.id 
                                LEFT JOIN Computer c ON c.id = ce.ComputerId 
                                LEFT JOIN EmployeeTraining et ON et.EmployeeId = e.Id
                                LEFT JOIN TrainingProgram tp ON tp.Id = et.TrainingProgramId 
                                WHERE e.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    EmployeeDetailsViewModel model = null;


                    while (reader.Read())
                    {
                        if (model == null)
                        {
                            //create new employee for our view model
                            model = new EmployeeDetailsViewModel();
                            model.Employee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                CurrentDepartment = new Department
                                {
                                    Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                                }
                            };
                        }

                        // if the computer ID is NOT null and the Unassign date IS null, create a model for the assigned computer
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")) && (reader.IsDBNull(reader.GetOrdinal("UnassignDate"))))
                        {
                            model.AssignedComputer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                            };
                        }

                        // if the computer employee ID is not null and the unassign date IS null, create a model for assign date
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerEmployeeId")) && (reader.IsDBNull(reader.GetOrdinal("UnassignDate"))))
                        {
                            model.ComputerEmployee = new ComputerEmployee
                            {
                                AssignDate = reader.GetDateTime(reader.GetOrdinal("AssignDate"))
                            };
                        }



                        if (!reader.IsDBNull(reader.GetOrdinal("TrainingProgramId")))
                        {
                            if (model.TrainingPrograms.Any(p => p.Id == reader.GetInt32(reader.GetOrdinal("TrainingProgramId"))))
                            {
                                break;
                            }
                            else
                            {
                                TrainingProgram trainingProgram = new TrainingProgram
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("TrainingProgramId")),
                                    Name = reader.GetString(reader.GetOrdinal("TrainingProgramName"))
                                };

                                model.TrainingPrograms.Add(trainingProgram);
                            }
                        }
                    }
                    reader.Close();
                    return model;
                }
            }
        }

        //GET: Students/Create
        public ActionResult Create()
        {
            // Create a new instance of a CreateEmployeeViewModel
            // If we want to get all the cohorts, we need to use the constructor that's expecting a connection string. 
            // When we create this instance, the constructor will run and get all the cohorts.
            CreateEmployeeViewModel employeeViewModel = new CreateEmployeeViewModel(_config.GetConnectionString("DefaultConnection"));

            // Once we've created it, we can pass it to the view
            return View(employeeViewModel);
        }

        // POST: Students/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateEmployeeViewModel model)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee
                                    ( FirstName, LastName, IsSuperVisor, DepartmentId )
                                    VALUES
                                    ( @firstName, @lastName, @isSuperVisor, @departmentId )";
                    cmd.Parameters.Add(new SqlParameter("@firstName", model.employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", model.employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@isSuperVisor", model.employee.IsSuperVisor));
                    cmd.Parameters.Add(new SqlParameter("@departmentId", model.employee.DepartmentId));
                    cmd.ExecuteNonQuery();

                    return RedirectToAction(nameof(Index));
                }
            }
        }
    }
}


