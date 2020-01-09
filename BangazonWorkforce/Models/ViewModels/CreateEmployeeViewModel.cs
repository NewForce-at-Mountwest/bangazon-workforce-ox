using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonWorkforce.Models.ViewModels
{
    public class CreateEmployeeViewModel
    {
        // This is where our dropdown options will go! SelectListItem is a built in type for dropdown lists
        public List<SelectListItem> Departments { get; set; }


        // An individual student. When we render the form (i.e. make a GET request to Students/Create) this will be null. When we submit the form (i.e. make a POST request to Students/Create), this will hold the data from the form.
        public Employee employee { get; set; }

        // Connection to the database
        protected string _connectionString;

        protected SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_connectionString);
            }
        }

        // Empty constructor so that we can create a new instance of this view model when we make our POST request (in which case it won't need a connection string)
        public CreateEmployeeViewModel() { }

        // This is an example of method overloading! We have one constructor with no parameter and another constructor that's expecting a connection string. We can call either one!
        public CreateEmployeeViewModel(string connectionString)
        {
            _connectionString = connectionString;

            // When we create a new instance of this view model, we'll call the internal methods to get all the cohorts from the database
            // Then we'll map over them and convert the list of cohorts to a list of select list items
            Departments = GetAllDepartments()
                .Select(department => new SelectListItem()
                {
                    Text = department.Name,
                    Value = department.Id.ToString()

                })
                .ToList();

            // Add an option with instructiosn for how to use the dropdown
            Departments.Insert(0, new SelectListItem
            {
                Text = "Choose a department",
                Value = "0"
            });

        }

        // Internal method -- connects to DB, gets all cohorts, returns list of cohorts
        protected List<Department> GetAllDepartments()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Department";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Department> departments = new List<Department>();
                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        });
                    }

                    reader.Close();

                    return departments;
                }
            }
        }
    }
}
