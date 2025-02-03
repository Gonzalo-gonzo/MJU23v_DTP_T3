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
                Console.WriteLine($"Fel vid inläsning av fil: {ex.Message}");
            }

            Console.WriteLine("Välkommen till länklistan! Skriv 'hjälp' för hjälp!");

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
                    Console.WriteLine("Hej då! Välkommen åter!");
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
                        Console.WriteLine("Inga länkar att visa.");
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
                    Console.Write("  ange kategori: ");
                    string category = Console.ReadLine();
                    Console.Write("  ange grupp: ");
                    string group = Console.ReadLine();
                    Console.Write("  ange namn: ");
                    string name = Console.ReadLine();
                    Console.Write("  ange beskrivning: ");
                    string description = Console.ReadLine();
                    Console.Write("  ange länk: ");
                    string url = Console.ReadLine();

                    if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        Console.WriteLine($"Fel: URL '{url}' är ogiltig. Ange en giltig URL.");
                        continue;
                    }

                    Link newLink = new Link(category, group, name, description, url);
                    links.Add(newLink);
                    Console.WriteLine($"Länk '{name}' har skapats.");
                }
                else if (command == "sök")
                {
                    // Implementera sökfunktionalitet.
                    if (cmdParts.Length < 3)
                    {
                        Console.WriteLine("Fel: Ange vad du vill söka efter och sökordet. Exempel: sök namn Skola");
                        continue;
                    }

                    string searchField = cmdParts[1].ToLower();
                    string searchTerm = string.Join(' ', cmdParts.Skip(2)).ToLower();

                    var searchResults = new List<Link>();

                    switch (searchField)
                    {
                        case "namn":
                            searchResults = links.Where(link => link.Name.ToLower().Contains(searchTerm)).ToList();
                            break;
                        case "kategori":
                            searchResults = links.Where(link => link.Category.ToLower().Contains(searchTerm)).ToList();
                            break;
                        case "grupp":
                            searchResults = links.Where(link => link.Group.ToLower().Contains(searchTerm)).ToList();
                            break;
                        default:
                            Console.WriteLine($"Fel: Ogiltigt sökfält '{searchField}'. Tillåtna fält är 'namn', 'kategori' och 'grupp'.");
                            continue;
                    }

                    if (searchResults.Any())
                    {
                        Console.WriteLine($"Sökresultat för '{searchTerm}' i fältet '{searchField}':");
                        foreach (var result in searchResults)
                        {
                            result.Print(links.IndexOf(result));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Inga resultat hittades för '{searchTerm}' i fältet '{searchField}'.");
                    }
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
                            Console.WriteLine($"Länkar sparade till {filePath}.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Fel vid sparning av fil: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Fel: Ange ett filnamn för att spara.");
                    }
                }
                else if (command == "ladda")
                {
                    if (cmdParts.Length == 2)
                    {
                        filePath = $@"..\..\..\links\{cmdParts[1]}";

                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine($"Fel: Filen '{filePath}' hittades inte. Kontrollera filnamnet och försök igen.");
                            continue;
                        }

                        try
                        {
                            links = LoadLinksFromFile(filePath);
                            Console.WriteLine($"Länkar laddade från {filePath}.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Fel vid laddning av fil: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Fel: Ange en giltig filväg.");
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

            // Hjälp för att söka efter länkar.
            Console.WriteLine("sök <fält> <sökord> - Sök efter länkar baserat på ett fält. Tillåtna fält är:");
            Console.WriteLine("                     namn, kategori eller grupp. Exempel:");
            Console.WriteLine("                     sök namn Skola");
        }

        private static void HandleUnknownCommand(string command)
        {
            var validCommands = new[] { "hjälp", "sluta", "lista", "ny", "spara", "ladda", "sök" };

            Console.WriteLine($"Okänt kommando: '{command}'. Tillgängliga kommandon är: {string.Join(", ", validCommands)}.");
        }
    }
}
