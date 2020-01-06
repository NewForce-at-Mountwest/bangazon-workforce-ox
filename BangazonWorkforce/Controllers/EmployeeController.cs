using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonWorkforce.Models;
//using BangazonWorkforce.Models.ViewModels;





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
        // GET: Students
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                     SELECT e.Id,
                     e.FirstName,
                    e.LastName,
                    e.DepartmentId,
                    d.Name,
                    e.IsSuperVisor
                    FROM Employee e
                    LEFT JOIN Department d ON e.DepartmentId = d.Id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
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

                        employees.Add(employee);
                    }

                    reader.Close();

                    return View(employees);
                }
            }
        }
    }
}

        //// GET: Students/Details/5
        //public ActionResult Details(int id)
        //{

        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                SELECT
        //                    Id, FirstName, LastName, SlackHandle, CohortId
        //                FROM Student
        //                WHERE Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));
        //            SqlDataReader reader = cmd.ExecuteReader();

        //            Student student = new Student();

        //            if (reader.Read())
        //            {
        //                student = new Student
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
        //                    SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
        //                    CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))

        //                };
        //            }
        //            reader.Close();


        //            return View(student);
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