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
    }
}



//        //// GET: Students/Details/5
//        public ActionResult Details(int id)
//        {

//            using (SqlConnection conn = Connection)
//            {
//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"
//                                SELECT e.Id AS EmployeeId, 
//                                e.FirstName, 
//                                e.LastName, 
//                                d.[Name] AS DepartmentName, 
//                                c.Id AS ComputerId, 
//                                c.Make, 
//                                c.Manufacturer,
//                                ce.Id AS ComputerEmployeeId,
//                                ce.AssignDate,
//                                ce.UnassignDate,
//                                tp.Id AS TrainingProgramId,
//                                tp.Name AS TrainingProgramName
//                                FROM Employee e
//                                LEFT JOIN Department d ON e.DepartmentId = d.Id
//                                LEFT JOIN ComputerEmployee ce ON ce.EmployeeId = e.id
//                                LEFT JOIN Computer c ON c.id = ce.ComputerId
//                                LEFT JOIN EmployeeTraining et ON et.EmployeeId = e.Id
//                                LEFT JOIN TrainingProgram tp ON tp.Id = et.TrainingProgramId
//                                WHERE e.Id = @id";

//                    cmd.Parameters.Add(new SqlParameter("@id", id));

//                    SqlDataReader reader = cmd.ExecuteReader();

//                    Employee employee = new Employee();

//                    while (reader.Read())
//                    {
//                        employee = new Employee
//                        {
//                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
//                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
//                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
//                            CurrentDepartment = new Department
//                            {
//                                Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
//                            }
//                        };
//                    }

//                    if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")) && (reader.IsDBNull(reader.GetOrdinal("UnassignDate"))))
//                    {
//                        AssignedComputer = new Computer
//                        {
//                            Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
//                            Make = reader.GetString(reader.GetOrdinal("Make")),
//                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
//                        };
//                    }

//                    reader.Close();

//                    return View(employee);
//                }
//            }
//        }
//    }
//}

////GET: Students/Create
//public ActionResult Create()
//{
//    // Create a new instance of a CreateStudentViewModel
//    // If we want to get all the cohorts, we need to use the constructor that's expecting a connection string. 
//    // When we create this instance, the constructor will run and get all the cohorts.
//    CreateStudentViewModel studentViewModel = new CreateStudentViewModel(_config.GetConnectionString("DefaultConnection"));

//    // Once we've created it, we can pass it to the view
//    return View(studentViewModel);
//}

//// POST: Students/Create

//[HttpPost]
//[ValidateAntiForgeryToken]
//public async Task<ActionResult> Create(CreateStudentViewModel model)
//{
//    using (SqlConnection conn = Connection)
//    {
//        conn.Open();
//        using (SqlCommand cmd = conn.CreateCommand())
//        {
//            cmd.CommandText = @"INSERT INTO Student
//        ( FirstName, LastName, SlackHandle, CohortId )
//        VALUES
//        ( @firstName, @lastName, @slackHandle, @cohortId )";
//            cmd.Parameters.Add(new SqlParameter("@firstName", model.student.FirstName));
//            cmd.Parameters.Add(new SqlParameter("@lastName", model.student.LastName));
//            cmd.Parameters.Add(new SqlParameter("@slackHandle", model.student.SlackHandle));
//            cmd.Parameters.Add(new SqlParameter("@cohortId", model.student.CohortId));
//            cmd.ExecuteNonQuery();

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}

//// GET: Students/Edit/5
//public ActionResult Edit(int id)
//{
//    // Create a new instance of a StudentEditViewModel
//    // Pass it the studentId and a connection string to the database
//    StudentEditViewModel viewModel = new StudentEditViewModel(id, _config.GetConnectionString("DefaultConnection"));

//    // The view model's constructor will work its magic
//    // Pass the new instance of the view model to the view

//    return View(viewModel);
//}

// POST: Students/Edit/5
//[HttpPost]
//[ValidateAntiForgeryToken]
//public ActionResult Edit(int id, StudentEditViewModel studentViewModel)
//{
//    try
//    {
//        using (SqlConnection conn = Connection)
//        {
//            conn.Open();
//            using (SqlCommand cmd = conn.CreateCommand())
//            {

//                // First, update the student's information, including their cohortId
//                // Wipe out all their previously assigned exercises in the join table
//                string command = @"UPDATE Student
//                                    SET firstName=@firstName, 
//                                    lastName=@lastName, 
//                                    slackHandle=@slackHandle, 
//                                    cohortId=@cohortId
//                                    WHERE Id = @id
//                                    DELETE FROM StudentExercise WHERE studentId =@id";

//                // Loop over the selected exercises and add a new entry for each exercise
//                studentViewModel.SelectedExercises.ForEach(exerciseId =>
//                {
//                    command += $" INSERT INTO StudentExercise (StudentId, ExerciseId) VALUES (@id, {exerciseId})";

//                });
//                cmd.CommandText = command;
//                cmd.Parameters.Add(new SqlParameter("@firstName", studentViewModel.Student.FirstName));
//                cmd.Parameters.Add(new SqlParameter("@lastName", studentViewModel.Student.LastName));
//                cmd.Parameters.Add(new SqlParameter("@slackHandle", studentViewModel.Student.SlackHandle));
//                cmd.Parameters.Add(new SqlParameter("@cohortId", studentViewModel.Student.CohortId));
//                cmd.Parameters.Add(new SqlParameter("@id", id));

//                int rowsAffected = cmd.ExecuteNonQuery();

//            }

//        }

//        return RedirectToAction(nameof(Index));
//    }
//    catch
//    {
//        return View(studentViewModel);
//    }
//}

//        // GET: Students/Delete/5
//        public ActionResult Delete(int id)
//        {
//            using (SqlConnection conn = Connection)
//            {
//                conn.Open();
//                using (SqlCommand cmd = conn.CreateCommand())
//                {
//                    cmd.CommandText = @"
//                        SELECT
//                            Id, firstName, lastName, slackHandle, cohortId
//                        FROM Student
//                        WHERE Id = @id";
//                    cmd.Parameters.Add(new SqlParameter("@id", id));
//                    SqlDataReader reader = cmd.ExecuteReader();

//                    Student Student = null;

//                    if (reader.Read())
//                    {
//                        Student = new Student
//                        {
//                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
//                            FirstName = reader.GetString(reader.GetOrdinal("firstName")),
//                            LastName = reader.GetString(reader.GetOrdinal("lastName")),
//                            SlackHandle = reader.GetString(reader.GetOrdinal("slackHandle")),
//                            CohortId = reader.GetInt32(reader.GetOrdinal("cohortId"))

//                        };
//                    }
//                    reader.Close();

//                    return View(Student);
//                }
//            }
//        }

//        // POST: Students/Delete/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Delete(int id, IFormCollection collection)
//        {
//            try
//            {

//                using (SqlConnection conn = Connection)
//                {
//                    conn.Open();
//                    using (SqlCommand cmd = conn.CreateCommand())
//                    {
//                        cmd.CommandText = @"DELETE FROM StudentExercise WHERE studentId = @id
//                        DELETE FROM Student WHERE Id = @id";
//                        cmd.Parameters.Add(new SqlParameter("@id", id));

//                        int rowsAffected = cmd.ExecuteNonQuery();

//                    }
//                }

//                return RedirectToAction(nameof(Index));
//            }
//            catch
//            {
//                return View();
//            }
//        }
//    }
//}