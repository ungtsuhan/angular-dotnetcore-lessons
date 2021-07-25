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