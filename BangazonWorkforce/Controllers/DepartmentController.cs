using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BangazonWorkforce.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace BangazonWorkforce.Controllers
{
    ///  Controller for departments with create and read functionality.


    public class DepartmentsController : Controller
    {
        private readonly IConfiguration _config;

        public DepartmentsController(IConfiguration config)
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
        // GET ALL DEPARTMENTS

        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
            SELECT t.Id,
                t.Name,
                t.Budget,
                E.Id AS 'Employee Id',
                E.FirstName,
                E.LastName,
                E.DepartmentId
            FROM Department t LEFT JOIN Employee E ON E.DepartmentId = t.Id
        ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Department> departments = new List<Department>();
                    while (reader.Read())
                    {
                        Department department = new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            Employees = new List<Employee>()
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("Employee Id")))
                        {
                            Employee employee = new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Employee Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"))
                            };
                            if (departments.Any(d => d.Id == department.Id))
                            {
                                Department departmentToReference = departments.Where(d => d.Id == department.Id).FirstOrDefault();

                                if (!departmentToReference.Employees.Any(s => s.Id == employee.Id))
                                {
                                    departmentToReference.Employees.Add(employee);
                                }
                            }
                            else
                            {
                                department.Employees.Add(employee);
                                departments.Add(department);
                            }
                        }
                        else if (department.Employees.Count() == 0)
                        {
                            departments.Add(department);
                        };
                    }
                    reader.Close();
                    return View(departments);
                }

            }
        }

        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" Select d.Id as 'departmentId',
                    d.Name, d.Budget,
                     e.Id AS 'Employee Id', e.FirstName,
                      e.LastName, e.DepartmentId FROM Department d
                        FULL JOIN Employee e on e.departmentId = d.id WHERE d.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Department department = null;

                    while (reader.Read())
                    {
                        if(department == null) 
                        {
                            department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("departmentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                                Employees = new List<Employee>()
                            };
                        };
                        //this will add an employee if it exists in the Dept Emp list
                        if(!reader.IsDBNull(reader.GetOrdinal("Employee Id")))
                        {
                            Employee employee = new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Employee Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"))
                            };
                            department.Employees.Add(employee);
                        }
                            }

                    reader.Close();
                    return View(department);
                }
            }
        }

        // GETPOST: Department/
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department(Name, Budget)
VALUES
(@Name,@Budget)";
                    cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                    cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));
                    cmd.ExecuteNonQuery();
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        // GET: Department/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Department/Edit/5
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

        // GET: Department/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Department/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}