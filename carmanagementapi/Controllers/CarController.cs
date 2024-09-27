using carmanagementapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace carmanagementapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public CarController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult GetCarDetails()
        {
            List<Car> cars = new List<Car>();
            string query = "SELECT * FROM CarModel ORDER BY DateOfManufacturing DESC, SortOrder";

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("dbconn")))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Car car = new Car
                            {
                                Id = Convert.ToInt32(reader["ID"]),
                                Brand = reader["Brand"].ToString(),
                                Class = reader["Class"].ToString(),
                                ModelName = reader["ModelName"].ToString(),
                                ModelCode = reader["ModelCode"].ToString(),
                                Description = reader["Description"].ToString(),
                                Features = reader["Features"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                DateOfManufacturing = Convert.ToDateTime(reader["DateOfManufacturing"]),
                                Active = Convert.ToBoolean(reader["Active"]),
                                SortOrder = Convert.ToInt32(reader["SortOrder"]),
                                Images = new List<string>()
                            };

                            string imagesString = reader["Images"].ToString();
                            if (!string.IsNullOrEmpty(imagesString))
                            {
                                string[] imageArray = imagesString.Split(',');
                                for (int i = 0; i < imageArray.Length; i++)
                                {
                                    car.Images.Add(imageArray[i]);
                                }
                            }

                            cars.Add(car);
                        }
                    }
                }
            }

            return Ok(cars);
        }



        [HttpPost]
        public IActionResult AddCarDetails([FromForm] Car car)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<string> imagePaths = new List<string>();

            var files = Request.Form.Files;
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        var imageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
                        imagePaths.Add(imageUrl);
                    }
                }
            }

            car.Images = imagePaths;

            string query = @"INSERT INTO CarModel (Brand, Class, ModelName, ModelCode, Description, Features, Price, DateOfManufacturing, Active, SortOrder, Images) 
                     VALUES (@Brand, @Class, @ModelName, @ModelCode, @Description, @Features, @Price, @DateOfManufacturing, @Active, @SortOrder, @Images);
                     SELECT CAST(SCOPE_IDENTITY() as int)";

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("dbconn")))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Brand", car.Brand);
                    cmd.Parameters.AddWithValue("@Class", car.Class);
                    cmd.Parameters.AddWithValue("@ModelName", car.ModelName);
                    cmd.Parameters.AddWithValue("@ModelCode", car.ModelCode);
                    cmd.Parameters.AddWithValue("@Description", car.Description);
                    cmd.Parameters.AddWithValue("@Features", car.Features);
                    cmd.Parameters.AddWithValue("@Price", car.Price);
                    cmd.Parameters.AddWithValue("@DateOfManufacturing", car.DateOfManufacturing);
                    cmd.Parameters.AddWithValue("@Active", car.Active);
                    cmd.Parameters.AddWithValue("@SortOrder", car.SortOrder);

                    string imagesString = string.Join(",", car.Images);
                    cmd.Parameters.AddWithValue("@Images", imagesString);

                    con.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    car.Id = newId;
                }
            }

            return CreatedAtAction(nameof(GetCarDetails), new { id = car.Id }, car);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCarDetails(int id, [FromForm] Car car)
        {
            if (id != car.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<string> imagePaths = new List<string>();

            var files = Request.Form.Files;
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        var imageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
                        imagePaths.Add(imageUrl);
                    }
                }
            }

            // Combine new images with existing ones
            if (car.Images == null)
            {
                car.Images = new List<string>();
            }
            car.Images.AddRange(imagePaths);

            string query = @"UPDATE CarModel 
                     SET Brand = @Brand, Class = @Class, ModelName = @ModelName, ModelCode = @ModelCode, 
                         Description = @Description, Features = @Features, Price = @Price, 
                         DateOfManufacturing = @DateOfManufacturing, Active = @Active, SortOrder = @SortOrder, 
                         Images = @Images
                     WHERE ID = @Id";

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("dbconn")))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Brand", car.Brand);
                    cmd.Parameters.AddWithValue("@Class", car.Class);
                    cmd.Parameters.AddWithValue("@ModelName", car.ModelName);
                    cmd.Parameters.AddWithValue("@ModelCode", car.ModelCode);
                    cmd.Parameters.AddWithValue("@Description", car.Description);
                    cmd.Parameters.AddWithValue("@Features", car.Features);
                    cmd.Parameters.AddWithValue("@Price", car.Price);
                    cmd.Parameters.AddWithValue("@DateOfManufacturing", car.DateOfManufacturing);
                    cmd.Parameters.AddWithValue("@Active", car.Active);
                    cmd.Parameters.AddWithValue("@SortOrder", car.SortOrder);

                    string imagesString = string.Join(",", car.Images);
                    cmd.Parameters.AddWithValue("@Images", imagesString);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }
            }

            return NoContent();
        }




        [HttpDelete("{id}")]
        public IActionResult DeleteCarDetails(int id)
        {
            string query = "DELETE FROM CarModel WHERE ID = @Id";

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("dbconn")))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }
            }

            return NoContent();
        }

        [HttpGet("search")]
        public IActionResult SearchCars([FromQuery] string searchTerm)
        {
            List<Car> cars = new List<Car>();
            string query = @"SELECT * FROM CarModel 
                             WHERE ModelName LIKE @searchTerm + '%' OR ModelCode LIKE @searchTerm + '%'
                             ORDER BY DateOfManufacturing DESC, SortOrder";

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("dbconn")))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", searchTerm ?? (object)DBNull.Value);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cars.Add(ReadCarFromReader(reader));
                        }
                    }
                }
            }

            return Ok(cars);
        }

        [HttpGet("sort")]
        public IActionResult SortCars([FromQuery] string sortBy = "DateOfManufacturing")
        {
            List<Car> cars = new List<Car>();
            string query = @"SELECT * FROM CarModel 
                             ORDER BY 
                                CASE 
                                    WHEN @sortBy = 'DateOfManufacturing' THEN CONVERT(VARCHAR, DateOfManufacturing, 121)
                                    WHEN @sortBy = 'SortOrder' THEN CONVERT(VARCHAR, SortOrder)
                                END DESC";

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("dbconn")))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@sortBy", sortBy);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cars.Add(ReadCarFromReader(reader));
                        }
                    }
                }
            }

            return Ok(cars);
        }

        private Car ReadCarFromReader(SqlDataReader reader)
        {
            Car car = new Car
            {
                Id = Convert.ToInt32(reader["ID"]),
                Brand = reader["Brand"].ToString(),
                Class = reader["Class"].ToString(),
                ModelName = reader["ModelName"].ToString(),
                ModelCode = reader["ModelCode"].ToString(),
                Description = reader["Description"].ToString(),
                Features = reader["Features"].ToString(),
                Price = Convert.ToDecimal(reader["Price"]),
                DateOfManufacturing = Convert.ToDateTime(reader["DateOfManufacturing"]),
                Active = Convert.ToBoolean(reader["Active"]),
                SortOrder = Convert.ToInt32(reader["SortOrder"]),
                Images = new List<string>()
            };

            string imagesString = reader["Images"].ToString();
            if (!string.IsNullOrEmpty(imagesString))
            {
                car.Images = imagesString.Split(',').ToList();
            }

            return car;
        }
    }
}