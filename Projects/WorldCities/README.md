# World Cities



## Chapter 4 Data Model with Entity Framework Core

### Creating .NET and Angular Project

- Run `dotnet new angular -o WorldCities`

### Edit or delete the following .NET backend files

- WeatherForecast.cs (delete)

- /Controllers/WeatherForecastController.cs (delete)

### Edit or delete the following .Angular frontend files

- /ClientApp/src/app/counter/ (delete - whole folder)

- /ClientApp/src/app/fetch-data/ (delete - whole folder)

### Create Sepeate App Routing Module

- Create `app-routing.module.ts` in `src\app`

- Import `app-routing.module.ts` in `app.module.ts`

### Display image in home component

- Download [World Cities image](https://www.pexels.com/photo/close-up-of-globe-335393) and place it in `wwwroot/img` folder.

- Edit home component to include image

### Download World Cities data

- download excel from this [source](https://simplemaps.com/data/world-cities)

### Importing the Excel files

- Install `EPPlus`

- Move Excel file to `/Data/Source/`

### Install Entity Framework Core

- Install `Microsoft.EntityFrameworkCore`

- Install `Microsoft.EntityFrameworkCore.Tools`

- Install `Microsoft.EntityFrameworkCore.SqlServer`

### Create City Entity

```cs
using System.ComponentModel.DataAnnotations;

namespace WorldCities.Data.Models
{
    public class City
    {
        #region Constructor
        public City()
        {

        }
        #endregion

        #region Properties
        /// <summary>
        /// The uniqued id and primary key for this City
        /// </summary>
        [Key]
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// City name (in UTF8 format)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// City name (in ASCII format)
        /// </summary>
        public string Name_ASCII { get; set; }

        /// <summary>
        /// City Latitude
        /// </summary>
        public decimal Lat { get; set; }

        /// <summary>
        /// City Longitude
        /// </summary>
        public decimal Lon { get; set; }
        #endregion

        /// <summary>
        /// Country Id (foreign key)
        /// </summary>
        public int CountryId { get; set; }
    }
}
```

### Create Country Entity

```cs
using System.ComponentModel.DataAnnotations;

namespace WorldCities.Data.Models
{
    public class Country
    {
        #region Constructor
        public Country()
        {

        }
        #endregion

        #region Properties
        /// <summary>
        /// The unique id and primary key for this country
        /// </summary>
        [Key]
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Country name (in UTF8 format)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Country code (in ISO 3166-1 ALPHA-2 format)
        /// </summary>
        public string ISO2 { get; set; }

        /// <summary>
        /// Country code (in ISO 3166-1 ALPHA-3 format)
        /// </summary>
        public string ISO3 { get; set; }
        #endregion
    }
}
```

### Define Relationship

- Add below code in City

```cs
[ForeignKey(nameof(Country))]
public int CountryId { get; set; }

#region Navigation Properties
/// <summary>
/// The country related to this city
/// </summary>
public virtual Country Country { get; set; }
#endregion
```

- Add below code in Country

```cs
#region Navigation Properties
public virtual List<City> Cities { get; set; }
#endregion
```

### Define database table names

- For the City Entity

```cs
[Table("Cities")]
public class City
```

- For the Country Entity

```cs
[Table("Countries")]
public class Country
```

### Create SQL Database Instance

- Create a database `WorldCities`

### Add Login

- Expand Security

- Right click login and choose new login

- Choose SQL Server Authentication

- Username: WorldCities

- Password: WorldCities

- Disable Enforce password policy

- Select default database is `WorldCities`

### Map Login to the database

- Click the WorldCities Login

- Click User Mapping

- Click Checkbox of WorldCities

- In role membership, assign db_owner

### Settip up DBContext

- create `ApplicationDbContext.cs` and add below code

```cs
using Microsoft.EntityFrameworkCore;
using WorldCities.Data.Models;

namespace WorldCities.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(): base()
        {

        }

        public ApplicationDbContext(DbContextOptions options): base(options)
        {

        }

        public DbSet<City> Cities { get; set; }

        public DbSet<Country> Countries { get; set; }
    }
}

```

### Update Connection string in appsettings.json

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=WorldCities;User Id=WorldCities;Password=WorldCities;Integrated Security=False;MultipleActiveResultSets=True"
  }
```

### Store connection string in user secret

- right click solution and click manage user secret

- cut connection string from `appsettings.json` to `secrets.json` 

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=WorldCities;User Id=WorldCities;Password=WorldCities;Integrated Security=False;MultipleActiveResultSets=True"
}
```

- secrets.json is stored in `\Users\UserName\AppData\Roaming]Microsoft\UserSecrets\`

### Update Startup.cs

- add ApplicationDbContext in `ConfigureServices` function in `Startup.cs`

```cs
// Add ApplicationDbContext and SQL Server support
services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(
        Configuration.GetConnectionString("DefaultConnection")
    )
);
```

### Add Initial Migration

- Run `dotnet ef migrations add "Initial" -o "Data/Migrations"`

- Error message is showed ` No type was specified for the decimal property 'Lat' on entity type 'City'`

- Add type for decimal in City.cs

```cs
[Column(TypeName="decimal(7,4)")]
public decimal Lat { get; set; }

[Column(TypeName = "decimal(7,4)")]
public decimal Lon { get; set; }
```
- Run `dotnet ef migrations add "Initial" -o "Data/Migrations"` again

### Updating the database

- Run `dotnet ef database update`

### Implement SeedController

- Create SeedController with code below

```cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using WorldCities.Data;
using WorldCities.Data.Models;

namespace WorldCities.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SeedController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [HttpGet]
        public async Task<ActionResult> Import()
        {
            // prevents non-development environments from running this method
            if (!_env.IsDevelopment())
                throw new SecurityException("Not allowed");

            var path = Path.Combine(_env.ContentRootPath, "Data/Source/worldcities.xlsx");

            using var stream = System.IO.File.OpenRead(path);
            using var excelPackage = new ExcelPackage(stream);

            // get the first worksheet
            var worksheet = excelPackage.Workbook.Worksheets[0];

            // define how many rows we want to pocess
            var nEndRow = worksheet.Dimension.End.Row;

            // initialize the record counters
            var numberOfCountriesAdded = 0;
            var numberOfCitiesAdded = 0;

            // create a lookup dictionary
            // containing all the countries already existing
            // into the Database (it will be empty on first run).
            var countriesByName = _context.Countries
                .AsNoTracking()
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            // iterates through all rows, skipping the first one
            for (int nRow = 2; nRow <= nEndRow; nRow++)
            {
                var row = worksheet.Cells[nRow, 1, nRow, worksheet.Dimension.End.Column];
                var countryName = row[nRow, 5].GetValue<string>();
                var iso2 = row[nRow, 6].GetValue<string>();
                var iso3 = row[nRow, 7].GetValue<string>();

                //skip this country if it already exists in the database
                if (countriesByName.ContainsKey(countryName))
                    continue;

                // create the Country entity and fill it with xlsx data
                var country = new Country
                {
                    Name = countryName,
                    ISO2 = iso2,
                    ISO3 = iso3
                };

                // add the new country to the DB Context
                await _context.Countries.AddAsync(country);

                // store the country in our Lookup to retrieve its Id Later on
                countriesByName.Add(countryName, country);

                // increment the counter
                numberOfCountriesAdded++;
            }

            if (numberOfCountriesAdded > 0)
                await _context.SaveChangesAsync();

            // create a Lookup dictionary
            // containing all the cities already existing
            // into the Database (it will be empty on first run)
            var cities = _context.Cities
                .AsNoTracking()
                .ToDictionary(x => (
                    Name: x.Name,
                    Lat: x.Lat,
                    Lon: x.Lon,
                    CountryId: x.CountryId
                ));

            // iterates through all rows, skipping the first one
            for (int nRow = 2; nRow <= nEndRow; nRow++)
            {
                var row = worksheet.Cells[nRow, 1, nRow, worksheet.Dimension.End.Column];
                var name = row[nRow, 1].GetValue<string>();
                var nameAscii = row[nRow, 2].GetValue<string>();
                var lat = row[nRow, 3].GetValue<decimal>();
                var lon = row[nRow, 4].GetValue<decimal>();
                var countryName = row[nRow, 5].GetValue<string>();

                // retrieve country Id by countryName
                var countryId = countriesByName[countryName].Id;

                // skip this city if it already exists in the database
                if (cities.ContainsKey((Name: name, Lat: lat, Lon: lon, CountryId: countryId)))
                    continue;

                // create the City entity and fill it with xlsx data
                var city = new City
                {
                    Name = name,
                    Name_ASCII = nameAscii,
                    Lat = lat,
                    Lon = lon,
                    CountryId = countryId
                };

                // add the new city to the DB Context
                _context.Cities.Add(city);

                // increment the counter
                numberOfCitiesAdded++;
            }

            // save all the cities into the Database
            if (numberOfCitiesAdded > 0)
                await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                Cities = numberOfCitiesAdded,
                Countries = numberOfCountriesAdded
            });
        }
    }
}
```

- `Run` and call this api `/api/Seed/Import`

- data are imported to SQL

### Implement CitiesController

- Create CitiesController using this option `API Controller with actions, using Entity Framework`
   
    - Model class: City

    - Data context class: Application DbContext

    - Controller name: CitiesController

- Below code is generated by default

```cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCities.Data;
using WorldCities.Data.Models;

namespace WorldCities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Cities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            return await _context.Cities.ToListAsync();
        }

        // GET: api/Cities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<City>> GetCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);

            if (city == null)
            {
                return NotFound();
            }

            return city;
        }

        // PUT: api/Cities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCity(int id, City city)
        {
            if (id != city.Id)
            {
                return BadRequest();
            }

            _context.Entry(city).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Cities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<City>> PostCity(City city)
        {
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCity", new { id = city.Id }, city);
        }

        // DELETE: api/Cities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CityExists(int id)
        {
            return _context.Cities.Any(e => e.Id == id);
        }
    }
}
```

### Implement CountriesController

- Create CountriesController using this option `API Controller with actions, using Entity Framework`
   
    - Model class: Country

    - Data context class: Application DbContext

    - Controller name: CountriesController

- Below code is generated by default

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCities.Data;
using WorldCities.Data.Models;

namespace WorldCities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CountriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            return await _context.Countries.ToListAsync();
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            return country;
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.Id)
            {
                return BadRequest();
            }

            _context.Entry(country).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }
    }
}
```