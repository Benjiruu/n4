using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VaccinAssigment;

namespace Vaccination
{
    public class Person
    {
        public string PersonalNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int HealthcareEmployee { get; set; }
        public int RiskGroup { get; set; }
        public int Infection { get; set; }
    }
    public class VaccinPerson
    {
        public string VPersonalNumber { get; set; }
        public string VLastName { get; set; }
        public string VFirstName { get; set; }
        public int VaccinDose { get; set; }
    }
}
