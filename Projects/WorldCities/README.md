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
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=WorldCities;User Id=WorldCities;Password=WorldCities;Integrated Securit=False;MultipleActiveResltSets=True"
  }
```

### Store connection string in user secret

- right click solution and click manage user secret

- cut connection string from `appsettings.json` to `secrets.json` 

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=WorldCities;User Id=WorldCities;Password=WorldCities;Integrated Securit=False;MultipleActiveResltSets=True"
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