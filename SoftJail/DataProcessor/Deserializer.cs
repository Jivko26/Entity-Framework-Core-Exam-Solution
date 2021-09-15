namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var departments = new List<Department>();

            var departmentCells = JsonConvert.DeserializeObject<List<DepartmentDTO>>(jsonString);

            foreach (var departmentCell in departmentCells)
            {
                if (!IsValid(departmentCell) || !departmentCell.Cells.Any() || !departmentCell.Cells.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");

                    continue;
                }

                var department = new Department
                {
                    Name = departmentCell.Name,
                    Cells = departmentCell.Cells.Select(x => new Cell
                    {
                        CellNumber = x.CellNumber,
                        HasWindow = x.HasWindow
                    }).ToList()
                };

                departments.Add(department);

                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count()} cells");
            }

            context.Departments.AddRange(departments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var prisoners = new List<Prisoner>();

            var prisonerMails = JsonConvert.DeserializeObject<List<PrisonerDTO>>(jsonString);

            foreach (var prisonerMail in prisonerMails)
            {
                if (!IsValid(prisonerMail) || !prisonerMail.Mails.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");

                    continue;
                }

                var incarcerationDate = DateTime.ParseExact(prisonerMail.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var IsValidReleaseDate = DateTime.TryParseExact(prisonerMail.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture
                    , DateTimeStyles.None, out DateTime releaseDate);

                var prisoner = new Prisoner
                {
                    FullName = prisonerMail.FullName,
                    Nickname = prisonerMail.Nickname,
                    Age = prisonerMail.Age,
                    IncarcerationDate = incarcerationDate,
                    ReleaseDate = IsValidReleaseDate ? (DateTime?)releaseDate : null,
                    Bail = prisonerMail.Bail,
                    Mails = prisonerMail.Mails.Select(x => new Mail
                    {
                        Description = x.Description,
                        Sender = x.Sender,
                        Address = x.Address
                    }).ToList()
                };

                prisoners.Add(prisoner);

                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var officers = new List<Officer>();

            var officersPrisoners = XmlConverter.Deserializer<OfficerDTO>(xmlString, "Officers");

            foreach (var officerPrisoner in officersPrisoners)
            {
                if (!IsValid(officerPrisoner))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var officer = new Officer
                {
                    FullName = officerPrisoner.Name,
                    Salary = officerPrisoner.Money,
                    Position = Enum.Parse<Position>(officerPrisoner.Position),
                    Weapon = Enum.Parse<Weapon>(officerPrisoner.Weapon),
                    DepartmentId = officerPrisoner.DepartmentId,
                    OfficerPrisoners = officerPrisoner.Prisoners.Select(p => new OfficerPrisoner { PrisonerId = p.Id }).ToList(),
                };

                officers.Add(officer);

                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count()} prisoners)");
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}