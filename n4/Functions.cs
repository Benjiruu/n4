using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Vaccination;

namespace VaccinAssigment
{
    public class Functions
    {
        public string[] CreateVaccinationOrder(string[] input, int doses, bool ageLimit)
        {
            List<Person> persons = PersonsToList(input, ageLimit);
            List<VaccinPerson> transformedPersons = FilterAndTransformPersons(persons, doses);
            
            // Calculate the remaining doses
            int remainingDoses = doses - transformedPersons.Sum(person => person.VaccinDose);
            
            // Transform filtered persons to CSV lines
            string[] csvLines = transformedPersons.Select(person =>
                $"{person.PersonalNumber},{person.LastName},{person.FirstName},{person.VaccinDose}")
                .ToArray();

            csvLines = csvLines.Append($"Remaining Doses: {remainingDoses}").ToArray();

            return csvLines;
        }

        private List<Person> PersonsToList(string[] input, bool ageLimit)
        {
            List<Person> persons = new List<Person>();

            foreach (string person in input)
            {
                string[] entries = person.Split(',');

                string personalNumber = entries[0];
                string lastName = entries[1];
                string firstName = entries[2];
                int healthcareEmployee = int.Parse(entries[3]);
                int riskGroup = int.Parse(entries[4]);
                int infection = int.Parse(entries[5]);

                if (ageLimit && BirthDate(personalNumber) < 18)
                {
                    // Skip individuals under 18 if age limit is enabled
                    continue;
                }

                var people = new Person
                {
                    PersonalNumber = personalNumber,
                    LastName = lastName,
                    FirstName = firstName,
                    HealthcareEmployee = healthcareEmployee,
                    RiskGroup = riskGroup,
                    Infection = infection,
                };

                persons.Add(people);
            }

            return persons;
        }

        private List<VaccinPerson> FilterAndTransformPersons(List<Person> persons, int availableDoses)
        {
            // Sort the unique persons
            List<Person> filteredPersons = persons
                .OrderBy(person => BirthDate(person.PersonalNumber))
                .ThenBy(person => person.PersonalNumber)
                .ToList();

            // Ensure the number of available doses does not exceed the required doses
            if (availableDoses < filteredPersons.Count)
            {
                filteredPersons = filteredPersons.Take(availableDoses).ToList();
            }

            // Transform filteredPersons to VaccinPerson objects
            List<VaccinPerson> transformedPersons = filteredPersons
                .Select(person => new VaccinPerson
                {
                    PersonalNumber = person.PersonalNumber,
                    LastName = person.LastName,
                    FirstName = person.FirstName,
                    VaccinDose = (2 - person.Infection) // Calculate the number of vaccine doses needed
                })
                .ToList();

            return transformedPersons;
        }

        private int BirthDate(string personalNumber)
        {
            DateTime today = DateTime.Today;

            int year = int.Parse(personalNumber.Substring(0, 4));
            int month = int.Parse(personalNumber.Substring(4, 2));
            int day = int.Parse(personalNumber.Substring(6, 2));

            DateTime birthDate = new DateTime(year, month, day);
            int result = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-result))
            {
                result--;
            }

            return result;
        }
    }
}
