﻿using System.Collections.Generic;
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
        
        #region Navigation Properties
        public virtual List<City> Cities { get; set; }
        #endregion
    }
}
