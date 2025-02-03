using System.Diagnostics;

namespace MJU23v_DTP_T2
{
    internal class Program
    {
        static List<Link> links = new List<Link>();

        class Link
        {
            public string Category { get; set; }
            public string Group { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }

            public Link(string category, string group, string name, string description, string url)
            {
                Category = category;
                Group = group;
                Name = name;
                Description = description;
                Url = url;
            }

            public Link(string line)
            {
                string[] parts = line.Split('|');
                if (parts.Length != 5)
                {
                    throw new ArgumentException("Felaktigt format i länkraden. Kontrollera att varje rad innehåller exakt 5 delar separerade med '|'.");
                }
                Category = parts[0];
                Group = parts[1];
                Name = parts[2];
                Description = parts[3];
                Url = parts[4];
            }

            public void Print(int index)
            {
                Console.WriteLine($"|{index,-2}|{Category,-10}|{Group,-10}|{Name,-20}|{Description,-40}|");
            }

            public string Serialize()
            {
                return $"{Category}|{Group}|{Name}|{Description}|{Url}";
            }
        }

        static void Main(string[] args)
        {
            string filePath = @"..\..\..\links\links.lis";

            try
            {
                links = LoadLinksFromFile(filePath);
            }
            catch (Exception ex)
            {
                PrintError($"Fel vid inläsning av fil: {ex.Message}");
            }

            PrintInfo("Välkommen till länklistan! Skriv 'hjälp' för hjälp!");

            do
            {
                Console.Write("> ");
                string cmdInput = Console.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(cmdInput))
                {
                    HandleUnknownCommand("");
                    continue;
                }

                string[] cmdParts = cmdInput.Split();
                string command = cmdParts[0];

                if (command == "sluta")
                {
                    PrintInfo("Hej då! Välkommen åter!");
                    break;
                }
                else if (command == "hjälp")
                {
                    PrintHelp();
                }
                else if (command == "lista")
                {
                    if (links.Count == 0)
                    {
                        PrintWarning("Inga länkar att visa.");
                    }
                    else
                    {
                        int index = 0;
                        foreach (Link linkObj in links)
                            linkObj.Print(index++);
                    }
                }
                else if (command == "ny")
                {
                    Console.WriteLine("Skapa en ny länk:");

                    // Förbättrad validering för att undvika tomma fält.
                    Console.Write("  ange kategori: ");
                    string category = ReadNonEmptyInput("kategori");

                    Console.Write("  ange grupp: ");
                    string group = ReadNonEmptyInput("grupp");

                    Console.Write("  ange namn: ");
                    string name = ReadNonEmptyInput("namn");

                    Console.Write("  ange beskrivning: ");
                    string description = ReadNonEmptyInput("beskrivning");

                    Console.Write("  ange länk: ");
                    string url = ReadNonEmptyInput("URL");

                    // Validera att URL-formatet är korrekt.
                    if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        PrintError($"URL '{url}' är ogiltig. Ange en giltig URL.");
                        continue;
                    }

                    Link newLink = new Link(category, group, name, description, url);
                    links.Add(newLink);
                    PrintSuccess($"Länk '{name}' har skapats.");
                }
                else if (command == "spara")
                {
                    if (cmdParts.Length == 2)
                    {
                        filePath = $@"..\..\..\links\{cmdParts[1]}";
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(filePath))
                            {
                                foreach (Link linkObj in links)
                                {
                                    writer.WriteLine(linkObj.Serialize());
                                }
                            }
                            PrintSuccess($"Länkar sparade till {filePath}.");
                        }
                        catch (Exception ex)
                        {
                            PrintError($"Fel vid sparning av fil: {ex.Message}");
                        }
                    }
                    else
                    {
                        PrintError("Ange ett filnamn för att spara.");
                    }
                }
                else if (command == "ladda")
                {
                    if (cmdParts.Length == 2)
                    {
                        filePath = $@"..\..\..\links\{cmdParts[1]}";

                        if (!File.Exists(filePath))
                        {
                            PrintError($"Filen '{filePath}' hittades inte. Kontrollera filnamnet och försök igen.");
                            continue;
                        }

                        try
                        {
                            links = LoadLinksFromFile(filePath);
                            PrintSuccess($"Länkar laddade från {filePath}.");
                        }
                        catch (Exception ex)
                        {
                            PrintError($"Fel vid laddning av fil: {ex.Message}");
                        }
                    }
                    else
                    {
                        PrintError("Ange en giltig filväg.");
                    }
                }
                else
                {
                    HandleUnknownCommand(command);
                }

            } while (true);
        }

        private static List<Link> LoadLinksFromFile(string filePath)
        {
            List<Link> linksList = new List<Link>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                int index = 0;
                string currentLine;

                while ((currentLine = reader.ReadLine()) != null)
                {
                    Link linkObj = new Link(currentLine);
                    linksList.Add(linkObj);
                }
            }

            return linksList;
        }

        private static void PrintHelp()
        {
            // Fullständig hjälptext med detaljer och exempel.
            Console.WriteLine("Kommandon och deras användning:");

            // Hjälp för generella kommandon.
            Console.WriteLine("hjälp           - Skriv ut denna hjälptext.");
            Console.WriteLine("sluta           - Avsluta programmet.");

            // Hjälp för kommandon som hanterar länkar.
            Console.WriteLine("lista           - Lista alla länkar i systemet.");

            // Hjälp för att skapa nya länkar.
            Console.WriteLine("ny              - Skapa en ny länk. Du kommer att bli ombedd att ange:");
            Console.WriteLine("                  kategori, grupp, namn, beskrivning och URL.");

            // Hjälp för att spara och ladda filer.
            Console.WriteLine("spara <filnamn> - Spara alla länkar till en fil med angivet namn. Exempel:");
            Console.WriteLine("                  spara mina_lankar.txt");

            // Hjälp för att ladda filer.
            Console.WriteLine("ladda <filnamn> - Ladda länkar från en fil med angivet namn. Exempel:");
            Console.WriteLine("                  ladda mina_lankar.txt");

        }

        private static void HandleUnknownCommand(string command)
        {
            PrintError($"Okänt kommando: '{command}'. Skriv 'hjälp' för en lista över tillgängliga kommandon.");
        }

        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static string ReadNonEmptyInput(string fieldName)
        {
            string input;
            do
            {
                input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input))
                {
                    PrintError($"Fältet '{fieldName}' får inte vara tomt. Försök igen:");
                }
            } while (string.IsNullOrWhiteSpace(input));

            return input;
        }
    }
}
