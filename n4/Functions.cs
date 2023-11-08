using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Vaccination;
using System.Diagnostics;

namespace VaccinAssigment
{
    public class Functions
    {
        public string[] CreateVaccinationOrder(string[] input, int doses, bool ageLimit)
        {
            List<Person> healthcareEmployees = new List<Person>();
            List<Person> olderThan65 = new List<Person>();
            List<Person> riskGroup = new List<Person>();
            List<Person> remainingPersons = new List<Person>();

            foreach (string personData in input)
            {
                string[] entries = personData.Split(',');

                string personalNumber = entries[0];
                string lastName = entries[1];
                string firstName = entries[2];
                int healthcareEmployee = int.Parse(entries[3]);
                int risk = int.Parse(entries[4]);
                int infection = int.Parse(entries[5]);

                personalNumber = ConvertToCorrectFormat(personalNumber);

                Person person = new Person
                {
                    PersonalNumber = personalNumber,
                    LastName = lastName,
                    FirstName = firstName,
                    HealthcareEmployee = healthcareEmployee,
                    RiskGroup = risk,
                    Infection = infection,
                };

                if (ageLimit && BirthDate(personalNumber) < 18)
                {
                    continue;
                }

                if (healthcareEmployee == 1)
                {
                    healthcareEmployees.Add(person);
                }
                else if (BirthDate(personalNumber) >= 65)
                {
                    olderThan65.Add(person);
                }
                else if (risk == 1)
                {
                    riskGroup.Add(person);
                }
                else
                {
                    remainingPersons.Add(person);
                }
            }

            // Sort each category individually
            healthcareEmployees.Sort((a, b) =>
            {
                int ageComparison = BirthDate(a.PersonalNumber).CompareTo(BirthDate(b.PersonalNumber));
                if (ageComparison == 0)
                {
                    return string.Compare(a.LastName, b.LastName, StringComparison.Ordinal);
                }
                return ageComparison;
            });

            olderThan65.Sort((a, b) => string.Compare(a.LastName, b.LastName, StringComparison.Ordinal));

            riskGroup.Sort((a, b) =>
            {
                int ageComparison = BirthDate(a.PersonalNumber).CompareTo(BirthDate(b.PersonalNumber));
                if (ageComparison == 0)
                {
                    return string.Compare(a.LastName, b.LastName, StringComparison.Ordinal);
                }
                return ageComparison;
            });

            remainingPersons.Sort((a, b) =>
            {
                int ageComparison = BirthDate(a.PersonalNumber).CompareTo(BirthDate(b.PersonalNumber));
                if (ageComparison == 0)
                {
                    return string.Compare(a.LastName, b.LastName, StringComparison.Ordinal);
                }
                return ageComparison;
            });

            // Concatenate the lists in the desired order
            List<Person> allPersons = healthcareEmployees
                .Concat(olderThan65)
                .Concat(riskGroup)
                .Concat(remainingPersons)
                .ToList();

            List<VaccinPerson> transformedPersons = FilterAndTransformPersons(allPersons, doses);

            string[] csvLines = transformedPersons.Select(person =>
                $"{person.VPersonalNumber},{person.VLastName},{person.VFirstName},{person.VaccinDose}")
                .ToArray();

            return csvLines;
        }
        //converts to YYYYMMDD-NNNN format
        public string ConvertToCorrectFormat(string personalNumber)
        {
            if (personalNumber.Length == 10 || personalNumber.Length == 11)
            {
                string yearPart = personalNumber.Substring(0, 2); // Extract the first two digits of the personal number
                int year = int.Parse(yearPart);

                if (year >= 0 && year <= 24)
                {
                    year += 2000; // Adjust for years between 00 and 22
                }
                else if (year >= 25 && year <= 99)
                {
                    year += 1900;
                }

                string fullYear = year.ToString("D4") + personalNumber.Substring(2, 4) + "-" + personalNumber.Substring(6); // Constructs the full year part

                fullYear = fullYear.Replace("--", "-");

                return fullYear;
            }

            return personalNumber;
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
            // Ensure the number of available doses does not exceed the required doses
            if (availableDoses < persons.Count)
            {
                persons = persons.Take(availableDoses).ToList();
            }

            // Transform filteredPersons to VaccinPerson objects and apply sorting
            List<VaccinPerson> transformedPersons = persons
                .OrderBy(person =>
                {
                    if (person.HealthcareEmployee == 1)
                        return 0; // Healthcare employees first
                    else if (BirthDate(person.PersonalNumber) >= 65)
                        return 1; // Age 65+ next
                    else if (person.RiskGroup == 1)
                        return 2; // Risk group
                    else
                        return 3; // Others
                })
                .ThenByDescending(person => BirthDate(person.PersonalNumber)) // Then sort by age
                .ThenBy(person => person.LastName) // Then sort by last name
                .ThenBy(person => person.FirstName) // Then sort by first name
                .Select(person => new VaccinPerson
                {
                    VPersonalNumber = person.PersonalNumber,
                    VLastName = person.LastName,
                    VFirstName = person.FirstName,
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
