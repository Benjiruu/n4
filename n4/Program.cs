using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VaccinAssigment;

public class Vaccin
{
    private static bool running = true;
    private static int vaccinAmount = 0;
    private static bool ageLimit = false;
    private static string ageDisplay = "Nej";

    private static string inputCSVPath = @"D:\2023\Progamering\C#\Inlamning4\NyaFiler\Test.csv";
    private static string outdataCSVPath = @"D:\2023\Progamering\C#\Inlamning4\NyaFiler\Vaccinations.csv";

    public static void Main()
    {


        while (running)
        {

            Console.WriteLine("Huvudmeny\n");
            Console.WriteLine($"Antal vaccindoser: {vaccinAmount}");
            Console.WriteLine($"Åldersgräns 18 år: {ageDisplay}");
            Console.WriteLine($"Indata: {inputCSVPath}");
            Console.WriteLine($"Utdata: {outdataCSVPath}");

            int option = ShowMenu("\nAlternativ", new[]
            {
                    "Ändra antal vaccindoser",
                    "Ändra åldersgräns",
                    "Skapa prioritetsordning",
                    "Ändra indata sökväg",
                    "Ändra utdata sökväg",
                    "Avsluta"
            });
            Console.Clear();

            Action Navigate = option switch
            {
                0 => new Action(AddVaccinAmount),
                1 => new Action(ChangeAgeLimit),
                2 => new Action(CreatePriorityOrder),
                3 => new Action(IndataFileSearchChange),
                4 => new Action(OutDataFileSearchChange),
                5 => new Action(Exit)
            };
            Navigate.Invoke();
        }
    }

    public static void AddVaccinAmount()
    {
        Console.Clear();
        while (true)
        {

            Console.Write("Ändra antalet vaccindoser: ");
            string inputAmount = Console.ReadLine();

            if (inputAmount != null && int.TryParse(inputAmount, out int result))
            {
                vaccinAmount += result;
                Console.WriteLine("Antal vaccindoser uppdaterat till " + vaccinAmount);
                Console.WriteLine("Klicka på Enter för att komma till huvudmeny");
                Console.ReadKey();

                break;
            }
            else
            {
                Console.WriteLine("Skriv en hel siffra");

            }

        }
    }

    public static void ChangeAgeLimit()
    {
        Console.Clear();
        int option = ShowMenu("Ändra Åldersgräns", new[]
        {
            "Sätt åldersgräns",
            "Ingen åldersgräns"
        });
        if (option == 1)
        {
            ageLimit = true;
            ageDisplay = "Ja";
            Console.WriteLine("Ändrad till vaccinera personer under 18 år");
            Console.WriteLine("Klicka på Enter för att komma till huvudmeny");
            Console.ReadKey();

        }
        else
        {
            ageLimit = false;
            ageDisplay = "Nej";
            Console.WriteLine("Ändrad till vaccinera inte personer under 18 år");
            Console.WriteLine("Klicka på Enter för att komma till huvudmeny");
            Console.ReadKey();

        }
    }

    public static void CreatePriorityOrder()
    {

        Functions functions = new Functions();
        string[] input = File.ReadAllLines(inputCSVPath);

        try
        {
            ErrorHandle(input);

            //Confirmation for the user to overwrite output file 
            if (File.Exists(outdataCSVPath))
            {
                Console.WriteLine("Utgångsfilen finns redan.");

                int option = ShowMenu("Vill du skriva över filen?", new[]
                {
                "Ja",
                "Nej"
            });

                if (option == 0)
                {
                    // User selected "Ja," proceed with overwriting
                    Console.WriteLine("Skriver över utdatafilen..");
                    Console.WriteLine("Klicka på Enter för att komma till huvudmeny");
                    Console.ReadKey();

                }
                else if (option == 1)
                {
                    // User selected "Nej," return to the main menu
                    Console.WriteLine("Åtgärden avbröts.");
                    Console.WriteLine("Klicka på Enter för att komma till huvudmeny");
                    Console.ReadKey();

                    return;
                }
            }

            string[] outPut = functions.CreateVaccinationOrder(input, vaccinAmount, ageLimit);

            int totalDosesUsed = outPut
                .Select(x => x.Split(','))
                .Where(x => int.Parse(x[3]) > 0 && vaccinAmount > 1)
                .Sum(x => int.Parse(x[3]));

            vaccinAmount -= totalDosesUsed;

            File.WriteAllLines(outdataCSVPath, outPut);
            Console.WriteLine("Prioriteringsordning har skapats och sparats.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void IndataFileSearchChange()
    {
        Console.Clear();
        while (true)
        {
            Console.Write("Välj ny sökväg för Indata: ");
            string changeFileInputPath = Console.ReadLine();

            inputCSVPath = changeFileInputPath;

            if (File.Exists(inputCSVPath))
            {
                Console.WriteLine("Ny sökväg tillagt");
                Console.WriteLine("Klicka på Enter för att komma till huvudmeny");
                Console.ReadKey();
                break;
            }
            else
            {
                Console.WriteLine("Mappen existerar inte eller ogiltig sökväg, testa igen!");

            }
        }
    }

    public static void OutDataFileSearchChange()
    {
        Console.Clear();
        while (true)
        {
            Console.Write("Välj ny sökväg för Utdata: ");
            string changeFileOutputPath = Console.ReadLine();

            if (File.Exists(changeFileOutputPath))
            {
                outdataCSVPath = changeFileOutputPath;
                Console.WriteLine("Ny sökväg tillagt");
                Console.WriteLine("Klicka på Enter för att komma till huvudmeny");
                Console.ReadKey();
                break;
            }
            else
            {
                Console.WriteLine("Mappen existerar inte eller ogiltig sökväg, testa igen!");
            }
        }
    }


    private static void Exit()
    {
        running = false;
    }

    public static bool IsAllDigits(string input)
    {
        int counter = 0;

        foreach (char c in input)
        {
            if (c == '-' && counter < 1)
            {
                counter++;
            }
            else if (!char.IsDigit(c))
            {
                return false;
            }
        }
        return true;
    }

    public static void ErrorHandle(string[] inputs)
    {
        foreach (string line in inputs)
        {
            string[] input = line.Split(',');



            if (input.Length != 6)
            {
                throw new FormatException("För få värden som är separerade med ,");
            }

            string personalNumberCheck = @"^\d{8}-\d{4}$|^\d{10}-\d{4}$|^\d{12}$|^\d{10}$";
            string names = "^[a-zA-ZåäöÅÄÖ]+$";
            string value = "^[01]$";

            if (input[0] == null || !Regex.IsMatch(input[0], personalNumberCheck) || !IsAllDigits(input[0]))
            {

                throw new FormatException($"Index 0.{input[0]} Felaktigt format på personnummer. Accepterade format: YYYYMMDD-NNNN, YYMMDD-NNNN, YYYYMMDDNNNN, YYMMDDNNNN");
            }


            if (input[1] == null || !Regex.IsMatch(input[1], names))
            {
                throw new FormatException("Index 1. Efternamnetamnet är felaktigt. Accepterade tecken: A-Ö, a-ö");
            }

            if (input[2] == null || !Regex.IsMatch(input[2], names))
            {
                throw new FormatException("Index 2. Förnamnet är felaktig. Accepterade tecken: A-Ö, a-ö");
            }

            if (input[3] == null || !Regex.IsMatch(input[3], value))
            {
                throw new FormatException("Index 3. Innehåller felaktigt värde. Accepterade värden: 0, 1");
            }

            if (input[4] == null || !Regex.IsMatch(input[4], value))
            {
                throw new FormatException("Index 4. Innehåller felaktigt värde. Accepterade värden: 0, 1");
            }

            if (input[5] == null || !Regex.IsMatch(input[5], value))
            {
                throw new FormatException("Index 5. Innehåller felaktigt värde. Accepterade värden: 0, 1");
            }
        }
    }



    public static int ShowMenu(string prompt, IEnumerable<string> options)
    {
        if (options == null || options.Count() == 0)
        {
            throw new ArgumentException("Cannot show a menu for an empty list of options.");
        }

        Console.WriteLine(prompt);

        // Hide the cursor that will blink after calling ReadKey.
        Console.CursorVisible = false;

        // Calculate the width of the widest option so we can make them all the same width later.
        int width = options.Max(option => option.Length);

        int selected = 0;
        int top = Console.CursorTop;
        for (int i = 0; i < options.Count(); i++)
        {
            // Start by highlighting the first option.
            if (i == 0)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
            }

            var option = options.ElementAt(i);
            // Pad every option to make them the same width, so the highlight is equally wide everywhere.
            Console.WriteLine("- " + option.PadRight(width));

            Console.ResetColor();
        }
        Console.CursorLeft = 0;
        Console.CursorTop = top - 1;

        ConsoleKey? key = null;
        while (key != ConsoleKey.Enter)
        {
            key = Console.ReadKey(intercept: true).Key;

            // First restore the previously selected option so it's not highlighted anymore.
            Console.CursorTop = top + selected;
            string oldOption = options.ElementAt(selected);
            Console.Write("- " + oldOption.PadRight(width));
            Console.CursorLeft = 0;
            Console.ResetColor();

            // Then find the new selected option.
            if (key == ConsoleKey.DownArrow)
            {
                selected = Math.Min(selected + 1, options.Count() - 1);
            }
            else if (key == ConsoleKey.UpArrow)
            {
                selected = Math.Max(selected - 1, 0);
            }

            // Finally highlight the new selected option.
            Console.CursorTop = top + selected;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            string newOption = options.ElementAt(selected);
            Console.Write("- " + newOption.PadRight(width));
            Console.CursorLeft = 0;
            // Place the cursor one step above the new selected option so that we can scroll and also see the option above.
            Console.CursorTop = top + selected - 1;
            Console.ResetColor();
        }

        // Afterwards, place the cursor below the menu so we can see whatever comes next.
        Console.CursorTop = top + options.Count();

        // Show the cursor again and return the selected option.
        Console.CursorVisible = true;
        return selected;
    }




    [TestClass]
    public class FunctionsTests
    {
        [TestMethod]
        public void TestCreateVaccinationOrder_AllConditionsMet()
        {
            Functions functions = new Functions();
            string[] input = new string[]
            {
               "20010101-1234,LastName1,FirstName1,1,0,0",
               "20020101-5678,LastName2,FirstName2,0,1,0"
            };

            int doses = 10;
            bool ageLimit = false;

            string[] result = functions.CreateVaccinationOrder(input, doses, ageLimit);

            // Verify that the result has the correct length based on the provided input
            Assert.AreEqual(input.Length, result.Length);

            // Verify that the expected individuals are included in the result
            Assert.IsTrue(result.Contains("20010101-1234,LastName1,FirstName1,2"));
            Assert.IsTrue(result.Contains("20020101-5678,LastName2,FirstName2,2"));
        }


        [TestMethod]
        public void TestCreateVaccinationOrder_AgeLimitEnabled()
        {
            Functions functions = new Functions();
            string[] input = new string[]
            {
              "20010101-1234,LastName1,FirstName1,1,0,0",
              "20180101-5678,LastName2,FirstName2,0,1,0"
            };

            int doses = 10;
            bool ageLimit = true;

            string[] result = functions.CreateVaccinationOrder(input, doses, ageLimit);

            // Verify that, when age limit is enabled, only individuals 18 years and older are included
            Assert.AreEqual(1, result.Length);
            Assert.IsTrue(result.Contains("20010101-1234,LastName1,FirstName1,2"));
        }


        [TestMethod]
        public void TestCreateVaccinationOrder_InsufficientDoses()
        {
            Functions functions = new Functions();
            string[] input = new string[]
            {
                "20010101-1234,LastName1,FirstName1,1,0,0",
                "20020101-5678,LastName2,FirstName2,0,1,0"
            };

            int doses = 1;
            bool ageLimit = false;

            string[] result = functions.CreateVaccinationOrder(input, doses, ageLimit);

            // Verify that the method handles insufficient doses correctly
            Assert.AreEqual(doses, result.Length);
        }


        [TestMethod]
        public void TestCreateVaccinationOrder_SufficientDoses()
        {
            Functions functions = new Functions();
            string[] input = new string[]
            {
                "20010101-1234,LastName1,FirstName1,1,0,0",
                "20020101-5678,LastName2,FirstName2,0,1,0",
                "20030101-9876,LastName3,FirstName3,1,0,0"
            };

            int doses = 3;  // Number of doses is sufficient for all individuals
            bool ageLimit = false;

            string[] result = functions.CreateVaccinationOrder(input, doses, ageLimit);

            // Verify that, when there are sufficient doses, all individuals are included
            Assert.AreEqual(input.Length, result.Length);
        }

        [TestMethod]
        public void TestPrioritySorting()
        {
            Functions functions = new Functions();

            // Define input data with healthcare employees having different birthdates
            string[] input = new string[]
            {
              "20020101-5678,LastName1,FirstName1,1,0,0",
              "19571212-2222,LastName2,FirstName2,0,0,0",
              "20121212-2345,LastName3,FirstName3,0,1,0",
              "19450606-6532,LastName4,FirstName4,1,0,0",
            };

            int doses = 10;
            bool ageLimit = false;

            string[] result = functions.CreateVaccinationOrder(input, doses, ageLimit);

            // The priority order you expect
            string[] expectedOrder = new string[]
            {
              "19450606-6532,LastName4,FirstName4,2",
              "20020101-5678,LastName1,FirstName1,2",
              "19571212-2222,LastName2,FirstName2,2",
              "20121212-2345,LastName3,FirstName3,2",
            };

            // Compare the actual result with the expected order
            CollectionAssert.AreEqual(expectedOrder, result);
        }

        [TestMethod]
        public void TestConvertToCorrectFormat()
        {
            Functions functions = new Functions();

            // Test input formats with incorrect year parts
            string[] testCases = new string[]
            {
               "9901011234",  // Incorrect year part (YYMMDDNNNN), should convert to 19990101-1234
               "950101-1234",  // Incorrect year part (YYMMDD-NNNN), should convert to 19950101-1234
            };

            // Expected results
            string[] expectedResults = new string[]
            {
               "19990101-1234",
               "19950101-1234",
            };

            for (int i = 0; i < testCases.Length; i++)
            {
                string result = functions.ConvertToCorrectFormat(testCases[i]);
                Assert.AreEqual(expectedResults[i], result);
            }
        }


    }
}