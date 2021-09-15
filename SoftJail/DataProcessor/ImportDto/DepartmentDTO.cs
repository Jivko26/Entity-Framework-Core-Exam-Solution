using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    public class DepartmentDTO
    {
        [Required]
        [StringLength(25, MinimumLength = 3)]
        public string Name { get; set; }

        public List<CellDTO> Cells { get; set; }
    }

    //•	Id – integer, Primary Key
    //•	Name – text with min length 3 and max length 25 (required)
    //•	Cells - collection of type Cell
    public class CellDTO
    {
        [Range(1, 1000)]
        public int CellNumber { get; set; }

        public bool HasWindow { get; set; }
    }

    //    •	Id – integer, Primary Key
    //•	CellNumber – integer in the range[1, 1000] (required)
    //•	HasWindow – bool (required)
    //•	DepartmentId - integer, foreign key(required)
    //•	Department – the cell's department (required)
    //•	Prisoners - collection of type Prisoner
}
